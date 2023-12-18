# Face recognition DBContext

## Creo un progetto classe
dotnet new classlib

## Aggiungo EF Core per connettermi al DB
dotnet add package Microsoft.EntityFrameworkCore Microsoft.EntityFrameworkCore.Sqlite Microsoft.EntityFrameworkCore.Design

## Faccio lo scaffolding del DB
dotnet ef dbcontext scaffold 'Datasource="face.db"' Microsoft.EntityFrameworkCore.Sqlite