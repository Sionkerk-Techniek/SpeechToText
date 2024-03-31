using SpeechToText.ViewModels;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading;

namespace SpeechToText
{
    public class TelegramConnection
    {
        internal string Token => Settings.Instance.TelegramToken;
        private static string ChatId => Settings.Instance.TelegramGroup;

        private readonly TelegramBotClient _client;

        public TelegramConnection()
        {
            // Create client
            if (_client != null)
               return;

            _client = new TelegramBotClient(Settings.Instance.TelegramToken);

            // Subscribe to messages
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            using CancellationTokenSource cts = new();
            _client.StartReceiving(
                updateHandler: HandleMessage,
                pollingErrorHandler: HandlePollingError,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Logging.Log("Started listening to telegram updates");
        }

        private async Task HandleMessage(ITelegramBotClient bot, Update update, CancellationToken cancelToken)
        {
            if (update.Message == null)
                return;

            string id = update.Message.Chat.Id.ToString();
            string text = update.Message.Text ?? "null";
            Logging.Log($"Telegram message received from {id}: " + text);

            // Only react to messages from these two sources
            if (id != Settings.Instance.TelegramGroup && id != Settings.Instance.TelegramDebugGroup)
            {
                Logging.Log("Ignoring message from unauthorized user or group");
                return;
            }

            // Handle commands, send confirmations
            if (text.StartsWith("/starttranslation"))
            {
                Logging.Log("Received /starttranslation");
                bool success = PlaystateViewModel.ChangeFromTelegramCommand(translate: true);
                if (success)
                    await Send("Starting translation");
                else
                    await Send("Translation is already in progress");
            }
            else if (text.StartsWith("/stoptranslation"))
            {
                Logging.Log("Received /stoptranslation");
                bool success = PlaystateViewModel.ChangeFromTelegramCommand(translate: false);
                if (success)
                    await Send("Stopping translation");
                else
                    await Send("Translation is already stopped");
            }
            else if (text.StartsWith("/ping"))
            {
                await Send("Pong!", id);
            }
        }

        private Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string error = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error: {apiRequestException.ErrorCode}: {apiRequestException.Message}",
                _ => exception.ToString()
            };

            Logging.Log(error, LogLevel.Exception);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send a message to <see cref="ChatId"/> if <see cref="PlaystateViewModel.Instance.IsPosting"/> is true
        /// </summary>
        public async Task Send(string text)
        {
            if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(ChatId) || !PlaystateViewModel.Instance.IsPosting)
                return;

            await _client.SendTextMessageAsync(ChatId, text);
        }

        /// <summary>
        /// Send a message to <paramref name="chatId"/>
        /// </summary>
        public async Task Send(string text, string chatId)
        {
            await _client.SendTextMessageAsync(chatId, text);
        }
    }
}
