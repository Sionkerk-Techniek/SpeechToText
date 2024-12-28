using Microsoft.UI.Xaml;
using SpeechToText.ViewModels;

namespace SpeechToText
{
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        private readonly WindowViewModel _windowViewModel;
        public readonly PlaystateViewModel Playstate;
        public readonly LogCollectionViewModel Logs;
        public readonly AudiosourceViewModel Audiosource;
        public readonly ErrormessageViewModel Errormessage;

        public MainWindow()
        {
            // TODO: use constructor which accepts viewmodels as arguments
            _windowViewModel = new WindowViewModel(this, messagesHeight: 250);
            Playstate = new PlaystateViewModel();
            Logs = new LogCollectionViewModel();
            Errormessage = new ErrormessageViewModel();
            Audiosource = new AudiosourceViewModel();

            this.SetTitleBar(Titlebar);
            this.InitializeComponent();
        }
    }
}
