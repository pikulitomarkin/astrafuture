"""
Cliente para comunicação com a API backend
"""
import logging
from typing import Optional, List, Dict, Any
import requests
from datetime import datetime, date

logger = logging.getLogger(__name__)


class APIClient:
    """Cliente para interagir com a API do Astra Agenda"""
    
    def __init__(self, base_url: str, api_key: str):
        self.base_url = base_url.rstrip('/')
        self.api_key = api_key
        self.session = requests.Session()
        self.session.headers.update({
            'Content-Type': 'application/json',
            'X-API-Key': api_key
        })
    
    def _request(self, method: str, endpoint: str, **kwargs) -> Dict[Any, Any]:
        """Faz requisição HTTP à API"""
        url = f"{self.base_url}/{endpoint.lstrip('/')}"
        
        try:
            response = self.session.request(method, url, **kwargs)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            logger.error(f"Erro na requisição {method} {url}: {str(e)}")
            raise
    
    # Clientes
    def get_customer_by_phone(self, phone: str) -> Optional[Dict]:
        """Busca cliente por telefone"""
        try:
            return self._request('GET', f'/customers/phone/{phone}')
        except requests.exceptions.HTTPError as e:
            if e.response.status_code == 404:
                return None
            raise
    
    def create_customer(self, name: str, phone: str, email: Optional[str] = None) -> Dict:
        """Cria novo cliente"""
        data = {
            'name': name,
            'phone': phone,
            'email': email
        }
        return self._request('POST', '/customers', json=data)
    
    # Agendamentos
    def get_available_slots(self, date: date, resource_id: Optional[str] = None) -> List[Dict]:
        """Lista horários disponíveis para uma data"""
        params = {'date': date.isoformat()}
        if resource_id:
            params['resourceId'] = resource_id
        
        return self._request('GET', '/appointments/available', params=params)
    
    def create_appointment(self, customer_id: str, start_time: datetime, 
                          end_time: datetime, service: str,
                          resource_id: Optional[str] = None) -> Dict:
        """Cria novo agendamento"""
        data = {
            'customerId': customer_id,
            'startTime': start_time.isoformat(),
            'endTime': end_time.isoformat(),
            'service': service,
            'status': 'scheduled'
        }
        if resource_id:
            data['resourceId'] = resource_id
        
        return self._request('POST', '/appointments', json=data)
    
    def get_customer_appointments(self, customer_id: str) -> List[Dict]:
        """Lista agendamentos de um cliente"""
        return self._request('GET', f'/appointments/customer/{customer_id}')
    
    def cancel_appointment(self, appointment_id: str) -> Dict:
        """Cancela um agendamento"""
        return self._request('DELETE', f'/appointments/{appointment_id}')
    
    def update_appointment_status(self, appointment_id: str, status: str) -> Dict:
        """Atualiza status de um agendamento"""
        data = {'status': status}
        return self._request('PATCH', f'/appointments/{appointment_id}/status', json=data)
    
    # Health check
    def health_check(self) -> bool:
        """Verifica se a API está respondendo"""
        try:
            response = self.session.get(f"{self.base_url}/health", timeout=5)
            return response.status_code == 200
        except Exception:
            return False
