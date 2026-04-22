FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/GoodHamburger.Api/GoodHamburger.Api.csproj", "src/GoodHamburger.Api/"]
COPY ["src/GoodHamburger.Application/GoodHamburger.Application.csproj", "src/GoodHamburger.Application/"]
COPY ["src/GoodHamburger.Domain/GoodHamburger.Domain.csproj", "src/GoodHamburger.Domain/"]
COPY ["src/GoodHamburger.Infrastructure/GoodHamburger.Infrastructure.csproj", "src/GoodHamburger.Infrastructure/"]

RUN dotnet restore "src/GoodHamburger.Api/GoodHamburger.Api.csproj"

COPY . .
RUN dotnet publish "src/GoodHamburger.Api/GoodHamburger.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "GoodHamburger.Api.dll"]
