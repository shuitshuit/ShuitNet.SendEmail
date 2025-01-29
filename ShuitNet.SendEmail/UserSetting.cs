using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShuitNet.SendEmail
{
    public class UserSetting
    {
        [JsonPropertyName("smtp-server")]
        public string SmtpServer { get; set; }

        [JsonPropertyName("smtp-port")]
        public int SmtpPort { get; set; }

        [JsonPropertyName("smtp-ssl")]
        public bool SmtpSsl { get; set; }

        [JsonPropertyName("smtp-username")]
        public string SmtpUsername { get; set; }

        [JsonPropertyName("smtp-password")]
        public string SmtpPassword { get; set; }

        public UserSetting()
        {
            SmtpServer = "smtp.example.com";
            SmtpPort = 587;
            SmtpSsl = true;
            SmtpUsername = "user";
            SmtpPassword = "password";
        }

        public UserSetting(string smtpServer, int smtpPort, bool smtpSsl,
            string smtpUsername, string smtpPassword)
        {
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            SmtpSsl = smtpSsl;
            SmtpUsername = smtpUsername;
            SmtpPassword = smtpPassword;
        }
    }
}
