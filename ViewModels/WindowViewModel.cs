using System;
using System.ComponentModel;
using System.Drawing;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinUIEx;

namespace SpeechToText.ViewModels
{
    /// <summary>
    /// Handles resizing of the window, because apparently basic functionality isn't included
    /// </summary>
    public partial class WindowViewModel : INotifyPropertyChanged
    {
        public static WindowViewModel Instance { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly AppWindow _appWindow;

        public int MessagesHeight { get; }
        private int _extraHeight = 0;  // For error messages

        private bool _showMessages = false;
        public bool ShowMessages
        {
            get => _showMessages;
            set
            {
                if (_showMessages == value)
                    return;

                _showMessages = value;
                ToggleMessages();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowMessages)));
            }
        }

        private bool _onTop = false;
        public bool OnTop
        {
            get => _onTop;
            set
            {
                if (_onTop == value) 
                    return;

                _onTop = value;
                OverlappedPresenter presenter = _appWindow.Presenter as OverlappedPresenter;
                presenter.IsAlwaysOnTop = value;
            }
        }

        /// <summary>
        /// Sets various window settings such as size
        /// </summary>
        /// <param name="window">The window object to change</param>
        /// <param name="extendsIntoTitlebar">Set to true if the window provides its own titlebar</param>
        /// <param name="messagesHeight">Additional height in pixels added when messages are shown</param>
        public WindowViewModel(WindowEx window, int messagesHeight)
        {
            MessagesHeight = messagesHeight;

            // Get the window handle use it to get the AppWindow
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Use own titlebar
            _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

            // Set icon
            System.Drawing.Icon _icon = new(
                typeof(App).Assembly.GetManifestResourceStream("SpeechToText.Assets.Icon.ico"));
            _appWindow.SetIcon(Win32Interop.GetIconIdFromIcon(_icon.Handle));

            Instance = this;
        }

        /// <summary>s
        /// Resize the window to <paramref name="width"/> pixels wide
        /// and <paramref name="height"/> pixels high
        /// </summary>
        public void Resize(double width, double height)
        {
            _appWindow.Resize(new SizeInt32((int)width, (int)height));
        }

        /// <summary>
        /// Add <paramref name="width"/> pixels to the width
        /// and <paramref name="height"/> pixels to the height
        /// </summary>
        public void ResizeDelta(int deltaWidth, int deltaHeight)
        {
            SizeInt32 size = _appWindow.Size;
            _appWindow.Resize(new SizeInt32(size.Width + deltaWidth, size.Height + deltaHeight));
            _extraHeight += deltaHeight;
        }

        /// <summary>
        /// 'Hide' or 'show' the messages in the window by making the window larger/smaller
        /// </summary>
        public void ToggleMessages()
        {
            if (_showMessages)
                Resize(App.MainWindow.Width, App.MainWindow.Height + MessagesHeight + _extraHeight);
            else
                Resize(App.MainWindow.Width, App.MainWindow.Height - MessagesHeight + _extraHeight);
        }
    }
}
