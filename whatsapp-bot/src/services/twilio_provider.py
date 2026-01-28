"""
Provedor Twilio para WhatsApp
"""
import logging
from typing import Optional, Dict, Any
from twilio.rest import Client
from twilio.request_validator import RequestValidator
from .whatsapp_provider import WhatsAppProvider

logger = logging.getLogger(__name__)


class TwilioProvider(WhatsAppProvider):
    """Implementação Twilio para WhatsApp"""
    
    def __init__(self, account_sid: str, auth_token: str, whatsapp_number: str):
        self.account_sid = account_sid
        self.auth_token = auth_token
        self.whatsapp_number = whatsapp_number
        self.client = Client(account_sid, auth_token)
        self.validator = RequestValidator(auth_token)
        
    async def send_message(self, to: str, message: str) -> Dict[str, Any]:
        """
        Envia mensagem via Twilio
        
        Args:
            to: Número de telefone (formato: +5511999999999)
            message: Texto da mensagem
            
        Returns:
            Dict com informações da mensagem enviada
        """
        try:
            # Garantir formato WhatsApp
            if not to.startswith('whatsapp:'):
                to = f'whatsapp:{to}'
            
            msg = self.client.messages.create(
                body=message,
                from_=self.whatsapp_number,
                to=to
            )
            
            logger.info(f"Twilio message sent: {msg.sid}")
            
            return {
                'success': True,
                'message_id': msg.sid,
                'status': msg.status,
                'provider': 'twilio'
            }
            
        except Exception as e:
            logger.error(f"Error sending Twilio message: {str(e)}")
            return {
                'success': False,
                'error': str(e),
                'provider': 'twilio'
            }
    
    async def send_media(self, to: str, media_url: str, caption: Optional[str] = None) -> Dict[str, Any]:
        """Envia mídia via Twilio"""
        try:
            if not to.startswith('whatsapp:'):
                to = f'whatsapp:{to}'
            
            msg = self.client.messages.create(
                body=caption or '',
                from_=self.whatsapp_number,
                to=to,
                media_url=[media_url]
            )
            
            logger.info(f"Twilio media sent: {msg.sid}")
            
            return {
                'success': True,
                'message_id': msg.sid,
                'status': msg.status,
                'provider': 'twilio'
            }
            
        except Exception as e:
            logger.error(f"Error sending Twilio media: {str(e)}")
            return {
                'success': False,
                'error': str(e),
                'provider': 'twilio'
            }
    
    def validate_webhook(self, request_data: Dict) -> bool:
        """
        Valida webhook do Twilio
        
        Args:
            request_data: Dados do request (url, signature, form_data)
            
        Returns:
            True se válido
        """
        try:
            url = request_data.get('url', '')
            signature = request_data.get('signature', '')
            params = request_data.get('params', {})
            
            return self.validator.validate(url, params, signature)
            
        except Exception as e:
            logger.error(f"Error validating Twilio webhook: {str(e)}")
            return False
    
    def parse_incoming_message(self, request_data: Dict) -> Dict[str, str]:
        """
        Extrai informações da mensagem Twilio
        
        Args:
            request_data: Form data do webhook Twilio
            
        Returns:
            Dict com from_number, message, media_url
        """
        return {
            'from_number': request_data.get('From', ''),
            'to_number': request_data.get('To', ''),
            'message': request_data.get('Body', '').strip(),
            'media_url': request_data.get('MediaUrl0', ''),
            'message_sid': request_data.get('MessageSid', ''),
            'profile_name': request_data.get('ProfileName', '')
        }
