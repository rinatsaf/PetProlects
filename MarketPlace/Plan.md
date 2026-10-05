# MarketPlace — Онлайн Маркетплейс (аналог Ozon)

## Цель
Построить микросервисный маркетплейс на .NET 9 с использованием DDD, CQRS, событийной шины и разнообразных БД для демонстрации навыков junior .NET разработчика.

---

## Технологический стек

| Компонент | Технология |
|---|---|
| **Runtime** | .NET 9 |
| **API Gateway** | YARP (Reverse Proxy от Microsoft) |
| **Message Broker** | RabbitMQ + MassTransit |
| **Identity** | JWT (собственная реализация) |
| **Базы данных** | PostgreSQL (Identity, Order, Payment, Review, Delivery), MongoDB (Product), Redis (Basket) |
| **ORM** | EF Core (запись) + Dapper (чтение) — гибрид в OrderService |
| **Логирование** | Serilog + Seq |
| **Контейнеризация** | Docker + Docker Compose |

---

## Архитектура

```
Client (браузер / Postman)
       │
       ▼
  ┌───────────┐
  │   YARP    │ ← API Gateway (аутентификация, маршрутизация, rate limiting)
  │  Gateway  │
  └────┬──────┘
       │
       ├────── HTTP ────► Identity Service
       │
       ├────── HTTP ────► Product Service
       │
       ├────── HTTP ────► Basket Service
       │
       ├────── HTTP ────► Order Service
       │
       ├────── HTTP ────► Payment Service
       │
       ├────── HTTP ────► Review Service
       │
       └────── HTTP ────► Notification Service

Асинхронное взаимодействие (RabbitMQ):

Identity ──► UserRegisteredEvent ──► Notification Service
Basket   ──► BasketCheckedOutEvent ─► Order Service
Order    ──► OrderSubmittedEvent ───► Payment, Notification, Delivery
Payment  ──► PaymentCompletedEvent ─► Order, Notification, Delivery
```

---

## Микросервисы

### 1. **Identity Service** — PostgreSQL
- Регистрация / логин / refresh token
- Роли: Customer, Seller, Admin
- Domain: User, Role

### 2. **Product Service** — MongoDB
- Каталог товаров, категории
- Поиск и фильтрация
- Domain: Product, Category, ProductReview

### 3. **Basket Service** — Redis
- Корзина пользователя (TTL — 7 дней)
- AddItem / RemoveItem / Checkout
- При чекауте публикует `BasketCheckedOutEvent`

### 4. **Order Service** — PostgreSQL (EF + Dapper)
- **Write model** → EF Core (заказ, статусы)
- **Read model** → Dapper (история заказов, аггрегация)
- Domain Events: `OrderSubmitted`, `OrderPaid`, `OrderShipped`

### 5. **Payment Service** — PostgreSQL
- Симуляция платежей
- Событие `PaymentCompletedEvent`

### 6. **Notification Service** — MongoDB
- Email-уведомления (симуляция)
- История отправленных уведомлений

### 7. **Review Service** — PostgreSQL
- Отзывы на товары + рейтинг
- Средний рейтинг товара (пересчёт при добавлении)

### 8. **Delivery Service** — PostgreSQL
- Доставка, статусы, отслеживание

---

## Коммуникация

| Тип | Протокол | Где |
|---|---|---|
| Синхронная | HTTP (REST) | Client → Gateway → Services |
| Асинхронная | RabbitMQ (MassTransit) | Между сервисами |
| Domain Events | MediatR + MassTransit | Внутри OrderService |

---

## DDD (Domain-Driven Design)

Каждый сервис делится на 4 слоя:

```
┌──────────────────┐
│       Api        │ — Controllers, Middleware, DTOs
├──────────────────┤
│   Application    │ — CQRS handlers, Validators, Use Cases
├──────────────────┤
│     Domain       │ — Entities, Value Objects, Domain Events, Aggregates
├──────────────────┤
│  Infrastructure  │ — EF Core DbContext, Repositories, Migrations, Dapper
└──────────────────┘
```

**Domain Events** — только в Order Service (самый сложный бизнес-процесс).
В остальных сервисах — просто MassTransit-события на уровне Application.

---

## Этапы реализации

### Этап 1 — Инфраструктура
- Создание `.sln` и структуры папок
- `docker-compose.yml` (Postgres, MongoDB, Redis, RabbitMQ, Seq)
- Настройка Serilog + Seq в BuildingBlocks

### Этап 2 — Building Blocks
- `Shared.Logging` — конфигурация Serilog
- `Shared.Abstractions` — базовые типы (ValueObject, AggregateRoot, IDomainEvent)
- `EventBus` — MassTransit + RabbitMQ + контракты событий

### Этап 3 — Identity Service
- WebAPI + JWT
- EF Core миграции (Users, Roles)
- Register, Login, Refresh

### Этап 4 — Product Service
- MongoDB (Product, Category)
- CRUD + поиск

### Этап 5 — Basket Service
- Redis (корзина)
- Checkout → публикация события

### Этап 6 — Order Service
- EF Core (write) + Dapper (read)
- Domain Events
- Consumer BasketCheckedOutEvent

### Этап 7 — Payment Service
- Симуляция платежей
- Consumer OrderSubmittedEvent

### Этап 8 — Notification Service
- Consumers: UserRegistered, OrderSubmitted, PaymentCompleted

### Этап 9 — Review Service
- CRUD отзывов + рейтинг

### Этап 10 — Delivery Service
- Статусы доставки

### Этап 11 — API Gateway (YARP)
- Конфигурация маршрутов
- JWT middleware
- Rate limiting

### Этап 12 — Сквозная проверка
- Полный flow: регистрация → логин → товар → корзина → заказ → оплата → уведомление

---

## Структура папок

```
MarketPlace/
├── Plan.md
├── src/
│   ├── ApiGateway/
│   │   └── ApiGateway/
│   ├── Services/
│   │   ├── IdentityService/
│   │   │   ├── IdentityService.Api/
│   │   │   ├── IdentityService.Application/
│   │   │   ├── IdentityService.Domain/
│   │   │   └── IdentityService.Infrastructure/
│   │   ├── ProductService/
│   │   │   ├── ProductService.Api/
│   │   │   ├── ProductService.Application/
│   │   │   ├── ProductService.Domain/
│   │   │   └── ProductService.Infrastructure/
│   │   ├── BasketService/
│   │   │   ├── BasketService.Api/
│   │   │   ├── BasketService.Application/
│   │   │   ├── BasketService.Domain/
│   │   │   └── BasketService.Infrastructure/
│   │   ├── OrderService/
│   │   │   ├── OrderService.Api/
│   │   │   ├── OrderService.Application/
│   │   │   ├── OrderService.Domain/
│   │   │   └── OrderService.Infrastructure/
│   │   ├── PaymentService/
│   │   │   ├── PaymentService.Api/
│   │   │   ├── PaymentService.Application/
│   │   │   ├── PaymentService.Domain/
│   │   │   └── PaymentService.Infrastructure/
│   │   ├── NotificationService/
│   │   │   ├── NotificationService.Api/
│   │   │   ├── NotificationService.Application/
│   │   │   ├── NotificationService.Domain/
│   │   │   └── NotificationService.Infrastructure/
│   │   ├── ReviewService/
│   │   │   ├── ReviewService.Api/
│   │   │   ├── ReviewService.Application/
│   │   │   ├── ReviewService.Domain/
│   │   │   └── ReviewService.Infrastructure/
│   │   └── DeliveryService/
│   │       ├── DeliveryService.Api/
│   │       ├── DeliveryService.Application/
│   │       ├── DeliveryService.Domain/
│   │       └── DeliveryService.Infrastructure/
│   └── BuildingBlocks/
│       ├── EventBus/
│       ├── Shared.Logging/
│       └── Shared.Abstractions/
├── tests/
└── docker-compose.yml
```
