using MauiApp1.ViewModels;
namespace MauiApp1.Views;
public partial class AddEventPage : ContentPage
{
    public AddEventPage(AddEventViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}