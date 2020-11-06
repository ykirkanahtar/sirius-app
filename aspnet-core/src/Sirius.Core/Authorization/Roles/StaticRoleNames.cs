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
            public const string SiteManager = "SiteManager";
            public const string BlockManager = "BlockManager";
            public const string HousingOwner = "HousingOwner";
            public const string HousingTenant = "HousingTenant";
        }
    }
}
