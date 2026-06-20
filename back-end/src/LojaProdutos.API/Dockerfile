FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/LojaProdutos.Domain/LojaProdutos.Domain.csproj src/LojaProdutos.Domain/
COPY src/LojaProdutos.Application/LojaProdutos.Application.csproj src/LojaProdutos.Application/
COPY src/LojaProdutos.Infrastructure/LojaProdutos.Infrastructure.csproj src/LojaProdutos.Infrastructure/
COPY src/LojaProdutos.API/LojaProdutos.API.csproj src/LojaProdutos.API/
COPY LojaProdutos.sln .

RUN dotnet restore src/LojaProdutos.API/LojaProdutos.API.csproj

COPY . .
RUN dotnet publish src/LojaProdutos.API/LojaProdutos.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LojaProdutos.API.dll"]
