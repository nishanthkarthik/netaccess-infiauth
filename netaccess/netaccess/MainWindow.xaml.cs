using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace netaccess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Storyboard ShowCredPanelKey { get; private set; }
        public Storyboard HideCredPanelKey { get; private set; }
        private LibNetAccess _netAccess;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowCredPanelKey = FindResource("ShowCredPanelKey") as Storyboard;
            HideCredPanelKey = FindResource("HideCredPanelKey") as Storyboard;
            if (ShowCredPanelKey != null) ShowCredPanelKey.Begin();
            _netAccess = new LibNetAccess();
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            StopButton.IsEnabled = false;
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            speechSynthesizer.SpeakAsync("Authenticating net access");
            _netAccess.Authenticate();
            speechSynthesizer.SpeakAsync("complete");
        }

        private void CredentialButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowCredPanelKey.Begin();
        }

        private void UsernameBox_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(TextBox))
            {
                if ((String.IsNullOrWhiteSpace(((TextBox)sender).Text)) || ((TextBox)sender).Text == "roll no.")
                    ((TextBox)sender).Text = "";
            }
            else if (sender is PasswordBox)
            {
                if ((String.IsNullOrWhiteSpace(((PasswordBox)sender).Password)) || ((PasswordBox)sender).Password == "xxxxxx")
                    ((PasswordBox)sender).Password = "";
            }

        }

        private void UsernameBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(TextBox))
            {
                if (String.IsNullOrWhiteSpace(((TextBox)sender).Text))
                    ((TextBox)sender).Text = "roll no.";
            }
            else if (sender is PasswordBox)
            {
                if (String.IsNullOrWhiteSpace(((PasswordBox)sender).Password))
                    ((PasswordBox)sender).Password = "xxxxxx";
            }
        }

        private void CredentialSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(UsernameBox.Text) || String.IsNullOrWhiteSpace(PasswordBox.Password))
                MessageBox.Show("Please enter valid roll no. and password to login.", "Invaild details",
                    MessageBoxButton.OK);
            else
            {
                _netAccess.AddCredentials(UsernameBox.Text, PasswordBox.Password);
                if (!_netAccess.Authenticate())
                    return;
                HideCredPanelKey.Begin();
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
