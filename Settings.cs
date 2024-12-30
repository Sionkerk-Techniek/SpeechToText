using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SpeechToText.ViewModels;
using Windows.Storage;
using static SpeechToText.Logging;

namespace SpeechToText
{
    public class Settings
    {
        public static Settings Instance { get; private set; } = new();

        private static readonly StorageFolder _folder = ApplicationData.Current.LocalFolder;
        private const string _fileName = "settings.json";
        private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public string AzureKey { get; init; } = "key";
        public string AzureRegion { get; init; } = "region";
        public string LastAudioDeviceId { get; set; }
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;
        public string TelegramToken { get; init; }
        public Dictionary<string, string> TelegramGroup { get; init; }
        public string TelegramDebugGroup { get; init; }

        /// <summary>
        /// Deserialize settings from <see cref="_fileName"/> located in <see cref="_folder"/>
        /// </summary>
        public async static Task Load()
        {
            try
            {
                using Stream stream = await _folder.OpenStreamForReadAsync(_fileName);
                Instance = JsonSerializer.Deserialize<Settings>(stream);
                Instance.CheckForDefaults();
                Log("Settings loaded");
            }
            catch (FileNotFoundException e)
            {
                Log(e, LogLevel.Exception);
                await Instance.Save();  // Create a new file with default values
                ShowError("Geen instellingen gevonden, er is een nieuw bestand gemaakt",
                    "Open instellingen", ShowSettings);
            }
            catch (IOException e)
            {
                Log(e, LogLevel.Exception);
                ShowError($"Instellingen konden niet worden geopend: {e.GetType()}",
                    "Exit", App.Current.Exit, e.Message);
            }
            catch (Exception e)
            {
                Log(e, LogLevel.Exception);
                ShowError($"Error: {e.GetType()}", "Exit", App.Current.Exit, e.Message);
            }
        }

        /// <summary>
        /// Serialize settings to <see cref="_fileName"/>, overwriting old settings
        /// </summary>
        public async Task Save()
        {
            using Stream stream = await _folder.OpenStreamForWriteAsync(_fileName,
                CreationCollisionOption.ReplaceExisting);
            JsonSerializer.Serialize(stream, this, _options);
        }

        /// <summary>
        /// Open the folder the settings file is in and quit
        /// </summary>
        public static async void ShowSettings()
        {
            await Windows.System.Launcher.LaunchFolderAsync(_folder);
            App.Current.Exit();
        }

        /// <summary>
        /// Show a warning banner at the top of the UI
        /// </summary>
        /// <param name="title">Title of the banner</param>
        /// <param name="actionmessage">Text on the button</param>
        /// <param name="action">Function that is called by the button</param>
        /// <param name="description">Optional additional text between the title and button</param>
        private static void ShowError(string title, string actionmessage, Action action, string description = "")
            => ErrormessageViewModel.ShowFromBackgroundthread(title, actionmessage, action, description);

        /// <summary>
        /// Display an error message if Azure or Telegram settings are left on the default value
        /// </summary>
        private void CheckForDefaults()
        {
            if (AzureKey is "key" || AzureRegion is "region")
            {
                Log("Azure key or region is not set", LogLevel.Exception);
                ShowError("Er is nog geen Azure sleutel en regio ingesteld",
                    "Open instellingen", ShowSettings);
            }

            if (string.IsNullOrWhiteSpace(TelegramToken) || string.IsNullOrWhiteSpace(TelegramDebugGroup)
                || TelegramGroup?.Count == 0)
            {
                Log("Telegram token, debug group or target group has not been set", LogLevel.Exception);
                ShowError("Telegram token of groep is not niet ingesteld",
                    "Open instellingen", ShowSettings);
            }
        }
    }
}
