FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Worker/Worker.csproj", "Worker/"]
RUN dotnet restore "./Worker/Worker.csproj"

COPY Worker/ ./Worker/
COPY Common/ ./Common/

WORKDIR "/src/Worker"
RUN dotnet publish "./Worker.csproj" -c Release -o /app/publish/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "Worker.dll" ]
