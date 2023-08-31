using System.ComponentModel;
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

        public PlaystateViewModel()
        {
            Instance = this;
        }
    }
}
