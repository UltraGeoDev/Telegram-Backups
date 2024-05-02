using System.Configuration;
using Backups;
using Microsoft.Extensions.Logging;
using WTelegram;

class Program
{
    private readonly string? AppId = ConfigurationManager.AppSettings.Get("api_id");
    private readonly string? AppHash = ConfigurationManager.AppSettings.Get("api_hash");
    private readonly string? PhoneNumber = ConfigurationManager.AppSettings.Get("phone_number");
    private readonly string? FirstName = ConfigurationManager.AppSettings.Get("first_name");
    private readonly string? LastName = ConfigurationManager.AppSettings.Get("last_name");
    private readonly string? Password = ConfigurationManager.AppSettings.Get("password");
    private readonly int limit = int.Parse(ConfigurationManager.AppSettings.Get("limit")!);
    private readonly string? token = ConfigurationManager.AppSettings.Get("token");

    public string? Configs(string what)
    {
        switch (what)
        {
            case "api_id":
                return AppId;
            case "api_hash":
                return AppHash;
            case "phone_number":
                return PhoneNumber;
            case "verification_code":
                Console.Write("Code: ");
                return Console.ReadLine();
            case "first_name":
                return FirstName;
            case "last_name":
                return LastName;
            case "password":
                return Password;
            default:
                return null;
        }
    }

    public async Task RunBackup(string[] args)
    {
        // Create logger
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Program");

        // Create telegram client and connect
        using var client = new Client(Configs);

        // Create backup instance
        var backup = new CreateBackup(client, logger, token, limit);

        // Create backup
        logger.LogInformation("Starting backup...");
        await backup.Create();
        logger.LogInformation("Backup finished");
    }

    static async Task Main(string[] args)
    {
        // Start program
        var program = new Program();
        await program.RunBackup(args);
    }
}
