using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Mandelbrot
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        private ContainerVisual _root;
        IAsyncAction asyncAction;

        int faktor = 200;
        double genauigkeit = 0.1;
        int iterationen = 10000;

        public MapPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            asyncAction.Cancel();
        }

        private void CreateMap(int faktor, double genauigkeit, int iterationen)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _root = FileManager.GetVisual(MapStackPanel);

            // Get the current compositor
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            CompositionColorBrush white = compositor.CreateColorBrush(Colors.White);
            CompositionColorBrush orange = compositor.CreateColorBrush(Colors.Orange);
            CompositionColorBrush red = compositor.CreateColorBrush(Colors.Red);
            CompositionColorBrush blue = compositor.CreateColorBrush(Colors.Blue);
            CompositionColorBrush darkBlue = compositor.CreateColorBrush(Colors.DarkBlue);
            CompositionColorBrush cyan = compositor.CreateColorBrush(Colors.Cyan);
            CompositionColorBrush aqua = compositor.CreateColorBrush(Colors.Aqua);
            CompositionColorBrush black = compositor.CreateColorBrush(Colors.Black);

            int berechnungen = 0;

            double width = Window.Current.Bounds.Width;
            double height = Window.Current.Bounds.Height;

            double pixelOverall = (width * height) / faktor;
            double pixelWidth = width/faktor;
            double pixelHeight = height/faktor;
            Debug.WriteLine("PixelHeight: " + pixelHeight);

            // Ursprung = (x/1,5|y/2)
            // Oben links Ecke = (x-ursprungX|y-ursprungY)
            // Oben links Ecke + x1 = (x-ursprungX|y-ursprungY)
            double ursprungX = (pixelWidth / 1.5);
            double ursprungY = (pixelHeight / 2);
            Complex ursprung = new Complex(ursprungX, ursprungY);
            Debug.WriteLine("Ursprung X: " + ursprungX + " Ursprung Y: " + ursprungY);

            double fortschritt = 0;
            //CancelButton.Visibility = Visibility.Visible;

            asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                async (workItem) =>
                {
                    for (double y = 0; y < pixelHeight; y = y + genauigkeit)
                    {
                        for (double x = 0; x < pixelWidth; x = x + genauigkeit)
                        {
                            SpriteVisual rect = compositor.CreateSpriteVisual();
                            //Debug.WriteLine(Math.Floor(Decimal.Ceiling(x / 10) / 2));

                            int periodizitaet = -1;
                            if (x - ursprung.Real < 5 && x - ursprung.Real > -5
                                && y - ursprung.Imaginary < 5 && y - ursprung.Imaginary > -5)
                            {
                                Complex complex = new Complex(x - ursprung.Real, y - ursprung.Imaginary);
                                Tuple<int, List<Complex>> tuple = FileManager.Berechne(complex.Real, complex.Imaginary, iterationen);
                                periodizitaet = tuple.Item1;
                                berechnungen++;
                                tuple = null;

                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }
                            
                            switch (periodizitaet)
                            {
                                case -1:
                                    rect.Brush = white;
                                    break;
                                case 1:
                                    rect.Brush = orange;
                                    break;
                                case 2:
                                    rect.Brush = red;
                                    break;
                                case 3:
                                    rect.Brush = cyan;
                                    break;
                                case 4:
                                    rect.Brush = blue;
                                    break;
                                case 5:
                                    rect.Brush = darkBlue;
                                    break;
                                case 6:
                                    rect.Brush = black;
                                    break;
                                default:
                                    rect.Brush = white;
                                    break;
                            }
                            
                            //rect.Brush = compositor.CreateColorBrush(Color.FromArgb(255, byte.Parse(((berechnungen/periodizitaet)/7).ToString()), byte.Parse(((berechnungen/periodizitaet) / 7).ToString()), byte.Parse((periodizitaet / 4).ToString()))); //(CompositionBrush) Color.FromArgb(255, (byte)periodizitaet, byte.Parse((periodizitaet / 2).ToString()), byte.Parse((periodizitaet / 5).ToString()));
                            rect.Size = new Vector2(faktor, faktor);
                            rect.Offset = new Vector3(float.Parse((x * faktor).ToString()), float.Parse((y * faktor).ToString()), 0);
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                new DispatchedHandler(() =>
                                {
                                    _root.Children.InsertAtTop(rect);
                                }));
                            rect = null;
                        }

                        fortschritt = Math.Round(100 / pixelHeight * y, 1);
                        Debug.WriteLine("Fortschritt: " + fortschritt);
                        
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                new DispatchedHandler(() =>
                                {
                                    ProgressTextBlock.Text = fortschritt.ToString() + " %";
                                    ProgressBar.Value = fortschritt;
                                }));
                    }

                    stopwatch.Stop();
                    string elapsedTime = stopwatch.Elapsed.ToString();

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        new DispatchedHandler(() =>
                        {
                            // Reset UI
                            //CancelButton.Visibility = Visibility.Collapsed;
                            ProgressBar.Value = 0;
                            ProgressTextBlock.Text = berechnungen.ToString() + " Berechnungen in " + elapsedTime;
                            Debug.WriteLine("Berechnungen: " + berechnungen + ", vergangene Zeit: " + elapsedTime);
                        }));
                });

            asyncAction.Completed = new AsyncActionCompletedHandler(
                async (IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                {
                    Debug.WriteLine("Beendet");
                    // Update the UI thread with the CoreDispatcher.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        new DispatchedHandler(() =>
                        {
                            if (asyncStatus == AsyncStatus.Canceled)
                            {
                                //CancelButton.Visibility = Visibility.Collapsed;
                                ProgressBar.Value = 0;
                            }
                        }));
                });
        }

        private void ZeichnenButton_Click(object sender, RoutedEventArgs e)
        {
            CreateMap(faktor, genauigkeit, iterationen);
        }

        private void ContentRoot_Tapped(object sender, TappedRoutedEventArgs e)
        {
            double x = e.GetPosition(ContentRoot).X;
            double y = e.GetPosition(ContentRoot).Y - ControlsGrid.ActualHeight;
            Debug.WriteLine(x + " " + y);
        }

        private void GenauigkeitTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double.TryParse(GenauigkeitTextBox.Text, out genauigkeit);
        }

        private void FaktorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(FaktorTextBox.Text, out faktor);
        }

        private void IterationenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(IterationenTextBox.Text, out iterationen);
        }
    }
}
