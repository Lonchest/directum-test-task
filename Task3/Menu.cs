using System;

namespace Task3;

internal class Menu
{
    private readonly IMeetingService _meetingService;

    public Menu(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public void ShowMenu()
    {
        Console.WriteLine("\n1 - Добавить встречу");
        Console.WriteLine("2 - Изменить встречу");
        Console.WriteLine("3 - Удалить встречу");
        Console.WriteLine("4 - Просмотреть встречи по дате");
        Console.WriteLine("5 - Экспорт встреч в файл");
        Console.WriteLine("6 - Выход");
    }

    public void AddMeeting()
    {
        Console.Write("Название: ");
        var title = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Вы не ввели название встречи");
        }
        if (_meetingService.MeetingWithTitleExists(title))
        {
            throw new InvalidOperationException("Встреча с таким названием уже существует");
        }

        Console.Write("Начало (в формате гггг-мм-дд чч:мм): ");
        var startDate = ValidateDate(Console.ReadLine());

        if (startDate <= DateTime.Now)
        {
            throw new ArgumentOutOfRangeException("Встречи всегда планируются только на будущее время");
        }

        Console.Write("Окончание(в формате гггг-мм-дд чч:мм): ");
        var endDate = ValidateDate(Console.ReadLine());

        if (endDate <= startDate)
        {
            throw new ArgumentOutOfRangeException("Время окончания должно быть позже начала");
        }

        Console.Write("Напомнить за (формат времени чч:мм): ");
        TimeSpan reminderTime;

        if (!TimeSpan.TryParse(Console.ReadLine(), out reminderTime))
        {
            throw new ArgumentOutOfRangeException("Вы ввели время в неверном формате");
        }

        var meeting = new Meeting
        {
            Title = title,
            StartDate = startDate,
            EndDate = endDate,
            RemindTime = reminderTime
        };

        _meetingService.AddMeeting(meeting);
        Console.WriteLine("Встреча добавлена.");
    }

    public void UpdateMeeting()
    {
        Console.Write("Название встречи, которую вы хотите изменить: ");
        var title = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title) || !_meetingService.MeetingWithTitleExists(title))
        {
            throw new InvalidOperationException("Встречи с таким названием не существует");
        }

        var meeting = _meetingService.GetMeetingByTitle(title);

        Console.Write("Новое название (оставьте пустым для пропуска): ");
        var newTitle = Console.ReadLine();
        newTitle = string.IsNullOrWhiteSpace(newTitle) ? meeting.Title : newTitle;

        Console.Write("Новое начало (оставьте пустым для пропуска): ");
        var startDateInput = Console.ReadLine();
        var newStartDate = string.IsNullOrWhiteSpace(startDateInput) ? meeting.StartDate : DateTime.Parse(startDateInput);
        if (newStartDate <= DateTime.Now)
        {
            throw new ArgumentOutOfRangeException("Встречи всегда планируются только на будущее время");
        }

        Console.Write("Новое окончание (оставьте пустым для пропуска): ");
        var endDateInput = Console.ReadLine();
        var newEndDate = string.IsNullOrWhiteSpace(endDateInput) ? meeting.EndDate : DateTime.Parse(endDateInput);
        if (newEndDate <= newStartDate)
        {
            throw new ArgumentOutOfRangeException("Время окончания должно быть позже начала");
        }

        Console.Write("Новое время напоминания (оставьте пустым для пропуска): ");
        var remindBeforeInput = Console.ReadLine();
        var newRemindBefore = string.IsNullOrWhiteSpace(remindBeforeInput) ? meeting.RemindTime : TimeSpan.Parse(remindBeforeInput);

        _meetingService.UpdateMeeting(meeting.Id, newTitle, newStartDate, newEndDate, newRemindBefore);

        Console.WriteLine("Встреча обновлена.");
    }

    public void DeleteMeeting()
    {
        Console.Write("Название встречи, которую нужно удалить: ");
        var title = Console.ReadLine();
        if (!_meetingService.MeetingWithTitleExists(title))
        {
            throw new InvalidOperationException("Встречи с таким названием не существует");
        }
        var meeting = _meetingService.GetMeetingByTitle(title);
        _meetingService.DeleteMeeting(meeting.Id);
        Console.WriteLine("Встреча удалена");
    }

    public void ShowMeetingsForDay()
    {
        Console.Write("Введите дату: ");
        var date = ValidateDate(Console.ReadLine());
        var meetings = _meetingService.GetMeetingsByDate(date);
        if (!meetings.Any())
        {
            Console.WriteLine("Нет встреч на этот день");
            return;
        }

        foreach (var meeting in meetings)
        {
            Console.WriteLine($"\nВстреча: {meeting.Title} " +
                $"\nВремя: c {meeting.StartDate:t} по {meeting.EndDate:t}");
        }
    }

    public void ExportMeetingsForDay()
    {
        Console.Write("Дата: ");
        var date = ValidateDate(Console.ReadLine());

        Console.Write("Путь к файлу: ");
        var path = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Вы не ввели путь к файлу");
        }
        _meetingService.ExportMeetingsForDate(date.Date, path);
        Console.WriteLine("Экспорт завершен");
    }

    public DateTime ValidateDate(string dateStr)
    {
        DateTime date;
        if (!DateTime.TryParse(dateStr, out date))
        {
            throw new ArgumentException("Вы ввели дату в неверном формате");
        }
        return date;
    }

    public void Start()
    {
        while (true)
        {
            ShowMenu();
            var input = Console.ReadLine();
            try
            {
                switch (input)
                {
                    case "1":
                        AddMeeting();
                        break;
                    case "2":
                        UpdateMeeting();
                        break;
                    case "3":
                        DeleteMeeting();
                        break;
                    case "4":
                        ShowMeetingsForDay();
                        break;
                    case "5":
                        ExportMeetingsForDay();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Неверная команда. Выберите команду из списка");
                        break;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Ошибка при вводе даты: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Невозможно выполнить операцию: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неизвестная ошибка: {ex.Message}");
            }
        }
    }

}
