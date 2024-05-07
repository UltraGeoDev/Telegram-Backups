FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o published
RUN ["chmod", "+x", "published/Telegram Backups.dll"]

ENTRYPOINT ["dotnet", "published/Telegram Backups.dll"]