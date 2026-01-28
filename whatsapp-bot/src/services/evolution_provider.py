"""
Provedor Evolution API para WhatsApp
"""
import logging
import httpx
from typing import Optional, Dict, Any
from .whatsapp_provider import WhatsAppProvider

logger = logging.getLogger(__name__)


class EvolutionProvider(WhatsAppProvider):
    """Implementação Evolution API para WhatsApp"""
    
    def __init__(self, base_url: str, api_key: str, instance_name: str):
        self.base_url = base_url.rstrip('/')
        self.api_key = api_key
        self.instance_name = instance_name
        self.client = httpx.AsyncClient(
            headers={
                'Content-Type': 'application/json',
                'apikey': api_key
            },
            timeout=30.0
        )
        
    async def send_message(self, to: str, message: str) -> Dict[str, Any]:
        """
        Envia mensagem via Evolution API
        
        Args:
            to: Número de telefone (formato: 5511999999999)
            message: Texto da mensagem
            
        Returns:
            Dict com informações da mensagem enviada
        """
        try:
            # Limpar número (remover whatsapp: se existir)
            number = to.replace('whatsapp:', '').replace('+', '').replace(' ', '')
            
            # Garantir formato com @s.whatsapp.net
            if '@' not in number:
                number = f"{number}@s.whatsapp.net"
            
            payload = {
                "number": number,
                "text": message,
                "delay": 1200  # 1.2 segundos de delay
            }
            
            url = f"{self.base_url}/message/sendText/{self.instance_name}"
            
            response = await self.client.post(url, json=payload)
            response.raise_for_status()
            
            data = response.json()
            
            logger.info(f"Evolution API message sent to {number}")
            
            return {
                'success': True,
                'message_id': data.get('key', {}).get('id', ''),
                'status': 'sent',
                'provider': 'evolution',
                'response': data
            }
            
        except httpx.HTTPStatusError as e:
            logger.error(f"Evolution API HTTP error: {e.response.status_code} - {e.response.text}")
            return {
                'success': False,
                'error': f"HTTP {e.response.status_code}: {e.response.text}",
                'provider': 'evolution'
            }
        except Exception as e:
            logger.error(f"Error sending Evolution API message: {str(e)}")
            return {
                'success': False,
                'error': str(e),
                'provider': 'evolution'
            }
    
    async def send_media(self, to: str, media_url: str, caption: Optional[str] = None) -> Dict[str, Any]:
        """
        Envia mídia via Evolution API
        
        Args:
            to: Número de telefone
            media_url: URL da mídia
            caption: Legenda opcional
        """
        try:
            number = to.replace('whatsapp:', '').replace('+', '').replace(' ', '')
            if '@' not in number:
                number = f"{number}@s.whatsapp.net"
            
            payload = {
                "number": number,
                "mediaUrl": media_url,
                "caption": caption or "",
                "delay": 1200
            }
            
            url = f"{self.base_url}/message/sendMedia/{self.instance_name}"
            
            response = await self.client.post(url, json=payload)
            response.raise_for_status()
            
            data = response.json()
            
            logger.info(f"Evolution API media sent to {number}")
            
            return {
                'success': True,
                'message_id': data.get('key', {}).get('id', ''),
                'status': 'sent',
                'provider': 'evolution',
                'response': data
            }
            
        except Exception as e:
            logger.error(f"Error sending Evolution API media: {str(e)}")
            return {
                'success': False,
                'error': str(e),
                'provider': 'evolution'
            }
    
    def validate_webhook(self, request_data: Dict) -> bool:
        """
        Valida webhook do Evolution API
        
        Args:
            request_data: Dados do webhook
            
        Returns:
            True se válido
        """
        # Evolution API usa API key na header para validação
        api_key = request_data.get('headers', {}).get('apikey', '')
        return api_key == self.api_key
    
    def parse_incoming_message(self, request_data: Dict) -> Dict[str, str]:
        """
        Extrai informações da mensagem Evolution API
        
        Args:
            request_data: JSON do webhook Evolution
            
        Returns:
            Dict com from_number, message, media_url
        """
        # Evolution API envia estrutura mais complexa
        data = request_data.get('data', {})
        key = data.get('key', {})
        message = data.get('message', {})
        
        # Extrair conteúdo da mensagem
        text = ''
        media_url = ''
        
        if 'conversation' in message:
            text = message['conversation']
        elif 'extendedTextMessage' in message:
            text = message['extendedTextMessage'].get('text', '')
        elif 'imageMessage' in message:
            media_url = message['imageMessage'].get('url', '')
            text = message['imageMessage'].get('caption', '')
        
        # Extrair número (remover @s.whatsapp.net)
        from_number = key.get('remoteJid', '').replace('@s.whatsapp.net', '')
        
        return {
            'from_number': f'whatsapp:+{from_number}',
            'to_number': '',  # Instance que recebeu
            'message': text.strip(),
            'media_url': media_url,
            'message_id': key.get('id', ''),
            'profile_name': data.get('pushName', '')
        }
    
    async def close(self):
        """Fecha conexão HTTP"""
        await self.client.aclose()
