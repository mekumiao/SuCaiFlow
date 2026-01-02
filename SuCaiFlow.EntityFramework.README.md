# SuCaiFlow Entity Framework Core Integration

This document describes the new Entity Framework Core integration pattern for SuCaiFlow, inspired by OpenIddict's flexible database configuration approach.

## Overview

The new EF Core integration provides a flexible, database-agnostic configuration pattern that allows SuCaiFlow to work with multiple database providers without hardcoding dependencies.

## Key Features

- **Database Agnostic**: Works with any EF Core provider (SQL Server, PostgreSQL, MySQL, SQLite, etc.)
- **Flexible Configuration**: Uses a builder pattern similar to OpenIddict
- **No Hardcoded Dependencies**: Removed direct dependency on PostgreSQL provider
- **Repository Pattern**: Maintains the existing repository pattern for data access

## Configuration Pattern

The new configuration follows a pattern similar to OpenIddict:

```csharp
// Register SuCaiFlow services
builder.Services.AddSuCaiFlow()
    .UseEntityFrameworkCore()
    .UseDbContext<SuCaiFlowDbContext>();

// Configure your chosen database provider
builder.Services.AddDbContext<SuCaiFlowDbContext>(options =>
    options.UseSqlServer(connectionString) // or UseNpgsql, UseMySql, etc.
           .UseSuCaiFlow()); // Apply SuCaiFlow-specific configurations
```

## Extension Methods

### AddSuCaiFlowEntityFrameworkCore()

Initializes SuCaiFlow Entity Framework Core integration.

### UseEntityFrameworkCore()

Configures SuCaiFlow to use Entity Framework Core as the backing store.

### UseDbContext<TContext>()

Specifies the DbContext type to use with SuCaiFlow.

### UseSuCaiFlow()

Configures Entity Framework Core to work with SuCaiFlow entities.

### AddSuCaiFlowEntityFrameworkStores()

Registers the SuCaiFlow repositories.

## Entity Configuration Pattern

SuCaiFlow now uses the `IEntityTypeConfiguration<T>` interface to define entity mappings, following EF Core best practices:

- `CollectionTaskConfiguration` - Configures the CollectionTask entity
- `CollectionTaskConfigConfiguration` - Configures the CollectionTaskConfig entity  
- `CollectedAssetConfiguration` - Configures the CollectedAsset entity

This approach allows the library to be used with any DbContext implementation without being tied to a specific one.

## Migration from Previous Versions

If you were using the old configuration:

**Before:**

```csharp
services.AddSuCaiFlowDbContext<SuCaiFlowDbContext>(connectionString); // Hardcoded to PostgreSQL
services.AddSuCaiFlowRepositories();
```

**After:**

```csharp
services.AddSuCaiFlow()
    .UseEntityFrameworkCore()
    .UseDbContext<SuCaiFlowDbContext>();

services.AddDbContext<SuCaiFlowDbContext>(options =>
    options.UseSqlServer(connectionString) // Choose your database provider
           .UseSuCaiFlow());
```

## Supported Database Providers

- SQL Server: `Microsoft.EntityFrameworkCore.SqlServer`
- PostgreSQL: `Npgsql.EntityFrameworkCore.PostgreSQL`
- MySQL: `Pomelo.EntityFrameworkCore.MySql`
- SQLite: `Microsoft.EntityFrameworkCore.Sqlite`
- In-Memory: `Microsoft.EntityFrameworkCore.InMemory`
- And any other EF Core provider

## Benefits

1. **Flexibility**: Choose any database provider that supports EF Core
2. **Maintainability**: No hardcoded database dependencies
3. **Testability**: Easy to switch between providers for testing
4. **Scalability**: Supports different database technologies as needed

## Example Usage

```csharp
// Program.cs or Startup.cs

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole();

// Add SuCaiFlow core services
builder.Services.AddSuCaiFlow();

// Add SuCaiFlow Entity Framework Core integration
builder.Services.AddSuCaiFlowEntityFrameworkCore()
    .UseEntityFrameworkCore()
    .UseDbContext<SuCaiFlowDbContext>();

// Configure your chosen database provider
builder.Services.AddDbContext<SuCaiFlowDbContext>(options =>
    options.UseInMemoryDatabase("SuCaiFlowDemo") // For demo purposes
           .UseSuCaiFlow());

// Register other services
builder.Services.AddScoped<ISiteCollector, ExampleSiteCollector>();

var app = builder.Build();
```
