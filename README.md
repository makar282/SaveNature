# 🌱 SaveNature

Веб-приложение на **ASP.NET Core** для учёта покупок, расчёта экологического рейтинга и предоставления рекомендаций по более экологичному выбору товаров.

## 🚀 Возможности

- 👤 Регистрация и вход пользователей  
- 🧾 Работа с чеками и товарами (сохранение, привязка к пользователю, подсчёт суммы)  
- 🌍 Экологический рейтинг (**EcoRating**) — анализ покупок пользователя и расчёт «зелёного» индекса  
- 💡 Рекомендации по более экологичным товарам  
- 🔐 Простая аутентификация (через собственный `PasswordHasher`)  

## 🛠️ Стек технологий

- **Backend:** .NET 8, ASP.NET Core  
- **База данных:** MS SQL Server (через EF Core + миграции)  
- **ORM:** Entity Framework Core  
- **Frontend:** JavaScript, Bootstrap, Chart.js  
- **Логирование:** встроенный `ILogger`  
- **Архитектура:** Repository + Services + Controllers (приближено к onion architecture)  

## 📂 Структура проекта

```

SaveNature/
├── Data/               # ApplicationDbContext, конфигурация моделей
├── Models/             # Доменные модели (User, Receipt, Item, EcoRating, Recommendation)
├── Repositories/       # Репозитории для работы с БД
├── Services/           # Логика приложения (UserService, PasswordHasher и др.)
├── Controllers/        # REST API контроллеры
├── Migrations/         # EF Core миграции
└── Program.cs          # Точка входа

````

## ⚙️ Установка и запуск

1. Клонировать репозиторий:
   ```bash
   git clone https://github.com/makar282/SaveNature.git
   cd SaveNature
   ```

2. Установить зависимости:

   ```
   dotnet restore
   ```

3. Применить миграции и создать базу данных:

   ```
   dotnet ef database update
   ```

4. Запустить проект:

   ```
   dotnet run
   ```


