namespace OpenRP.Boilerplate.Configuration
{
    public class Config
    {
        public string ConnectionString { get; set; }
        public string OpenAI_API { get; set; }
        // The model AI actors reply with; empty uses the Framework's default.
        public string OpenAI_Model { get; set; }
        // Optional: "none", "low", "medium" or "high". Empty keeps the features' default.
        public string OpenAI_ReasoningEffort { get; set; }
        public string DiscordBot_Token { get; set; }

        public Config()
        {
            ConnectionString = "server=localhost;user=root;password=;database=openrp";
            OpenAI_API = String.Empty;
            OpenAI_Model = String.Empty;
            OpenAI_ReasoningEffort = String.Empty;
            DiscordBot_Token = String.Empty;
        }
    }
}
