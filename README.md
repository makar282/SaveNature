# SaveNature

## Описание проекта
SaveNature — веб-приложение для анализа чеков и предоставления экологических рекомендаций на основе покупок. Пользователи сканируют QR-коды чеков или вводят QR-строки, приложение обрабатывает данные через API `proverkacheka.com`, сохраняет покупки в базе данных и выдаёт эко-рейтинг и рекомендации для товаров. Проект использует ASP.NET Core, Entity Framework Core, SQL Server LocalDB и фронтенд на JavaScript с Bootstrap.

---

## Техническая составляющая

### Стек технологий
- **Backend**: ASP.NET Core 9.0, C#, Entity Framework Core
- **Frontend**: HTML, JavaScript, Bootstrap, CSS
- **База данных**: SQL Server LocalDB
- **Внешний API**: `proverkacheka.com` (для парсинга чеков)
- **Инструменты**: Visual Studio 2022, Git

### Архитектура
- **Контроллеры**: `ReceiptController` обрабатывает запросы для добавления чеков (`POST /api/receipt/add-receipt`), получения списка чеков (`GET /api/receipt/get-receipts`) и расчёта эко-рейтинга (`GET /api/receipt/get-ecorating`).
- **Сервисы**: `RecommendationMatcherService` подбирает рекомендации, сопоставляя названия товаров с ключевыми словами в таблице `Recommendations`.
- **База данных**:
  - Таблицы: `Receipts` (чеки), `Items` (товары), `Recommendations` (рекомендации), `AspNetUsers` (пользователи).
  - Связи: `Receipt` → `Items` (один-ко-многим), `Item` → `Recommendation` (многие-к-одному, `RecommendationId` nullable).
- **Фронтенд**: `Profile.cshtml` отображает таблицу покупок (`loadPurchases`), графики (`initChart`) и форму для добавления чеков.

---

## Работа с чеками

### Откуда
- Чеки получаются через внешний API `proverkacheka.com` (эндпоинт `https://proverkacheka.com/api/v1/check/get`).
- Пользователь сканирует QR-код чека (например, с помощью телефона) или вводит QR-строку (например, `t=20240101T1200&fn=1234567890&fp=0987654321`).
- Данные отправляются на сервер через `POST /api/receipt/add-receipt`.

### Куда 
- Данные чека парсятся (JSON-ответ от API) и сохраняются в базе данных:
  - `Receipt`: сумма (`TotalAmount`), дата покупки (`PurchaseDate`), пользователь (`UserName`).
  - `Items`: товары с названием (`ProductName`), ценой (`Price`), связанной рекомендацией (`RecommendationId`).
- Рекомендации подбираются через `RecommendationMatcherService` по ключевым словам (например, «пластик» → рекомендация с `EcoScore` и текстом).

### Пример данных
- Ввод: QR-код или строка `t=20240101T1200&fn=1234567890&fp=0987654321`.
- Вывод в `/Profile`:
  - Товар: «Сковода с антипригарным покрытие», Эко-рейтинг: -10, Рекомендация: «Используйте посуду из нейтральных материалов, например нерж. сталь».

---

## Как взаимодействовать с проектом

### Установка и запуск
1. **Требования**:
   - Visual Studio 2022 с .NET 6.0 SDK.
   - SQL Server LocalDB (установлен с Visual Studio или отдельно).
   - NuGet-пакеты: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Newtonsoft.Json`.

2. **Клонирование и настройка**:
   ```bash
   git clone https://github.com/makar282/SaveNature.git
   cd SaveNature
   ```
   - Открой `SaveNature.sln` в Visual Studio.
   - Восстанови NuGet-пакеты: **Solution Explorer → Restore NuGet Packages**.

3. **Настройка базы данных**:
   - Проверь строку подключения в `appsettings.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SaveNature;Trusted_Connection=True"
     }
     ```
   - Примените миграции:
     ```bash
     dotnet ef database update
     ```

4. **Запуск**:
   - Нажми `F5` в Visual Studio или выполни:
     ```bash
     dotnet run
     ```
   - Приложение доступно по `http://localhost:{порт}` (порт см. `launchSettings.json`).

### Использование
1. **Регистрация/вход**:
   - Зарегистрируйся или войди через `/Register` или `/Login`.
   - Аутентификация требуется для всех операций с чеками.

2. **Добавление чека**:
   - Перейди на `/Purchases`.
   - Сканируй QR-код чека (с помощью телефона) или вставь QR-строку (например, `t=20240101T1200&fn=1234567890&fp=0987654321`) в форму.
   - Нажми «Добавить чек» (отправляет `POST /api/receipt/add-receipt`).
   - Данные чека сохраняются, товары отображаются в таблице покупок.

3. **Просмотр покупок**:
   - На странице `/Profile` таблица (`loadPurchases`) показывает чеки, товары, эко-рейтинг и рекомендации.
   - Эко-рейтинг рассчитывается через `GET /api/receipt/get-ecorating` (нормализация от -100 до 100).

4. **Работа с рекомендациями**:
   - Рекомендации подбираются автоматически для каждого товара (например, «Пластик» → «Используйте многоразовые материалы или многоразовый пластик, который затем можно утилизировать»).
   - Эко-рейтинг отображается с иконками (зелёная для >50, красная для <0).

---

## Взаимодействие с API

### Эндпоинты
- **`POST /api/receipt/add-receipt`**:
  - Тело запроса:
    ```json
    {
      "QrRaw": "t=20240101T1200&fn=1234567890&fp=0987654321",
      "QrUrl": null
    }
    ```
  - Ответ: `200 OK` с сообщением или `400 BadRequest` при ошибке.

- **`GET /api/receipt/get-receipts`**:
  - Возвращает список чеков пользователя с товарами и рекомендациями:
    ```json
    [
      {
        "Id": 1,
        "PurchaseDate": "2024-01-01T12:00:00Z",
        "Items": [
          {
            "ProductName": "Бутылка",
            "EcoScore": -50,
            "RecommendationText": "Не забудьте утилизировь в контейнер для пластикового мусора"
          }
        ]
      }
    ]
    ```

- **`GET /api/receipt/get-ecorating`**:
  - Возвращает средний эко-рейтинг пользователя (0–100):
    ```json
    { "EcoRating": 75 }
    ```

### Пример вызова API
```bash
curl -X POST http://localhost:5000/api/receipt/add-receipt -H "Content-Type: application/json" -d '{"QrRaw":"t=20240101T1200&fn=1234567890&fp=0987654321"}'
```

---

## Разработка и отладка

### Структура проекта
- **Controllers/ReceiptController.cs**: Логика API для чеков и рекомендаций.
- **Services/RecommendationMatcherService.cs**: Подбор рекомендаций по ключевым словам.
- **Models**: `Receipt`, `Item`, `Recommendation` для работы с базой.
- **Data/ApplicationDbContext.cs**: Контекст EF Core для базы данных.
- **Views/Profile.cshtml**: Фронтенд для профиля и покупок.
- **wwwroot/js**: JavaScript для форм, таблиц, графиков (`loadPurchases`, `initChart`).

### Отладка
- Логи: Включены через `ILogger` (см. `AddReceipt`, `GetReceipts`).
- Проверка базы: Используй **SQL Server Object Explorer** в Visual Studio или SSMS.
- Ошибки порта (например, 5000 занят):
  ```bash
  netstat -aon | findstr :5000
  taskkill /F /PID <PID>
  ```
  Или измени порт в `Properties/launchSettings.json`.

### Добавление данных
- Для тестов добавь записи в `Recommendations`:
  ```sql
  INSERT INTO Recommendations (Purchase, EcoScoreRecomendation, RecommendationText)
  VALUES ('пластик', -50, 'Используйте многоразовые материалы');
  ```

---

## Требования к окружению
- **ОС**: Windows (для LocalDB, Visual Studio).
- **Зависимости**:
  - .NET 6.0 SDK
  - SQL Server LocalDB
  - NuGet-пакеты (указаны в `SaveNature.csproj`)
- **API-токен**: Замени `ApiToken` в `ReceiptController.cs` на действующий токен от `proverkacheka.com`.
