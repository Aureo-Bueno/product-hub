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
│   │   ├── LojaProdutos.Domain/          # Entidades e interfaces
│   │   ├── LojaProdutos.Application/     # DTOs, serviços, interfaces
│   │   ├── LojaProdutos.Infrastructure/  # EF Core, repositórios, middleware
│   │   └── LojaProdutos.API/             # Controllers, Program.cs, Dockerfile
│   ├── LojaProdutos.sln
│   └── .dockerignore
├── front-end/
│   ├── index.html
│   ├── create.html
│   ├── list.html
│   ├── css/
│   ├── js/
│   ├── nginx.conf
│   └── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## Backend (API)

### Endpoints completos

#### CRUD básico

| Método | Rota               | Descrição                               |
| ------ | ------------------ | --------------------------------------- |
| GET    | `/categories`      | Lista (com paginação, busca, ordenação) |
| GET    | `/categories/{id}` | Busca por ID                            |
| POST   | `/categories`      | Cria nova categoria                     |
| PUT    | `/categories/{id}` | Atualiza categoria                      |
| DELETE | `/categories/{id}` | Soft delete                             |

#### Features sem IA

| Método | Rota                        | Descrição          |
| ------ | --------------------------- | ------------------ |
| GET    | `/categories/tree`          | Árvore hierárquica |
| GET    | `/categories/favorites`     | Lista favoritas    |
| POST   | `/categories/{id}/favorite` | Alterna favorito   |
| GET    | `/categories/stats`         | Estatísticas       |
| GET    | `/categories/export`        | Exportar CSV       |

#### Features com Gemini (IA)

| Método | Rota                               | Descrição                         |
| ------ | ---------------------------------- | --------------------------------- |
| POST   | `/categories/generate-description` | Gera descrição automaticamente    |
| POST   | `/categories/suggest`              | Sugere categorias por tema        |
| POST   | `/categories/correct-name`         | Corrige nome ortograficamente     |
| POST   | `/categories/check-duplicate`      | Detecta duplicidade               |
| POST   | `/categories/classify`             | Classifica texto em uma categoria |

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

### Páginas

| Página   | URL            | Descrição              |
| -------- | -------------- | ---------------------- |
| Home     | `/index.html`  | Página inicial         |
| Cadastro | `/create.html` | Formulário de cadastro |
| Listagem | `/list.html`   | Tabela com CRUD        |

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
