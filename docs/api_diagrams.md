# API Diagrams (Mermaid)

This file contains API-focused class diagrams and additional sequence diagrams for important endpoints and flows.

---

## API class diagram

```mermaid
classDiagram
    class LoginApi { +POST /api/login }
    class RegisterApi { +POST /api/register }
    class NewTokenApi { +POST /api/auth/refresh }
    class LogoutApi { +POST /api/logout }
    class TransactionsApi { +CRUD /api/transactions }
    class SubscriptionsApi { +GET /api/subscriptions }
    class BudgetsApi { +CRUD /api/budgets }
    class CategoriesApi { +CRUD /api/categories }
    class UserApi { +GET /api/user\n+    +PUT /api/user }
    class ReportsApi { +GET /api/reports/budgets }
    class SpentThisMonthApi { +GET /api/spent-last-month }
    class JwtService
    class IEmailService
    class BudgetHelpers
    class FinancetrackerContext

    LoginApi ..> FinancetrackerContext : reads Users
    RegisterApi ..> FinancetrackerContext : writes Users
    NewTokenApi ..> JwtService : validate/issue tokens
    TransactionsApi ..> FinancetrackerContext : reads/writes Transactions/Categories/Subscriptions
    TransactionsApi ..> IEmailService : NotifyBudgetsForTransactionAsync
    BudgetsApi ..> BudgetHelpers : NormalizeRangeForResponse/ValidateCategoryExists
    ReportsApi ..> ReportService : GenerateBudgetReportPdfAsync

```

---

## Additional sequence diagrams

### Register (POST /api/register)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/register
    participant Db as FinancetrackerContext

    Client->>API: POST /api/register { username, email, password }
    API->>Db: Check Username/Email uniqueness
    Db-->>API: unique/exists
    API->>API: Hash password (salt + PBKDF2)
    API->>Db: INSERT User
    Db-->>API: user created
    API-->>Client: 200 OK { userId, username, email }

```

### Refresh token (POST /api/auth/refresh)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/auth/refresh
    participant Jwt as JwtService
    participant Db as FinancetrackerContext

    Client->>API: POST /api/auth/refresh { refreshToken }
    API->>Jwt: Validate refresh token signature/claims
    Jwt-->>API: principal or error
    API->>Db: Load user by id (from claims)
    Db-->>API: user
    API->>Jwt: GenerateTokens(user)
    Jwt-->>API: new tokens
    API-->>Client: 200 OK { accessToken, refreshToken }

```

### Add budget (POST /api/budgets)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/budgets
    participant Auth
    participant Db as FinancetrackerContext
    participant Helpers as BudgetHelpers

    Client->>API: POST /api/budgets { categoryId?, amount, startDate?, endDate? }
    API->>Auth: validate token -> get userId
    Auth-->>API: userId
    API->>Helpers: ValidateCategoryExistsAsync(categoryId, db, userId)
    Helpers-->>API: valid/invalid
    API->>Db: INSERT Budget
    Db-->>API: budget created
    API->>Helpers: NormalizeRangeForResponse(startDate,endDate)
    API-->>Client: 201 Created { budget }

```

### Update transaction (PUT /api/transactions/{id})

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/transactions/{id}
    participant Auth
    participant Db as FinancetrackerContext
    participant Email as IEmailService

    Client->>API: PUT /api/transactions/{id} { CategoryId, Amount, Name, Description, Date, SubscriptionId }
    API->>Auth: validate token -> get userId
    Auth-->>API: userId
    API->>Db: SELECT transaction WHERE id & userId
    Db-->>API: transaction or null
    API->>Db: Verify category exists for user or public
    Db-->>API: exists
    API->>Db: (if provided) verify subscription belongs to user
    Db-->>API: exists
    API->>Db: UPDATE transaction fields
    Db-->>API: saved
    API->>Email: NotifyBudgetsForTransactionAsync(transaction)
    Email-->>API: result
    API-->>Client: 200 OK { updated transaction }

```

### Delete transaction (DELETE /api/transactions/{id})

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/transactions/{id}
    participant Auth
    participant Db as FinancetrackerContext

    Client->>API: DELETE /api/transactions/{id}
    API->>Auth: validate token -> get userId
    Auth-->>API: userId
    API->>Db: SELECT transaction WHERE id & userId
    Db-->>API: transaction or null
    API->>Db: DELETE transaction
    Db-->>API: removed
    API-->>Client: 204 No Content

```

### Get paged transactions (GET /api/transactions/paged)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/transactions/paged
    participant Auth
    participant Db as FinancetrackerContext

    Client->>API: GET /api/transactions/paged?page=2&size=50
    API->>Auth: validate token -> get userId
    Auth-->>API: userId
    API->>Db: COUNT transactions WHERE userId
    Db-->>API: total
    API->>Db: SELECT transactions ORDER BY date,createdAt SKIP/TOP
    Db-->>API: items
    API-->>Client: 200 OK { page,size,total,totalPages,items }

```

### Update current user (PUT /api/user)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/user
    participant Auth
    participant Db as FinancetrackerContext

    Client->>API: PUT /api/user { username?, email?, password? }
    API->>Auth: validate token -> get userId
    Auth-->>API: userId
    API->>Db: SELECT user WHERE userId
    Db-->>API: user
    API->>Db: (if username/email changed) check uniqueness
    Db-->>API: unique/exists
    API->>API: if password provided, rehash
    API->>Db: UPDATE user
    Db-->>API: saved
    API-->>Client: 200 OK { user }

```

### Category CRUD (POST/GET/PUT/DELETE /api/categories)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/categories
    participant Auth
    participant Db as FinancetrackerContext

    Client->>API: POST /api/categories { name, icon?, color?, type, isPublic }
    API->>Auth: validate token -> get userId
    API->>Db: INSERT category (UserId null if public)
    Db-->>API: created
    API-->>Client: 201 Created { category }

    Client->>API: GET /api/categories
    API->>Auth: validate token -> get userId
    API->>Db: SELECT categories WHERE UserId==userId OR UserId IS NULL
    Db-->>API: categories
    API-->>Client: 200 OK [categories]

    Client->>API: PUT /api/categories/{id}
    API->>Auth: validate token -> get userId
    API->>Db: SELECT category
    Db-->>API: category
    API->>Db: If owner (UserId==userId) UPDATE
    Db-->>API: saved
    API-->>Client: 200 OK { category }

    Client->>API: DELETE /api/categories/{id}
    API->>Auth: validate token -> get userId
    API->>Db: SELECT category
    Db-->>API: category
    API->>Db: If owner delete
    Db-->>API: removed
    API-->>Client: 204 No Content

```

### Get subscriptions (GET /api/subscriptions)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/subscriptions
    participant Auth
    participant Db as FinancetrackerContext

    Client->>API: GET /api/subscriptions
    API->>Auth: validate token -> get userId
    API->>Db: SELECT subscriptions WHERE userId ORDER BY createdAt
    Db-->>API: subscriptions
    API-->>Client: 200 OK [subscriptions]

```

### Generate report (GET /api/reports/budgets)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/reports/budgets
    participant Auth
    participant Report as ReportService
    participant Db as FinancetrackerContext

    Client->>API: GET /api/reports/budgets?from=...&to=...
    API->>Auth: validate token -> get userId
    API->>Report: GenerateBudgetReportPdfAsync(userId, from, to)
    Report->>Db: load Budgets & Transactions
    Db-->>Report: data
    Report-->>API: PDF bytes
    API-->>Client: 200 File (application/pdf)

```

### Spent this month (GET /api/spent-last-month)

```mermaid
sequenceDiagram
    participant Client
    participant API as /api/spent-last-month
    participant Auth
    participant Db as FinancetrackerContext

    Client->>API: GET /api/spent-last-month
    API->>Auth: validate token -> get userId
    API->>Db: SELECT transactions WHERE userId AND date >= cutoff
    Db-->>API: transactions
    API->>API: compute total spent (expenses)
    API-->>Client: 200 OK { spent }

```

---

Generated on 2025-11-03
