using Avalonia.Controls;
using System;

namespace Heating_Control_UI.Views.Pages;
public class PageControl : UserControl
{
    public event EventHandler<EventArgs>? NavigatedTo;
    public void TriggerNavigatedTo() => NavigatedTo?.Invoke(this, EventArgs.Empty);    
}
