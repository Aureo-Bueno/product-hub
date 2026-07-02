# Loja Produtos

Sistema de gerenciamento de categorias de produtos com **ASP.NET Core 10** (backend) e **HTML + Bootstrap 5** (frontend), orquestrado via **Docker Compose**.

---

## Sumário

- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Como rodar](#como-rodar)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Backend (API)](#backend-api)
- [Features sem IA](#features-sem-ia)
- [Features com Gemini](#features-com-gemini)
- [Frontend](#frontend)
- [Server-Sent Events (SSE)](#server-sent-events-sse)
- [Nginx](#nginx)
- [Docker](#docker)

---

## Arquitetura

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│   Frontend   │──────▶│    Nginx     │──────▶│   Backend    │──────▶│  SQL Server  │
│  (Bootstrap) │  :80  │  (proxy)     │ :7006 │  (.NET 10)   │ :1433 │              │
└──────────────┘       └──────────────┘       └──────────────┘       └──────────────┘
                                                       │
                                                       ▼
                                              ┌──────────────────┐
                                              │  Google Gemini   │
                                              │  (IA generativa) │
                                              └──────────────────┘
```

---

## Tecnologias

### Backend

- **.NET 10** (C#)
- **Entity Framework Core 10** — SQL Server
- **Serilog** — logging estruturado com Correlation ID
- **Google Gemini API** — features de IA generativa
- **Arquitetura Hexagonal** (Ports & Adapters)

### Frontend

- **HTML5** + **Bootstrap 5**
- **SweetAlert2** — notificações
- **Nginx** — servidor web e proxy reverso

### Infraestrutura

- **Docker** + **Docker Compose**

---

## Pré-requisitos

- [Docker](https://docs.docker.com/engine/install/) e [Docker Compose](https://docs.docker.com/compose/install/)
- (Opcional) [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) para desenvolvimento local

---

## Como rodar

```bash
# Configure a API key do Gemini (opcional — sem ela as features de IA ficam desabilitadas)
export GEMINI_API_KEY="sua-chave-aqui"

# Suba os containers
docker compose up --build
```

Acesse:

| Serviço    | URL                                        |
| ---------- | ------------------------------------------ |
| Frontend   | http://localhost:8080                      |
| API        | http://localhost:7006                      |
| Swagger UI | http://localhost:7006/swagger              |
| OpenAPI    | http://localhost:7006/openapi/v1.json      |
| SQL Server | `localhost:1433` (sa / `Str0ngPass_2024!`) |

---

## Estrutura do projeto

```
.
├── back-end/
│   ├── src/
│   │   ├── LojaProdutos.Domain/              # Entities + domain interfaces
│   │   ├── LojaProdutos.Application/         # DTOs, service interfaces, service implementations
│   │   ├── LojaProdutos.Infrastructure/      # EF Core, repositories, Gemini/Log services, DI
│   │   └── LojaProdutos.API/                 # Controllers, Program.cs, Dockerfile
│   ├── LojaProdutos.sln
│   └── .dockerignore
├── front-end/
│   ├── index.html, create.html, list.html    # Static pages
│   ├── js/
│   │   ├── app/                              # Reusable application classes
│   │   │   ├── ApiClient.js                  # Base HTTP client (GET/POST/PUT/DELETE)
│   │   │   ├── CategoryApi.js                # Category API endpoints
│   │   │   ├── ProductApi.js                 # Product API endpoints
│   │   │   ├── DepartmentApi.js              # Department API endpoints
│   │   │   ├── LogApi.js                     # SSE log stream URL
│   │   │   └── UiHelper.js                   # Static UI utilities (modals, pagination, tree, tags)
│   │   └── pages/                            # Page-specific controllers
│   │       ├── home.js                       # Home page (activity feed, stats, AI suggestions)
│   │       ├── create.js                     # Create page (forms, SSE desc gen, name correction)
│   │       └── list.js                       # List page (tabs: categories, departments, products)
│   ├── nginx.conf
│   └── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## Backend (API)

### Endpoints completos

#### CRUD básico

| Método | Rota                | Descrição                               |
| ------ | ------------------- | --------------------------------------- |
| GET    | `/categories`       | Lista (com paginação, busca, ordenação) |
| GET    | `/categories/{id}`  | Busca por ID                            |
| POST   | `/categories`       | Cria nova categoria                     |
| PUT    | `/categories/{id}`  | Atualiza categoria                      |
| DELETE | `/categories/{id}`  | Soft delete                             |
| GET    | `/products`         | Lista produtos (paginação, busca)       |
| GET    | `/products/{id}`    | Busca produto por ID                    |
| POST   | `/products`         | Cria novo produto                       |
| PUT    | `/products/{id}`    | Atualiza produto                        |
| DELETE | `/products/{id}`    | Soft delete produto                     |
| GET    | `/departments`      | Lista departamentos                     |
| GET    | `/departments/{id}` | Busca departamento por ID               |
| POST   | `/departments`      | Cria novo departamento                  |
| PUT    | `/departments/{id}` | Atualiza departamento                   |
| DELETE | `/departments/{id}` | Exclui departamento                     |

#### Product e Department endpoints

| Método | Rota                      | Descrição                |
| ------ | ------------------------- | ------------------------ |
| POST   | `/products/{id}/favorite` | Alterna favorito         |
| GET    | `/products/favorites`     | Lista produtos favoritos |

#### Features sem IA

| Método  | Rota                        | Descrição                      |
| ------- | --------------------------- | ------------------------------ |
| GET     | `/categories/tree`          | Árvore hierárquica             |
| GET     | `/categories/favorites`     | Lista favoritas                |
| POST    | `/categories/{id}/favorite` | Alterna favorito               |
| GET     | `/categories/stats`         | Estatísticas                   |
| GET     | `/categories/export`        | Exportar CSV                   |
| **GET** | **`/logs/stream`**          | **Feed de atividades via SSE** |

#### Features com Gemini (IA)

| Método  | Rota                                          | Descrição                               |
| ------- | --------------------------------------------- | --------------------------------------- |
| POST    | `/categories/generate-description`            | Gera descrição automaticamente          |
| **GET** | **`/categories/generate-description-stream`** | **Descrição via SSE (token streaming)** |
| POST    | `/categories/suggest`                         | Sugere categorias por tema              |
| POST    | `/categories/correct-name`                    | Corrige nome ortograficamente           |
| POST    | `/categories/check-duplicate`                 | Detecta duplicidade                     |
| POST    | `/categories/classify`                        | Classifica texto em uma categoria       |
| POST    | `/products/generate-description`              | Gera descrição de produto               |
| **GET** | **`/products/generate-description-stream`**   | **Descrição de produto via SSE**        |
| POST    | `/products/suggest`                           | Sugere produtos para uma categoria      |
| POST    | `/products/correct-name`                      | Corrige nome de produto                 |
| POST    | `/products/classify`                          | Classifica texto em categoria           |

### Modelo da entidade

| Campo       | Tipo           | Restrições                       |
| ----------- | -------------- | -------------------------------- |
| Id          | int            | PK, auto-increment               |
| Name        | string(200)    | Obrigatório, mínimo 5 caracteres |
| Description | string(1000)   | Opcional                         |
| DateCreate  | DateTime       | Obrigatório                      |
| DateUpdate  | DateTime?      | Opcional                         |
| Department  | string(100)    | Obrigatório                      |
| ParentId    | int?           | FK para hierarquia               |
| IsDeleted   | bool           | Soft delete                      |
| DeletedAt   | DateTime?      | Data da exclusão                 |
| IsFavorite  | bool           | Favorita                         |
| Tags        | List\<string\> | Etiquetas                        |

### Arquitetura Hexagonal

```
┌──────────────────────────────────────────────────────────┐
│                      API (Controllers)                    │
├──────────────────────────────────────────────────────────┤
│                    Application (Services)                 │
├──────────────────────────────────────────────────────────┤
│                    Domain (Entities)                      │
├──────────────────────────────────────────────────────────┤
│                  Infrastructure (EF Core)                 │
└──────────────────────────────────────────────────────────┘
```

---

## Features sem IA

### 1. Hierarquia de categorias

Categorias podem ter pai e filho. O endpoint `/categories/tree` retorna a estrutura aninhada.

### 2. Busca avançada

```
GET /categories?search=tec
```

Busca por nome, descrição e departamento.

### 3. Paginação

```
GET /categories?page=1&limit=10
```

### 4. Ordenação

```
GET /categories?sort=name&order=asc
GET /categories?sort=date&order=desc
GET /categories?sort=favorite
```

### 5. Soft Delete

Ao excluir, a categoria é marcada como removida (`IsDeleted = true`) e não é mais retornada nas listagens padrão (via `HasQueryFilter`). O registro permanece no banco.

### 6. Histórico de alterações

Toda criação, edição ou exclusão gera um registro na tabela `CategoryLogs` com os valores antigos e novos em JSON.

### 7. Favoritas

```
POST /categories/42/favorite   # alterna favorito
GET  /categories/favorites      # lista favoritas
```

### 8. Tags

Cada categoria pode ter múltiplas tags (armazenadas como JSON).

### 9. Estatísticas

```
GET /categories/stats
# { total: 120, createdThisMonth: 14, updatedToday: 3, favorites: 5, deleted: 2 }
```

### 10. Exportação CSV

```
GET /categories/export
# Download de categorias.csv
```

---

## Features com Gemini

### Configuração

Defina a variável de ambiente `GEMINI_API_KEY` com sua chave do Google AI Studio.

```bash
export GEMINI_API_KEY="sua-chave-aqui"
docker compose up --build
```

Sem a chave, as features de IA retornam valores padrão (fallback silencioso).

### 1. Gerar descrição automaticamente

```json
POST /categories/generate-description
{ "name": "Inteligência Artificial" }
// → { "description": "Categoria sobre machine learning, deep learning e IA..." }
```

### 2. Sugestão de categorias

```json
POST /categories/suggest
{ "prompt": "blog sobre tecnologia" }
// → { "suggestions": ["Programação", "Cloud", "DevOps", "IA", ...] }
```

### 3. Correção de nomes

```json
POST /categories/correct-name
{ "prompt": "progamação" }
// → { "corrected": "Programação" }
```

### 4. Detecção de duplicidade

```json
POST /categories/check-duplicate
{ "name": "IA" }
// → { "isDuplicate": true, "category": { "id": 1, "name": "Inteligência Artificial", ... } }
```

### 5. Classificação automática

```json
POST /categories/classify
{ "text": "Spring Boot é um framework Java..." }
// → { "category": "Programação" }
```

---

## Frontend

### JavaScript Architecture (ES Modules)

The frontend uses ES modules (`type="module"`) with no bundler — modern browsers load them natively. The code follows SOLID and DRY principles:

- **`app/ApiClient.js`** — base HTTP class (SRP); all API classes extend it
- **`app/CategoryApi.js`**, **`ProductApi.js`**, **`DepartmentApi.js`**, **`LogApi.js`** — domain-specific API clients
- **`app/UiHelper.js`** — stateless UI utilities shared across pages
- **`pages/home.js`**, **`create.js`**, **`list.js`** — page controllers that wire DOM events to API calls

All inline event handlers (`onclick`, `onkeyup`, etc.) have been removed from HTML; events are bound via `addEventListener` in the page controllers.

### Páginas

| Página   | URL            | Descrição              |
| -------- | -------------- | ---------------------- |
| Home     | `/index.html`  | Página inicial         |
| Cadastro | `/create.html` | Formulário de cadastro |
| Listagem | `/list.html`   | Tabela com CRUD        |

---

## Server-Sent Events (SSE)

The project uses **SSE** (Server-Sent Events) for real-time, one-way data streaming from the server to the browser. SSE was chosen over WebSocket because the communication is unidirectional (server → client) and SSE works natively over HTTP with no special handshake, making it simpler to deploy behind Nginx and Docker.

### Where SSE is used

| Feature                               | Endpoint                                            | Frontend Consumer                           |
| ------------------------------------- | --------------------------------------------------- | ------------------------------------------- |
| Activity Feed (live log)              | `GET /logs/stream`                                  | `home.js` → `EventSource` → `#activityFeed` |
| Category description (token-by-token) | `GET /categories/generate-description-stream?name=` | `create.js` → `EventSource` → textarea      |
| Product description (token-by-token)  | `GET /products/generate-description-stream?name=`   | `create.js` → `EventSource` → textarea      |

### Architecture

```
Browser                         ASP.NET Backend                 Gemini API
──────                              ────────────                 ──────────
EventSource ──GET /stream──────▶ Controller ──▶ LogService ──▶ SQL Server (poll)
       │                                │         (IAsyncEnumerable)
       │  ◀──data: {json}\n\n──── SSE ──┤
       │                                │
EventSource ──GET /desc-stream──────▶ Controller ──▶ GeminiService
       │                                │         (IAsyncEnumerable)
       │  ◀──data: {text:"chunk"}\n\n───┤
```

### How it works

#### 1. Activity Feed (`/logs/stream`)

1. `LogController.Stream()` sets SSE headers (`Content-Type: text/event-stream`, `Cache-Control: no-cache`)
2. Calls `ILogService.StreamRecentLogsAsync()` which returns `IAsyncEnumerable<LogEventDto>`
3. **On connection**: immediately sends logs from the last 5 minutes (historical batch)
4. Then enters a **polling loop**: every 3 seconds queries `CategoryLogRepository.GetRecentAsync(since)` for new entries
5. Each log is serialized as JSON and written as `data: {json}\n\n` to the response stream
6. The browser `EventSource.onmessage` parses each event and prepends it to `#activityFeed`
7. Connection is kept alive until the client navigates away (browser closes the EventSource)

#### 2. Description Generation (token streaming)

1. `CategoriesController.GenerateDescriptionStream()` / `ProductsController.GenerateDescriptionStream()` receive the item name
2. Call `IGeminiService.GenerateContentStreamAsync(prompt)` which sends a POST to the Gemini API with `?alt=sse`
3. The Gemini API returns a **server-sent event stream** where each event contains a `data:` line with JSON
4. Each chunk is parsed to extract the text token, then forwarded to the client as `data: {"text":"token"}\n\n`
5. When the Gemini stream ends, the controller sends `data: [DONE]\n\n` as a termination signal
6. The frontend `EventSource.onmessage` appends each token to the description textarea and closes the connection upon `[DONE]`

#### 3. Why SSE and not WebSocket

| Factor         | SSE                               | WebSocket                             |
| -------------- | --------------------------------- | ------------------------------------- |
| Direction      | Server → Client only              | Bidirectional                         |
| Protocol       | HTTP (standard)                   | ws:// / wss://                        |
| Auto-reconnect | Built-in (`EventSource`)          | Manual implementation                 |
| Nginx proxy    | Simple (`proxy_pass`)             | Requires upgrade headers              |
| Use case fit   | Push notifications, log streaming | Real-time chat, collaborative editing |

### SSE Format

```
data: {"id":1,"action":"created","categoryName":"Eletrônicos","message":"Nova categoria: Eletrônicos","createdAt":"2026-07-01T12:00:00Z"}

data: {"text":"Esta é uma categoria sobre "}

data: [DONE]
```

Each event is a single `data:` line followed by a blank line (`\n\n`). The `EventSource` API automatically splits on this boundary.

### Backend classes involved

| Class                   | Responsibility                                                                        |
| ----------------------- | ------------------------------------------------------------------------------------- |
| `LogsController`        | SSE endpoint `/logs/stream`, serializes `LogEventDto` with camelCase JSON             |
| `CategoriesController`  | SSE endpoint `GET /categories/generate-description-stream`                            |
| `ProductsController`    | SSE endpoint `GET /products/generate-description-stream`                              |
| `LogService`            | Implements `IAsyncEnumerable<LogEventDto>` with initial historical batch + 3s polling |
| `GeminiService`         | Wraps Gemini API's `streamGenerateContent?alt=sse`, yields text tokens                |
| `CategoryLogRepository` | Queries `CategoryLogs` table for recent entries                                       |

### Frontend classes involved

| Class                                   | Responsibility                                        |
| --------------------------------------- | ----------------------------------------------------- |
| `LogApi`                                | Provides `streamUrl()` pointing to `/api/logs/stream` |
| `HomePage._bindActivityFeed()`          | Creates `EventSource`, handles `onmessage`/`onerror`  |
| `CreatePage._generateCatDescription()`  | Creates `EventSource` to category description stream  |
| `CreatePage._generateProdDescription()` | Creates `EventSource` to product description stream   |
| `CategoryApi.descriptionStreamUrl()`    | Returns the SSE URL for category description          |
| `ProductApi.descriptionStreamUrl()`     | Returns the SSE URL for product description           |

---

## Nginx

O `front-end/nginx.conf` faz proxy reverso de `/api/*` → `http://backend:80/*`.

```nginx
location /api/ {
    rewrite ^/api/(.*) /$1 break;
    proxy_pass http://backend:80;
}
```

---

## Docker

### docker-compose.yml

```yaml
services:
  sqlserver: # SQL Server 2022
  backend: # API .NET 10 + Gemini
  frontend: # Nginx
```

### Variáveis de ambiente

#### Backend

| Variável                               | Descrição       | Default                           |
| -------------------------------------- | --------------- | --------------------------------- |
| `ConnectionStrings__DefaultConnection` | Conexão SQL     | `Server=localhost\SQLEXPRESS;...` |
| `ASPNETCORE_ENVIRONMENT`               | Ambiente        | `Development`                     |
| `GEMINI_API_KEY`                       | Chave do Gemini | _(vazio — IA desabilitada)_       |
| `GEMINI_MODEL`                         | Modelo Gemini   | `gemini-2.0-flash`                |

#### SQL Server

| Variável            | Descrição | Default            |
| ------------------- | --------- | ------------------ |
| `ACCEPT_EULA`       | Licença   | `Y`                |
| `MSSQL_SA_PASSWORD` | Senha SA  | `Str0ngPass_2024!` |
