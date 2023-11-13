using Avalonia.Controls;
using System;

namespace Heating_Control_UI.Views.Pages;
public class PageControl : UserControl
{
    // Definieren Sie ein benutzerdefiniertes Ereignis
    public event EventHandler<EventArgs>? NavigatedTo;

    // Methode zum Auslösen des Ereignisses
    public void TriggerNavigatedTo()
    {
        NavigatedTo?.Invoke(this, EventArgs.Empty);
    }
}
