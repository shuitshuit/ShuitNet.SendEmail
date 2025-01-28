using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ShuitNet.SendEmail
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
#if DEBUG
            args = new string[]
            {
                "--to", "mail@shuit.net",
                "--subject", "Test",
                "--body", "This is a test email",
                "--from", "shuit@shuit.net"
            };
            Console.WriteLine("Debug mode------------------------------");
            Console.WriteLine("args: " + string.Join(" ", args));
            Console.WriteLine("----------------------------------------");
            serviceCollection.AddLogging(builder =>
            {
                builder.AddNLog("nlog.config");
                builder.SetMinimumLevel(LogLevel.Debug);
            });
#else
            serviceCollection.AddLogging(builder =>
            {
                builder.AddNLog("nlog.config");
                builder.SetMinimumLevel(LogLevel.Information);
            });
#endif


            serviceCollection.AddSingleton<SendEmail>();

            var rootCommand = new RootCommand("Send an email");
            var toOption = new Option<string>("--to",
                "The email address of the recipient");
            rootCommand.AddOption(toOption);

            var fromOption = new Option<string>("--from",
                "The email address of the sender");
            rootCommand.AddOption(fromOption);

            var subjectOption = new Option<string>(["--subject", "-s"],
                "The subject of the email");
            rootCommand.AddOption(subjectOption);

            var bodyOption = new Option<string>(["--body", "-b"],
                "The body of the email");
            rootCommand.AddOption(bodyOption);

            var smtpServerOption = new Option<string>("--smtp-server",
                "The SMTP server to use");
            rootCommand.AddOption(smtpServerOption);

            var smtpPortOption = new Option<int>("--smtp-port",
                "The port of the SMTP server to use");
            rootCommand.AddOption(smtpPortOption);

            var smtpUsernameOption = new Option<string>("--smtp-username",
                "The username to use for the SMTP server");
            rootCommand.AddOption(smtpUsernameOption);

            var smtpPasswordOption = new Option<string>("--smtp-password",
                "The password to use for the SMTP server");
            rootCommand.AddOption(smtpPasswordOption);

            var smtpSslOption = new Option<bool>("--smtp-ssl",
                "Whether to use SSL for the SMTP server");
            rootCommand.AddOption(smtpSslOption);

            var fromNameOption = new Option<string>("--from-name",
                "The name of the sender");
            rootCommand.AddOption(fromNameOption);

            var ccOption = new Option<string>("--cc",
                "The email address of the CC recipient");
            rootCommand.AddOption(ccOption);

            var bccOption = new Option<string>("--bcc",
                "The email address of the BCC recipient");
            rootCommand.AddOption(bccOption);

            var attachmentOption = new Option<string>("--attachment",
                "The path to the attachment");
            rootCommand.AddOption(attachmentOption);

            var parseResult = rootCommand.Parse(args);
            var from = parseResult.GetValueForOption<string>(fromOption);

            serviceCollection.AddTransient<Settings>(x => new Settings(from));
            serviceCollection.AddTransient<SendEmail>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            rootCommand.SetHandler(async () =>
            {
                var sendEmail = serviceProvider.GetRequiredService<SendEmail>();

                try
                {
                    await sendEmail.SendEmailAsync(
                        parseResult.GetValueForOption<string>(toOption),
                        parseResult.GetValueForOption<string>(subjectOption),
                        parseResult.GetValueForOption<string>(bodyOption),
                        parseResult.GetValueForOption<string>(smtpServerOption),
                        parseResult.GetValueForOption<string>(smtpUsernameOption),
                        parseResult.GetValueForOption<string>(smtpPasswordOption),
                        from,
                        parseResult.GetValueForOption<string>(fromNameOption),
                        parseResult.GetValueForOption<string>(ccOption),
                        parseResult.GetValueForOption<string>(bccOption),
                        parseResult.GetValueForOption<string>(attachmentOption),
                        parseResult.GetValueForOption<bool>(smtpSslOption),
                        parseResult.GetValueForOption<int>(smtpPortOption)
                    );
                    Console.WriteLine("Email sent successfully");
                }
                catch (SendException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"State: {ex.State}");
                }
            });

            int code = rootCommand.Invoke(args);
            Console.WriteLine($"Exit code: {code}");
        }
    }
}
