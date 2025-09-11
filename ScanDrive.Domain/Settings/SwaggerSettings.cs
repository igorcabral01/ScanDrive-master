namespace ScanDrive.Domain.Settings
{
    public class SwaggerSettings
    {
        public string Title { get; set; } = "ScanDrive API";
        public string Version { get; set; } = "v1";
        public string Description { get; set; } = "API para gerenciamento de lojas e anúncios de veículos";
        public ContactInfo Contact { get; set; } = new();
        public SecurityInfo Security { get; set; } = new();
    }

    public class ContactInfo
    {
        public string Name { get; set; } = "ScanDrive Team";
        public string Email { get; set; } = "contato@scandrive.com";
    }

    public class SecurityInfo
    {
        public string Description { get; set; } = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'";
        public string Name { get; set; } = "Authorization";
        public string In { get; set; } = "Header";
        public string Type { get; set; } = "ApiKey";
        public string Scheme { get; set; } = "Bearer";
    }
} 