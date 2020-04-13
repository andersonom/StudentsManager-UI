FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8088
EXPOSE 8088

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["StudentsManager.UI.csproj", "./"]
RUN dotnet restore "StudentsManager.UI.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "StudentsManager.UI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "StudentsManager.UI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "StudentsManager.UI.dll"]
