using System.Text.RegularExpressions;

namespace EventsDB.Helpers;


public static class Validator
{
    private static readonly Regex TimeRegex =
        new(@"^([01]\d|2[0-3]):([0-5]\d)$", RegexOptions.Compiled);

    private static readonly Regex DateRegex =
        new(@"^(0[1-9]|1[0-2])\.(0[1-9]|[12]\d|3[01])$", RegexOptions.Compiled);

    public static bool IsValidTime(string time) => TimeRegex.IsMatch(time);

    public static bool IsValidDate(string date) => DateRegex.IsMatch(date);

    public static bool IsNotEmpty(string value) => !string.IsNullOrWhiteSpace(value);
}
