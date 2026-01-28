"""
Interface abstrata para provedores de WhatsApp
"""
from abc import ABC, abstractmethod
from typing import Optional, Dict, Any


class WhatsAppProvider(ABC):
    """Interface para provedores de WhatsApp"""
    
    @abstractmethod
    async def send_message(self, to: str, message: str) -> Dict[str, Any]:
        """Envia mensagem de texto"""
        pass
    
    @abstractmethod
    async def send_media(self, to: str, media_url: str, caption: Optional[str] = None) -> Dict[str, Any]:
        """Envia mídia (imagem, vídeo, documento)"""
        pass
    
    @abstractmethod
    def validate_webhook(self, request_data: Dict) -> bool:
        """Valida webhook recebido"""
        pass
    
    @abstractmethod
    def parse_incoming_message(self, request_data: Dict) -> Dict[str, str]:
        """Extrai informações da mensagem recebida"""
        pass
