namespace Task3;

internal class Meeting
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan RemindTime { get; set; }

    public DateTime RemindDate => StartDate - RemindTime;

    public bool IsReminderSent { get; set; } = false;
}
