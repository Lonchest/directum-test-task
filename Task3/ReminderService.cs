namespace Task3;

internal interface IReminderService
{
    void CheckReminder(object? state);
    void Stop();
    void Start();
}

internal class ReminderService : IReminderService
{
    private readonly IMeetingService _meetingService;
    private Timer? timerForRemindNotify;

    public ReminderService(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public void CheckReminder(object? state)
    {
        var now = DateTime.Now;
        var meetingsForRemind = _meetingService.GetRemindersToNotify(now);
        foreach (var meeting in meetingsForRemind)
        {
            Console.WriteLine($"\n[УВЕДОМЛНИЕ] Встреча \"{meeting.Title}\" будет {meeting.StartDate:f}");
            _meetingService.ReminderSent(meeting.Id);
        }
    }

    public void Start()
    {
        //раз в минуту поток из пула будет искать встречи, о которых нужно уведомить
        timerForRemindNotify = new Timer(CheckReminder, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public void Stop()
    {
        timerForRemindNotify?.Dispose();
    }
}
