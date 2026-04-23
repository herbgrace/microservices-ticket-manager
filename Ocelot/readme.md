# OcelotAPIGateway — SEN300 BookStore

**API Gateway** for all six BookStore services.
All client traffic enters through the gateway — Ocelot routes each request to the correct backend service using Eureka for service discovery.

> **Note:** The gateway requires Eureka to be running. Stand up all individual services and verify their endpoints directly before introducing the gateway.

---

## Ports

| Environment | Port |
|---|---|
| Local (dotnet run) | 5041 |
| Docker container | 5041 → 8080 internal |

---

## Infrastructure Setup (first-time, run once)

```bash
# 1. Create Docker network (shared across all SEN300 services — skip if already exists)
docker network create netSEN300

# 2. Start Eureka — required for the gateway to discover backend services
docker run -d -p 8761:8761 --name SEN300EurekaRegistry --net netSEN300 steeltoeoss/eureka-server:latest
```

> **IMPORTANT:** Eureka does not tolerate underscores `_` in hostnames.
> All SEN300 container names use camelCase — do not rename them.

---

## Build & Run (Docker)

```bash
docker build --no-cache -t sen300ocelotgatewayapi:1 .
docker run -d -p 5041:8080 --name SEN300APIGatewayOcelot --net netSEN300 sen300ocelotgatewayapi:1
```

> `--no-cache` is recommended — Ocelot sometimes picks up a stale `ocelot.json` if the image is cached.
> If routes seem wrong after a config change, run `dotnet clean` then rebuild.

---

## Dependencies

| Infrastructure | Why |
|---|---|
| Eureka (8761) | Required — gateway uses service discovery to find all backends |
| All 6 services | Optional individually — gateway routes only to services that are registered |

---

## How It Works

```
Client → http://localhost:5041/bookserviceapi/api/books
                    ↓
            Ocelot Gateway
                    ↓
    Eureka lookup: SEN300BookServiceAPI
                    ↓
        BookService /api/books
```

Ocelot reads `ocelot.json` at startup for all route definitions. Each service registers itself with Eureka using its container name as the service ID. The gateway resolves those names at request time.

---

## Route Prefixes

| Gateway Path Prefix | Routes To | Service |
|---|---|---|
| `/bookserviceapi/{everything}` | `SEN300BookServiceAPI` | BookService (8081) |
| `/basketserviceapi/{everything}` | `SEN300BasketServiceAPI` | BasketService (8082) |
| `/orderserviceapi/{everything}` | `SEN300OrderServiceAPI` | OrderService (8083) |
| `/authserviceapi/{everything}` | `SEN300AuthServiceAPI` | AuthService (8084) |
| `/userserviceapi/{everything}` | `SEN300UserServiceAPI` | UserService (8085) |
| `/messageserviceapi/{everything}` | `SEN300MessageServiceAPI` | MessageService (8086) |

All routes support GET, POST, PUT, DELETE.

---

## Gateway Features (configured in ocelot.json)

| Feature | Setting |
|---|---|
| Load balancing | `LeastConnection` on all routes |
| Response caching | 15-second TTL on all routes |
| Resilience | Polly policies enabled |
| Service discovery | Eureka |

---

## Teaching Sequence

1. **First** — verify all six services work on their individual ports (no gateway)
2. **Next** — start Eureka, then start the gateway
3. **Then** — confirm each service registers with Eureka (Eureka UI at http://localhost:8761)
4. **Then** — switch all client calls to go through port 5041 using the gateway prefixes above
5. **Later** — explore load balancing by running multiple instances of one service

---

## Optional: Full Stack Startup Order

```bash
# Infrastructure first
docker run -d ... SEN300BookServiceDBMongo
docker run -d ... SEN300BasketServiceDBRedis
docker run -d ... SEN300OrderServiceDBSqlServer
docker run -d ... SEN300UserServiceDBSqlServer
docker run -d ... SEN300MessageServiceQueueRabbitMQ
docker run -d ... SEN300EurekaRegistry

# Services (any order after infrastructure is healthy)
docker run -d ... SEN300BookServiceAPI
docker run -d ... SEN300BasketServiceAPI
docker run -d ... SEN300AuthServiceAPI
docker run -d ... SEN300UserServiceAPI
docker run -d ... SEN300MessageServiceAPI
docker run -d ... SEN300OrderServiceAPI

# Gateway last (needs all services registered in Eureka)
docker run -d ... SEN300APIGatewayOcelot
```

Or use `docker-compose`:

```bash
docker-compose -f docker-compose2025.yml up -d
```