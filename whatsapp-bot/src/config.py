"""
Configurações do Bot WhatsApp
"""
import os
from typing import Optional
from pydantic_settings import BaseSettings
from pydantic import Field


class Settings(BaseSettings):
    """Configurações da aplicação"""
    
    # API Backend
    api_base_url: str = Field(
        default="http://localhost:5000/api",
        description="URL base da API backend"
    )
    api_key: str = Field(
        default="",
        description="Chave de API para autenticação"
    )
    
    # WhatsApp Provider
    whatsapp_provider: str = Field(
        default="twilio",
        description="Provedor WhatsApp: 'twilio' ou 'evolution'"
    )
    
    # Twilio WhatsApp
    twilio_account_sid: str = Field(
        default="",
        description="Twilio Account SID"
    )
    twilio_auth_token: str = Field(
        default="",
        description="Twilio Auth Token"
    )
    twilio_whatsapp_number: str = Field(
        default="whatsapp:+14155238886",
        description="Número WhatsApp do Twilio"
    )
    
    # Evolution API
    evolution_api_url: str = Field(
        default="",
        description="URL base da Evolution API"
    )
    evolution_api_key: str = Field(
        default="",
        description="API Key da Evolution API"
    )
    evolution_instance_name: str = Field(
        default="",
        description="Nome da instância Evolution API"
    )
    
    # Servidor
    port: int = Field(default=5000, description="Porta do servidor")
    flask_env: str = Field(default="production", description="Ambiente Flask")
    secret_key: str = Field(default="change-me", description="Secret key para Flask")
    
    # Redis (opcional)
    redis_url: Optional[str] = Field(
        default=None,
        description="URL do Redis para cache"
    )
    
    # Configurações gerais
    timezone: str = Field(default="America/Sao_Paulo", description="Fuso horário")
    log_level: str = Field(default="INFO", description="Nível de log")
    
    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
        case_sensitive = False


# Instância global de configurações
settings = Settings()
