using System.Globalization;

namespace TMS.BusinessLayer.Common;

public static class TimeHelper
{
    public static DateOnly WeekStart(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek + 6) % 7; // Monday = 0
        return date.AddDays(-diff);
    }

    public static TimeZoneInfo GetZone(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception)
        {
            return TimeZoneInfo.Utc;
        }
    }

    public static DateOnly LocalToday(TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));

    public static DateTime ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local))
        {
            local = local.AddHours(1);
        }

        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

    /// <summary>Accepts 09:00, 9:00, 9:00 AM, 17:30.</summary>
    public static bool TryParseTime(string? text, out TimeOnly time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return TimeOnly.TryParse(text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
    }

    public static string FormatHm(DateTime? utc, TimeZoneInfo zone) =>
        utc is null ? string.Empty : ToLocal(utc.Value, zone).ToString("HH:mm", CultureInfo.InvariantCulture);

    public static string FormatClock(DateTime? utc, TimeZoneInfo zone) =>
        utc is null ? string.Empty : ToLocal(utc.Value, zone).ToString("hh:mm tt", CultureInfo.InvariantCulture);

    public static string FormatMinutes(int minutes)
    {
        var h = minutes / 60;
        var m = minutes % 60;
        return h > 0 ? $"{h}h {m}m" : $"{m}m";
    }

    public static string FormatHours(decimal hours) =>
        hours.ToString("0.##", CultureInfo.InvariantCulture);

    public static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        var first = parts[0][0];
        var last = parts.Length > 1 ? parts[^1][0] : (char?)null;
        return (first.ToString() + last).ToUpperInvariant();
    }
}
