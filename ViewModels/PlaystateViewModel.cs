using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace SpeechToText.ViewModels
{
    public class PlaystateViewModel : INotifyPropertyChanged
    {
        public static PlaystateViewModel Instance { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void UpdateAll()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                _isPaused = false;

                UpdateAll();

                if (_isPlaying)
                    App.Recognizer.Start();
                else
                    App.Recognizer.Stop();
            }
        }

        private bool _isPaused = false;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                UpdateAll();

                if (_isPaused)
                    App.Recognizer.Stop();
                else
                    App.Recognizer.Start();
            }
        }

        public bool IsStopped => !IsPlaying;

        public bool IsPosting { get; set; } = false;

        #pragma warning disable IDE0060 // Remove unused parameters
        public void Play(object sender, RoutedEventArgs e) => IsPlaying = true;
        public void Pause(object sender, RoutedEventArgs e) => IsPaused = !IsPaused;
        public void Stop(object sender, RoutedEventArgs e) => IsPlaying = false;
        #pragma warning restore IDE0060

        public static void ChangeFromBackgroundthread(bool isPlaying)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() => Instance.IsPlaying = isPlaying);
        }

        public static async Task ChangeFromTelegramCommand(bool translate)
        {
            if (translate)
            {
                if (Instance.IsPlaying)
                    await App.TelegramConnection.Send("Translation is already in progress");
                else
                {
                    await App.TelegramConnection.Send("Starting translation");
                    ChangeFromBackgroundthread(translate);
                }
            }
            else
            {
                if (Instance.IsStopped)
                    await App.TelegramConnection.Send("Translation is already stopped");
                else
                {
                    await App.TelegramConnection.Send("Stopping translation");
                    ChangeFromBackgroundthread(translate);
                }
            }
        }

        public PlaystateViewModel()
        {
            Instance = this;
        }
    }
}
