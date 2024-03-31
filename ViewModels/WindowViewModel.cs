using System;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace SpeechToText.ViewModels
{
    /// <summary>
    /// Handles resizing of the window, because apparently basic functionality isn't included
    /// </summary>
    public class WindowViewModel : INotifyPropertyChanged
    {
        public static WindowViewModel Instance { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly AppWindow _appWindow;

        public int Width { get; private set; }
        public int Height { get; private set; }
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
        /// <param name="width">The window width in pixels</param>
        /// <param name="height">The window height in pixels</param>
        /// <param name="resizable">Whether the user is allowed to resize the window</param>
        /// <param name="maximizable">Whether the user is allowed to maximize the window</param>
        /// <param name="extendsIntoTitlebar">Set to true if the window provides its own titlebar</param>
        /// <param name="messagesHeight">Additional height in pixels added when messages are shown</param>
        public WindowViewModel(Window window, int width, int height,
            bool resizable, bool maximizable, bool extendsIntoTitlebar, int messagesHeight)
        {
            Width = width;
            Height = height;
            MessagesHeight = messagesHeight;

            // Get the window handle use it to get the AppWindow
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Use own titlebar, disable resizing by the user
            _appWindow.TitleBar.ExtendsContentIntoTitleBar = extendsIntoTitlebar;
            OverlappedPresenter presenter = _appWindow.Presenter as OverlappedPresenter;
            presenter.IsResizable = resizable;
            presenter.IsMaximizable = maximizable;
            presenter.IsAlwaysOnTop = true;

            Resize(width, height);
            Instance = this;
        }

        /// <summary>
        /// Resize the window to <paramref name="width"/> pixels wide
        /// and <paramref name="height"/> pixels high
        /// </summary>
        public void Resize(int width, int height)
        {
            _appWindow.Resize(new SizeInt32(width, height));
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
                Resize(Width, Height + MessagesHeight + _extraHeight);
            else
                Resize(Width, Height + _extraHeight);
        }
    }
}
