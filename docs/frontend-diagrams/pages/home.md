# Home Page — Relations

This diagram shows the main relations for the `Home` page (representative).

````mermaid
# Home Page — Relations

This diagram shows the main relations for the `Home` page (representative).

```mermaid
flowchart LR
  Home[Home Page]
  AppLayout[AppLayout]
  AuthP[AuthProvider]
  ToastP[ToastProvider]
  TransactionsContainer[TransactionsContainer]
  SpentCards[SpentThisMonth / Spent Cards]
  TxAPI[apis/Transaction.ts]
  StatsAPI[apis/Statistics.ts]
  Http[httpService]

  Home --> AppLayout
  Home --> AuthP
  Home --> TransactionsContainer
  Home --> SpentCards

  TransactionsContainer --> TxAPI
  SpentCards --> StatsAPI

  TxAPI --> Http
  StatsAPI --> Http
````
