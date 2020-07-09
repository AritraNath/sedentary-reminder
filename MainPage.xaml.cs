using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library // QueryString.NET
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.ApplicationModel.Core;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TimerTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string timerStartSpeech = "You will be reminded every ";
        private static string title = "Time to Move!";
        private static string content = "The set time has elapsed!";
        private string hr, min;
        public string LiveTime => DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

        private readonly ObservableCollection<Int32> hours = new ObservableCollection<Int32>();
        private readonly ObservableCollection<Int32> minutes = new ObservableCollection<Int32>();

        private DispatcherTimer countDownTimer;

        private Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;
        private static int time;

        public MainPage()
        {
            this.InitializeComponent();
            this.ExtendAcrylicIntoTitleBar();
            Window.Current.SetTitleBar(customTitle);

            PopulateComboBoxes();

            DispatcherTimer Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            DataContext = this;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
        }

        private void PopulateComboBoxes()
        {
            for (int i = 0; i < 24; i++)
                hours.Add(i);
            for (int i = 1; i < 60; i++)
                minutes.Add(i);

            hoursComboBox.SelectedIndex = 0;
            minutesComboBox.SelectedIndex = 29;
        }

        private void Timer_Tick(object sender, object e)
        {
            Time.Text = DateTime.Now.ToString("ddd, dd MMM, hh:mm:ss tt");
        }

        private void ExtendAcrylicIntoTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void RemindButtonClick(object sender, RoutedEventArgs e)
        {
            if (remindButton.IsChecked == true)
            {
                ReadTimeInput();
                SaveSettingToFile();
                PlayAudio();

                StartReminders();
                remindButton.Content = "Stop Reminding Me!";
            }
            else
            {
                remindButton.Content = "Start Reminders";
                countDownTimer.Stop();
            }
        }

        private void StartReminders()
        {
            
            
            countDownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(time)
            };
            if(remindButton.IsChecked == true)
                countDownTimer.Tick += CountDownTimer_Tick;
            countDownTimer.Start();
        }

        private void CountDownTimer_Tick(object sender, object e)
        {
            //Time.Text = (++i).ToString();
            Notify();
        }

        private void Notify()
        {
            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        readonly ToastContent toastContent = new ToastContent()
        {
            Scenario = ToastScenario.Alarm,

            Visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    HeroImage = new ToastGenericHeroImage()
                    {
                        Source = "https://picsum.photos/364/180?image=1043"
                    },
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = title
                        },

                        new AdaptiveText()
                        {
                            Text = content
                        }
                    }
                }

            },
            Actions = new ToastActionsCustom()
            {
                
                Buttons =
                {
                    new ToastButtonDismiss()
                    {
                        
                    }
                }
            },
            // Arguments when the user taps body of toast
            //Launch = new QueryString()
            //{
            //    { "action", "viewClass" },
            //    { "classId", "3910938180" }
            //}.ToString()
        };

        private void SaveSettingToFile()
        {
            localSettings.Values["hourSetting"] = hr;
            localSettings.Values["minuteSetting"] = min;

            ReadSettingFromFile();
        }

        private void ReadSettingFromFile()
        {
            Object hr = localSettings.Values["hourSetting"];
            Object min = localSettings.Values["minuteSetting"];
        }

        private void ReadTimeInput()
        {
            hr = hoursComboBox.SelectedValue.ToString();
            min = minutesComboBox.SelectedValue.ToString();

            bool isParsable = Int32.TryParse(hr, out int number);
            if (isParsable)
                time += (number * 60);

            isParsable = Int32.TryParse(min, out number);
            if (isParsable)
                time += (number);
        }

        private async void PlayAudio()
        {
            MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            _ = await synth.SynthesizeTextToStreamAsync(
                timerStartSpeech + time.ToString() + ((time == 1) ? " minute" : " minutes"));
            mediaElement.Play();
        }
    }
}
