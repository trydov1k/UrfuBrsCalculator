# Калькулятор баллов БРС УрФУ

Веб-приложение для расчёта итогового балла и оценки по дисциплинам в балльно-рейтинговой системе (БРС) Уральского федерального университета.

## Стек

- **.NET 10** — C#, ASP.NET Core Web API, Blazor WebAssembly
- **MudBlazor** — UI
- **EF Core** — PostgreSQL
- **ASP.NET Core Identity + JWT** — аутентификация

## Структура solution

```
src/
  BrsCalculator.Domain/          # Сущности, правила БРС, расчёт оценок
  BrsCalculator.Application/     # DTO, шаблоны дисциплин, планировщик «Что если?»
  BrsCalculator.Infrastructure/  # EF Core, Identity, JWT
  BrsCalculator.Api/             # REST API
  BrsCalculator.Client/          # Blazor WASM + MudBlazor
```

## Быстрый старт

### 1. API

Запустите PostgreSQL: `docker compose up -d postgres`.

```bash
cd src/BrsCalculator.Api
dotnet run
```

Строка подключения — в `appsettings.json` / `appsettings.Development.json`. Swagger (Development): `https://localhost:7212/swagger`.

### Полный запуск в Docker

API (включая Blazor UI), PostgreSQL:

```bash
docker compose up -d --build
```

| Сервис | URL |
|--------|-----|
| Приложение (UI + API) | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

UI и API на одном origin — отдельный nginx не нужен. В Production-сборке WASM `ApiBaseUrl` пустой, запросы идут на `http://localhost:8080`.

Остановка: `docker compose down`. Данные БД сохраняются в volume `pgdata`.

### 2. Клиент

```bash
cd src/BrsCalculator.Client
dotnet run
```

URL API задаётся в `wwwroot/appsettings.Development.json` (`ApiBaseUrl`: `https://localhost:7212/`).

### 3. Регистрация и работа

1. Откройте клиент, зарегистрируйтесь (пароль ≥ 8 символов, с цифрой).
2. Создайте семестр и дисциплину с типами занятий.
3. Настройте дерево компонентов через API (`/nodes`) или доработайте UI.
4. Вводите баллы в листовых компонентах — итог пересчитывается автоматически.
5. Режим «Что если?» — планирование целевой оценки.
6. Экспорт таблицы в PNG/PDF (html2canvas + jsPDF).

## Правила расчёта (кратко)

- Иерархия: предмет → тип занятия → аттестация → компоненты (рекурсивно).
- Родительский балл = Σ(балл потомка × коэффициент).
- Сдача: итог ≥ 40 и экзамен ≥ 40 (если экзамен предусмотрен).
- Оценки: 5 (≥80), 4 (60–79.99), 3 (40–59.99), иначе 2.

## MVP (фронтенд)

- Регистрация, вход, восстановление пароля (токен в dev-режиме на экране)
- Семестры: создание, редактирование, удаление
- Дисциплины: создание с шаблоном, переименование, удаление
- Дисциплина → вкладка **Структура**: дерево, добавление/редактирование/удаление компонентов, флаг экзамена, предупреждение о сумме весов
- Дисциплина → вкладка **Баллы**: ввод в листьях, итог и оценка
- **Что если?**: временные баллы, пересчёт в UI, целевая оценка 3/4/5, распределение недостающих баллов
- Экспорт PNG/PDF

После изменений API перезапустите `BrsCalculator.Api`, чтобы подтянуть endpoint `POST .../preview` и каскадное удаление узлов.
