using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities;
public class PageNavigator
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Carousel _mainContentControl;
    private readonly ServiceProvider _serviceProvider;
    private readonly List<ContentControl> _stack = new();
    public ContentControl? CurrentPage
    {
        get
        {
            if (_mainContentControl.Items.Count == 0) return null;
            return (ContentControl?)_mainContentControl.Items.Last();
        }
    }

    private void Next(ContentControl contentControl)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _mainContentControl.Items.Add(contentControl);
            _mainContentControl.Next();

            var type = contentControl.GetType();
            if (type.BaseType == typeof(PageControl))
                ((PageControl)contentControl).TriggerNavigatedTo();

        });
    }


    private async Task Previous()
    {
        if (_mainContentControl.Items.Count <= 1) return;

        _mainContentControl.Previous();

        var current = _mainContentControl.Items[_mainContentControl.Items.Count - 2];
        var type = current.GetType();
        if (type.BaseType == typeof(PageControl))
            ((PageControl)current).TriggerNavigatedTo();

        await Task.Delay(2_000);
        _mainContentControl.Items.RemoveAt(_mainContentControl.Items.Count - 1);



        //_mainContentControl.
    }

    public PageNavigator(Carousel mainContentControl, ServiceProvider serviceProvider)
    {
        _mainContentControl = mainContentControl;
        _serviceProvider = serviceProvider;
        //var transition = new PageSlide(TimeSpan.FromMilliseconds(2000), PageSlide.SlideAxis.Horizontal);
        ////var transition = new CrossFade(TimeSpan.FromMilliseconds(2000));
        //mainContentControl.PageTransition = transition;
    }

    private void SetPageTransition(IPageTransition? pageTransition)
    {
        _mainContentControl.PageTransition = pageTransition;
    }

    public void Pop(IPageTransition? pageTransition = null)
    {
        _semaphore.Wait();
        try
        {
            //if (_stack.Count <= 1) return;
            //_stack.RemoveAt(_stack.Count - 1);
            //var lastPage = _stack.Last();

            SetPageTransition(pageTransition);
            Previous();
            //CurrentPage = lastPage;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task PopAsync(IPageTransition? pageTransition = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            //if (_stack.Count <= 1) return;
            //_stack.RemoveAt(_stack.Count - 1);
            //var lastPage = _stack.Last();
            SetPageTransition(pageTransition);
            await Previous();
            //CurrentPage = lastPage;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    public viewT Push<viewT>(IPageTransition? pageTransition = null) where viewT : ContentControl
    {
        var view = _serviceProvider.GetService<viewT>();
        Push(view, pageTransition);

        return view;
    }

    public static IPageTransition DefaultSlideTransition { get; private set; } = new PageSlide(TimeSpan.FromMilliseconds(400), PageSlide.SlideAxis.Horizontal)
    {
        SlideOutEasing = new SineEaseInOut(),
        SlideInEasing = new SineEaseInOut(),
    };

    public void Push(ContentControl contentControl, IPageTransition? pageTransition = null)
    {
        _semaphore.Wait();
        try
        {
            if (contentControl == CurrentPage) return;

            //_stack.Add(contentControl);
            SetPageTransition(pageTransition);
            Next(contentControl);
            //CurrentPage = contentControl;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private TimeSpan TryGetAnimationDuration(IPageTransition pageTransition)
    {
        if (pageTransition is null)
            throw new ArgumentNullException(nameof(pageTransition));


        Type pageTransitionType = pageTransition.GetType();

        PropertyInfo durationProperty = pageTransitionType.GetProperty("Duration");

        if (durationProperty is not null && durationProperty.PropertyType == typeof(TimeSpan))
        {
            object durationValue = durationProperty.GetValue(pageTransition);

            if (durationValue != null)
            {
                return (TimeSpan)durationValue;
            }
        }

        return TimeSpan.Zero;
    }
    public void DestroyPage(ContentControl contentControl)
    {
        _stack.Remove(contentControl);
        _mainContentControl.Items.Remove(contentControl);
    }

    public async Task<viewT> PushAsync<viewT>(IPageTransition? pageTransition = null) where viewT : ContentControl
    {
        var view = _serviceProvider.GetService<viewT>();
        await PushAsync(view, pageTransition);

        return view;
    }

    public async Task PushAsync(ContentControl contentControl, IPageTransition? pageTransition = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (contentControl == CurrentPage) return;

            _stack.Add(contentControl);
            SetPageTransition(pageTransition);
            Next(contentControl);

            if (pageTransition is not null)
            {
                var duration = TryGetAnimationDuration(pageTransition);
                await Task.Delay(duration);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
