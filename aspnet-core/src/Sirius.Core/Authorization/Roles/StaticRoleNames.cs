namespace Sirius.Authorization.Roles
{
    public static class StaticRoleNames
    {
        public static class Host
        {
            public const string Admin = "Admin";
        }

        public static class Tenants
        {
            public const string Admin = "Admin";
            public const string SiteManagement = "SiteManagement";
            public const string SiteAuditor = "SiteAuditor";
            // public const string BlockManagement = "BlockManagement";
            // public const string HousingOwner = "HousingOwner";
            // public const string HousingTenant = "HousingTenant";
        }
    }
}
