using Microsoft.Extensions.DependencyInjection;
using Tracker.Services.DPRSService.Auth;
using Tracker.Services.DPRSService.TokenManagement;
using Tracker.Services.DPRSService.Users;
using Tracker.Services.Utility;
using Tracker.Services;
using NSTracker;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NSTracker.Services.Navigation;
using NSTracker.Services.AutoSuggestions;
using Microsoft.Win32;

internal static class Program
{
    private static Mutex? _mutex;

    [STAThread]
    static async Task Main()
    {
        AddToStartup();

        const string appName = "NSTracker";
        bool isNewInstance;

        // Create the mutex
        _mutex = new Mutex(true, appName, out isNewInstance);

        if (!isNewInstance)
        {
            BringExistingInstanceToFront(appName);
            return;
        }
        Application.EnableVisualStyles();
        ApplicationConfiguration.Initialize();
        Application.SetCompatibleTextRenderingDefault(false);


        try
        {
            var services = ConfigureServices();
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var navigationService = serviceProvider.GetRequiredService<INavigationService>();
                var tokenManager = serviceProvider.GetRequiredService<ITokenManager>();
                bool isTokenValid = await tokenManager.IsTokenValidAsync();
                if (isTokenValid)
                {
                    navigationService.ShowTrackerForm();
                }
                else
                {
                    navigationService.ShowLoginForm();
                }
                var suggestionService = serviceProvider.GetRequiredService<ISuggestionService>();
                suggestionService.Start();
                NotificationHelper.SetAppUserModelID("com.natrixsoftware.notificationapp");
                Application.Run();

            }
        }
        finally
        {
            ReleaseMutex();
        }
    }

    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("NatrixApiClient")
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(Endpoint.BaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        string encryptionKey = CipherUtils.LoadKey();
        if (encryptionKey == null)
        {
            encryptionKey = CipherUtils.GenerateKey();
            CipherUtils.SaveKey(encryptionKey);
        }

        services.AddDataProtection();
        services.AddTransient<AuthenticationDelegatingHandler>();
        services.AddTransient<ITokenManager, TokenManager>();
        services.AddAutoMapper(typeof(TrackerMapperProfile));
        services.AddTransient<LoginForm>();
        services.AddTransient<TrackerForm>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ISuggestionService, SuggestionService>();

        return services;
    }

    private static void AddToStartup()
    {
        try
        {
            string appName = "NSTracker";
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(appName, appPath);
            }
        }
        catch (Exception ex)
        {
            // Simple error logging
            Debug.WriteLine($"Failed to add to startup: {ex.Message}");
        }
    }

    // To remove from the app from startup.
    private static void RemoveFromStartup()
    {
        try
        {
            string appName = "NSTracker";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(appName, false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to remove from startup: {ex.Message}");
        }
    }

    public static void ReleaseMutex()
    {
        if (_mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutex = null;
        }
    }

    private static void BringExistingInstanceToFront(string appName)
    {
        var currentProcess = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
        {
            if (process.Id != currentProcess.Id)
            {
                IntPtr handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    if (NativeMethods.IsIconic(handle))
                    {
                        NativeMethods.ShowWindow(handle, NativeMethods.SW_RESTORE);
                    }
                    NativeMethods.SetForegroundWindow(handle);
                }
                break;
            }
        }
    }
}

internal static class NativeMethods
{
    public const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
