namespace ScanDrive.Domain.Settings;

public static class Roles
{
    public const string Admin = "Admin";
    public const string ShopOwner = "ShopOwner";
    public const string ShopSeller = "ShopSeller";
    public const string User = "User";
    
    public static readonly string[] All = new[]
    {
        Admin,
        ShopOwner,
        ShopSeller,
        User
    };
} 