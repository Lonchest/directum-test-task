namespace Task3;

internal class Program
{
    static void Main(string[] args)
    {
        var meetingService = new MeetingService();
        var reminderService = new ReminderService(meetingService);
        var menu = new Menu(meetingService);

        reminderService.Start();
        menu.Start();
        reminderService.Dispose();
    }
}
