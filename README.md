# Apicafe — CRUD de Cafés com EF Core + SQLite

API ASP.NET Core (.NET 10) com persistência em SQLite via Entity Framework Core, interface integrada na mesma origem e Collection de testes no Bruno.

## Tema e objetivo
Cadastro de **cafés** (marca, volume em ml e tipo).

Objetivo desta versão (AS): consolidar o projeto das avaliações AP1 e AP2 com o código **organizado por responsabilidade** — entidade em `Models/`, tipo de entrada em `Dtos/` e contexto em `Data/` — mantendo o `Program.cs` apenas com inicialização, middleware e rotas, e demonstrar o funcionamento com a Collection do Bruno.

## Requisitos
- .NET 10 SDK
- [Bruno](https://www.usebruno.com/) (para abrir a Collection em `bruno/`)

## Preparação e execução

```bash
dotnet restore
dotnet build
dotnet run
```

Execute os comandos na pasta do projeto (onde está o `Apicafe.csproj`). A API sobe em **http://localhost:5162** (perfil `http` do `Properties/launchSettings.json`) e a interface abre na mesma porta.

### Banco de dados
O projeto **não usa migrations**. O banco SQLite `cafes.db` é criado automaticamente na primeira execução por `db.Database.EnsureCreated()` (no `Program.cs`), na pasta de onde a aplicação é executada. Os dados ficam gravados nesse arquivo e continuam disponíveis depois de reiniciar a API. Para começar do zero, pare a API e apague o `cafes.db`.

## Estrutura de pastas

```
Apicafe/
├── Apicafe.csproj                    # projeto .NET 10 (ASP.NET Core, EF Core, SQLite)
├── Program.cs                        # inicialização, registro do contexto, middleware e rotas
├── Models/
│   └── CafeEntity.cs                 # entidade: tabela Cafes (Id, Marca, Volume, Tipo)
├── Dtos/
│   └── CafeInputDto.cs               # tipo de entrada (record) usado no POST e no PUT
├── Data/
│   └── ApiDbContext.cs               # contexto do EF Core (DbSet de CafeEntity)
├── wwwroot/
│   └── index.html                    # interface servida pela própria API
├── bruno/                            # Collection do Bruno
│   ├── bruno.json
│   ├── environments/local.bru        # ambiente local (baseUrl = http://localhost:5162)
│   └── 01-… a 09-….bru               # requisições do roteiro de testes
├── Properties/launchSettings.json    # porta de execução (5162)
├── appsettings*.json                 # configurações de log
├── Apicafe.postman_collection.json   # collection da AP2 (Postman)
└── .gitignore                        # ignora bin/, obj/ e bancos locais
```

### Por que os tipos foram separados
| Pasta | Tipo | Responsabilidade |
|-------|------|------------------|
| `Models/` | `CafeEntity` | Entidade: espelha a tabela `Cafes` e inclui o `Id`, gerado pelo banco. |
| `Dtos/` | `CafeInputDto` | Entrada: só `Marca`, `Volume` e `Tipo`. Sem `Id`, o cliente não escolhe o identificador. |
| `Data/` | `ApiDbContext` | Contexto do EF Core: liga a entidade ao SQLite. |

### Como o `Program.cs` encontra os tipos
Cada tipo tem um namespace igual ao da sua pasta (`Apicafe.Models`, `Apicafe.Dtos`, `Apicafe.Data`) e o `Program.cs` os importa com `using`:

```csharp
using Apicafe.Data;     // ApiDbContext
using Apicafe.Dtos;     // CafeInputDto
using Apicafe.Models;   // CafeEntity
using Microsoft.EntityFrameworkCore;
```

O `Program.cs` mantém apenas: criação do `builder` e registro do contexto (`AddDbContext` + `UseSqlite`), criação do banco (`EnsureCreated`), middleware de arquivos estáticos (`UseDefaultFiles`, `UseStaticFiles`) e as rotas da API. Nenhum tipo é definido dentro dele.

## Rotas da API

| Método | Rota              | Corpo           | Status de sucesso | Outros status | Descrição            |
|--------|-------------------|-----------------|-------------------|---------------|----------------------|
| GET    | `/api/cafes`      | —               | 200               | —             | Lista todos os cafés |
| GET    | `/api/cafes/{id}` | —               | 200               | 404           | Busca por id         |
| POST   | `/api/cafes`      | `CafeInputDto`  | 201               | 400           | Cria um café         |
| PUT    | `/api/cafes/{id}` | `CafeInputDto`  | 200               | 404, 400      | Atualiza um café     |
| DELETE | `/api/cafes/{id}` | —               | 204               | 404           | Remove um café       |

O `400` ocorre quando o corpo não é um JSON válido ou um campo tem tipo incompatível (por exemplo, `volume` com texto).

### Exemplos de JSON

Corpo enviado no POST e no PUT:

```json
{
  "marca": "Melitta",
  "volume": 500,
  "tipo": "Filtrado"
}
```

Resposta do POST (`201 Created`, com o `Id` gerado pelo banco) e do PUT (`200 OK`):

```json
{
  "id": 1,
  "marca": "Melitta",
  "volume": 500,
  "tipo": "Filtrado"
}
```

Se o corpo trouxer um `id`, ele é ignorado: o identificador é sempre gerado pelo banco.

## Interface
Interface em `wwwroot/index.html`, servida pela própria API em http://localhost:5162. Permite listar, cadastrar, editar e excluir cafés usando os endpoints reais.

## Collection do Bruno
A Collection está na pasta **`bruno/`** deste repositório.

1. No Bruno: **Open Collection** e selecione a pasta `bruno/`.
2. Escolha o ambiente **local** (canto superior direito). Ele define `baseUrl = http://localhost:5162`.
3. Com a API rodando (`dotnet run`), execute as requisições em ordem:

| # | Requisição | Resultado esperado |
|---|------------|--------------------|
| 01 | GET `/api/cafes` | 200 e a listagem |
| 02 | POST `/api/cafes` | 201 e o café criado (o `id` é guardado na variável `cafeId`) |
| 03 | GET `/api/cafes/{{cafeId}}` | 200 e os mesmos dados criados |
| 04 | PUT `/api/cafes/{{cafeId}}` | 200 e os dados atualizados |
| 05 | GET `/api/cafes/{{cafeId}}` | 200 e os dados já atualizados (confirma o PUT) |
| 06 | DELETE `/api/cafes/{{cafeId}}` | 204, sem corpo |
| 07 | GET `/api/cafes/{{cafeId}}` | 404 após a exclusão |
| 08 | POST `/api/cafes` | 201 (segundo registro, **não** excluído; `id` guardado em `cafeIdPersistencia`) |
| 09 | GET `/api/cafes/{{cafeIdPersistencia}}` | 200 — execute **depois de parar e reiniciar** a API |

O identificador usado nas requisições vem sempre da resposta do POST (variáveis `cafeId` e `cafeIdPersistencia`); não é preciso que seja 1. Entre as requisições 08 e 09, pare a API (`Ctrl+C`), execute `dotnet run` de novo e **não feche o Bruno**, para manter a variável.

Todas as requisições têm *assert* do status HTTP esperado e, quando há corpo na resposta, *tests* que conferem os dados. Opcionalmente, pela linha de comando:

```bash
npm install -g @usebruno/cli
cd bruno
bru run --env local
```

## Vídeo de demonstração — AS
<!-- Cole aqui o link público do vídeo da AS -->
**[Cole aqui o link do vídeo da AS]**

## Vídeo de demonstração — AP2
<!-- Cole aqui o link público do vídeo -->
**[Link do vídeo]**

## Observações
- Não versionar `bin/`, `obj/` nem o arquivo `cafes.db` (já listados no `.gitignore`).
- Dados persistem após reiniciar a aplicação.
- A Collection do Postman (`Apicafe.postman_collection.json`) é da AP2 e foi mantida; a Collection desta AS é a da pasta `bruno/`.
