namespace Btms.Common.Extensions;

public static class LinksBuilder
{
    public static class Notification
    {
        public const string ResourceName = "import-notifications";

        public static string BuildSelfNotificationLink(string id)
        {
            return BuildSelfLink(ResourceName, id);
        }

        public static string BuildRelatedMovementLink(string id)
        {
            return BuildRelatedLink(ResourceName, id, Movement.ResourceName);
        }
    }

    public static class Movement
    {
        public const string ResourceName = "movements";

        public static string BuildSelfMovementLink(string id)
        {
            return BuildSelfLink(ResourceName, id);
        }

        public static string BuildRelatedMovementLink(string id)
        {
            return BuildRelatedLink(ResourceName, id, Notification.ResourceName);
        }
    }

    public static class Gmr
    {
        public const string ResourceName = "gmrs";
    }

    public static string BuildSelfLink(string type, string id)
    {
        return $"/api/{type}/{id}";
    }

    public static string BuildRelatedLink(string type, string id, string related)
    {
        return $"/api/{type}/{id}/{related}";
    }

    public static string BuildSelfRelationshipLink(string type, string id, string relationship)
    {
        return $"/api/{type}/{id}/relationships/{relationship}";
    }
}