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
            serviceCollection.AddSingleton<TemplateManager>();

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

            var templateOption = new Option<string>("--template",
                "Use template for email content");
            rootCommand.AddOption(templateOption);

            var varsOption = new Option<string>("--vars",
                "Template variables (format: key1=value1,key2=value2)");
            rootCommand.AddOption(varsOption);
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

            #region templateCommands
            var templateCommand = new Command("template", "Manage email templates");

            var templateCreateCommand = new Command("create", "Create a new email template");
            var templateNameArg = new Argument<string>("name", "Template name");
            templateCreateCommand.AddArgument(templateNameArg);
            var templateSubjectOption = new Option<string>(["--subject", "-s"], "Template subject");
            templateCreateCommand.AddOption(templateSubjectOption);
            var templateBodyOption = new Option<string>(["--body", "-b"], "Template body");
            templateCreateCommand.AddOption(templateBodyOption);

            var templateListCommand = new Command("list", "List all templates");

            var templateDeleteCommand = new Command("delete", "Delete a template");
            var templateDeleteNameArg = new Argument<string>("name", "Template name to delete");
            templateDeleteCommand.AddArgument(templateDeleteNameArg);

            templateCommand.AddCommand(templateCreateCommand);
            templateCommand.AddCommand(templateListCommand);
            templateCommand.AddCommand(templateDeleteCommand);
            #endregion

            var parseResult = rootCommand.Parse(args);

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

            templateCreateCommand.SetHandler(async (x) =>
            {
                try
                {
                    var templateManager = serviceProvider.GetRequiredService<TemplateManager>();
                    var templateCreateResult = templateCreateCommand.Parse(args);
                    
                    var template = new EmailTemplate
                    {
                        Name = templateCreateResult.GetValueForArgument(templateNameArg)!,
                        Subject = templateCreateResult.GetValueForOption(templateSubjectOption) ?? "",
                        Body = templateCreateResult.GetValueForOption(templateBodyOption) ?? ""
                    };
                    
                    await templateManager.SaveTemplateAsync(template);
                    Console.WriteLine($"Template '{template.Name}' created successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating template: {ex.Message}");
                    x.ExitCode = 1;
                }
            });

            templateListCommand.SetHandler(async (x) =>
            {
                try
                {
                    var templateManager = serviceProvider.GetRequiredService<TemplateManager>();
                    var templates = await templateManager.ListTemplatesAsync();
                    
                    if (templates.Count == 0)
                    {
                        Console.WriteLine("No templates found");
                        return;
                    }
                    
                    Console.WriteLine("Available templates:");
                    foreach (var template in templates)
                    {
                        Console.WriteLine($"  {template.Name}");
                        if (template.Variables.Count > 0)
                        {
                            Console.WriteLine($"    Variables: {string.Join(", ", template.Variables)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error listing templates: {ex.Message}");
                    x.ExitCode = 1;
                }
            });

            templateDeleteCommand.SetHandler(async (x) =>
            {
                try
                {
                    var templateManager = serviceProvider.GetRequiredService<TemplateManager>();
                    var templateDeleteResult = templateDeleteCommand.Parse(args);
                    var templateName = templateDeleteResult.GetValueForArgument(templateDeleteNameArg)!;
                    
                    var success = await templateManager.DeleteTemplateAsync(templateName);
                    if (success)
                    {
                        Console.WriteLine($"Template '{templateName}' deleted successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Template '{templateName}' not found");
                        x.ExitCode = 1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting template: {ex.Message}");
                    x.ExitCode = 1;
                }
            });

            rootCommand.AddCommand(templateCommand);

            rootCommand.SetHandler(async (x) =>
            {
                try
                {
                    var from = parseResult.GetValueForOption(fromOption) ?? "";
                    if (string.IsNullOrEmpty(from))
                    {
                        await rootCommand.InvokeAsync(["--help"]);
                        return;
                    }
                    serviceCollection.AddTransient<Settings>(l => new Settings(from));
                    serviceProvider = serviceCollection.BuildServiceProvider();
                    var sendEmail = serviceProvider.GetRequiredService<SendEmail>();
                    var templateManager = serviceProvider.GetRequiredService<TemplateManager>();

                    var subject = parseResult.GetValueForOption(subjectOption);
                    var body = parseResult.GetValueForOption(bodyOption);
                    var templateName = parseResult.GetValueForOption(templateOption);
                    var varsString = parseResult.GetValueForOption(varsOption);

                    if (!string.IsNullOrEmpty(templateName))
                    {
                        try
                        {
                            var template = await templateManager.LoadTemplateAsync(templateName);
                            if (template == null)
                            {
                                Console.WriteLine($"Template '{templateName}' not found");
                                x.ExitCode = 1;
                                return;
                            }

                            var variables = new Dictionary<string, string>();
                            if (!string.IsNullOrEmpty(varsString))
                            {
                                foreach (var pair in varsString.Split(','))
                                {
                                    var parts = pair.Split('=', 2);
                                    if (parts.Length == 2)
                                    {
                                        variables[parts[0].Trim()] = parts[1].Trim();
                                    }
                                }
                            }

                            var (templateSubject, templateBody) = templateManager.ProcessTemplateContent(template, variables);
                            subject = string.IsNullOrEmpty(subject) ? templateSubject : subject;
                            body = string.IsNullOrEmpty(body) ? templateBody : body;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing template: {ex.Message}");
                            x.ExitCode = 1;
                            return;
                        }
                    }

                    try
                    {
                        await sendEmail.SendEmailAsync(
                            parseResult.GetValueForOption(toOption),
                            subject,
                            body,
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
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    x.ExitCode = 1;
                }
                catch (Exception)
                {
                    Console.WriteLine("An error occurred while processing the command");
                    x.ExitCode = 1;
                }
            });

            Console.WriteLine($"Exit code: {rootCommand.Invoke(args)}");
        }
    }
}
