using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Utils
{
    public static class NavigationKeys
    {
        public const string TicketAdded = nameof(TicketAdded);
        public const string NewTicket = nameof(NewTicket);
        public const string ShowFiles = nameof(ShowFiles);
        public const string RemoveAllFiles = nameof(RemoveAllFiles);
        public const string TicketSelected = nameof(TicketSelected);

        public const string HomePage = "NavigationPage/HomePage";
        public const string GalleryPage = nameof(GalleryPage);
        public const string ReportDetailPage = nameof(ReportDetailPage);
    }

    public static class CacheKeys
    {
        public const string Tickets = nameof(Tickets);
        public const string ZammadAccount = nameof(ZammadAccount);
        public const string UserAccount = nameof(UserAccount);
        public const string Groups = nameof(Groups);
    }

    public static class AppKeys
    {
        public static string ZammadApiBaseUrl => AppSettingsManager.Settings["ZammadApiBaseUrl"];
    }
}
