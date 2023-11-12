using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities;
public class PageNavigator
{
    public IReadOnlyList<ContentControl> PageStack => _stack;

    public static IPageTransition DefaultHorizontalSlideTransition { get; private set; } = new PageSlide(TimeSpan.FromMilliseconds(400), PageSlide.SlideAxis.Horizontal)
    {
        SlideOutEasing = new SineEaseInOut(),
        SlideInEasing = new SineEaseInOut(),
    };

    private static IPageTransition? _defaultVerticalSlideTransition = null;
    public static IPageTransition DefaultVerticalSlideTransition
    {
        get
        {
            if(_defaultVerticalSlideTransition is not  null) return _defaultVerticalSlideTransition;

            var compositeTransition = new CompositePageTransition();
            compositeTransition.PageTransitions.Add(new PageSlide(TimeSpan.FromMilliseconds(1_000), PageSlide.SlideAxis.Vertical));
            compositeTransition.PageTransitions.Add(new CrossFade(TimeSpan.FromMilliseconds(1_000)));
            return _defaultVerticalSlideTransition = compositeTransition;
        }
    }

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Carousel _mainCarousel;
    private readonly ServiceProvider _serviceProvider;
    private readonly List<ContentControl> _stack = new();

    public ContentControl? CurrentPage
    {
        get
        {
            if (_mainCarousel.Items.Count == 0) return null;
            return (ContentControl?)_mainCarousel.Items.Last();
        }
    }

    public PageNavigator(Carousel mainCarousel, ServiceProvider serviceProvider)
    {
        _mainCarousel = mainCarousel;
        _serviceProvider = serviceProvider;
    }

    public void Pop(IPageTransition? pageTransition = null)
    {
        _semaphore.Wait();
        try
        {
            if (_stack.Count <= 1) return;
            _stack.RemoveAt(_stack.Count - 1);

            _ = Previous(pageTransition);
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
            if (_stack.Count <= 1) return;

            await Previous(pageTransition);
        }
        finally
        {
            _semaphore.Release();
        }
    }


    public viewT Push<viewT>(IPageTransition? pageTransition = null) where viewT : ContentControl
    {
        var view = _serviceProvider.GetService<viewT>() ?? throw new Exception("Unknown view!");
        Push(view, pageTransition);

        return view;
    }



    public void Push(ContentControl contentControl, IPageTransition? pageTransition = null)
    {
        _semaphore.Wait();
        try
        {
            if (contentControl == CurrentPage) return;

            _stack.Add(contentControl);
            Next(contentControl, pageTransition);
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private static TimeSpan TryGetAnimationDuration(IPageTransition pageTransition)
    {
        if (pageTransition is null)
            throw new ArgumentNullException(nameof(pageTransition));

        if (pageTransition is CompositePageTransition compositePageTransition)
        {
            var durationList = new List<TimeSpan>();
            foreach (var transitions in compositePageTransition.PageTransitions)
            {
                var transitionsTimeSpan = TryGetAnimationDuration(transitions);
                if (transitionsTimeSpan != TimeSpan.Zero)
                    durationList.Add(transitionsTimeSpan);
            }


            var maxTransition = TimeSpan.Zero;
            foreach (var transitionsTimeSpan in durationList)
            {
                if (maxTransition < transitionsTimeSpan)
                    maxTransition = transitionsTimeSpan;
            }

            return maxTransition;
        }

        Type pageTransitionType = pageTransition.GetType();

        var durationProperty = pageTransitionType.GetProperty("Duration");
        if (durationProperty is null)
            return TimeSpan.Zero;

        if (durationProperty is not null && durationProperty.PropertyType == typeof(TimeSpan))
        {
            var durationValue = durationProperty.GetValue(pageTransition);

            if (durationValue is null)
                return TimeSpan.Zero;

            return (TimeSpan)durationValue;
        }

        return TimeSpan.Zero;
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
            Next(contentControl, pageTransition);

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

    public void DestroyPage(ContentControl contentControl)
    {
        _stack.Remove(contentControl);
        _mainCarousel.Items.Remove(contentControl);
    }

    public void DestroyPage<viewT>() where viewT : ContentControl
    {
        var foundView = _stack.FirstOrDefault(view => view.GetType() == typeof(viewT)) ?? throw new Exception("View not Found!");
        _stack.Remove(foundView);
        _mainCarousel.Items.Remove(foundView);
    }

    private void Next(ContentControl contentControl, IPageTransition? pageTransition)
    {
        SetPageTransition(pageTransition);

        Dispatcher.UIThread.Post(() =>
        {
            _mainCarousel.Items.Add(contentControl);
            _mainCarousel.Next();

            var type = contentControl.GetType();
            if (type.BaseType == typeof(PageControl))
                ((PageControl)contentControl).TriggerNavigatedTo();
        });
    }

    private async Task Previous(IPageTransition? pageTransition)
    {
        if (_mainCarousel.Items.Count <= 1) return;

        SetPageTransition(pageTransition);
        _mainCarousel.Previous();

        var current = _mainCarousel.Items[_mainCarousel.Items.Count - 2];
        var type = current.GetType();
        if (type.BaseType == typeof(PageControl))
            ((PageControl)current).TriggerNavigatedTo();

        if (pageTransition is not null)
        {
            var duration = TryGetAnimationDuration(pageTransition);
            await Task.Delay(duration);
        }

        _mainCarousel.Items.RemoveAt(_mainCarousel.Items.Count - 1);
    }

    private void SetPageTransition(IPageTransition? pageTransition) => _mainCarousel.PageTransition = pageTransition;
}
