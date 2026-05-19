namespace EventsDB.Models;

public class Record
{
    public int Id { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Record() { }

    public Record(int id, string time, string date, string location, string name)
    {
        Id = id;
        Time = time;
        Date = date;
        Location = location;
        Name = name;
    }

    public int TimeInMinutes()
    {
        var parts = Time.Split(':');
        if (parts.Length != 2) return 0;
        if (!int.TryParse(parts[0], out int hours)) return 0;
        if (!int.TryParse(parts[1], out int minutes)) return 0;
        return hours * 60 + minutes;
    }

    public int DateAsNumber()
    {
        var parts = Date.Split('.');
        if (parts.Length != 2) return 0;
        if (!int.TryParse(parts[0], out int month)) return 0;
        if (!int.TryParse(parts[1], out int day)) return 0;
        return month * 100 + day;
    }

    public override string ToString()
        => $"[{Id}] {Time} | {Date} | {Location} | {Name}";
}
