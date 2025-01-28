using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuitNet.SendEmail
{
    public class UserSetting
    {
        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public bool SmtpSsl { get; set; }

        public string SmtpUsername { get; set; }

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
