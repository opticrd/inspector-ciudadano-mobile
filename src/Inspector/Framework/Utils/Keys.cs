

namespace Inspector.Framework.Utils
{
    public static class NavigationKeys
    {
        public const string TicketAdded = nameof(TicketAdded);
        public const string NewTicket = nameof(NewTicket);
        public const string ShowFiles = nameof(ShowFiles);
        public const string RemoveAllFiles = nameof(RemoveAllFiles);
        public const string TicketSelected = nameof(TicketSelected);
        public const string CommentSelected = nameof(CommentSelected);
        public const string IsEditing = nameof(IsEditing);

        public const string HomePage = "NavigationPage/HomePage";
        public const string GalleryPage = nameof(GalleryPage);
        public const string PreviewGalleryPage = nameof(PreviewGalleryPage);
        public const string ReportDetailPage = nameof(ReportDetailPage);
        public const string LoginPage = nameof(LoginPage);
    }

    public static class CacheKeys
    {
        public const string Tickets = nameof(Tickets);
        public const string ZammadAccount = nameof(ZammadAccount);
        public const string UserAccount = nameof(UserAccount);
        public const string Groups = nameof(Groups);
        public const string OAuthToken = nameof(OAuthToken);
    }

    public static class AppKeys
    {
        public static string ZammadApiBaseUrl => AppSettingsManager.Settings["ZammadApiBaseUrl"];
        public static string TerritorialDivisionApiBaseUrl => AppSettingsManager.Settings["TerritorialDivisionApiBaseUrl"];
        public static string IAmApiBaseUrl => AppSettingsManager.Settings["IAmApiBaseUrl"];
        public static string IamAuthToken => AppSettingsManager.Settings["IamAuthToken"];
        public static string DigitalGobApiBaseUrl => AppSettingsManager.Settings["DigitalGobApiBaseUrl"];
        public static string XAccessToken => AppSettingsManager.Settings["XAccessToken"];
    }
}
