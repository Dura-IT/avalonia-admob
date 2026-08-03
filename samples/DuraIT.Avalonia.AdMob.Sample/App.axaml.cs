using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DuraIT.Avalonia.AdMob;
using DuraIT.Avalonia.AdMob.Sample.Logging;
using DuraIT.Avalonia.AdMob.Sample.ViewModels;
using DuraIT.Avalonia.AdMob.Sample.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob.Sample
{
    public partial class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            var services = BuildServices();
            var mainVm = services.GetRequiredService<MainViewModel>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow { DataContext = mainVm };
            }
            else if (ApplicationLifetime is IActivityApplicationLifetime activity)
            {
                activity.MainViewFactory = () => new MainView { DataContext = mainVm };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                singleView.MainView = new MainView { DataContext = mainVm };
            }

            base.OnFrameworkInitializationCompleted();
        }

        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The logger factory and provider are held for the application's lifetime by the AdMob runtime; there is no earlier scope in which to dispose them."
        )]
        private static ServiceProvider BuildServices()
        {
            var sink = new UiLogSink();
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddProvider(new UiLoggerProvider(sink))
            );

            var services = new ServiceCollection();
            services.AddSingleton(sink);

            // Test ads: every format serves Google's sample creatives, never real impressions. The
            // UI logger factory routes the library's ad-load/show outcomes into the on-screen log.
            services.AddAdMob(options => options.UseTestAds = true, loggerFactory);

            services.AddSingleton<MainViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
