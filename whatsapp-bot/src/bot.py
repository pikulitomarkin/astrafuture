"""
Bot WhatsApp - Astra Agenda
Processa mensagens e gerencia agendamentos via WhatsApp
"""
import logging
from flask import Flask, request, jsonify
from twilio.twiml.messaging_response import MessagingResponse
from config import settings
from handlers.message_handler import MessageHandler
from services.api_client import APIClient

# Configurar logging
logging.basicConfig(
    level=getattr(logging, settings.log_level),
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Inicializar Flask
app = Flask(__name__)
app.config['SECRET_KEY'] = settings.secret_key

# Inicializar serviços
api_client = APIClient(settings.api_base_url, settings.api_key)
message_handler = MessageHandler(api_client)


@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        'status': 'healthy',
        'service': 'whatsapp-bot',
        'version': '1.0.0'
    }), 200


@app.route('/webhook', methods=['POST'])
def webhook():
    """
    Webhook para receber mensagens do WhatsApp via Twilio
    """
    try:
        # Extrair dados da mensagem
        incoming_msg = request.values.get('Body', '').strip()
        from_number = request.values.get('From', '')
        
        logger.info(f"Mensagem recebida de {from_number}: {incoming_msg}")
        
        # Processar mensagem
        response_text = message_handler.process_message(incoming_msg, from_number)
        
        # Criar resposta TwiML
        resp = MessagingResponse()
        resp.message(response_text)
        
        return str(resp), 200
        
    except Exception as e:
        logger.error(f"Erro ao processar webhook: {str(e)}", exc_info=True)
        
        resp = MessagingResponse()
        resp.message("❌ Desculpe, ocorreu um erro. Tente novamente mais tarde.")
        return str(resp), 200


@app.route('/send', methods=['POST'])
def send_message():
    """
    Endpoint para enviar mensagens proativas
    Usado para lembretes e notificações
    """
    try:
        data = request.get_json()
        to_number = data.get('to')
        message = data.get('message')
        
        if not to_number or not message:
            return jsonify({'error': 'Missing to or message'}), 400
        
        from twilio.rest import Client
        client = Client(settings.twilio_account_sid, settings.twilio_auth_token)
        
        msg = client.messages.create(
            body=message,
            from_=settings.twilio_whatsapp_number,
            to=f'whatsapp:{to_number}'
        )
        
        logger.info(f"Mensagem enviada para {to_number}: {msg.sid}")
        
        return jsonify({
            'success': True,
            'message_sid': msg.sid
        }), 200
        
    except Exception as e:
        logger.error(f"Erro ao enviar mensagem: {str(e)}", exc_info=True)
        return jsonify({'error': str(e)}), 500


if __name__ == '__main__':
    logger.info(f"🤖 Iniciando WhatsApp Bot na porta {settings.port}")
    logger.info(f"📡 Webhook: http://localhost:{settings.port}/webhook")
    logger.info(f"🏥 Health: http://localhost:{settings.port}/health")
    
    app.run(
        host='0.0.0.0',
        port=settings.port,
        debug=(settings.flask_env == 'development')
    )
