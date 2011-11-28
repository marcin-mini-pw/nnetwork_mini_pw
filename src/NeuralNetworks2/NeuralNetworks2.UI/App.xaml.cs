using System;
using System.Windows;
using NeuralNetworks2.UI.ViewModels;

namespace NeuralNetworks2.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var mainWindowViewModel = MainWindowViewModel.Instance;
            var mainWindow = new MainWindow { DataContext = mainWindowViewModel };
            mainWindow.Closed += MainWindow_Closed;

            //jeśli viewmodel poprosi o zamknięcie okienka, czynimy to
            EventHandler handler = null;
            handler = delegate
            {
                mainWindowViewModel.RequestClose -= handler;
                mainWindow.Close();
                Shutdown();
            };
            mainWindowViewModel.RequestClose += handler;
            if(!mainWindowViewModel.Init())
            {
                return;
            }

            mainWindow.Show();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Shutdown();
        }
    }
}