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

        private static string GetBasePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "C:\\Program Files\\shuitNet\\sendEmail\\conf.d";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "/etc/shuitNet/sendEmail/conf.d";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        private UserSetting LoadConfig(string address)
        {
            var file = File.ReadAllText(_filePath);
            Console.WriteLine(file);
            var json = JsonSerializer.Deserialize<JsonElement>(file);
            var userSettings = json.GetProperty(address);
            return new UserSetting(
                userSettings.GetProperty("smtpServer").GetString()!,
                userSettings.GetProperty("smtpPort").GetInt32(),
                userSettings.GetProperty("smtpSsl").GetBoolean(),
                userSettings.GetProperty("smtpUsername").GetString()!,
                userSettings.GetProperty("smtpPassword").GetString()!);
        }
    }
}
