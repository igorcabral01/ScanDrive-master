namespace ScanDrive.Domain.Settings;

public static class Claims
{
    public static class Modules
    {
        public const string Administration = "Module.Administration";
        public const string Reports = "Module.Reports";
        public const string Financial = "Module.Financial";
        public const string Employees = "Module.Employees";
        public const string History = "Module.History";
        public const string Chat = "Module.Chat";
        public const string ChatQuestions = "Module.ChatQuestions";
        public const string Vehicles = "Module.Vehicles";
        public const string Shops = "Module.Shops";
        public const string Leads = "Module.Leads";
        public const string TestDrives = "Module.TestDrives";
    }

    public static class Permissions
    {
        public const string View = "Permission.View";
        public const string Create = "Permission.Create";
        public const string Edit = "Permission.Edit";
        public const string Delete = "Permission.Delete";
        public const string Approve = "Permission.Approve";
        public const string Export = "Permission.Export";
    }

    public static class DefaultClaims
    {
        public static Dictionary<string, string[]> RoleClaims = new()
        {
            {
                Roles.Admin, new[]
                {
                    $"{Modules.Administration}:{Permissions.View}",
                    $"{Modules.Administration}:{Permissions.Create}",
                    $"{Modules.Administration}:{Permissions.Edit}",
                    $"{Modules.Administration}:{Permissions.Delete}",
                    $"{Modules.Reports}:{Permissions.View}",
                    $"{Modules.Reports}:{Permissions.Export}",
                    $"{Modules.Financial}:{Permissions.View}",
                    $"{Modules.Financial}:{Permissions.Edit}",
                    $"{Modules.Financial}:{Permissions.Approve}",
                    $"{Modules.Employees}:{Permissions.View}",
                    $"{Modules.Employees}:{Permissions.Create}",
                    $"{Modules.Employees}:{Permissions.Edit}",
                    $"{Modules.Employees}:{Permissions.Delete}",
                    $"{Modules.History}:{Permissions.View}",
                    $"{Modules.Chat}:{Permissions.View}",
                    $"{Modules.ChatQuestions}:{Permissions.View}",
                    $"{Modules.ChatQuestions}:{Permissions.Create}",
                    $"{Modules.ChatQuestions}:{Permissions.Edit}",
                    $"{Modules.ChatQuestions}:{Permissions.Delete}",
                    $"{Modules.Vehicles}:{Permissions.View}",
                    $"{Modules.Vehicles}:{Permissions.Create}",
                    $"{Modules.Vehicles}:{Permissions.Edit}",
                    $"{Modules.Vehicles}:{Permissions.Delete}",
                    $"{Modules.Shops}:{Permissions.View}",
                    $"{Modules.Shops}:{Permissions.Create}",
                    $"{Modules.Shops}:{Permissions.Edit}",
                    $"{Modules.Shops}:{Permissions.Delete}",
                    $"{Modules.Leads}:{Permissions.View}",
                    $"{Modules.Leads}:{Permissions.Create}",
                    $"{Modules.Leads}:{Permissions.Edit}",
                    $"{Modules.Leads}:{Permissions.Delete}",
                    $"{Modules.TestDrives}:{Permissions.View}",
                    $"{Modules.TestDrives}:{Permissions.Create}",
                    $"{Modules.TestDrives}:{Permissions.Edit}",
                    $"{Modules.TestDrives}:{Permissions.Delete}"
                }
            },
            {
                Roles.ShopOwner, new[]
                {
                    $"{Modules.Reports}:{Permissions.View}",
                    $"{Modules.Financial}:{Permissions.View}",
                    $"{Modules.Employees}:{Permissions.View}",
                    $"{Modules.Employees}:{Permissions.Create}",
                    $"{Modules.History}:{Permissions.View}",
                    $"{Modules.Chat}:{Permissions.View}",
                    $"{Modules.ChatQuestions}:{Permissions.View}",
                    $"{Modules.Vehicles}:{Permissions.View}",
                    $"{Modules.Vehicles}:{Permissions.Create}",
                    $"{Modules.Vehicles}:{Permissions.Edit}",
                    $"{Modules.Vehicles}:{Permissions.Delete}",
                    $"{Modules.Shops}:{Permissions.View}",
                    $"{Modules.Shops}:{Permissions.Edit}",
                    $"{Modules.Leads}:{Permissions.View}",
                    $"{Modules.Leads}:{Permissions.Create}",
                    $"{Modules.Leads}:{Permissions.Edit}",
                    $"{Modules.Leads}:{Permissions.Delete}",
                    $"{Modules.TestDrives}:{Permissions.View}",
                    $"{Modules.TestDrives}:{Permissions.Create}",
                    $"{Modules.TestDrives}:{Permissions.Edit}",
                    $"{Modules.TestDrives}:{Permissions.Delete}"
                }
            },
            {
                Roles.ShopSeller, new[]
                {
                    $"{Modules.History}:{Permissions.View}",
                    $"{Modules.Chat}:{Permissions.View}",
                    $"{Modules.ChatQuestions}:{Permissions.View}",
                    $"{Modules.Vehicles}:{Permissions.View}",
                    $"{Modules.Vehicles}:{Permissions.Create}",
                    $"{Modules.Vehicles}:{Permissions.Edit}",
                    $"{Modules.Shops}:{Permissions.View}",
                    $"{Modules.Leads}:{Permissions.View}",
                    $"{Modules.Leads}:{Permissions.Create}",
                    $"{Modules.Leads}:{Permissions.Edit}",
                    $"{Modules.TestDrives}:{Permissions.View}",
                    $"{Modules.TestDrives}:{Permissions.Create}",
                    $"{Modules.TestDrives}:{Permissions.Edit}"
                }
            },
            {
                Roles.User, new[]
                {
                    $"{Modules.Chat}:{Permissions.View}",
                    $"{Modules.Vehicles}:{Permissions.View}",
                    $"{Modules.Shops}:{Permissions.View}",
                    $"{Modules.TestDrives}:{Permissions.View}"
                }
            }
        };
    }
} 