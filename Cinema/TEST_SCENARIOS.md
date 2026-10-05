# Сценарии тестирования Cinema API (актуально на 2026-05-09)

Документ для ручной проверки API в Postman перед демонстрацией/защитой.

## Подготовка

1. API запущен: `http://localhost:5276`.
2. Запущены PostgreSQL, Redis, Hangfire.
3. Импортированы:
   - `postman/Cinema-Role-UseCases.postman_collection.json`
   - `postman/Cinema-Local.postman_environment.json`
4. Тестовые пользователи (см. `postman/README.md`):
   - `postman.customer@cinema.local / Postman1!`
   - `postman.cashier@cinema.local / Postman1!`
   - `postman.admin@cinema.local / Postman1!`

## Переменные окружения Postman

`baseUrl`, `customerEmail`, `customerPassword`, `cashierEmail`, `cashierPassword`, `adminEmail`, `adminPassword`, `hallId`, `movieId`, `sessionId`, `orderId`, `paymentId`, `externalPaymentId`, `ticketId`, `ticketCode`.

Проверка структуры ответов:
- `SessionDto` содержит `movieTitle`, `hallName`, `hallAddress`.
- `OrderDto` содержит `ticketsCount`, `ticketIds`, `paymentIds`.
- `MovieDto` содержит `genreIds`, `genres`.

## P0 Критичные сценарии

### P0.1 Регистрация, логин, профиль

1. `POST /api/auth/register`
2. `POST /api/auth/login`
3. `GET /api/user/profile`

Ожидание:
- Register/Login: `204`.
- Profile: `200`.
- В profile нет `PasswordHash`.

### P0.2 Создание зала (Staff)

1. Логин под Cashier или Admin.
2. `POST /api/halls`.
3. `GET /api/halls/{hallId}`.

Пример body:
```json
{
  "name": "Hall A",
  "address": "Main st, 1",
  "rowsCount": 10,
  "seatsPerRow": 12,
  "type": "Standard",
  "isActive": true
}
```

Ожидание:
- `201 Created`.
- У зала сгенерированы места.

### P0.3 Создание фильма с жанрами (Staff)

1. Логин под Staff.
2. `POST /api/movie`.

Ключевое: `genreIds` не пустой, без дублей, только существующие id.

Дополнительно:
- Проверить, что в ответе есть `genreIds` и `genres`.

### P0.4 Создание сеанса и автогенерация билетов (Staff)

1. `POST /api/sessions`.

Пример body:
```json
{
  "movieId": {{movieId}},
  "hallId": {{hallId}},
  "startTime": "2026-05-10T18:00:00+03:00",
  "endTime": "2026-05-10T20:00:00+03:00",
  "basePrice": 450,
  "status": 0
}
```

Ожидание:
- `201 Created`.
- Билеты на места созданы со статусом `Available`.
- В `SessionDto` заполнены `movieTitle`, `hallName`, `hallAddress`.

### P0.5 Создание заказа (Customer)

1. `GET /api/tickets/session/{sessionId}/available` и выбрать `seatId`.
2. `POST /api/orders`.

Пример body:
```json
{
  "sessionId": {{sessionId}},
  "seatIds": [1, 2]
}
```

Ожидание:
- `201 Created`.
- Заказ в `Pending`.
- Билеты выбранных мест становятся `Reserved`.
- В `OrderDto` корректны `ticketsCount`, `ticketIds`.

Примечание:
- Для Customer `userId` можно не передавать: используется текущий авторизованный пользователь.
- Для Staff/Admin `userId` можно передать, чтобы создать заказ за другого пользователя.

### P0.6 Успешная оплата

1. `POST /api/payments`
2. `GET /api/payments/{paymentId}`
3. `GET /api/orders/{orderId}`
4. `GET /api/tickets/order/{orderId}`

Пример body:
```json
{
  "orderId": {{orderId}},
  "returnUrl": "https://example.com/return"
}
```

Ожидание:
- Платеж: `Succeeded`.
- Заказ: `Paid`, заполнен `PaidAt`.
- Билеты: `Reserved -> Active`.
- В `OrderDto` есть `paymentIds` (с id созданного платежа).

### P0.7 Check-in и запрет повторного прохода (Staff)

1. `POST /api/tickets/{ticketId}/check-in`
2. Повторить тот же запрос

Ожидание:
- Первый: `200`, билет `Used`.
- Повтор: `409 Conflict`.

### P0.8 Неуспешная/отмененная оплата

1. Создать новый заказ и платеж.
2. Обновить статус staff-эндпоинтом: `POST /api/payments/{id}/status`.

Пример body:
```json
{
  "status": 3,
  "rawPayload": "manual test"
}
```

Ожидание:
- Платеж `Failed` или `Cancelled`.
- Заказ `Cancelled`.
- Билеты возвращаются в `Available`.

## P1 Важные сценарии

### P1.1 YooKassa webhook

1. `POST /api/payments/yookassa/webhook`.
2. Проверить изменение статусов заказа/платежа/билетов.

### P1.2 Жизненный цикл сеансов (Hangfire)

Ожидание переходов:
- `Planned -> Active -> Finished`.

### P1.3 Очистка просроченных заказов

Ожидание:
- Просроченный заказ авто-отменяется.
- Билеты возвращаются в `Available`.

### P1.4 Ролевой доступ

Проверить, что:
- Customer получает `403` на staff-эндпоинтах.
- Staff/Admin работают в пределах своих прав.

### P1.5 Customer не может создать заказ за другого пользователя

`POST /api/orders` с чужим `userId` -> `403 Forbidden`.

### P1.6 Профиль без userId в URL

`GET /api/user/profile` возвращает профиль текущего пользователя.

## P2 Негативные проверки

1. Невалидный фильм (`genreIds` пустой/дубли/несуществующие) -> `400`/`404`.
2. `PUT /api/movie/{id}` с несуществующим `genreIds` -> `404`.
3. Невалидный сеанс (`EndTime <= StartTime`) -> `400`.
4. Пересечение сеансов в одном зале -> `409`.
5. Двойная бронь одного места разными пользователями -> один успех, второй `409`.
6. Попытка check-in по коду с пустым body или несуществующим кодом.

Актуальный body для check-in по коду:
```json
{
  "code": "H1-S10-SEAT15"
}
```

## P3 Операционные проверки

1. `/hangfire` доступен только для admin.
2. Зарегистрированы recurring jobs:
   - `expired-orders-cleanup`
   - `session-lifecycle`
   - `preferences-weekly-decay`
3. Рекомендации: купленные фильмы не попадают в выдачу для пользователя.

## Рекомендуемый порядок на защите

1. P0.1 -> P0.6
2. P0.7 -> P0.8
3. P1.1 -> P1.4
4. 2-3 кейса из P2
5. P3.1/P3.2
