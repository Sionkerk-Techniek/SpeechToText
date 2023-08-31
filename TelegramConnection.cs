using SpeechToText.ViewModels;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Web;

namespace SpeechToText
{
    internal class TelegramConnection
    {
        private static string Token => Settings.Instance.TelegramToken;
        private static string ChatId => Settings.Instance.TelegramGroup;
        private readonly static HttpClient _httpClient = new();

        public static async Task Send(string text)
        {
            if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(ChatId) || !PlaystateViewModel.Instance.IsPosting)
                return;

            string safeText = HttpUtility.UrlEncode(text);
            await _httpClient.SendAsync(new HttpRequestMessage(
                HttpMethod.Post, $"https://api.telegram.org/bot{Token}/sendMessage?chat_id={ChatId}&text={safeText}"));
        }
    }
}
