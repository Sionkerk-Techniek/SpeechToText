using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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

        public async Task LogAsync(string message, LogLevel level = LogLevel.Info)
        {
            if (level < Settings.Instance.MinimumLogLevel)
                return;

            await _stream.WriteLineAsync($"[{_prefix[level]} {DateTime.Now:HH:mm:ss.fff}] {message}");
            await _stream.FlushAsync();
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            Task.Run(async () => await Instance.LogAsync(message, level)).Wait();
        }

        public static void Log(object obj, LogLevel level = LogLevel.Info)
        {
            Task.Run(async () => await Instance.LogAsync(obj.ToString(), level));
        }
    }
}
