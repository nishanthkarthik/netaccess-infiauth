using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using System.Windows;
using System.Windows.Threading;
using Microsoft.ApplicationInsights;
using ModernWPF;

namespace netaccess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibNetAccess _netAccess;
        private DispatcherTimer _timer;
        private readonly TelemetryClient _telemetry = new TelemetryClient();
        private Stopwatch _stopwatch;
        private DateTime _time;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            _telemetry.InstrumentationKey = "1d0ad5f2-7647-4016-926a-edce544b11c8";
            Closing += MainWindow_Closing;
            Activated += MainWindow_Activated;
            Deactivated += MainWindow_Deactivated;

            // Set session data:
            _telemetry.Context.User.Id = Environment.UserName;
            _telemetry.Context.Session.Id = Guid.NewGuid().ToString();
            _telemetry.Context.Device.Language = CultureInfo.InstalledUICulture.EnglishName;
            _telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            // Log a page view:
            _telemetry.TrackPageView("MainWindow");
        }

        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            if (_stopwatch != null)
                _telemetry.TrackRequest("WindowActive", _time, _stopwatch.Elapsed, "true", true);
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            _telemetry.TrackEvent("MainPageActive");
            _time = DateTime.Now;
            _stopwatch = Stopwatch.StartNew();
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_telemetry != null)
            {
                _telemetry.Flush(); // only for desktop apps
            }
            OnClosing(e);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ModernTheme.ApplyTheme(ModernTheme.Theme.Dark, Accent.LightBlue);
            _netAccess = new LibNetAccess();
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            StopButton.IsEnabled = StartButton.IsEnabled = IntervalBox.IsEnabled = false;
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            if (_netAccess.Authenticate())
            {
                StatusBlock.Text = "logged in";
                SystemSounds.Beep.Play();
                ModernTheme.ApplyTheme(ModernTheme.Theme.Dark, Accent.Green);
                _telemetry.TrackEvent("AuthenticatedTick");
            }
            else if (!_netAccess.IsConnected)
            {
                StatusBlock.Text = "connectivity issue";
                ModernTheme.ApplyTheme(ModernTheme.Theme.Dark, Accent.Orange);
                _telemetry.TrackEvent("NetworkIssueTick");
            }
            else
            {
                StatusBlock.Text = "wrong credentials";
                ModernTheme.ApplyTheme(ModernTheme.Theme.Dark, Accent.Red);
                _telemetry.TrackEvent("WrongLoginTick");
            }
        }

        private void CredentialButton_OnClick(object sender, RoutedEventArgs e)
        {
            _netAccess.AddCredentials(UsernameBox.Text, PasswordBox.Password);
            if (_netAccess.Authenticate())
            {
                StartButton.IsEnabled = IntervalBox.IsEnabled = true;
                StatusBlock.Text = "logged in";
                ModernTheme.ApplyTheme(ModernTheme.Theme.Dark, Accent.Green);
                _telemetry.TrackEvent("Authenticated", new Dictionary<string, string>() { { "roll", "valid" } });
            }
            else if (!_netAccess.IsConnected)
            {
                StatusBlock.Text = "connectivity issue";
                ModernTheme.ApplyTheme(ModernTheme.Theme.Dark, Accent.Orange);
                _telemetry.TrackEvent("NetworkIssue");
            }
            else
            {
                StatusBlock.Text = "wrong credentials";
                ModernTheme.ApplyTheme(ModernTheme.Theme.Dark, Accent.Red);
                _telemetry.TrackEvent("WrongLogin");
            }
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _timer.Interval = new TimeSpan(0, 0, int.Parse(IntervalBox.Text));
                _timer.Start();
                StartButton.IsEnabled = false;
                IntervalBox.IsReadOnly = true;
                StopButton.IsEnabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                _telemetry.TrackException(exception);
            };
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            StartButton.IsEnabled = true;
            IntervalBox.IsReadOnly = false;
            StopButton.IsEnabled = false;
        }
    }
}
