using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SpeechToText
{
    public enum LogLevel
    {
        None,
        Debug,
        Info,
        Warning,
        Exception
    }

    public class Logging
    {
        public static Logging Instance { get; private set; }

        private static readonly StorageFolder _folder = ApplicationData.Current.LocalFolder;
        private StorageFile _file;
        private StreamWriter _stream;

        /// <summary>
        /// Create a new logging instance and logfile
        /// </summary>
        public static void CreateLogger()
        {
            Instance = new Logging();

            string filename = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log";
            Task.Run(async () => 
            { 
                Instance._file = await _folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                Instance._stream = new StreamWriter(await Instance._file.OpenStreamForWriteAsync());
                await Instance.LogAsync("Logging started");
            }).Wait();
        }

        /// <summary>
        /// Write any remaining data in the stream to the file before destroying this object
        /// </summary>
        ~Logging()
        {
            _stream?.Flush();
            _stream?.Dispose();
        }

        private static readonly Dictionary<LogLevel, string> _prefix = new()
        {
            { LogLevel.None, "LOG" },
            { LogLevel.Debug, "LOG" },
            { LogLevel.Info, "LOG" },
            { LogLevel.Warning, "WRN" },
            { LogLevel.Exception, "EXC" }
        };

        /// <summary>
        /// Write <paramref name="message"/> to the file and Telegram debug group 
        /// if <paramref name="level"/> is at least <see cref="Settings.MinimumLogLevel"/>
        /// </summary>
        public async Task LogAsync(string message, LogLevel level = LogLevel.Info)
        {
            if (level < Settings.Instance.MinimumLogLevel)
                return;
            
            // Write to file
            await _stream.WriteLineAsync($"[{_prefix[level]} {DateTime.Now:HH:mm:ss.fff}] {message}");
            await _stream.FlushAsync();
            
            // Also send the message to the debug group
            await LogRemote(message);
        }

        /// <summary>
        /// Write <paramref name="message"/> to the file and Telegram debug group
        /// if <paramref name="level"/> is at least <see cref="Settings.MinimumLogLevel"/>
        /// </summary>
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            Task.Run(async () => await Instance.LogAsync(message, level)).Wait();
        }

        /// <summary>
        /// Write <paramref name="obj"/>.ToString() to he file and Telegram debug group
        /// if <paramref name="level"/> is at least <see cref="Settings.MinimumLogLevel"/>
        /// </summary>
        public static void Log(object obj, LogLevel level = LogLevel.Info)
        {
            Task.Run(async () => await Instance.LogAsync(obj.ToString(), level)).Wait();
        }

        /// <summary>
        /// Send <paramref name="message"/> to the Telegram debug group
        /// </summary>
        private static async Task LogRemote(string message)
        {
            if (App.TelegramConnection == null)
                return;

            try
            {
                string id = Settings.Instance.TelegramDebugGroup; // private remote monitoring group
                await App.TelegramConnection.Send(message, id);
            }
            catch (Exception) { }
        }
    }
}
