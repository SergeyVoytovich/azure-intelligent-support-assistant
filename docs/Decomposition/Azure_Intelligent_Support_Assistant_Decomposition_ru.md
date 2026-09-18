## Общая декомпозиция проекта

### Этап 1 — Engineering Foundation

* ~~**1.1** Создать публичный GitHub repository~~
* ~~**1.2** Подготовить `main`, `develop`, правила именования веток~~
* ~~**1.3** Настроить GitHub Branch Protection / Rulesets~~
* ~~**1.4** Добавить базовые repository-файлы: `.gitignore`, `.editorconfig`, `LICENSE`~~
* ~~**1.5** Создать .NET Solution и проекты Clean Architecture~~
* ~~**1.6** Настроить `Directory.Build.props` и `Directory.Packages.props`~~
* ~~**1.7** Создать первые unit/architecture test projects~~
* **1.8** Создать Vue 3 + TypeScript + Vite frontend
* **1.9** Добавить Vitest / Vue Test Utils
* ~~**1.10** Подготовить Dockerfile backend~~
* **1.11** Подготовить Dockerfile frontend
* ***1.12** **Создать Docker Compose с backend/frontend/Azurite/WireMock*
* ~~**1.13** Сделать первый TDD vertical slice `POST /api/chat`~~
* ~~**1.14** Создать Azure DevOps Project~~
* ~~**1.15** Создать Azure Boards: Epic → Features → User Stories~~
* ~~**1.16** Подключить GitHub к Azure DevOps~~
* ~~**1.17** Создать минимальную PR CI pipeline~~
* ~~**1.18** Создать структуру Bicep~~
* ~~**1.19** Описать базовую Azure-инфраструктуру~~
* ~~**1.20** Создать Budget + alerts~~
* ~~**1.21** Настроить Service Connection через Workload Identity Federation~~
* ~~**1.22** Настроить Managed Identity и первоначальный RBAC~~

### Этап 2 — AI, RAG и backend

* ~~**2.1** Подготовить тестовую fictional company и Knowledge Base~~
* ~~**2.2** Создать Blob Storage ingestion flow~~
* ~~**2.3** Интегрировать Document Intelligence~~
* ~~**2.4** Реализовать и протестировать chunking~~
* ~~**2.5** Развернуть embedding model~~
* **2.6** Создать AI Search index
* **2.7** Настроить vector/HNSW search
* **2.8** Добавить hybrid search
* **2.9** Добавить semantic ranking
* **2.10** Реализовать `IKnowledgeRetriever`
* **2.11** Реализовать Prompt Builder через TDD
* **2.12** Интегрировать Azure OpenAI
* **2.13** Реализовать grounded response + sources
* **2.14** Интегрировать Azure AI Language
* **2.15** Реализовать Escalation Policy
* **2.16** Укрепить `/api/chat`: validation, timeouts, ProblemDetails, correlation ID
* **2.17** Добавить container integration tests
* **2.18** Добавить реальные Azure integration tests
* **2.19** Довести coverage минимум до 70%

### Этап 3 — Vue и Continuous Delivery

* **3.1** Реализовать `SupportApiClient`
* **3.2** Реализовать `useChat`
* **3.3** Реализовать Vue chat UI
* **3.4** Добавить loading/error/escalation states
* **3.5** Показывать источники ответа
* **3.6** Добавить frontend tests
* **3.7** Расширить Azure Pipeline quality gates
* **3.8** Добавить static analysis
* **3.9** Добавить gitleaks / dependency scanning / Trivy
* **3.10** Валидировать Bicep в CI
* **3.11** Настроить build artifacts
* **3.12** Развернуть dev/test environment
* **3.13** Автоматизировать deployment Azure Functions
* **3.14** Автоматизировать deployment Static Web Apps
* **3.15** Добавить `/api/health`
* **3.16** Добавить smoke tests
* **3.17** Настроить `develop → dev/test`, `main → demo`

### Этап 4 — Production hardening и portfolio

* **4.1** Application Insights
* **4.2** Structured logging
* **4.3** Custom metrics
* **4.4** Log Analytics + KQL
* **4.5** Error-rate alert
* **4.6** Response-time alert
* **4.7** Token-usage alert
* **4.8** Dashboard / Workbook
* **4.9** AI cost calculation
* **4.10** Финальный RBAC/security review
* **4.11** ADR
* **4.12** Architecture diagram
* **4.13** README EN/DE/RU
* **4.14** Technical docs EN/DE/RU
* **4.15** Reflection EN/DE/RU
* **4.16** Demo guide EN/DE/RU
* **4.17** Финальная проверка Git history
* **4.18** `develop → main`
* **4.19** GitHub Release `v1.0.0`
* **4.20** Репетиция 10-минутного demo
