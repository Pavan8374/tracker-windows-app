using Microsoft.Extensions.DependencyInjection;
using NSTracker;
using NSTracker.Services.Navigation;

/// <summary>
/// Navigation service.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Navigation servie.
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Show login form
    /// </summary>
    public void ShowLoginForm()
    {
        var loginForm = _serviceProvider.GetRequiredService<LoginForm>();
        loginForm.Show();
    }

    /// <summary>
    /// Show tracker form
    /// </summary>
    public void ShowTrackerForm()
    {
        var trackingForm = _serviceProvider.GetRequiredService<TrackerForm>();
        trackingForm.Show();
    }
}