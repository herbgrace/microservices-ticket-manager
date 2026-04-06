# UserService — SEN300 BookStore

Owns the **UserServiceDB** database and the `Users` table.
Handles user registration and credential verification.
AuthService calls `POST /api/users/login` to verify credentials before issuing a JWT.

---

## Ports

| Environment | Port |
|---|---|
| Local (dotnet run) | 8085 |
| Docker container | 8085 → 8080 internal |

---

## Infrastructure Setup (first-time, run once)

```bash
# 1. Create Docker network (shared across all SEN300 services — skip if already exists)
docker network create netSEN300

# 2. Create persistent volume for UserService SQL Server data
docker volume create userservicevolume

# 3. Start SQL Server for UserService
#    NOTE: host port 1434 avoids conflict with OrderService which uses 1433
docker run --name SEN300UserServiceDBSqlServer -p 1434:1433 --net netSEN300 -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=abc123!!@" -v userservicevolume:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2019-latest

# 4. Wait ~15 seconds for SQL Server to initialize, then create the database
docker exec -it SEN300UserServiceDBSqlServer /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "abc123!!@" -Q "CREATE DATABASE UserServiceDB" -No
```

> EnsureCreated in Program.cs will automatically create the `Users` table on first run.

---

## Build & Run (Docker)

```bash
docker build -t sen300userserviceapi:1 .
docker run -d -p 8085:8080 --name SEN300UserServiceAPI --net netSEN300 sen300userserviceapi:1
```

---

## Optional Services (graceful if absent)

- **Eureka** — service discovery, currently commented out in Program.cs

---

## Testing Sequence

1. **Now** — basic REST API + SQL Server (`GET /api/users`, `POST /api/users`)
2. **Next** — spin up AuthService, which calls `POST /api/users/login` to verify credentials and issue a JWT
3. **Later** — introduce Eureka for service discovery

---

## Key Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | /api/users | No | All users |
| GET | /api/users/{userGuid} | No | Single user by GUID |
| POST | /api/users | No | Create user |
| POST | /api/users/login | No | Verify credentials — called by AuthService |
| PUT | /api/users/{userGuid} | No | Update user |
| DELETE | /api/users/{userGuid} | No | Delete user |
| GET | /scalar/v1 | No | Scalar API docs (dev only) |

---

## Optional Infrastructure

```bash
docker run --name SEN300EurekaRegistry -p 8761:8761 --net netSEN300 -d steeltoeoss/eureka-server:latest
```

---

## Helpful Docker Commands

```bash
docker volume list
docker volume create userservicevolume
docker volume rm userservicevolume
```
