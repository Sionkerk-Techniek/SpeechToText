using SpeechToText.ViewModels;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using System.Threading;
using System.Collections.Generic;
using static SpeechToText.Logging;

namespace SpeechToText
{
    public class TelegramConnection
    {
        private readonly TelegramBotClient _client;

        public TelegramConnection()
        {
            // Create client
            _client = new TelegramBotClient(Settings.Instance.TelegramToken);

            // Subscribe to all new updates
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = [],
                DropPendingUpdates = true
            };

            using CancellationTokenSource cts = new();
            _client.StartReceiving(
                updateHandler: HandleMessage,
                errorHandler: HandlePollingError,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Log("Started listening to Telegram updates");
            Task.Run(VerifyConnection).Wait();
        }

        /// <summary>
        /// Process incoming commands from whitelisted sources and ignore all other updates
        /// </summary>
        private async Task HandleMessage(ITelegramBotClient bot,
            Update update, CancellationToken cancelToken)
        {
            // Updates that are not messages are ignored
            if (update.Message == null)
                return;

            // Extract and log message text and source
            string id = update.Message.Chat.Id.ToString();
            string text = update.Message.Text ?? "null";
            Log($"Telegram message received from {id}: " + text);

            // Only react to messages from these two sources
            if (!Settings.Instance.TelegramGroup.ContainsValue(id) 
                && id != Settings.Instance.TelegramDebugGroup)
            {
                Log("Ignoring message from unauthorized user or group");
                return;
            }

            // Handle commands, send confirmations
            if (text.StartsWith("/starttranslation"))
            {
                Log("Received /starttranslation");
                bool success = PlaystateViewModel.ChangeFromTelegramCommand(translate: true);
                if (success)
                    await Broadcast("Starting translation");
                else
                    await Send("Translation is already in progress", id);
            }
            else if (text.StartsWith("/stoptranslation"))
            {
                Log("Received /stoptranslation");
                bool success = PlaystateViewModel.ChangeFromTelegramCommand(translate: false);
                if (success)
                    await Broadcast("Stopping translation");
                else
                    await Send("Translation is already stopped", id);
            }
            else if (text.StartsWith("/ping"))
            {
                await Send("Pong!", id);
            }
        }

        /// <summary>
        /// Log errors and show a message in the UI when a Telegram error occurs
        /// </summary>
        private Task HandlePollingError(ITelegramBotClient botClient,
            Exception exception, CancellationToken cancellationToken)
        {
            string error = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error: {apiRequestException.ErrorCode}:" +
                    $" {apiRequestException.Message}",
                _ => exception.ToString()
            };

            Log(error, LogLevel.Exception);
            ShowError($"Telegram error: {exception.GetType()}", exception.Message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send a message to <paramref name="chatId"/>
        /// </summary>
        public async Task Send(string text, string chatId)
        {
            await _client.SendMessage(chatId, text);
        }

        /// <summary>
        /// Send each translation to the appropiate channel
        /// </summary>
        /// <param name="translations">language:translation dictionary</param>
        public async Task Send(IReadOnlyDictionary<string, string> translations)
        {
            foreach (KeyValuePair<string, string> translation in translations)
                await Send(translation.Value, Settings.Instance.TelegramGroup[translation.Key]);
        }

        /// <summary>
        /// Send <paramref name="text"/> to all groups
        /// </summary>
        public async Task Broadcast(string text)
        {
            foreach (string chatId in Settings.Instance.TelegramGroup.Values)
                await Send(text, chatId);
        }

        /// <summary>
        /// Check if the bot token is valid, and if the bot can send messages
        /// to all groups the bot should have access to
        /// </summary>
        /// <returns>false if there are configuration errors, otherwise true</returns>
        private async Task<bool> VerifyConnection()
        {
            try
            {
                // Verify token
                await _client.GetMe();

                // Verify write access to debug group
                ChatFullInfo info = await _client.GetChat(Settings.Instance.TelegramDebugGroup);
                if (!info.Permissions.CanSendMessages)
                {
                    Log($"Unable to write to debug group {info.Id}", LogLevel.Warning);
                    ShowError("Telegram connection unsuccessful: no debug group access");
                    return false;
                }

                // Verify write access to target group
                foreach (string chatId in Settings.Instance.TelegramGroup.Values)
                {
                    info = await _client.GetChat(chatId);
                    if (!info.Permissions.CanSendMessages)
                    {
                        Log($"Unable to write to target group {chatId}", LogLevel.Warning);
                        ShowError($"Telegram connection unsuccessful: no access to {chatId}");
                        return false;
                    }
                }

                return true;
            }
            catch (ApiRequestException e)
            {
                Log($"ApiRequestException when validating Telegram connection:" +
                    $" {e.ErrorCode}: {e.Message}", LogLevel.Exception);
                ShowError("Telegram returned an error", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Show the Telegram error to the user
        /// </summary>
        private static void ShowError(string title, string description = "")
        {
            Log("SpeechRecognition.ShowError: " + title);
            ErrormessageViewModel.ShowFromBackgroundthread(title, "ok", () => {}, description);
        }
    }
}
