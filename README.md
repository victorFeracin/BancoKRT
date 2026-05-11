# BancoKRT

API ASP.NET Core para cadastro, consulta, atualização, exclusão lógica e processamento de limite PIX por conta, usando DynamoDB como persistência.

## Visão geral

O projeto está organizado em camadas:

- `src/BancoKRT.Api`: camada HTTP, controllers, contratos, Swagger e tratamento de exceções.
- `src/BancoKRT.Application`: casos de uso, DTOs e orquestração das regras de negócio.
- `src/BancoKRT.Domain`: entidades, value objects e validações de domínio.
- `src/BancoKRT.Infrastructure`: acesso ao DynamoDB, mapeamentos e inicialização da tabela.
- `tests/BancoKRT.Tests`: testes unitários.

## Tecnologias e dependências

- .NET 8 (`net8.0`)
- ASP.NET Core Web API
- Swagger / Swashbuckle
- Amazon DynamoDB SDK para .NET
- DynamoDB Local via Docker Compose
- xUnit, Moq e FluentAssertions para testes

## Pré-requisitos

Antes de rodar o projeto, tenha instalado:

- .NET SDK 8.0
- Docker Desktop ou Docker Engine com Docker Compose
- Git

## Configuração do banco de dados

O projeto usa DynamoDB Local em container Docker. A configuração padrão da API está em [`src/BancoKRT.Api/appsettings.json`](./src/BancoKRT.Api/appsettings.json):

- `TableName`: `PixLimitAccounts`
- `Region`: `us-east-1`
- `ServiceUrl`: `http://localhost:8000`
- `AccessKey`: `test`
- `SecretKey`: `test`

O arquivo [`docker-compose.yml`](./docker-compose.yml) sobe o DynamoDB Local na porta `8000`, com persistência em `.docker/dynamodb`.

Importante:

- A tabela é criada automaticamente na inicialização da API, caso ainda não exista.
- Não é necessário criar a tabela manualmente para o ambiente local padrão.

## Como rodar o projeto

### 1. Clonar o repositório

```powershell
git clone https://github.com/victorFeracin/BancoKRT.git
cd BancoKRT
```

### 2. Subir o DynamoDB Local

```powershell
docker compose up -d
```

Para verificar se o container está em execução:

```powershell
docker ps
```

### 3. Restaurar as dependências do .NET

```powershell
dotnet restore .\BancoKRT.sln
```

### 4. Rodar a API

Recomendado usar o perfil `http` para simplificar o ambiente local:

```powershell
dotnet run --project .\src\BancoKRT.Api\BancoKRT.Api --launch-profile http
```

Com isso, a aplicação deve ficar disponível em:

- API: `http://localhost:5057`
- Swagger: `http://localhost:5057/swagger`

## Como parar o ambiente

Para parar a API, encerre o processo no terminal.

Para parar o banco local:

```powershell
docker compose down
```

Se quiser remover também os dados persistidos em `.docker/dynamodb`, apague esse diretório manualmente.

## Configuração por ambiente

As configurações principais ficam em:

- [`src/BancoKRT.Api/appsettings.json`](./src/BancoKRT.Api/appsettings.json)
- [`src/BancoKRT.Api/appsettings.Development.json`](./src/BancoKRT.Api/appsettings.Development.json)

Se precisar apontar para outro DynamoDB, ajuste a seção `DynamoDb`:

```json
"DynamoDb": {
  "TableName": "PixLimitAccounts",
  "Region": "us-east-1",
  "ServiceUrl": "http://localhost:8000",
  "AccessKey": "test",
  "SecretKey": "test"
}
```

## Executando os testes

Para rodar a suíte de testes:

```powershell
dotnet test .\BancoKRT.sln
```

## Estrutura de persistência

A tabela DynamoDB usa:

- Chave de partição (`PK`)
- Chave de ordenação (`SK`)

O repositório responsável pela persistência é [`src/BancoKRT.Infrastructure/Repositories/PixLimitAccountRepository.cs`](./src/BancoKRT.Infrastructure/Repositories/PixLimitAccountRepository.cs), e a criação automática da tabela acontece em [`src/BancoKRT.Infrastructure/Persistence/Initialization/DynamoDbTableInitializer.cs`](./src/BancoKRT.Infrastructure/Persistence/Initialization/DynamoDbTableInitializer.cs).

## Observações

- O Swagger é habilitado apenas em ambiente de desenvolvimento.
- O banco local persiste dados no diretório `.docker/dynamodb`.
- Caso altere nome da tabela ou porta do DynamoDB, ajuste tanto o `docker-compose.yml` quanto a configuração `DynamoDb` da API.
