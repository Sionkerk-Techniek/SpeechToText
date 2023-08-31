using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using SpeechToText.ViewModels;
using static SpeechToText.Logging;

namespace SpeechToText
{
    public class SpeechRecognition
    {
        private AudioConfig _audioConfig;
        private TranslationRecognizer _translator;

        private bool _isReady = false;

        /// <summary>
        /// Create the configuration and 
        /// </summary>
        public void Initialise()
        {
            if (AudiosourceViewModel.Instance.SelectedDevice == null)
            {
                ShowMessage("Selecteer een audiobron");
            }

            SpeechTranslationConfig translationConfig = SpeechTranslationConfig.FromSubscription(
                Settings.Instance.AzureKey, Settings.Instance.AzureRegion);
            translationConfig.SpeechRecognitionLanguage = "nl-NL";
            translationConfig.AddTargetLanguage("uk");

            _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            //_audioConfig = AudioConfig.FromMicrophoneInput(AudiosourceViewModel.Instance.SelectedDevice.Id);
            _translator = new TranslationRecognizer(translationConfig, _audioConfig);
            _translator.Recognized += OnSpeechTranslated;
            _translator.SessionStarted += (object sender, SessionEventArgs e)
                => ShowMessage("Spraakherkenning gestart");
            _translator.SessionStopped += (object sender, SessionEventArgs e)
                => ShowMessage("Spraakherkenning gestopt");
            _translator.Canceled += OnTranslationError;

            _isReady = true;
        }

        public void Reset()
        {
            _translator?.Dispose();
            _audioConfig?.Dispose();

            Initialise();
        }

        public void Start()
        {
            if (!_isReady || _translator is null)
                Reset();

            _translator.StartContinuousRecognitionAsync();
        }

        public void Stop()
        {
            _translator?.StopContinuousRecognitionAsync();
        }

        public void OnTranslationError(object sender, TranslationRecognitionCanceledEventArgs e)
        {
            PlaystateViewModel.ChangeFromBackgroundthread(isPlaying: false);
            _isReady = false;  // Translator has to be restarted, possibly with new settings

            switch (e.ErrorCode)
            {
                case CancellationErrorCode.NoError:
                    break;
                case CancellationErrorCode.AuthenticationFailure:
                case CancellationErrorCode.Forbidden:
                    Log($"{e.ErrorCode}, {e.ErrorDetails}", LogLevel.Exception);
                    ShowError("Authentificatie mislukt", "Instellingen openen", Settings.ShowSettings,
                        "Controleer de key-instelling en probeer opnieuw");
                    break;
                case CancellationErrorCode.ConnectionFailure:
                    Log($"{e.ErrorCode}, {e.ErrorDetails}", LogLevel.Exception);
                    ShowError("Geen verbinding", "Instellingen openen", Settings.ShowSettings,
                        "Controleer de regio-instelling en probeer opnieuw");
                    break;
                default:
                    Log($"{e.ErrorCode}, {e.ErrorDetails}", LogLevel.Exception);
                    ShowError($"Er is een fout opgetreden: {e.ErrorCode}",
                        "Check instellingen misschien?", Settings.ShowSettings,
                        description: $"Details: {e.ErrorDetails}");
                    break;
            }
        }

        private void OnSpeechTranslated(object sender, TranslationRecognitionEventArgs e)
        {
            TranslationRecognitionResult result = e.Result;
            switch (result.Reason)
            {
                case ResultReason.TranslatedSpeech:
                    ShowMessage($"Recognized: {result.Text}",
                        $"Translation: {result.Translations[_translator.TargetLanguages[0]]}");
                    Log($"Recognized: {result.Text}\n Translation: " +
                        $"{result.Translations[_translator.TargetLanguages[0]]}");
                    Task.Run(async () => await TelegramConnection.Send(result.Translations[_translator.TargetLanguages[0]]));
                    // TODO: do something with the translation
                    break;
                case ResultReason.NoMatch:
                    ShowMessage($"Geen spraak herkend");
                    Log($"No speech recognized");
                    break;
                case ResultReason.Canceled:
                    CancellationDetails details = CancellationDetails.FromResult(result);
                    ShowMessage($"Geen vertalingsresultaat: {details.Reason}",
                        $"Error: {details.ErrorCode}, details: {details.ErrorDetails}");
                    Log($"No result: {details.Reason}. Error: {details.ErrorCode}, " +
                        $"details: {details.ErrorDetails}", LogLevel.Warning);
                    break;
                default:
                    Log($"Default case, result reason is {result.Reason}");
                    break;
            }
        }

        private static void ShowMessage(string message, string subtext = "")
        {
            LogCollectionViewModel.AddLogFromBackgroundthread(message, subtext);
        }

        private static void ShowError(string title, string actionmessage, Action action, string description = "")
        {
            Log("SpeechRecognition.ShowError: " + title);
            ErrormessageViewModel.ShowFromBackgroundthread(title, actionmessage, action, description);
        }
    }
}
