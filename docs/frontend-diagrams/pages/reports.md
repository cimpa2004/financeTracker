# Reports Page — Relations

Relations for the Reports page and components.

# Reports Page — Relations

Relations for the Reports page and components.

```mermaid
flowchart LR
  Reports[Reports Page]
  AppLayout[AppLayout]
  ReportsAPI[apis/Reports.ts]
  Http[httpservice]

  Reports --> AppLayout
  Reports --> ReportsAPI
  ReportsAPI --> Http
```
