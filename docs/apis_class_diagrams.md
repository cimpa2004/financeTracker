## APIs / Controllers class diagrams (Mermaid)

```mermaid
classDiagram
    %% API mappings (minimal method names reflect routes)
    class LoginApi { +POST /api/login : LoginUser }
    class RegisterApi { +POST /api/register : RegisterUser }
    class NewTokenApi { +POST /api/auth/refresh : RefreshToken }
    class LogoutApi { +POST /api/logout : LogoutUser }
    class TransactionsApi {
      +POST /api/transactions : AddTransaction
      +GET /api/transactions : GetTransactions
      +GET /api/transactions/{id} : GetTransactionById
      +GET /api/transactions/paged : GetPagedTransactions
      +DELETE /api/transactions/{id} : DeleteTransaction
      +PUT /api/transactions/{id} : UpdateTransaction
      +PUT /api/transactions/{id}/setSubscription : SetTransactionSubscription
      +GET /api/transactions/last3 : GetLast3Transactions
    }
    class SubscriptionsApi { +GET /api/subscriptions : GetSubscriptions
    +GET /api/subscriptions/last3 : GetLast3Subscriptions }
    class BudgetsApi {
      +POST /api/budgets : AddBudget
      +GET /api/budgets : GetBudgets
      +GET /api/budgets/{id} : GetBudgetById
      +GET /api/budgets/{id}/status : GetBudgetStatus
      +GET /api/budgets/{id}/transactions : GetBudgetTransactions
      +GET /api/budgets/status : GetAllBudgetsStatus
      +PUT /api/budgets/{id} : UpdateBudget
      +DELETE /api/budgets/{id} : DeleteBudget
    }
    class CategoriesApi {
      +POST /api/categories : AddCategory
      +GET /api/categories : GetCategories
      +GET /api/categories/{id} : GetCategoryById
      +PUT /api/categories/{id} : UpdateCategory
      +DELETE /api/categories/{id} : DeleteCategory
    }
    class UserApi { +GET /api/user : GetCurrentUser
    +PUT /api/user : UpdateCurrentUser }
    class ReportsApi { +GET /api/reports/budgets : GenerateBudgetReport }
    class SpentThisMonthApi { +GET /api/spent-last-month : GetSpentLastMonth }

    %% dependencies / relationships
    LoginApi ..> FinancetrackerContext : reads Users
    RegisterApi ..> FinancetrackerContext : writes Users
    NewTokenApi ..> JwtService : validate/issue
    TransactionsApi ..> FinancetrackerContext : reads/writes Transactions/Categories/Subscriptions
    TransactionsApi ..> IEmailService : notify budgets
    SubscriptionsApi ..> FinancetrackerContext : reads Subscriptions
    BudgetsApi ..> BudgetHelpers : uses
    ReportsApi ..> ReportService : generates PDF

```

Generated on: 2025-11-03
