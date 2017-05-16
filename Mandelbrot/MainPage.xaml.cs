using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace Mandelbrot
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CoreDispatcher dispatcher;

        public MainPage()
        {
            this.InitializeComponent();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            setDataContext();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void setDataContext()
        {
            ContentRoot.DataContext = (App.Current as App)._itemViewHolder;
        }

        private void SeeMapButton_Click(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            (App.Current as App)._itemViewHolder.page = typeof(MapPage);
        }

        private void SendNumberTextBox_Click(object sender, RoutedEventArgs e)
        {
            double x = -0.5;
            Double.TryParse(XTextBox.Text, out x);
            double y = 0.5;
            Double.TryParse(YTextBox.Text, out y);
            OutputTextBlock.Text = "";

            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                async (workItem) =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                new DispatchedHandler(() =>
                                {
                                    ProgressRing.IsActive = true;
                                }));

                    Tuple<int, List<Complex>> tuple = FileManager.Berechne(x, y);

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                new DispatchedHandler(() =>
                                {
                                    PeriodizitaetTextBlock.Text = tuple.Item1.ToString();
                                    if (tuple.Item2.Count > 3)
                                    {
                                        foreach (Complex z in tuple.Item2)
                                        {
                                            OutputTextBlock.Text += z.ToString() + "\n";
                                        }
                                    }

                                    ProgressRing.IsActive = false;
                                }));
                });
        }
    }
}
