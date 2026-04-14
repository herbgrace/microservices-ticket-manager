# OrderService — SEN300 BookStore

Owns the **OrderServiceDB** database, the `Orders` table, and the `Books` table.
Handles order creation and retrieval. `POST /api/orders` requires a valid JWT from AuthService.
Notifies MessageService on order creation (graceful if MessageService is unavailable).

---

## Ports

| Environment | Port |
|---|---|
| Local (dotnet run) | 8083 |
| Docker container | 8083 → 8080 internal |

---

## Infrastructure Setup (first-time, run once)

```bash
# 1. Create Docker network (shared across all SEN300 services)
docker network create netSEN300

# 2. Create persistent volume for OrderService SQL Server data
docker volume create orderservicevolume

# 3. Start SQL Server for OrderService
docker run --name SEN300OrderServiceDBSqlServer -p 1433:1433 --net netSEN300 -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=abc123!!@" -v orderservicevolume:/var/opt/mssql  -d mcr.microsoft.com/mssql/server:2019-latest

# 4. Wait ~15 seconds for SQL Server to initialize, then create the database
docker exec -it SEN300OrderServiceDBSqlServer /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "abc123!!@" -Q "CREATE DATABASE OrderServiceDB" -No
```

> EnsureCreated in Program.cs will automatically create the `Orders` and `Books` tables on first run.

---

## Build & Run (Docker)

```bash
docker build -t sen300orderserviceapi:1 .
docker run -d -p 8083:8080 --name SEN300OrderServiceAPI --net netSEN300 sen300orderserviceapi:1
```

---

## Optional Services (graceful if absent)

- **MessageService** — POST /api/orders will still succeed; notification is skipped with a warning log
- **Eureka** — service discovery, currently commented out in Program.cs

---

## Teaching Sequence

1. **Now** — basic REST API + SQL Server (GET endpoints work without auth)
2. **Next** — spin up UserService + AuthService, get a JWT, test POST /api/orders
3. **Later** — spin up RabbitMQ + MessageService to see the full notification flow
4. **Later still** — introduce Eureka for service discovery

---

## Key Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | /api/orders | No | All orders |
| GET | /api/orders/with-books | No | All orders with books |
| GET | /api/orders/{orderGuid} | No | Single order by GUID |
| GET | /api/orders/user/{userGuid} | No | Orders for a user |
| POST | /api/orders | JWT required | Create order |
| DELETE | /api/orders/{orderGuid} | No | Delete order |
| GET | /api/orders/test | No | Health check |
| GET | /api/orders/get-my-ip | No | Diagnostics |

---

## Optional Infrastructure

```bash
docker run --name SEN300OrderServiceQueueRabbitMQ -p 15672:15672 -p 5672:5672 --net netSEN300 -d rabbitmq:3-management
docker run --name SEN300EurekaRegistry -p 8761:8761 --net netSEN300 -d steeltoeoss/eureka-server:latest
```

---

## Helpful Docker Commands

```bash
docker volume list
docker volume create orderservicevolume
docker volume rm orderservicevolume
```
