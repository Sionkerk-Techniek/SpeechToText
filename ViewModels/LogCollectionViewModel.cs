using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace SpeechToText.ViewModels
{
    public class LogCollectionViewModel
    {
        public ObservableCollection<Log> Logs { get; } = new();

        public void UpdateSelected(object sender, SelectionChangedEventArgs e)
        {
            foreach (Log item in e.AddedItems)
                item.SetExpanded(true);

            foreach (Log item in e.RemovedItems)
                item.SetExpanded(false);
        }

        public void AddLog(string message, string subtext = "")
        {
            Logs.Add(new Log(message, subtext, DateTime.Now, new SolidColorBrush(Colors.DarkGray)));
        }

        public void AddLogFromBackgroundthread(string message)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() => AddLog(message));
        }

        public static void AddLogFromBackgroundthread(string message, string subtext)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() => Instance.AddLog(message, subtext));
        }

        public static LogCollectionViewModel Instance { get; private set; }
        public LogCollectionViewModel()
        {
            Instance = this;
        }
    }
}
