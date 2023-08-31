using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace SpeechToText.ViewModels
{
    public class Log : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string MainText { get; }
        public string SubText { get; }
        public bool ShowSubtext => !string.IsNullOrWhiteSpace(SubText) && Expanded;
        public DateTime Time { get; }
        public Brush Background { get; }
        public bool Expanded { get; private set; } = false;
        
        public Log(string text, string subtext, DateTime time, Brush background)
        {
            MainText = text;
            SubText = subtext;
            Time = time;
            Background = background;
            Expanded = false;
        }

        public void SetExpanded(bool value)
        {
            if (Expanded == value) return;

            Expanded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Expanded)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowSubtext)));
        }

        public override string ToString()
        {
            return $"{MainText} ({Time})";
        }
    }
}
