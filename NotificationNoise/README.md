# NotificationNoise

## Что делает проект

`NotificationNoise` — это небольшая event-driven система, которая забирает уведомления, классифицирует их как полезные или шум, строит агрегированные инсайты и формирует digest-сводки.

Система разделена на четыре микросервиса:

- `Ingestion` получает уведомления из Gmail и публикует событие `notification.received`.
- `Classifier` потребляет `notification.received`, классифицирует каждое сообщение и публикует `notification.classified`.
- `Insights` потребляет `notification.classified` и поддерживает статистику и рекомендации.
- `DigestSender` читает данные из `Insights` и создаёт digest-записи.

Общие контракты и базовые RabbitMQ-примитивы вынесены в отдельные shared-проекты.

## Правила архитектуры

Каждый микросервис теперь следует одной и той же модели слоёв:

- `*.Api`: composition root, HTTP-контроллеры, background consumers, транспортные адаптеры.
- `*.Application`: use case, интерфейсы (порты), DTO, используемые прикладной логикой.
- `*.Domain`: основные сущности и доменные переходы состояния.
- `*.Infrastructure`: база данных, HTTP-клиенты, реализации репозиториев, регистрация зависимостей.

Направление зависимостей:

- `Api -> Application`
- `Infrastructure -> Application` и в некоторых местах `Infrastructure -> Domain`
- `Application -> Domain`
- `Domain ->` без зависимостей на другие проекты

## Поток событий

1. `Ingestion` получает письма из Gmail и сохраняет новые уведомления.
2. `Ingestion` публикует `NotificationReceived`.
3. `Classifier` вычисляет классификацию и сохраняет `Classification`.
4. `Classifier` публикует `NotificationClassified`.
5. `Insights` обновляет статистику по отправителям, дневную статистику и рекомендации.
6. `DigestSender` запрашивает данные из `Insights`, сохраняет digest и отправляет его через текущий адаптер доставки.

## Общие проекты

### `NotificationNoise.Contracts`

- `Notifications/NotificationReceived.cs`: интеграционное событие, публикуемое после ingestion.
- `Notifications/NotificationClassified.cs`: интеграционное событие, публикуемое после classification.

### `NotificationNoise.BuildingBlocks`

- `Messaging/IEventPublisher.cs`: прикладная абстракция для публикации событий.
- `Messaging/IEventConsumer.cs`: прикладная абстракция для потребления событий.
- `Messaging/RabbitMq/RabbitMqPublisher.cs`: RabbitMQ-реализация для публикации событий.
- `Messaging/RabbitMq/RabbitMqConsumer.cs`: RabbitMQ-реализация для потребления topic-очередей.

## Микросервис Ingestion

### `NotificationNoise.Ingestion.Api`

- `Program.cs`: composition root сервиса ingestion.
- `Controllers/HealthController.cs`: endpoint проверки доступности.
- `Controllers/GmailAuthController.cs`: запускает Gmail OAuth и завершает callback.
- `Controllers/IngestionController.cs`: HTTP-endpoint для запуска backfill.
- `Controllers/DebugController.cs`: debug-endpoint, публикующий синтетический `notification.received`.
- `Services/NotificationReceivedPublisher.cs`: API-адаптер, который преобразует доменный `Notification` в `NotificationReceived`.
- `Services/NotificationDebugService.cs`: создаёт и публикует синтетическое уведомление для ручной проверки пайплайна.
- `FakeIngestion/FakeIngestionOptions.cs`: объект конфигурации для генерации тестовых событий.
- `FakeIngestion/FakeIngestionHostedService.cs`: опциональный background-сервис, генерирующий fake-события `notification.received`.

### `NotificationNoise.Ingestion.Application`

- `GmailOAuthService.cs`: use case для построения OAuth URL и обмена auth code на токены.
- `GmailBackfillService.cs`: основной use case ingestion; читает Gmail, сохраняет новые уведомления, публикует интеграционные события.
- `IGmailMessageClient.cs`: порт для чтения сообщений из Gmail.
- `IGmailTokenStore.cs`: порт для чтения и обновления сохранённых Gmail-токенов.
- `IGoogleOAuthClient.cs`: порт для построения authorization URL и обмена/обновления OAuth-токенов.
- `INotificationRepository.cs`: порт для сохранения уведомлений.
- `INotificationReceivedPublisher.cs`: порт для публикации `NotificationReceived` после сохранения.
- `ITokenProtector.cs`: порт для шифрования и дешифрования сохранённых токенов.
- `GmailMessageMetadata.cs`: application DTO с данными Gmail-сообщения, которые нужны для backfill.
- `GmailTokenData.cs`: application DTO для расшифрованных токенов.
- `OAuthSettings.cs`: типизированные OAuth-настройки, используемые прикладными сервисами.
- `OAuthTokenResponse.cs`: DTO для ответа обмена/обновления токена.

### `NotificationNoise.Ingestion.Domain`

- `Notifications/Notification.cs`: сохраняемая aggregate-сущность для сырых входящих уведомлений.
- `GmailToken.cs`: сохраняемая сущность Gmail-токена.

### `NotificationNoise.Ingestion.Infrastructure`

- `DependencyInjection/ServiceCollectionExtensions.cs`: регистрирует DbContext, репозитории, HTTP-клиенты и сервисы защиты токенов.
- `Persistence/IngestionDbContext.cs`: EF Core context для уведомлений и Gmail-токенов.
- `Persistence/NotificationRepository.cs`: EF-реализация `INotificationRepository`.
- `GmailApiClient.cs`: HTTP-адаптер, реализующий `IGmailMessageClient`.
- `GmailApiModels.cs`: raw DTO для десериализации ответов Gmail API.
- `GoogleOAuthClient.cs`: HTTP-адаптер, реализующий `IGoogleOAuthClient`.
- `GmailTokenStore.cs`: EF-реализация `IGmailTokenStore`.
- `TokenProtector.cs`: адаптер ASP.NET Data Protection, реализующий `ITokenProtector`.

## Микросервис Classifier

### `NotificationNoise.Classifier.Api`

- `Program.cs`: composition root сервиса classifier.
- `Controllers/HealthController.cs`: endpoint проверки доступности.
- `Controllers/DebugController.cs`: HTTP-endpoint, возвращающий текущее количество классификаций.
- `Messaging/NotificationReceivedHandler.cs`: транспортный адаптер, который десериализует RabbitMQ-сообщение и вызывает прикладную логику.
- `Messaging/RabbitConsumerHostedService.cs`: hosted service-обёртка, запускающая consumer.
- `Services/NotificationClassifiedPublisher.cs`: API-адаптер, публикующий `NotificationClassified`.

### `NotificationNoise.Classifier.Application`

- `NotificationClassificationService.cs`: основной use case classifier; проверяет идемпотентность, применяет правила, сохраняет результат, публикует событие.
- `IClassificationRepository.cs`: порт для сохранения классификации и получения счётчика записей.
- `IClassificationResultPublisher.cs`: порт для публикации `NotificationClassified`.
- `Rules/IRulesEngine.cs`: контракт для вычисления правил классификации.
- `Rules/SimpleRulesEngine.cs`: текущая эвристическая реализация правил.
- `Rules/RuleResult.cs`: DTO результата, который возвращает rules engine.

### `NotificationNoise.Classifier.Domain`

- `Classification.cs`: сохраняемая сущность классификации.

### `NotificationNoise.Classifier.Infrastructure`

- `DependencyInjection/ServiceCollectionExtensions.cs`: регистрирует DbContext и реализации репозиториев.
- `Persistence/ClassifierDbContext.cs`: EF Core context для классификаций.
- `Persistence/ClassificationRepository.cs`: EF-реализация `IClassificationRepository`.

## Микросервис Insights

### `NotificationNoise.Insights.Api`

- `Program.cs`: composition root сервиса insights.
- `Controllers/HealthController.cs`: endpoint проверки доступности.
- `Controllers/StatsController.cs`: HTTP-endpoints для топ-отправителей и трендов.
- `Controllers/RecommendationsController.cs`: HTTP-endpoints для списка рекомендаций и их dismiss.
- `Messaging/NotificationClassifiedHandler.cs`: транспортный адаптер, который десериализует RabbitMQ-сообщение и вызывает прикладную логику.
- `Messaging/RabbitConsumerHostedService.cs`: hosted service-обёртка, запускающая consumer.

### `NotificationNoise.Insights.Application`

- `InsightsQueryService.cs`: read use case для топ-отправителей, трендов и рекомендаций.
- `NotificationInsightsService.cs`: write use case, который обновляет sender stats, daily stats и рекомендации на основе `NotificationClassified`.
- `IInsightsRepository.cs`: порт и для чтения read model, и для обновления агрегатов внутри insights-сервиса.
- `TopSenderItem.cs`: read-model DTO для статистики по отправителям.
- `TrendItem.cs`: read-model DTO для дневных трендов.
- `RecommendationListItem.cs`: read-model DTO для списка рекомендаций.

### `NotificationNoise.Insights.Domain`

- `SenderStats.cs`: aggregate со счётчиками по отправителю.
- `DailyStats.cs`: aggregate с дневными счётчиками.
- `Recommendation.cs`: сущность рекомендации и её переходы состояния.

### `NotificationNoise.Insights.Infrastructure`

- `DependencyInjection/ServiceCollectionExtensions.cs`: регистрирует DbContext и реализацию репозитория.
- `Persistence/InsightsDbContext.cs`: EF Core context для статистики по отправителям, дневной статистики и рекомендаций.
- `Persistence/InsightsRepository.cs`: EF-реализация `IInsightsRepository`.

## Микросервис DigestSender

### `NotificationNoise.DigestSender.Api`

- `Program.cs`: composition root сервиса digest и применение миграций на старте.
- `Controllers/HealthController.cs`: endpoint проверки доступности.
- `Controllers/DigestController.cs`: HTTP-endpoint для ручного запуска формирования digest.

### `NotificationNoise.DigestSender.Application`

- `DigestService.cs`: основной use case digest; получает данные из insights, сохраняет digest, отправляет digest и помечает его как отправленный.
- `IInsightsClient.cs`: порт для запроса данных из сервиса insights.
- `IDigestRepository.cs`: порт для сохранения digest.
- `IDigestDelivery.cs`: порт канала доставки.
- `DigestSnapshot.cs`: DTO, возвращаемый после агрегации insights; включает `TopSenderItem`, `TrendItem` и `RecommendationItem`.

### `NotificationNoise.DigestSender.Domain`

- `Digest.cs`: сохраняемая сущность digest с переходом состояния через `MarkSent`.

### `NotificationNoise.DigestSender.Infrastructure`

- `DependencyInjection/ServiceCollectionExtensions.cs`: регистрирует DbContext, репозиторий, адаптер доставки и HTTP-клиент.
- `Persistence/DigestDbContext.cs`: EF Core context для digest-записей.
- `Persistence/DigestRepository.cs`: EF-реализация `IDigestRepository`.
- `InsightsHttpClient.cs`: HTTP-адаптер, реализующий `IInsightsClient`.
- `LogDigestDelivery.cs`: текущий адаптер доставки, который просто пишет payload digest в лог вместо email/push.

## Порядок запуска

Сначала нужно поднять зависимости:

1. PostgreSQL
2. RabbitMQ

Затем запускать сервисы в таком порядке:

1. `NotificationNoise.Ingestion.Api`
2. `NotificationNoise.Classifier.Api`
3. `NotificationNoise.Insights.Api`
4. `NotificationNoise.DigestSender.Api`

При использовании Docker Compose этот порядок уже заложен в `docker/docker-compose.yml`.

## Текущие границы и практические замечания

- `Program.cs` теперь намеренно тонкие и используются только как composition root.
- Контроллеры — тонкие адаптеры; в них не должно быть бизнес-правил.
- RabbitMQ handlers — тонкие адаптеры; они десериализуют сообщение и передают его в `Application`.
- Репозитории живут в `Infrastructure`; `Application` зависит только от интерфейсов.
- Доменные объекты — простые persistence-friendly aggregate-сущности с минимальным поведением.

## Что логично улучшить дальше

- Добавить фильтрацию по пользователю в query `Insights` (`top-senders`, `trends`, `recommendations`), чтобы read side был корректно multi-tenant.
- Вынести имена топиков и очередей RabbitMQ в константы или options, чтобы убрать повторяющиеся строковые литералы.
- Добавить outbox pattern, чтобы запись в БД и публикация событий были надёжными и атомарными.
- Добавить тесты на application services (`GmailBackfillService`, `NotificationClassificationService`, `NotificationInsightsService`, `DigestService`).
