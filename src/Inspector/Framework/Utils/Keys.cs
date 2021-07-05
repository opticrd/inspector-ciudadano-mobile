using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Utils
{
    public static class NavigationKeys
    {
        public const string TicketAdded = nameof(TicketAdded);
        public const string HomePage = "NavigationPage/HomePage";
    }

    public static class CacheKeys
    {
        public const string Tickets = nameof(Tickets);
        public const string ZammadAccount = nameof(ZammadAccount);
    }

    public static class AppKeys
    {
        public static string ZammadApiBaseUrl = AppSettingsManager.Settings["ZammadApiBaseUrl"];
    }
}
