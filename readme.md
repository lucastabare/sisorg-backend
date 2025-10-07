# Sisorg

API .NET 8 para cargar, consultar y borrar “registries” a partir de archivos `.txt` con filas `Name#Value#Color`.  
Incluye capa de Infraestructura (EF Core + MySQL), Repositorios/Servicios y tests con MSTest.

## Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core + Pomelo MySQL
- MySQL 8 (Docker)
- Swagger (OpenAPI)
- MSTest

## Estructura

```
Sisorg.sln
├─ Sisorg.Api/                
│  ├─ Controllers/
│  ├─ Domain/Entities/
│  ├─ Infrastructure/         
│  ├─ Middleware/             
│  ├─ Repositories/           
│  ├─ Services/               
│  ├─ ViewModels/
│  ├─ appsettings*.json
│  └─ Sisorg.Api.csproj
└─ Sisorg.Tests/              
   └─ Sisorg.Tests.csproj
```

## Requisitos

- .NET SDK 8.x
- Docker y Docker Compose

## Configuración

### 1 Base de datos (Docker)

Arranca MySQL:

```bash
docker compose up -d
```

### 2 Connection string

En `Sisorg.Api/appsettings.json` :

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=sisorg_db;User=sisorg_user;Password=sisorg_pass;TreatTinyAsBoolean=false"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:5173" ]
  }
}
```

### 3 Migraciones

Desde la carpeta `Sisorg.Api/`:

```bash
dotnet ef database update --context AppDbContext
```

```bash
dotnet ef migrations add InitialCreate --context AppDbContext
dotnet ef database update --context AppDbContext
```

## Ejecución

Desde `Sisorg.Api/`:

```bash
dotnet run
```

Swagger UI:

```
http://localhost:5196/swagger
```
## Endpoints

### POST `/api/registries`

- `multipart/form-data`, clave: `file`
- Cada línea del .txt: `Name#Value#Color`
  - `Name`: letras y números (regex `^[A-Za-z0-9]+$`)
  - `Value`: dígitos (se guarda como decimal)
  - `Color`: HEX de 6 dígitos sin `#` (ej: `FF0000`)

Ejemplo de archivo:

```
Argentina#1500#FF0000
Brazil#4005#00FF00
```

`curl`:

```bash
curl -X POST http://localhost:5196/api/registries   -F "file=@data.txt"
```

Respuesta 200:

```json
{
  "ID": 1,
  "Count": 2,
  "Timestamp": "2025-10-06T15:40:00Z",
  "Rows": [
    { "Name": "Argentina", "Value": 1500, "Color": "FF0000" },
    { "Name": "Brazil",    "Value": 4005, "Color": "00FF00" }
  ]
}
```

### GET `/api/registries/{id}`

```bash
curl http://localhost:5196/api/registries/1
```

### DELETE `/api/registries/{id}`

```bash
curl -X DELETE http://localhost:5196/api/registries/1 -i
```

## Manejo de errores

El middleware de errores devuelve JSON consistente:

```json
{
  "Code": "VALIDATION_ERROR | INTERNAL_ERROR",
  "Message": "detalle",
  "CorrelationId": "GUID"
}
```

HTTP 400 para errores de validación (formato/regex), 500 para inesperados.

## Tests

Ejecuta todos:

```bash
dotnet test
```

Solo unitarios :

```bash
dotnet test --filter TestCategory!=Integration
```

Los tests actuales no dependen de MySQL .  
Si agregas **integration tests** reales contra MySQL, primero asegurá el contenedor arriba:

```bash
docker compose up -d
dotnet test --filter TestCategory=Integration
```

## Swagger (resúmenes/summary)

- En `Sisorg.Api.csproj`: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- En `Program.cs`: `IncludeXmlComments(...)`

## Comandos útiles

```bash
dotnet build
dotnet restore

dotnet ef migrations add <Nombre> --context AppDbContext
dotnet ef database update --context AppDbContext
```

## Troubleshooting

- `No project was found` con `dotnet ef`: ejecuta los comandos EF desde `Sisorg.Api/` o usa `--project Sisorg.Api/Sisorg.Api.csproj`.
- `Couldn't connect to server`: contenedor MySQL apagado o puerto/credenciales distintas al connection string.
- Swagger sin summaries: verificá que el `.xml` exista en `bin/Debug/net8.0/` y que `IncludeXmlComments` apunte a ese archivo.
