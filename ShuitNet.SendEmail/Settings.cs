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
            if (string.IsNullOrWhiteSpace(address))
            {
                SmtpPassword = string.Empty;
                SmtpServer = string.Empty;
                SmtpPort = 0;
                SmtpUsername = string.Empty;
                SmtpSsl = false;
                _filePath = string.Empty;
                return;
            }
            var zone = address.Split('@')[1];
            var basePath = GetBasePath();
            _filePath = Path.Combine(basePath, $"{zone}.json");

            EnsureDirectoryExists(basePath);

            var config = LoadConfig(address);
            SmtpServer = config.SmtpServer;
            SmtpPort = config.SmtpPort;
            SmtpUsername = config.SmtpUsername;
            SmtpPassword = PasswordManager.DecryptPassword(config.SmtpPassword);
            SmtpSsl = config.SmtpSsl;
        }

        private static string GetBasePath()
        {
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userHome, ".shuitNet", "sendemail", "conf.d");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private UserSetting LoadConfig(string address)
        {
            try
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
            catch (FileNotFoundException)
            {
                throw new InvalidOperationException("Configuration file not found. Please add a user first.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Error parsing configuration file.", ex);
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException("User not found in configuration file.");
            }
        }

        public static async Task AddUserAsync(string email, string smtpServer,
            int smtpPort, bool smtpSsl, string smtpUsername, string smtpPassword)
        {
            var basePath = GetBasePath();
            EnsureDirectoryExists(basePath);
            var zone = email.Split('@')[1];
            var filePath = Path.Combine(basePath, $"{zone}.json");
            var encryptedPassword = PasswordManager.EncryptPassword(smtpPassword);
            var userSettings = new UserSetting(smtpServer, smtpPort, smtpSsl,
                smtpUsername, encryptedPassword);

            Dictionary<string, UserSetting> json;
            if (!File.Exists(filePath) || string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(filePath)))
            {
                json = new Dictionary<string, UserSetting>();
            }
            else
            {
                json = JsonSerializer.Deserialize<Dictionary<string, UserSetting>>
                    (await File.ReadAllTextAsync(filePath))!;
            }
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
