namespace Task3;

internal class Meeting
{
    //изначально делал с базой данных, поэтому было Id. Решил оставить, потому что так удобнее искать встречи
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan RemindTime { get; set; }

    public DateTime RemindDate => StartDate - RemindTime;

    public bool IsReminderSent { get; set; } = false;
}
