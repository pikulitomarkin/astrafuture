"""
Handler para processar mensagens do WhatsApp
"""
import logging
import re
from datetime import datetime, timedelta
from typing import Dict, Optional
from services.api_client import APIClient

logger = logging.getLogger(__name__)


class MessageHandler:
    """Processa mensagens recebidas do WhatsApp"""
    
    def __init__(self, api_client: APIClient):
        self.api = api_client
        self.user_sessions: Dict[str, Dict] = {}  # Cache de sessões de usuário
    
    def process_message(self, message: str, from_number: str) -> str:
        """
        Processa mensagem e retorna resposta
        """
        message = message.lower().strip()
        
        # Comandos básicos
        if message in ['oi', 'olá', 'ola', 'hey', 'inicio', 'start', 'menu']:
            return self._menu_principal()
        
        if message in ['ajuda', 'help', '?']:
            return self._help_message()
        
        if message == '1' or 'agendar' in message:
            return self._iniciar_agendamento(from_number)
        
        if message == '2' or 'meus agendamentos' in message:
            return self._listar_agendamentos(from_number)
        
        if message == '3' or 'cancelar' in message:
            return self._iniciar_cancelamento(from_number)
        
        # Processar fluxo de agendamento
        session = self.user_sessions.get(from_number, {})
        if session.get('status') == 'aguardando_nome':
            return self._processar_nome(from_number, message)
        
        if session.get('status') == 'aguardando_data':
            return self._processar_data(from_number, message)
        
        if session.get('status') == 'aguardando_horario':
            return self._processar_horario(from_number, message)
        
        if session.get('status') == 'aguardando_servico':
            return self._processar_servico(from_number, message)
        
        # Mensagem não reconhecida
        return ("❓ Desculpe, não entendi sua mensagem.\n\n"
                "Digite *menu* para ver as opções disponíveis.")
    
    def _menu_principal(self) -> str:
        """Retorna menu principal"""
        return ("🌟 *Bem-vindo ao Astra Agenda!*\n\n"
                "Escolha uma opção:\n\n"
                "1️⃣ Fazer novo agendamento\n"
                "2️⃣ Ver meus agendamentos\n"
                "3️⃣ Cancelar agendamento\n\n"
                "Digite o número da opção desejada.")
    
    def _help_message(self) -> str:
        """Mensagem de ajuda"""
        return ("ℹ️ *Como usar o Astra Agenda*\n\n"
                "Comandos disponíveis:\n"
                "• *menu* - Ver menu principal\n"
                "• *agendar* - Fazer novo agendamento\n"
                "• *meus agendamentos* - Ver seus agendamentos\n"
                "• *cancelar* - Cancelar um agendamento\n\n"
                "Precisa de ajuda? Entre em contato conosco!")
    
    def _iniciar_agendamento(self, from_number: str) -> str:
        """Inicia processo de agendamento"""
        # Verificar se cliente já existe
        phone = from_number.replace('whatsapp:', '')
        customer = self.api.get_customer_by_phone(phone)
        
        if customer:
            # Cliente já cadastrado
            self.user_sessions[from_number] = {
                'status': 'aguardando_data',
                'customer': customer
            }
            return (f"✅ Olá *{customer['name']}*!\n\n"
                   "📅 Para qual data você gostaria de agendar?\n"
                   "Digite no formato: DD/MM/YYYY\n"
                   "Exemplo: 30/01/2026")
        else:
            # Cliente novo - solicitar nome
            self.user_sessions[from_number] = {
                'status': 'aguardando_nome'
            }
            return ("👋 Olá! Vejo que é sua primeira vez aqui.\n\n"
                   "📝 Por favor, digite seu nome completo:")
    
    def _processar_nome(self, from_number: str, nome: str) -> str:
        """Processa nome do novo cliente"""
        # Criar cliente
        phone = from_number.replace('whatsapp:', '')
        
        try:
            customer = self.api.create_customer(
                name=nome.title(),
                phone=phone
            )
            
            self.user_sessions[from_number] = {
                'status': 'aguardando_data',
                'customer': customer
            }
            
            return (f"✅ Prazer em conhecê-lo, *{customer['name']}*!\n\n"
                   "📅 Para qual data você gostaria de agendar?\n"
                   "Digite no formato: DD/MM/YYYY\n"
                   "Exemplo: 30/01/2026")
        
        except Exception as e:
            logger.error(f"Erro ao criar cliente: {e}")
            return "❌ Erro ao cadastrar. Tente novamente mais tarde."
    
    def _processar_data(self, from_number: str, data_str: str) -> str:
        """Processa data escolhida"""
        try:
            # Tentar parsear data
            data = datetime.strptime(data_str, '%d/%m/%Y').date()
            
            # Validar se data é futura
            if data < datetime.now().date():
                return "❌ Data inválida. Por favor, escolha uma data futura."
            
            # Buscar horários disponíveis
            slots = self.api.get_available_slots(data)
            
            if not slots:
                return ("❌ Não há horários disponíveis nesta data.\n"
                       "Por favor, escolha outra data.")
            
            # Atualizar sessão
            session = self.user_sessions[from_number]
            session['status'] = 'aguardando_horario'
            session['data'] = data
            session['slots'] = slots
            
            # Montar mensagem com horários
            horarios_text = "\n".join([
                f"{i+1}. {slot['startTime'][11:16]}"
                for i, slot in enumerate(slots[:10])  # Max 10 horários
            ])
            
            return (f"✅ Data selecionada: *{data.strftime('%d/%m/%Y')}*\n\n"
                   f"⏰ Horários disponíveis:\n{horarios_text}\n\n"
                   "Digite o número do horário desejado:")
        
        except ValueError:
            return ("❌ Formato de data inválido.\n"
                   "Use: DD/MM/YYYY\n"
                   "Exemplo: 30/01/2026")
    
    def _processar_horario(self, from_number: str, escolha: str) -> str:
        """Processa horário escolhido"""
        try:
            session = self.user_sessions[from_number]
            slots = session['slots']
            index = int(escolha) - 1
            
            if index < 0 or index >= len(slots):
                return "❌ Opção inválida. Digite um número da lista."
            
            slot = slots[index]
            session['status'] = 'aguardando_servico'
            session['slot'] = slot
            
            return ("✅ Horário selecionado!\n\n"
                   "💈 Qual serviço você deseja?\n"
                   "Digite o nome do serviço:")
        
        except ValueError:
            return "❌ Digite apenas o número da opção."
    
    def _processar_servico(self, from_number: str, servico: str) -> str:
        """Processa serviço e confirma agendamento"""
        try:
            session = self.user_sessions[from_number]
            customer = session['customer']
            slot = session['slot']
            
            # Criar agendamento
            start_time = datetime.fromisoformat(slot['startTime'])
            end_time = start_time + timedelta(hours=1)  # Duração padrão 1h
            
            appointment = self.api.create_appointment(
                customer_id=customer['id'],
                start_time=start_time,
                end_time=end_time,
                service=servico.title()
            )
            
            # Limpar sessão
            del self.user_sessions[from_number]
            
            return (f"✅ *Agendamento confirmado!*\n\n"
                   f"👤 Cliente: {customer['name']}\n"
                   f"📅 Data: {start_time.strftime('%d/%m/%Y')}\n"
                   f"⏰ Horário: {start_time.strftime('%H:%M')}\n"
                   f"💈 Serviço: {servico.title()}\n\n"
                   f"📱 Você receberá um lembrete antes do horário.\n\n"
                   f"Digite *menu* para mais opções.")
        
        except Exception as e:
            logger.error(f"Erro ao criar agendamento: {e}")
            return "❌ Erro ao confirmar agendamento. Tente novamente."
    
    def _listar_agendamentos(self, from_number: str) -> str:
        """Lista agendamentos do cliente"""
        phone = from_number.replace('whatsapp:', '')
        customer = self.api.get_customer_by_phone(phone)
        
        if not customer:
            return "❌ Você ainda não tem cadastro. Digite *agendar* para começar."
        
        try:
            appointments = self.api.get_customer_appointments(customer['id'])
            
            if not appointments:
                return "📅 Você não tem agendamentos no momento."
            
            # Filtrar apenas agendamentos futuros
            future = [apt for apt in appointments 
                     if datetime.fromisoformat(apt['startTime']) >= datetime.now()]
            
            if not future:
                return "📅 Você não tem agendamentos futuros."
            
            text = "📅 *Seus agendamentos:*\n\n"
            for apt in future[:5]:  # Max 5
                start = datetime.fromisoformat(apt['startTime'])
                text += (f"🗓️ {start.strftime('%d/%m/%Y às %H:%M')}\n"
                        f"   {apt.get('service', 'Serviço')}\n\n")
            
            return text + "Digite *menu* para mais opções."
        
        except Exception as e:
            logger.error(f"Erro ao listar agendamentos: {e}")
            return "❌ Erro ao buscar agendamentos."
    
    def _iniciar_cancelamento(self, from_number: str) -> str:
        """Inicia processo de cancelamento"""
        return ("⚠️ Para cancelar um agendamento, "
               "entre em contato diretamente conosco.\n\n"
               "Digite *menu* para voltar.")
