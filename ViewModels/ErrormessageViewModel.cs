using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SpeechToText.ViewModels
{
    public partial class ErrormessageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static ErrormessageViewModel Instance { get; private set; }

        public bool IsOpen { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public InfoBarSeverity Severity { get; private set; }

        public string ButtonMessage { get; private set; }
        public Action OnClick { get; set; }
        public bool ShowButton => ButtonMessage != null && OnClick != null;

        private int _height = 0;

        /// <summary>
        /// Show an infobar at the top of the UI. Use <see cref="ShowFromBackgroundthread"/>
        /// when not calling from the UI thread.
        /// </summary>
        /// <param name="title">Title of the banner</param>
        /// <param name="actionmessage">Text on the button</param>
        /// <param name="action">Function that is called by the button</param>
        /// <param name="description">Optional additional text between the title and button</param>
        /// <param name="severity">Influences color of the infobar, default is red</param>
        public void Show(string title, string actionmessage, Action action, 
            string description = "", InfoBarSeverity severity = InfoBarSeverity.Error)
        {
            if (IsOpen)
                Close();

            // The message should also be closed after performing the action
            void FullAction()
            {
                action();
                Close();
            }

            Title = title;
            Message = description;
            ButtonMessage = actionmessage;
            Severity = severity;
            OnClick = FullAction;
            IsOpen = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>
        /// Reset and hide the infobar
        /// </summary>
        public void Close()
        {
            IsOpen = false;
            Title = "";
            Message = "";
            ButtonMessage = "";
            OnClick = Close;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

            WindowViewModel.Instance.ResizeDelta(0, -_height);
            _height = 0;
        }

        public void PerformAction() => OnClick();

        /// <summary>
        /// Show an infobar at the top of the UI
        /// </summary>
        /// <param name="title">Title of the banner</param>
        /// <param name="actionmessage">Text on the button</param>
        /// <param name="action">Function that is called by the button</param>
        /// <param name="description">Optional additional text between the title and button</param>
        /// <param name="severity">Influences color of the infobar, default is red</param>
        public static void ShowFromBackgroundthread(string title, string actionmessage,
            Action action, string description = "", InfoBarSeverity severity = InfoBarSeverity.Error)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() => 
            {
                Instance.Show(title, actionmessage, action, description, severity);
                App.MainWindow.ErrorHeader.SizeChanged += ErrorHeaderResized;
            });
        }

        /// <summary>
        /// Resize the window so nothing falls off when showing the infobar
        /// </summary>
        private static void ErrorHeaderResized(object sender, SizeChangedEventArgs e)
        {
            App.MainWindow.ErrorHeader.SizeChanged -= ErrorHeaderResized;
            Instance._height = (int)e.NewSize.Height - (int)e.PreviousSize.Height;
            WindowViewModel.Instance.ResizeDelta(0, Instance._height);
        }

        public ErrormessageViewModel()
        {
            Instance = this;
        }
    }
}
