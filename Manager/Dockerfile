FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Manager/Manager.csproj", "Manager/"]
RUN dotnet restore "./Manager/Manager.csproj"

COPY Manager/ ./Manager/
COPY Common/ ./Common/

WORKDIR "/src/Manager"
RUN dotnet publish "./Manager.csproj" -c Release -o /app/publish/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "Manager.dll" ]
