# AuthService — SEN300 BookStore

Issues **JWT tokens**. Has no database.
Calls `POST /api/users/login` on UserService to verify credentials, then signs and returns a JWT.
All other services validate incoming JWTs using the shared key in appsettings.

---

## Ports

| Environment | Port |
|---|---|
| Local (dotnet run) | 8084 |
| Docker container | 8084 → 8080 internal |

---

## Infrastructure Setup (first-time, run once)

```bash
# 1. Create Docker network (shared across all SEN300 services — skip if already exists)
docker network create netSEN300
```

> AuthService has **no database**. No SQL Server container needed.
> UserService must be running before AuthService can issue tokens.

---

## Build & Run (Docker)

```bash
docker build -t sen300authserviceapi:1 .
docker run -d -p 8084:8080 --name SEN300AuthServiceAPI --net netSEN300 sen300authserviceapi:1
```

---

## Dependencies

| Service | Why |
|---|---|
| UserService (8085) | Credential verification — AuthService calls it at login time |

---

## Flow

```
Client → POST /api/auth/createtoken/method1  { email, password }
           ↓
AuthService → POST http://SEN300UserServiceAPI:8080/api/users/login
           ↓
UserService verifies password hash → returns { userGuid, username, email }
           ↓
AuthService signs JWT with claims → returns token string
```

---

## Optional Services (graceful if absent)

- **Eureka** — service discovery, currently commented out in Program.cs

---

## Teaching Sequence

1. **Now** — stand up UserService first (AuthService needs it to verify credentials)
2. **Next** — call `POST /api/auth/createtoken/method1` with a valid user's email + password
3. **Then** — use the returned JWT as a `Bearer` token on protected endpoints in OrderService
4. **Later** — use `GET /api/auth/testbasicauth` to manually inspect and validate a token

---

## Key Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | /api/auth/test1 | No | Health check |
| POST | /api/auth/createtoken/method1 | No | Login — returns JWT |
| GET | /api/auth/testbasicauth | Bearer JWT | Manually validates a token (teaching tool) |
| GET | /scalar/v1 | No | Scalar API docs (dev only) |

---

## Optional Infrastructure

```bash
docker run --name SEN300EurekaRegistry -p 8761:8761 --net netSEN300 -d steeltoeoss/eureka-server:latest
```
