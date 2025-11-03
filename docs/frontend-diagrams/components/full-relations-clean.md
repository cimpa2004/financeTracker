# Frontend components full relation diagram

# Frontend components full relation diagram

Notes:

- High-level, inferred relations generated from file names in `frontend/src`.
- This file offers two diagrams: a compact Overview (for quick orientation) and a Detailed view (grouped and simplified) for deeper reading.
- If you want exact, import-accurate edges I can parse the source files and update edges.

## Overview (compact)

```mermaid
graph LR
  %% High-level flow: Pages -> Layout -> Providers -> APIs -> httpService
  subgraph Pages
    Home[Home]
    AddTx[Add Transaction]
    AllTx[All Transactions]
    Reports[Reports]
    Profile[Profile]
  end

  AppLayout[AppLayout]

  subgraph Providers
    AuthP[AuthProvider]
    ToastP[ToastProvider]
  end

  subgraph APIs
    AuthAPI[Auth.ts]
    TxAPI[Transaction.ts]
    ReportsAPI[Reports.ts]
    StatsAPI[Statistics.ts]
    UserAPI[User.ts]
  end

  Http[httpService]

  Pages --> AppLayout
  AppLayout --> Providers
  Providers --> APIs
  APIs --> Http

  %% Representative arrows
  Home --> TxAPI
  AddTx --> TxAPI
  AllTx --> TxAPI
  Reports --> ReportsAPI
  Profile --> UserAPI
```

## Detailed (grouped & simplified)

```mermaid
flowchart TB
  %% Page group (representative)
  subgraph Pages [Pages]
    P_Home(Home)
    P_AddTx(AddTransactionForm)
    P_AllTx(AllTransactions)
    P_Reports(ReportsPage)
    P_Profile(Profile)
  end

  %% Components (representative)
  subgraph Components [Key Components]
    C_TransactionForm(TransactionForm)
    C_TransactionsContainer(TransactionsContainer)
    C_PagedTransactions(PagedTransactions)
    C_BudgetDetails(BudgetDetails)
    C_AddCategoryForm(AddCategoryForm)
    C_IconSelector(IconSelector)
    C_SpentCards(Spent Cards)
  end

  %% Layouts & Providers
  subgraph LP [Layout & Providers]
    L_App(AppLayout)
    Prov_Auth(AuthProvider)
    Prov_Toast(ToastProvider)
  end

  %% APIs & Service
  subgraph APIs [API Modules]
    A_Auth(Auth.ts)
    A_Tx(Transaction.ts)
    A_Reports(Reports.ts)
    A_Stats(Statistics.ts)
    A_User(User.ts)
    A_Budget(Budget.ts)
    A_Category(Category.ts)
  end

  HttpS[httpService]

  %% Layout/provider usage
  P_Home --> L_App
  P_AddTx --> L_App
  P_AllTx --> L_App
  P_Reports --> L_App
  P_Profile --> L_App

  P_Home --> Prov_Auth
  P_AddTx --> Prov_Auth

  %% Component usage (representative)
  P_AddTx --> C_TransactionForm
  C_TransactionForm --> A_Tx

  P_AllTx --> C_TransactionsContainer
  C_TransactionsContainer --> C_PagedTransactions
  C_PagedTransactions --> A_Tx

  P_Home --> C_SpentCards
  C_SpentCards --> A_Stats

  P_Reports --> A_Reports
  P_Profile --> A_User
  P_AddTx --> C_IconSelector
  P_AddTx --> C_AddCategoryForm

  %% APIs use http service
  A_Tx --> HttpS
  A_Reports --> HttpS
  A_Stats --> HttpS
  A_Auth --> HttpS
  A_User --> HttpS
  A_Budget --> HttpS
  A_Category --> HttpS

```

---

If this looks good I can:

- Parse `frontend/src` imports to produce exact edges and update this diagram automatically.
- Split the detailed diagram into per-page files for very large projects.
