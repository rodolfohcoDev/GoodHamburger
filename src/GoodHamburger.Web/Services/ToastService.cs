namespace GoodHamburger.Web.Services;

public enum ToastType { Success, Error, Info }

public class ToastMessage
{
    public string Text { get; set; } = "";
    public ToastType Type { get; set; }
}

public class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Show(string text, ToastType type = ToastType.Info)
        => OnShow?.Invoke(new ToastMessage { Text = text, Type = type });
}
