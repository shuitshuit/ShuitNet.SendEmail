using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ShuitNet.SendEmail
{
    public class Settings : UserSetting
    {
        private readonly string _filePath;

        public Settings(string address)
        {
            var zone = address.Split('@')[1];
            var basePath = GetBasePath();
            _filePath = Path.Combine(basePath, $"{zone}.json");

            var config = LoadConfig(address);
            SmtpServer = config.SmtpServer;
            SmtpPort = config.SmtpPort;
            SmtpUsername = config.SmtpUsername;
            SmtpPassword = config.SmtpPassword;
            SmtpSsl = config.SmtpSsl;
        }

        private static string GetBasePath() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Program Files\shuitNet\sendemail\conf.d"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "/etc/shuitNet/sendemail/conf.d"
                    : throw new PlatformNotSupportedException();

        private UserSetting LoadConfig(string address)
        {
            var file = File.ReadAllText(_filePath);
            Console.WriteLine(file);
            var json = JsonSerializer.Deserialize<JsonElement>(file);
            var userSettings = json.GetProperty(address);
            return new UserSetting(
                userSettings.GetProperty("smtp-server").GetString()!,
                userSettings.GetProperty("smtp-port").GetInt32(),
                userSettings.GetProperty("smtp-ssl").GetBoolean(),
                userSettings.GetProperty("smtp-username").GetString()!,
                userSettings.GetProperty("smtp-password").GetString()!);
        }

        public static async Task AddUserAsync(string email, string smtpServer,
            int smtpPort, bool smtpSsl, string smtpUsername, string smtpPassword)
        {
            var basePath = GetBasePath();
            var zone = email.Split('@')[1];
            var filePath = Path.Combine(basePath, $"{zone}.json");
            if (!File.Exists(filePath))
            {
                var stream = File.Create(filePath);
                stream.Close();
            }

            var userSettings = new UserSetting(smtpServer, smtpPort, smtpSsl,
                smtpUsername, smtpPassword);
            var json = JsonSerializer.Deserialize<Dictionary<string, UserSetting>>
                (await File.ReadAllTextAsync(filePath))!;
            if (!json.TryAdd(email, userSettings))
                throw new InvalidOperationException("The user already exists");
            var newSetting = JsonSerializer.Serialize(json, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(filePath, newSetting);
        }
    }
}
