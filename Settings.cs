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
        public static Settings Instance { get; private set; } = new Settings();

        private static readonly StorageFolder _folder = ApplicationData.Current.LocalFolder;
        private const string _fileName = "settings.json";

        public string AzureKey { get; init; } = "key";
        public string AzureRegion { get; init; } = "region";
        public string LastAudioDeviceId { get; set; }
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;
        public string TelegramToken { get; init; }
        public Dictionary<string, string> TelegramGroup { get; init; }
        public string TelegramDebugGroup { get; init; }

        public async static Task Load()
        {
            try
            {
                using Stream stream = await _folder.OpenStreamForReadAsync(_fileName);
                Instance = JsonSerializer.Deserialize<Settings>(stream);
                Log("Settings loaded");
            }
            catch (FileNotFoundException e)
            {
                Log(e, LogLevel.Exception);
                await Instance.Save();  // Create a new file with default values
                ShowError("Er is nog geen Azure sleutel en regio ingesteld",
                    "Open instellingen", ShowSettings);
            }
            catch (IOException e)
            {
                Log(e, LogLevel.Exception);
                ShowError("Instellingen konden niet worden geopend",
                    "Exit", App.Current.Exit);
            }
            catch (Exception e)
            {
                Log(e, LogLevel.Exception);
                ShowError($"Error: {e.GetType()}",
                    "Exit", App.Current.Exit, e.Message);
            }
        }

        public async Task Save()
        {
            using Stream stream = await _folder.OpenStreamForWriteAsync(_fileName,
                CreationCollisionOption.ReplaceExisting);
            JsonSerializerOptions options = new() { WriteIndented = true };
            JsonSerializer.Serialize(stream, this, options);
        }

        public static async void ShowSettings()
        {
            await Windows.System.Launcher.LaunchFolderAsync(_folder);
        }

        private static void ShowError(string title, string actionmessage, Action action, string description = "")
        {
            Log("Settings.ShowError: " + title);
            ErrormessageViewModel.ShowFromBackgroundthread(title, actionmessage, action, description);
        }
    }
}
