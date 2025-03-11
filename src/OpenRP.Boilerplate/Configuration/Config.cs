namespace OpenRP.Boilerplate.Configuration
{
    public class Config
    {
        public string ConnectionString { get; set; }
        public string OpenAI_API { get; set; }
        public string DiscordBot_Token { get; set; }

        public Config()
        {
            ConnectionString = "server=localhost;user=root;password=;database=openrp";
            OpenAI_API = String.Empty;
            DiscordBot_Token = String.Empty;
        }
    }
}
