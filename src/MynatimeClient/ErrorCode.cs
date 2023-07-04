
namespace Mynatime.Client;

using System;

public static class ErrorCode
{
    public const string UnknownError = "UnknownError";

    public const string PageParseMissCsrf = "PageParseMiss/CSRF";
    public const string PageParseMissIdentity = "PageParseMiss/Identity";
    public const string PageParseMissGroup = "PageParseMiss/Group";

    public const string MissingCsrfToken = "MissingCsrfToken";

    public const string LoggedOut = "LoggedOut";

    public const string InvalidUsernameOrPassword = "InvalidUsernameOrPassword";

    public const string StopNotFollowingStart = "StopNotFollowingStart";

    public const string UnknownEventType = "UnknownEventType";

    public const string NightlyItem = "NightlyItem";

    public const string ManyDaysItem = "ManyDaysItem";

    public static class InvalidPage
    {
        public const string Generic = "InvalidPage";
        public const string MissingCategories = "InvalidPage/MissingCategories";
        public const string MissingToken = "InvalidPage/MissingToken";
        public const string FormStartMissing = "InvalidPage/FormStartMissing";
        public const string FormEndMissing = "InvalidPage/FormEndMissing";
    }
    
    public static class InvalidForm
    {
        public const string DurationOrTimes = "InvalidForm/DurationOrTimes";
        public const string DurationMinimum = "InvalidForm/Duration/Minimum";
        public const string DurationMaximum = "InvalidForm/Duration/Maximum";
        public const string DurationNotNumber = "InvalidForm/Duration/NotNumber";
    }
}
