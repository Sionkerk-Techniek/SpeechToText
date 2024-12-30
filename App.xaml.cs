using Microsoft.UI.Xaml;
using SpeechToText.ViewModels;
using static SpeechToText.Logging;

namespace SpeechToText
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow MainWindow { get; private set; }
        public static SpeechRecognition Recognizer { get; private set; }
        public static TelegramConnection TelegramConnection { get; private set; }

        /// <summary>
        /// Program entry point
        /// </summary>
        public App()
        {
            UnhandledException += HandleException;
            InitializeComponent();
        }

        private void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            Log($"Unhandled exception: {e}, {e.Message}", LogLevel.Exception);
            Exit();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            CreateLogger();
            MainWindow = new MainWindow();
            MainWindow.Activate();
            await Settings.Load();
            Settings.Instance.CheckForDefaults();
            AudiosourceViewModel.Instance.Initialise();
            Recognizer = new SpeechRecognition();
            Recognizer.Initialise();
            TelegramConnection = new TelegramConnection();
        }
    }
}
