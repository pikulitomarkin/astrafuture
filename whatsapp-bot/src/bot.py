"""
Bot WhatsApp - Astra Agenda
Processa mensagens e gerencia agendamentos via WhatsApp
"""
import logging
import asyncio
from flask import Flask, request, jsonify
from config import settings
from handlers.message_handler import MessageHandler
from services.api_client import APIClient
from services.twilio_provider import TwilioProvider
from services.evolution_provider import EvolutionProvider

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

# Inicializar provedor WhatsApp baseado na configuração
if settings.whatsapp_provider == 'evolution':
    logger.info("Usando Evolution API como provedor WhatsApp")
    whatsapp_provider = EvolutionProvider(
        base_url=settings.evolution_api_url,
        api_key=settings.evolution_api_key,
        instance_name=settings.evolution_instance_name
    )
else:
    logger.info("Usando Twilio como provedor WhatsApp")
    whatsapp_provider = TwilioProvider(
        account_sid=settings.twilio_account_sid,
        auth_token=settings.twilio_auth_token,
        whatsapp_number=settings.twilio_whatsapp_number
    )

message_handler = MessageHandler(api_client, whatsapp_provider)


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
    Webhook universal para receber mensagens do WhatsApp
    Suporta Twilio e Evolution API
    """
    try:
        # Determinar tipo de webhook baseado no provider
        if settings.whatsapp_provider == 'evolution':
            # Evolution API envia JSON
            data = request.get_json() or {}
            logger.info(f"Evolution webhook recebido: {data.get('event', 'unknown')}")
            
            # Validar webhook
            webhook_data = {
                'headers': dict(request.headers),
                'body': data
            }
            if not whatsapp_provider.validate_webhook(webhook_data):
                logger.warning("Evolution webhook inválido")
                return jsonify({'error': 'Unauthorized'}), 401
            
            # Parsear mensagem
            msg_data = whatsapp_provider.parse_incoming_message(data)
            
        else:
            # Twilio envia form data
            logger.info(f"Twilio webhook recebido de {request.values.get('From', '')}")
            msg_data = whatsapp_provider.parse_incoming_message(dict(request.values))
        
        incoming_msg = msg_data.get('message', '')
        from_number = msg_data.get('from_number', '')
        
        if not incoming_msg or not from_number:
            # Responder de forma adequada para cada provedor
            if settings.whatsapp_provider == 'evolution':
                return jsonify({'error': 'Missing message or from_number'}), 400
            else:
                from twilio.twiml.messaging_response import MessagingResponse
                resp = MessagingResponse()
                resp.message("❌ Webhook inválido ou incompleto.")
                return str(resp), 200
        
        # Processar mensagem e montar resposta
        try:
            reply = message_handler.process_message(incoming_msg, from_number)
            
            if settings.whatsapp_provider == 'evolution':
                return jsonify({'reply': reply}), 200
            else:
                from twilio.twiml.messaging_response import MessagingResponse
                resp = MessagingResponse()
                resp.message(reply)
                return str(resp), 200
        except Exception as e:
            logger.error(f"Erro ao processar mensagem: {str(e)}", exc_info=True)
            if settings.whatsapp_provider == 'evolution':
                return jsonify({'error': 'Internal error'}), 500
            else:
                from twilio.twiml.messaging_response import MessagingResponse
                resp = MessagingResponse()
                resp.message("❌ Desculpe, ocorreu um erro. Tente novamente mais tarde.")
                return str(resp), 200
    except Exception as e:
        logger.error(f"Erro ao processar webhook: {str(e)}", exc_info=True)
        if settings.whatsapp_provider == 'evolution':
            return jsonify({'error': 'Internal error'}), 500
        else:
            from twilio.twiml.messaging_response import MessagingResponse
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
