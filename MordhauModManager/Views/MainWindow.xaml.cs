using MordhauModManager.ViewModels;

namespace MordhauModManager.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if(DataContext is IViewListener listener)
            {
                listener.OnLoaded();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IViewListener listener)
            {
                listener.OnClosing();
            }
        }
    }
}
