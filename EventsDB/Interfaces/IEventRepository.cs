using EventsDB.Models;

namespace EventsDB.Interfaces;

public interface IEventRepository
{
    Record Add(string time, string date, string location, string name);
    List<Record> GetAll();
    Record? GetById(int id);
    bool Update(Record record);
    bool Delete(int id);

    List<Record> GetSortedByTime();
    List<Record> GetSortedByLocation();
    List<Record> GetSortedByName();
    List<Record> GetSortedByDateAsc();
    List<Record> GetSortedByDateDesc();

    List<Record> SearchByName(string value);
    List<Record> SearchByTime(string value);
    List<Record> SearchByLocation(string value);
    List<Record> SearchByDate(string value);
}