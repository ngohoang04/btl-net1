// Models/CalendarEvent.cs
namespace MauiApp1.Models;

public class CalendarEvent
{
    public string Id { get; set; } // ID sự kiện trên Firebase
    public string UserId { get; set; } // Của user nào
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Category { get; set; } // Công việc, Gia đình...
    public string Color { get; set; } // Màu sắc hiển thị trên lịch

    public int ReminderMinutes { get; set; }

    public int NotificationId { get; set; }
}