using System.Text.Json.Serialization;

namespace ShuitNet.SendEmail
{
    public class EmailTemplate
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("variables")]
        public List<string> Variables { get; set; } = new List<string>();

        [JsonPropertyName("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        [JsonPropertyName("modified")]
        public DateTime Modified { get; set; } = DateTime.Now;
    }
}