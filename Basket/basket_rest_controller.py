from fastapi import FastAPI, HTTPException, status #, Depends, status, APIRouter
from fastapi.middleware.cors import CORSMiddleware
# from fastapi.security import OAuth2PasswordBearer
from dotenv import load_dotenv
from contextlib import asynccontextmanager
from uuid import UUID, uuid4
import os
import redis
import json
import requests
#import py_eureka_client.eureka_client as eureka_client
from ticket import Ticket
# from jose import JWTError, jwt

load_dotenv()
REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
REDIS_PORT = int(os.getenv("REDIS_PORT", 6379))
TICKET_SERVICE_URL = os.getenv("TICKET_SERVICE_URL", "http://localhost:8080/api/tickets")

EUREKA_SERVER = os.getenv("EUREKA_SERVER", "http://localhost:8761/eureka")
SERVICE_NAME = os.getenv("SERVICE_NAME", "BasketServiceAPI")
BASKET_SERVICE_PORT = int(os.getenv("BASKET_SERVICE_PORT", 8082))
BASKET_SERVICE_IP = os.getenv("BASKET_SERVICE_IP", "127.0.0.1")
ENABLE_EUREKA = os.getenv("ENABLE_EUREKA", "false").lower() == "true"

redis_client = redis.Redis(
    host=REDIS_HOST,
    port=REDIS_PORT,
    db=0,
    decode_responses=True
)

eureka_client = None

# oauth_scheme = OAuth2PasswordBearer(tokenUrl="token")
# SECRET_KEY = os.getenv("JWT_KEY", "fakeKey")
# ALGORITHM = "HS256"

# async def get_current_user(token: str = Depends(oauth_scheme)):
#     credentials_exception = HTTPException(
#         status_code=status.HTTP_401_UNAUTHORIZED,
#         detail="Could not validate credentials",
#         headers={"WWW-Authenticate": "Bearer"},
#     )
#     try:
#         print(SECRET_KEY, token)
#         payload = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])

#         print(payload)
#         username: str = payload.get("claims.Name")
#         if username is None:
#             raise credentials_exception
#         return username
#     except JWTError:
#         raise credentials_exception

# router = APIRouter(dependencies=[Depends(get_current_user)])

def get_user_basket_key(user_id: str) -> str:
    return f"basket:{user_id}"

def fetch_ticket_details(ticket_id: UUID):
    try:
        response = requests.get(f"{TICKET_SERVICE_URL}/{ticket_id}")
        if response.status_code == 200:
            return response.json()
        return None
    except Exception as e:
        print(f"Error contacting Ticket Service: {e}")
        return None


@asynccontextmanager
async def lifespan(app: FastAPI):
    if ENABLE_EUREKA:
        print("Registering with Eureka...")
        await eureka_client.init_async(
            eureka_server=EUREKA_SERVER,
            app_name=SERVICE_NAME,
            instance_port=BASKET_SERVICE_PORT,
            instance_ip=BASKET_SERVICE_IP
        )
    else:
        print("Eureka registration is disabled.")
    yield
    print("Lifespan shutdown complete.")


app = FastAPI(lifespan=lifespan)


app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Replace for production!
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/api/basket/{user_id}", response_model=list[Ticket])
def get_basket(user_id: str):    
    key = get_user_basket_key(user_id)
    items = redis_client.hvals(key)
    basket = []

    print(f"Fetching basket for user {user_id}: found {len(items)} item(s).")

    for item in items:
        try:
            data = json.loads(item)
            basket.append(Ticket(**data))
        except Exception as e:
            print(f"Error parsing Redis item: {e}")

    return basket


@app.post("/api/basket/{user_id}", status_code=status.HTTP_201_CREATED)
def add_to_basket(user_id: str, ticket: Ticket):    
    key = get_user_basket_key(user_id)

    if ticket.price <= 0:
        raise HTTPException(status_code=400, detail="Ticket price must be greater than 0.")

    # Ensure unique ticketId
    if not ticket.id:
        ticket.id = uuid4()

    # Optional: Validate ticket exists in catalog
    # print(f"Ticket Details: {ticket}")
    ticket_data = fetch_ticket_details(ticket.id)
    # print(f"Fetched ticket details for ID {ticket.id}: {ticket_data}")
    if not ticket_data:
        raise HTTPException(status_code=404, detail="Ticket not found in Catalog Service.")


    print(f"Adding ticket {ticket.id} with ID {ticket.id} to {key}")

    redis_client.hset(key, str(ticket.id), ticket.model_dump_json())

    return {"message": f"Ticket '{ticket.event}' added to basket."}


@app.put("/api/basket/{user_id}/{ticket_uuid}")
def update_basket_item(user_id: str, ticket_uuid: UUID, ticket: Ticket):    
    key = get_user_basket_key(user_id)

    if not redis_client.hexists(key, str(ticket_uuid)):
        raise HTTPException(status_code=404, detail="Ticket not found in basket.")

    print(f"Updating ticket {ticket_uuid} in {key}")

    redis_client.hset(key, str(ticket_uuid), ticket.model_dump_json())

    return {"message": f"Ticket '{ticket.event}' updated in basket."}



@app.delete("/api/basket/{user_id}/{ticket_uuid}")
def remove_from_basket(user_id: str, ticket_uuid: UUID):  
    key = get_user_basket_key(user_id)

    if redis_client.hdel(key, str(ticket_uuid)) == 0:
        raise HTTPException(status_code=404, detail="Ticket not found in basket.")

    print(f"Removed ticket {ticket_uuid} from {key}")

    return {"message": f"Ticket {ticket_uuid} removed from basket."}


@app.delete("/api/basket/{user_id}")
def clear_basket(user_id: str):    
    key = get_user_basket_key(user_id)
    redis_client.delete(key)
    print(f"Cleared basket for {key}")
    return {"message": "Basket cleared."}