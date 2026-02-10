using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace IslandPostPOS.Services;

public class NavigationService
{
    private readonly IServiceProvider _services;
    private readonly Frame _frame;

    public NavigationService(IServiceProvider services, Frame frame)
    {
        _services = services;
        _frame = frame;
    }

    public void Navigate<TPage>(object? parameter = null) where TPage : Page
    {
        var page = _services.GetRequiredService<TPage>();
        _frame.Navigate(page.GetType(), parameter);
    }
}
