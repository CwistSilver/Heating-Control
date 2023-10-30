using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities;
public class PageNavigator
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ContentControl _mainContentControl;
    private readonly ServiceProvider _serviceProvider;
    private readonly List<ContentControl> _stack = new();

    private ContentControl _contentControl;
    public ContentControl CurrentPage
    {
        get => _contentControl;
        private set
        {
            _contentControl = value;
            _mainContentControl.Content = _contentControl;
        }
    }

    public PageNavigator(ContentControl mainContentControl, ServiceProvider serviceProvider)
    {
        _mainContentControl = mainContentControl;
        _serviceProvider = serviceProvider;
    }

    public void Pop()
    {
        _semaphore.Wait();
        try
        {
            if (_stack.Count <= 1) return;
            _stack.RemoveAt(_stack.Count - 1);
            var lastPage = _stack.Last();
            CurrentPage = lastPage;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task PopAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_stack.Count <= 1) return;
            _stack.RemoveAt(_stack.Count - 1);
            var lastPage = _stack.Last();
            CurrentPage = lastPage;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    public viewT Push<viewT>() where viewT : ContentControl
    {
        var view = _serviceProvider.GetService<viewT>();
        Push(view);

        return view;
    }

    public void Push(ContentControl contentControl)
    {
        _semaphore.Wait();
        try
        {
            if (contentControl == CurrentPage) return;

            _stack.Add(contentControl);
            CurrentPage = contentControl;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<viewT> PushAsync<viewT>() where viewT : ContentControl
    {
        var view = _serviceProvider.GetService<viewT>();
        await PushAsync(view);

        return view;
    }

    public async Task PushAsync(ContentControl contentControl)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (contentControl == CurrentPage) return;

            _stack.Add(contentControl);
            CurrentPage = contentControl;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
