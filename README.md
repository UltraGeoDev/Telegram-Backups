# Telegram Backups

## Version 0.1.0

> [!WARNING]
> the project is at the testing stage
> if you find an error, write to [issues](https://github.com/UltraGeoDev/Telegram-Backups/issues)

Planned to add:
1. Pulling all types of messages
2. Ability to export the history of several chats
3. Saving to disk
4. Statistics collection

## Startup

1. add `App.config` to `src/Telegram Backups`

### example
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <appSettings>
        <add key="api_id" value="<YOUR_API_ID>" />
        <add key="api_hash" value="<YOUR_API_HASH>" />
        <add key="phone_number" value="<YOUR_PHONE_NUMBER>" />
        <add key="first_name" value="<YOUR_FIRST_NAME>" />
        <add key="last_name" value="<YOUR_LAST_NAME>" />
        <add key="password" value="<YOUR_2FA_PASSWORD>" />
        <add key="token" value="<YOUR_TOKEN>" />
        <add key="limit" value="100" />
    </appSettings>
</configuration>
```

explanation:
- `api_id` - Telegram API ID
- `api_hash` - Telegram API Hash
- `phone_number` - Telegram phone number
- `first_name` - Telegram first name
- `last_name` - Telegram last name
- `password` - Telegram 2FA password
- `token` - yandex disk oauth token
- `limit` - messages saved per chat

2. add `chats.json` to `src/Telegram Backups/data`

### example
```json
{
    "chats": [
        {
            "name": "Jack",
            "id": 000000000
        },
        {
            "name": "Jill",
            "id": 000000001
        }
    ],
    "total":2
}
```

3. build and run image
```bash
docker build -t telegram-backups:latest src/Telegram\ Backups
docker container run --rm -it telegram-backups:latest bash 
```

then you must follow the instructions in the terminal.

---
created by [UltraGeoPro](https://github.com/Ultrageopro1966)