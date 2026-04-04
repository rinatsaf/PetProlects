# database.md — База данных проекта Cinema Management System

## 1. Назначение базы данных

База данных предназначена для хранения основной бизнес-информации системы управления кинотеатром:

- пользователи
- фильмы и жанры
- залы и места
- сеансы
- заказы и билеты
- платежи
- пользовательские взаимодействия для рекомендаций

Временные блокировки мест **не хранятся как основная сущность в БД**.  
Для них используется **Redis** с TTL, чтобы автоматически освобождать место, если пользователь не завершил оплату.

---

## 2. Выбранная СУБД

**PostgreSQL**

Почему подходит:

- удобна для реляционной модели
- хорошо работает с ASP.NET Core и EF Core
- поддерживает индексы, ограничения, транзакции
- подходит для учебного и реального проекта

---

## 3. Общая модель данных

В проекте предлагается использовать **11 основных сущностей**:

1. `User`
2. `Movie`
3. `Genre`
4. `MovieGenre`
5. `Hall`
6. `Seat`
7. `Session`
8. `Order`
9. `Ticket`
10. `Payment`
11. `UserInteraction`
12. `UserPreference`

Если хочется оставить ровно 10–11 сущностей, можно убрать `UserPreference` и вычислять веса рекомендаций на лету по `UserInteraction`.

---

## 4. Список сущностей и полей

---

### 4.1 User

Хранит информацию о пользователях системы.

**Поля:**

- `Id` — bigint / uuid, PK
- `Email` — string, unique
- `PasswordHash` — string
- `FirstName` — string
- `LastName` — string
- `Role` — enum (`User`, `Admin`)
- `CreatedAt` — datetime
- `IsActive` — bool

**Назначение:**
- авторизация
- история заказов
- персональные рекомендации
- отправка билетов на email

---

### 4.2 Movie

Хранит информацию о фильмах.

**Поля:**

- `Id` — PK
- `Title` — string
- `Description` — text
- `DurationMinutes` — int
- `AgeRating` — string
- `Country` — string
- `ReleaseDate` — date
- `PosterUrl` — string
- `PopularityScore` — decimal
- `IsActive` — bool
- `CreatedAt` — datetime

**Назначение:**
- отображение каталога
- основа для рекомендаций
- связь с сеансами

---

### 4.3 Genre

Хранит жанры фильмов.

**Поля:**

- `Id` — PK
- `Name` — string, unique

**Примеры:**
- Action
- Comedy
- Drama
- Thriller

---

### 4.4 MovieGenre

Промежуточная таблица many-to-many между фильмами и жанрами.

**Поля:**

- `MovieId` — FK -> Movie
- `GenreId` — FK -> Genre

**Первичный ключ:**
- составной (`MovieId`, `GenreId`)

---

### 4.5 Hall

Хранит залы кинотеатра.

**Поля:**

- `Id` — PK
- `Name` — string
- `RowsCount` — int
- `SeatsPerRow` — int
- `Type` — string  
  Например:
  - Standard
  - VIP
  - IMAX
- `IsActive` — bool

**Назначение:**
- описание физического зала
- связь с местами и сеансами

---

### 4.6 Seat

Хранит места внутри зала.

**Поля:**

- `Id` — PK
- `HallId` — FK -> Hall
- `RowNumber` — int
- `SeatNumber` — int
- `SeatType` — string  
  Например:
  - Standard
  - VIP
  - Couple
- `BasePrice` — decimal

**Ограничение уникальности:**
- (`HallId`, `RowNumber`, `SeatNumber`)

**Назначение:**
- отображение схемы зала
- продажа билетов по месту

---

### 4.7 Session

Хранит сеансы показа фильма.

**Поля:**

- `Id` — PK
- `MovieId` — FK -> Movie
- `HallId` — FK -> Hall
- `StartTime` — datetime
- `EndTime` — datetime
- `BasePrice` — decimal
- `Status` — enum (`Planned`, `Active`, `Finished`, `Cancelled`)
- `CreatedAt` — datetime

**Назначение:**
- расписание показов
- связь фильма, зала и временного интервала

---

### 4.8 Order

Хранит заказ пользователя.

**Поля:**

- `Id` — PK
- `UserId` — FK -> User
- `Status` — enum (`Pending`, `AwaitingPayment`, `Paid`, `Cancelled`, `Expired`)
- `TotalAmount` — decimal
- `CreatedAt` — datetime
- `ExpiresAt` — datetime
- `PaidAt` — datetime, nullable

**Назначение:**
- объединяет несколько билетов в один заказ
- связан с оплатой
- хранит жизненный цикл покупки

---

### 4.9 Ticket

Хранит купленные билеты.

**Поля:**

- `Id` — PK
- `OrderId` — FK -> Order
- `SessionId` — FK -> Session
- `SeatId` — FK -> Seat
- `Price` — decimal
- `TicketCode` — string, unique
- `QrCodeUrl` — string, nullable
- `Status` — enum (`Active`, `Used`, `Cancelled`, `Refunded`)
- `CreatedAt` — datetime

**Ограничение уникальности:**
- (`SessionId`, `SeatId`)

Это ключевое ограничение, которое не позволяет продать одно и то же место дважды на один и тот же сеанс.

---

### 4.10 Payment

Хранит информацию о платеже через YooKassa.

**Поля:**

- `Id` — PK
- `OrderId` — FK -> Order
- `Provider` — string (`YooKassa`)
- `ExternalPaymentId` — string
- `Amount` — decimal
- `Currency` — string
- `Status` — enum (`Pending`, `Succeeded`, `Cancelled`, `Failed`)
- `PaymentMethod` — string (`SBP`)
- `ConfirmationUrl` — string, nullable
- `CreatedAt` — datetime
- `ConfirmedAt` — datetime, nullable
- `RawPayload` — text / jsonb, nullable

**Назначение:**
- хранение связи с внешним платёжным провайдером
- обработка webhook
- аудит статусов оплаты

---

### 4.11 UserInteraction

Хранит действия пользователя, нужные для рекомендаций.

**Поля:**

- `Id` — PK
- `UserId` — FK -> User
- `MovieId` — FK -> Movie
- `ActionType` — enum (`View`, `Click`, `OpenDetails`, `BuyTicket`)
- `WeightDelta` — decimal
- `CreatedAt` — datetime

**Назначение:**
- фиксирование интереса пользователя
- база для пересчёта весов рекомендаций

---

### 4.12 UserPreference

Хранит агрегированные предпочтения пользователя.

**Поля:**

- `Id` — PK
- `UserId` — FK -> User
- `FeatureKey` — string  
  Например:
  - Genre:Action
  - Genre:Comedy
  - Time:Evening
  - HallType:VIP
- `Weight` — decimal
- `UpdatedAt` — datetime

**Назначение:**
- быстрое ранжирование фильмов
- персонализация выдачи

**Ограничение уникальности:**
- (`UserId`, `FeatureKey`)

---

## 5. Связи между сущностями

### Основные связи

- `User 1 -> N Order`
- `User 1 -> N UserInteraction`
- `User 1 -> N UserPreference`
- `Movie N -> N Genre` через `MovieGenre`
- `Movie 1 -> N Session`
- `Hall 1 -> N Seat`
- `Hall 1 -> N Session`
- `Order 1 -> N Ticket`
- `Order 1 -> N Payment`  
  На практике можно ограничить до 1 -> 1 или 1 -> N, если поддерживать повторную попытку оплаты.
- `Session 1 -> N Ticket`
- `Seat 1 -> N Ticket`
- `Movie 1 -> N UserInteraction`

---

## 6. ER-логика в текстовом виде

```text
User
 ├── Order
 │    ├── Ticket
 │    │    ├── Session
 │    │    │    ├── Movie
 │    │    │    └── Hall
 │    │    └── Seat
 │    └── Payment
 │
 ├── UserInteraction ── Movie
 └── UserPreference

Movie ── MovieGenre ── Genre
Hall ── Seat
Hall ── Session
```

---

## 7. Что хранится в Redis, а не в БД

Для временной блокировки места лучше использовать Redis.

### Ключ:
```text
seat-hold:{sessionId}:{seatId}
```

### Значение:
```json
{
  "userId": 15,
  "orderId": 103,
  "expiresAt": "2026-03-08T19:35:00Z"
}
```

### TTL:
```text
5 минут
```

### Почему так лучше:
- автоматическое освобождение места
- не нужно чистить временные записи в БД
- быстрее работает при конкурентном доступе
- хорошо сочетается с SignalR

---

## 8. Индексы и ограничения

### Обязательные уникальные ограничения

1. `User.Email` — unique
2. `Genre.Name` — unique
3. `Seat(HallId, RowNumber, SeatNumber)` — unique
4. `Ticket(SessionId, SeatId)` — unique
5. `Ticket.TicketCode` — unique
6. `UserPreference(UserId, FeatureKey)` — unique
7. `MovieGenre(MovieId, GenreId)` — PK/unique

### Полезные индексы

1. `Session(MovieId, StartTime)`
2. `Session(HallId, StartTime)`
3. `Order(UserId, CreatedAt)`
4. `Payment(OrderId, Status)`
5. `UserInteraction(UserId, CreatedAt)`
6. `Ticket(OrderId)`
7. `Ticket(SessionId)`

---

## 9. Enum-статусы

### Role
- `User`
- `Admin`

### SessionStatus
- `Planned`
- `Active`
- `Finished`
- `Cancelled`

### OrderStatus
- `Pending`
- `AwaitingPayment`
- `Paid`
- `Cancelled`
- `Expired`

### PaymentStatus
- `Pending`
- `Succeeded`
- `Cancelled`
- `Failed`

### TicketStatus
- `Active`
- `Used`
- `Cancelled`
- `Refunded`

### InteractionActionType
- `View`
- `Click`
- `OpenDetails`
- `BuyTicket`

---

## 10. Нормальный поток данных в системе

### Сценарий покупки

1. Пользователь выбирает сеанс.
2. Система загружает проданные билеты из БД.
3. Временные блокировки мест берутся из Redis.
4. Пользователь выбирает места.
5. Создаётся `Order`.
6. Создаётся запись `Payment`.
7. Пользователь переходит на оплату через YooKassa.
8. После webhook:
   - `Payment.Status = Succeeded`
   - `Order.Status = Paid`
   - создаются `Ticket`
   - Redis hold удаляется
9. На email отправляется билет.

---

## 11. Почему такая модель базы хороша для семестровки

Преимущества:

- покрывает все основные процессы кинотеатра
- включает оплату и email-отправку билетов
- поддерживает realtime-бронирование мест
- включает модуль рекомендаций
- выглядит как серьёзная production-подобная модель
- при этом остаётся реализуемой в рамках учебного проекта

---

## 12. Упрощённый вариант, если нужно сократить объём

Если проект нужно упростить, можно оставить такие сущности:

1. `User`
2. `Movie`
3. `Genre`
4. `Hall`
5. `Seat`
6. `Session`
7. `Order`
8. `Ticket`
9. `Payment`
10. `UserInteraction`

А `MovieGenre` реализовать как join-таблицу EF Core без отдельной акцентной проработки, а `UserPreference` считать на лету.

---

## 13. Итог

Рекомендуемая база данных для проекта кинотеатра должна состоять из реляционной модели на PostgreSQL, где:

- постоянные данные хранятся в БД
- временные блокировки мест хранятся в Redis
- продажа места защищается уникальным ограничением на билет
- рекомендации строятся на основе пользовательских взаимодействий

Такая схема хорошо подходит для ASP.NET Core, EF Core, SignalR, Redis и YooKassa.
