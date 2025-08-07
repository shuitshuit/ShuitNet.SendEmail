using System.Text.Json;
using System.Text.RegularExpressions;

namespace ShuitNet.SendEmail
{
    public class TemplateManager
    {
        private readonly string _templatesPath;

        public TemplateManager()
        {
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _templatesPath = Path.Combine(userHome, ".shuitNet", "sendemail", "templates");
            
            if (!Directory.Exists(_templatesPath))
            {
                Directory.CreateDirectory(_templatesPath);
            }
        }

        public async Task SaveTemplateAsync(EmailTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new ArgumentException("Template name cannot be empty");

            var fileName = GetSafeFileName(template.Name) + ".json";
            var filePath = Path.Combine(_templatesPath, fileName);
            
            template.Variables = ExtractVariables(template.Subject + " " + template.Body);
            template.Modified = DateTime.Now;
            
            var json = JsonSerializer.Serialize(template, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<EmailTemplate?> LoadTemplateAsync(string templateName)
        {
            var fileName = GetSafeFileName(templateName) + ".json";
            var filePath = Path.Combine(_templatesPath, fileName);
            
            if (!File.Exists(filePath))
                return null;
                
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<EmailTemplate>(json);
        }

        public async Task<List<EmailTemplate>> ListTemplatesAsync()
        {
            var templates = new List<EmailTemplate>();
            var jsonFiles = Directory.GetFiles(_templatesPath, "*.json");
            
            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var template = JsonSerializer.Deserialize<EmailTemplate>(json);
                    if (template != null)
                        templates.Add(template);
                }
                catch
                {
                    // Skip invalid template files
                }
            }
            
            return templates.OrderBy(t => t.Name).ToList();
        }

        public Task<bool> DeleteTemplateAsync(string templateName)
        {
            var fileName = GetSafeFileName(templateName) + ".json";
            var filePath = Path.Combine(_templatesPath, fileName);
            
            if (!File.Exists(filePath))
                return Task.FromResult(false);
                
            File.Delete(filePath);
            return Task.FromResult(true);
        }

        public string ProcessTemplate(EmailTemplate template, Dictionary<string, string> variables)
        {
            var subject = template.Subject;
            var body = template.Body;
            
            foreach (var kvp in variables)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                subject = subject.Replace(placeholder, kvp.Value);
                body = body.Replace(placeholder, kvp.Value);
            }
            
            return $"Subject: {subject}\nBody: {body}";
        }

        public (string Subject, string Body) ProcessTemplateContent(EmailTemplate template, Dictionary<string, string> variables)
        {
            var subject = template.Subject;
            var body = template.Body;
            
            foreach (var kvp in variables)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                subject = subject.Replace(placeholder, kvp.Value);
                body = body.Replace(placeholder, kvp.Value);
            }
            
            return (subject, body);
        }

        private List<string> ExtractVariables(string text)
        {
            var regex = new Regex(@"\{\{([^}]+)\}\}");
            var matches = regex.Matches(text);
            return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
        }

        private string GetSafeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}