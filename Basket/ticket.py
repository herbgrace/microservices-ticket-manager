from pydantic import BaseModel, Field
from uuid import UUID
from datetime import datetime
from typing import Optional

class Ticket(BaseModel):
    id: UUID
    event: str
    price: float
    description: str
    event_date: datetime