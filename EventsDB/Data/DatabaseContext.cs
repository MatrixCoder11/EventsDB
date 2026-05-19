using Microsoft.Data.Sqlite;
using EventsDB.Models;

namespace EventsDB.Data;


public class DatabaseContext : IDisposable
{
    private readonly SqliteConnection _connection;

    public DatabaseContext(string dbPath = "events.db")
    {
        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();
        InitializeSchema();
    }


    private void InitializeSchema()
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS Events (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                Time     TEXT NOT NULL,
                Date     TEXT NOT NULL,
                Location TEXT NOT NULL,
                Name     TEXT NOT NULL
            );
            """;
        Execute(sql);
        const string sqlUsers = """
           CREATE TABLE IF NOT EXISTS Users (
        Id           INTEGER PRIMARY KEY AUTOINCREMENT,
        Username     TEXT NOT NULL UNIQUE,
        PasswordHash TEXT NOT NULL,
        Role         TEXT NOT NULL DEFAULT 'Viewer'
    );
    """;

       Execute(sqlUsers);
    }

   
    public int Insert(Record record)
    {
        const string sql = """
            INSERT INTO Events (Time, Date, Location, Name)
            VALUES (@time, @date, @location, @name);
            SELECT last_insert_rowid();
            """;

        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@time",     record.Time);
        cmd.Parameters.AddWithValue("@date",     record.Date);
        cmd.Parameters.AddWithValue("@location", record.Location);
        cmd.Parameters.AddWithValue("@name",     record.Name);

        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public bool Update(Record record)
    {
        const string sql = """
            UPDATE Events
            SET Time = @time, Date = @date, Location = @location, Name = @name
            WHERE Id = @id;
            """;

        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id",       record.Id);
        cmd.Parameters.AddWithValue("@time",     record.Time);
        cmd.Parameters.AddWithValue("@date",     record.Date);
        cmd.Parameters.AddWithValue("@location", record.Location);
        cmd.Parameters.AddWithValue("@name",     record.Name);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        const string sql = "DELETE FROM Events WHERE Id = @id;";
        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<Record> GetAll(string orderBy = "Id", bool descending = false)
    {
        var allowed = new HashSet<string> { "Id", "Time", "Date", "Location", "Name" };
        if (!allowed.Contains(orderBy)) orderBy = "Id";

        string dir = descending ? "DESC" : "ASC";
        string sql = $"SELECT Id, Time, Date, Location, Name FROM Events ORDER BY {orderBy} {dir};";

        return ReadRecords(sql);
    }

 
    public List<Record> Search(string field, string value)
    {
        var allowed = new HashSet<string> { "Time", "Date", "Location", "Name" };
        if (!allowed.Contains(field)) return [];

        string sql = $"SELECT Id, Time, Date, Location, Name FROM Events WHERE {field} = @value;";
        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@value", value);
        return ReadRecords(cmd);
    }

    public List<Record> SearchLike(string field, string value)
    {
        var allowed = new HashSet<string> { "Time", "Date", "Location", "Name" };
        if (!allowed.Contains(field)) return [];

        string sql = $"SELECT Id, Time, Date, Location, Name FROM Events WHERE {field} LIKE @value;";
        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@value", $"%{value}%");
        return ReadRecords(cmd);
    }

    public Record? GetById(int id)
    {
        const string sql = "SELECT Id, Time, Date, Location, Name FROM Events WHERE Id = @id;";
        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", id);
        var list = ReadRecords(cmd);
        return list.FirstOrDefault();
    }

 

    private void Execute(string sql)
    {
        using var cmd = CreateCommand(sql);
        cmd.ExecuteNonQuery();
    }

    private SqliteCommand CreateCommand(string sql)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd;
    }

    private static List<Record> ReadRecords(SqliteCommand cmd)
    {
        var list = new List<Record>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapRecord(reader));
        return list;
    }

    private List<Record> ReadRecords(string sql)
    {
        using var cmd = CreateCommand(sql);
        return ReadRecords(cmd);
    }

    private static Record MapRecord(SqliteDataReader r) => new(
        r.GetInt32(0),
        r.GetString(1),
        r.GetString(2),
        r.GetString(3),
        r.GetString(4)
    );

    public User? GetUserByUsername(string username)
    {
        const string sql = "SELECT Id, Username, PasswordHash, Role FROM Users WHERE Username = @username;";
        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@username", username);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            Role = Enum.Parse<UserRole>(reader.GetString(3))
        };
    }

    public void CreateUser(User user)
    {
        const string sql = """
        INSERT INTO Users (Username, PasswordHash, Role)
        VALUES (@username, @passwordHash, @role);
        """;
        using var cmd = CreateCommand(sql);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@role", user.Role.ToString());
        cmd.ExecuteNonQuery();
    }

    public bool HasAnyUsers()
    {
        const string sql = "SELECT COUNT(*) FROM Users;";
        using var cmd = CreateCommand(sql);
        var count = Convert.ToInt32(cmd.ExecuteScalar());
        return count > 0;
    }

    public void Dispose() => _connection.Dispose();
}
