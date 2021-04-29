FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Discounting.API/Discounting.API.csproj", "Discounting.API/"]
COPY ["Discounting.Common/Discounting.Common.csproj", "Discounting.Common/"]
COPY ["Discounting.Helpers/Discounting.Helpers.csproj", "Discounting.Helpers/"]
COPY ["Discounting.Extensions/Discounting.Extensions.csproj", "Discounting.Extensions/"]
COPY ["Discounting.Logics/Discounting.Logics.csproj", "Discounting.Logics/"]
COPY ["Discounting.Data/Discounting.Data.csproj", "Discounting.Data/"]
COPY ["Discounting.Entities/Discounting.Entities.csproj", "Discounting.Entities/"]
COPY ["Discounting.API.Common/Discounting.API.Common.csproj", "Discounting.API.Common/"]
COPY ["Discounting.Seeding/Discounting.Seeding.csproj", "Discounting.Seeding/"]
RUN dotnet restore "Discounting.API/Discounting.API.csproj"
COPY . .
WORKDIR "/src/Discounting.API"
RUN dotnet build "Discounting.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Discounting.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "Discounting.API.dll"]
