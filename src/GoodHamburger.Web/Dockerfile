FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/GoodHamburger.Web/GoodHamburger.Web.csproj", "src/GoodHamburger.Web/"]
COPY ["src/GoodHamburger.Domain/GoodHamburger.Domain.csproj", "src/GoodHamburger.Domain/"]

RUN dotnet restore "src/GoodHamburger.Web/GoodHamburger.Web.csproj"

COPY . .
RUN dotnet publish "src/GoodHamburger.Web/GoodHamburger.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "GoodHamburger.Web.dll"]
