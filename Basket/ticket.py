from pydantic import BaseModel
from uuid import UUID
from datetime import datetime

class Ticket(BaseModel):
    id: UUID
    event: str
    price: float
    description: str
    eventDate: datetime