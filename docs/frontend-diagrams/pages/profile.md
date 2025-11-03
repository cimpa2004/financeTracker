# Profile Page — Relations

Relations for the Profile page.

# Profile Page — Relations

Relations for the Profile page.

```mermaid
flowchart LR
  Profile[Profile Page]
  AppLayout[AppLayout]
  UserAPI[apis/User.ts]
  Http[httpservice]

  Profile --> AppLayout
  Profile --> UserAPI
  UserAPI --> Http
```
