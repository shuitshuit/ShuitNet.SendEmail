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

            #region rootOption
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
            #endregion

            #region addUserCommand
            var addUser = new Command("add-user", "Add a user. " +
                "Administrative privileges required");

            var addUserEmail = new Argument<string>("email", "The email address of the user");
            addUser.AddArgument(addUserEmail);

            var addUserSmtpServer = new Option<string>(["--smtp-server", "-s"], "The SMTP server to use");
            addUser.AddOption(addUserSmtpServer);

            var addUserSmtpPort = new Option<int>(["--smtp-port", "-p"], "The port of the SMTP server to use");
            addUser.AddOption(addUserSmtpPort);

            var addUserSmtpSsl = new Option<bool>(["--smtp-ssl", "-l"], "Whether to use SSL for the SMTP server");
            addUser.AddOption(addUserSmtpSsl);

            var addUserSmtpUsername = new Option<string>(["--smtp-username", "-n"], "The name of the user");
            addUser.AddOption(addUserSmtpUsername);

            var addUserSmtpPassword = new Option<string>(["--smtp-password", "-w"], "The password of the user");
            addUser.AddOption(addUserSmtpPassword);
            #endregion

            var parseResult = rootCommand.Parse(args);
            var from = parseResult.GetValueForOption(fromOption) ?? "";

            serviceCollection.AddTransient<Settings>(x => new Settings(from));
            serviceCollection.AddTransient<SendEmail>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var addUserResult = addUser.Parse(args);
            addUser.SetHandler(async (x) =>
            {
                try
                {
                    await Settings.AddUserAsync(
                        addUserResult.GetValueForArgument(addUserEmail)!,
                        addUserResult.GetValueForOption(addUserSmtpServer)!,
                        addUserResult.GetValueForOption(addUserSmtpPort)!,
                        addUserResult.GetValueForOption(addUserSmtpSsl)!,
                        addUserResult.GetValueForOption(addUserSmtpUsername)!,
                        addUserResult.GetValueForOption(addUserSmtpPassword)!
                    );
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    x.ExitCode = 1;
                }
                catch (Exception)
                {
                    Console.WriteLine("An error occurred while adding the user");
                    x.ExitCode = 1;
                }
            });
            rootCommand.AddCommand(addUser);

            rootCommand.SetHandler(async (x) =>
            {
                var sendEmail = serviceProvider.GetRequiredService<SendEmail>();
                if (string.IsNullOrEmpty(from))
                {
                    Console.WriteLine("The --from option is required");
                    x.ExitCode = 1;
                    return;
                }
                try
                {
                    await sendEmail.SendEmailAsync(
                        parseResult.GetValueForOption(toOption),
                        parseResult.GetValueForOption(subjectOption),
                        parseResult.GetValueForOption(bodyOption),
                        parseResult.GetValueForOption(smtpServerOption),
                        parseResult.GetValueForOption(smtpUsernameOption),
                        parseResult.GetValueForOption(smtpPasswordOption),
                        from,
                        parseResult.GetValueForOption(fromNameOption),
                        parseResult.GetValueForOption(ccOption),
                        parseResult.GetValueForOption(bccOption),
                        parseResult.GetValueForOption(attachmentOption),
                        parseResult.GetValueForOption(smtpSslOption),
                        parseResult.GetValueForOption(smtpPortOption)
                    );
                    Console.WriteLine("Email sent successfully");
                }
                catch (SendException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"State: {ex.State}");
                    x.ExitCode = 1;
                }
                catch (Exception)
                {
                    Console.WriteLine("An error occurred while sending the email");
                    x.ExitCode = 1;
                }
            });

            Console.WriteLine($"Exit code: {rootCommand.Invoke(args)}");
        }
    }
}
