using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiApp1.Models
{
    public partial class TodoItem : ObservableObject
    {
        public string Id { get; set; }
        public string UserId { get; set; }

        [ObservableProperty]
        string title;

        [ObservableProperty]
        DateTime deadline;

        [ObservableProperty]
        string priority; // "Cao", "Trung bình", "Thấp"

        [ObservableProperty]
        bool isCompleted;

        [ObservableProperty]
        string color; // Màu sắc theo mức độ ưu tiên

        [ObservableProperty]
        string content;
    }
}