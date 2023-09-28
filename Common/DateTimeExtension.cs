namespace RSPL.Common
{
    public static class DateTimeExtension
    {
        public static DateTime UtcToIndianTime(this DateTime dateTime)
        {
            return dateTime.AddHours(5).AddMinutes(30);
        }

    }
}
