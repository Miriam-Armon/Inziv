using HardwareMonitor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HardwareMonitor.Logging;
using System.IO;
using System.Windows;
using HardwareMonitor.IServices;
using HardwareMonitor.ViewModels;
using HardwareMonitor.Models;
using HardwareMonitor.Views;
using System.Reflection;

namespace HardwareMonitor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<HardwareMonitorOptions>(
            configuration.GetSection("HardwareMonitorOptions"));

        // logging: Debug + file logger
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddDebug();
            builder.AddProvider(new FileLoggerProvider(Path.Combine(AppContext.BaseDirectory, "logs", $"{Assembly.GetEntryAssembly()?.GetName().Name}.log")));
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // DI registrations
        services.AddSingleton<IHardwareMonitor, HardwareMonitor.Services.HardwareMonitor>();
        services.AddSingleton<IDataProvider, DataProvider>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        var serviceProvider = services.BuildServiceProvider();

        // Global exception logging
        var logger = serviceProvider.GetRequiredService<ILogger<App>>();

        AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
        {
            if (ev.ExceptionObject is Exception ex)
                logger.LogCritical(ex, "Unhandled domain exception");
            else
                logger.LogCritical("Unhandled domain exception: {Value}", ev.ExceptionObject);
        };

        DispatcherUnhandledException += (s, ev) =>
        {
            logger.LogCritical(ev.Exception, "Unhandled dispatcher exception");
            // keep default termination behavior for critical failures (do not set ev.Handled = true)
        };

        TaskScheduler.UnobservedTaskException += (s, ev) =>
        {
            logger.LogCritical(ev.Exception, "Unobserved task exception");
            ev.SetObserved();
        };

        // start hardware monitor
        try
        {
            serviceProvider.GetRequiredService<IHardwareMonitor>().StartMonitoring();
            logger.LogInformation("Hardware monitor started.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to start hardware monitor");
            MessageBox.Show("Fatal error starting hardware monitor. See logs.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        // construct and show main window
        try
        {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            logger.LogInformation("MainWindow shown.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to construct or show MainWindow (likely XAML/resource issue)");
            MessageBox.Show("Fatal error initializing UI. See logs.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }
    }
}
