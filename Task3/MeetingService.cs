namespace Task3;

internal interface IMeetingService
{
    void AddMeeting(Meeting meeting);
    void DeleteMeeting(Guid id);
    void ExportMeetingsForDate(DateTime date, string pathToFile);
    Meeting GetMeetingByTitle(string title);
    IReadOnlyCollection<Meeting> GetMeetingsByDate(DateTime date);
    IReadOnlyCollection<Meeting> GetRemindersToNotify(DateTime time);
    bool MeetingWithTitleExists(string title);
    void ReminderSent(Guid id);
    void UpdateMeeting(Guid id, string newTitle, DateTime newStart, DateTime newEnd, TimeSpan newReminder);
}

internal class MeetingService : IMeetingService
{
    private List<Meeting> meetings = new();

    //lock нужен для синхронизации потокрв, так как List<Meeting> не потокобезопасный
    //Может случиться так, что основной поток добавляет или удаляет встречу
    //в тот момент, когда другой поток из пула проходиться по списку для нахождения встреч,
    //о которых нужно уведомить
    private readonly object lockMeetingModify = new();

    private bool IsMeetingsOverlaps(Meeting a, Meeting b)
    {
        return a.StartDate < b.EndDate && b.StartDate < a.EndDate;
    }

    public void AddMeeting(Meeting meeting)
    {
        
        if (meetings.Any(m => IsMeetingsOverlaps(m, meeting)))
        {
            throw new InvalidOperationException("Встреча пересекается с уже существующей");
        }

        lock (lockMeetingModify)
        {
            meetings.Add(meeting);
        }
    }

    public void UpdateMeeting(Guid id, string newTitle, DateTime newStart, DateTime newEnd, TimeSpan newReminder)
    {
        var meeting = meetings.FirstOrDefault(m => m.Id == id);
        if (meeting == null)
        {
            throw new ArgumentNullException("Такой встречи не существует");
        }

        var updatedMeeting = new Meeting
        {
            Id = meeting.Id,
            Title = newTitle,
            StartDate = newStart,
            EndDate = newEnd,
            RemindTime = newReminder
        };

        if (meetings.Where(m => m.Id != id).Any(m => IsMeetingsOverlaps(m, updatedMeeting)))
        {
            throw new InvalidOperationException("Встреча пересекается с уже существующей");
        }

        lock (lockMeetingModify)
        {
            meeting.Title = updatedMeeting.Title;
            meeting.StartDate = updatedMeeting.StartDate;
            meeting.EndDate = updatedMeeting.EndDate;
            meeting.RemindTime = updatedMeeting.RemindTime;

            // предполагаю, что после изменения данных встречи о ней снова нужно уведомить
            meeting.IsReminderSent = false;
        }
    }

    public void DeleteMeeting(Guid id)
    {
        lock (lockMeetingModify)
        {
            meetings.RemoveAll(m => m.Id == id);
        }
    }

    public bool MeetingWithTitleExists(string title)
    {
        return meetings.Any(m => m.Title == title);
    }

    public IReadOnlyCollection<Meeting> GetMeetingsByDate(DateTime date)
    {
        return meetings
            .Where(m => m.StartDate.Date == date.Date)
            .OrderBy(m => m.StartDate)
            .ToList();
    }

    //можно было бы сделать асинхронным, но объем файлов небольшой, поэтому в этом нет особой пользы
    public void ExportMeetingsForDate(DateTime date, string pathToFile)
    {
        var meetingsForDay = GetMeetingsByDate(date);
        using (var writer = new StreamWriter(pathToFile))
        {
            foreach (var meeting in meetingsForDay)
            {
                writer.WriteLine($"Встреча \"{meeting.Title}\" будет с {meeting.StartDate:f} по {meeting.EndDate:f}");
            }
        }
    }

    public IReadOnlyCollection<Meeting> GetRemindersToNotify(DateTime time)
    {
        lock (lockMeetingModify)
        {
            return meetings.Where(m => !m.IsReminderSent && m.RemindDate <= time && m.StartDate > time).ToList();
        }
    }

    public void ReminderSent(Guid id)
    {
        lock (lockMeetingModify)
        {
            var meeting = meetings.FirstOrDefault(m => m.Id == id);
            if (meeting != null)
            {
                meeting.IsReminderSent = true;
            }
        }
    }

    public Meeting GetMeetingByTitle(string title)
    {
        var meeting = meetings.FirstOrDefault(m => m.Title == title);
        if (meeting == null)
        {
            throw new ArgumentNullException("Такой встречи не существует");
        }
        return meeting;
    }
}
