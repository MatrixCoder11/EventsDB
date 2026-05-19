using EventsDB.Data;
using EventsDB.Models;

namespace EventsDB.Repositories;


public class EventRepository
{
    private readonly DatabaseContext _db;

    public EventRepository(DatabaseContext db)
    {
        _db = db;
    }

 

    public Record Add(string time, string date, string location, string name)
    {
        var record = new Record(0, time, date, location, name);
        record.Id = _db.Insert(record);
        return record;
    }

  

    public List<Record> GetAll() => _db.GetAll();

    public Record? GetById(int id) => _db.GetById(id);

 

    public List<Record> GetSortedByTime()
        => _db.GetAll().OrderBy(r => r.TimeInMinutes()).ToList();

    public List<Record> GetSortedByLocation()
        => _db.GetAll("Location");

    public List<Record> GetSortedByName()
        => _db.GetAll("Name");

    public List<Record> GetSortedByDateAsc()
        => _db.GetAll().OrderBy(r => r.DateAsNumber()).ToList();

    public List<Record> GetSortedByDateDesc()
        => _db.GetAll().OrderByDescending(r => r.DateAsNumber()).ToList();


    public List<Record> SearchByName(string value)     => _db.SearchLike("Name",     value);
    public List<Record> SearchByTime(string value)     => _db.Search("Time",         value);
    public List<Record> SearchByLocation(string value) => _db.SearchLike("Location", value);
    public List<Record> SearchByDate(string value)     => _db.Search("Date",         value);


    public bool Update(Record record) => _db.Update(record);


    public bool Delete(int id) => _db.Delete(id);
}
