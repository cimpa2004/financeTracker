# Routes -> Pages map

````mermaid
flowchart TD
  WelcomePage[/WelcomePage]\n  Login[/Login]\n  Register[/Register]\n+  Protected[ProtectedRoute]\n+  AppLayout[AppLayout]\n+  Home[/Home]\n+  AddTransaction[/AddTransaction]\n+  AddCategory[/AddCategoryPage]\n+  BudgetCharts[/BudgetChartsPage]\n+  AddBudget[/AddBudgetPage]\n+  Statistics[/StatisticsPage]\n+  Reports[/ReportsPage]\n+  AllTransactions[/AllTransactions]\n+  Profile[/Profile]\n+
  WelcomePage -->|index| WelcomePage
  Login -->|/login| Login
  Register -->|/register| Register
  # Routes -> Pages map

  ```mermaid
  flowchart TD
    WelcomePage[/WelcomePage]
    Login[/Login]
    Register[/Register]
    Protected[ProtectedRoute]
    AppLayout[AppLayout]
    Home[/Home]
    AddTransaction[/AddTransaction]
    AddCategory[/AddCategoryPage]
    BudgetCharts[/BudgetChartsPage]
    AddBudget[/AddBudgetPage]
    Statistics[/StatisticsPage]
    Reports[/ReportsPage]
    AllTransactions[/AllTransactions]
    Profile[/Profile]

    WelcomePage -->|index| WelcomePage
    Login -->|/login| Login
    Register -->|/register| Register
    Protected --> AppLayout
    AppLayout --> Home
    AppLayout --> AddTransaction
    AppLayout --> AddCategory
    AppLayout --> BudgetCharts
    AppLayout --> AddBudget
    AppLayout --> Statistics
    AppLayout --> Reports
    AppLayout --> AllTransactions
    AppLayout --> Profile

    subgraph ProtectedRoutes
      Protected
      AppLayout
    end

    style ProtectedRoutes stroke:#333,stroke-width:1,fill:#f9f9f9

  ```
      AppLayout --> Statistics
````
