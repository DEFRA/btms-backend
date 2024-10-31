namespace Cdms.Model.Extensions;

public static class LinksBuilder
{
    public static class Notification
    {
        public static string BuildSelfNotificationLink(string id)
        {
            return LinksBuilder.BuildSelfLink("notification", id);
        }

        public static string BuildRelatedMovementLink(string id)
        {
            return LinksBuilder.BuildRelatedLink("notification", id, "movements");
        }
    }

    public static class Movement
    {
        public static string BuildSelfMovementLink(string id)
        {
            return LinksBuilder.BuildSelfLink("movements", id);
        }

        public static string BuildRelatedMovementLink(string id)
        {
            return LinksBuilder.BuildRelatedLink("movements", id, "notifications");
        }
    }

    public static string BuildSelfLink(string type, string id)
    {
        return $"/api/{type}/{id}";
    }

    public static string BuildRelatedLink(string type, string id, string related)
    {
        return $"/api/{type}/{id}/{related}";
    }
}