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
        bool isCalculating = false;
        bool canvasIsEmpty = true;
        int faktor = 200;
        double genauigkeit = 0.1;
        int iterationen = 10000;
        double pixelHeight;
        double pixelWidth;
        double width = 100;
        double height = 100;
        double ursprungX;
        double ursprungY;
        Complex ursprung;

        public MapPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateUrsprung();
            GetHeightAndWidth();
        }

        private void GetHeightAndWidth()
        {
            width = Window.Current.Bounds.Width;
            height = Window.Current.Bounds.Height - ControlsGrid.ActualHeight;

            pixelWidth = width / faktor;
            pixelHeight = height / faktor;
        }

        private void CreateUrsprung()
        {
            // Ursprung = (x/1,5|y/2)
            // Oben links Ecke = (x-ursprungX|y-ursprungY)
            // Oben links Ecke + x1 = (x-ursprungX|y-ursprungY)

            ursprungX = (pixelWidth / 1.5);
            ursprungY = (pixelHeight / 2);
            ursprung = new Complex(ursprungX, ursprungY);

            Debug.WriteLine(ursprungX + " " + ursprungY);
            Debug.WriteLine("PixelWidth: " + pixelWidth + " pixelHeight: " + pixelHeight);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            asyncAction.Cancel();
        }

        private void CreateMap()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            isCalculating = true;
            canvasIsEmpty = false;
            GetHeightAndWidth();

            _root = FileManager.GetVisual(MapStackPanel);

            // Get the current compositor
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            /*
            CompositionColorBrush white = compositor.CreateColorBrush(Colors.White);
            CompositionColorBrush orange = compositor.CreateColorBrush(Colors.Orange);
            CompositionColorBrush red = compositor.CreateColorBrush(Colors.Red);
            CompositionColorBrush blue = compositor.CreateColorBrush(Colors.Blue);
            CompositionColorBrush darkBlue = compositor.CreateColorBrush(Colors.DarkBlue);
            CompositionColorBrush cyan = compositor.CreateColorBrush(Colors.Cyan);
            CompositionColorBrush aqua = compositor.CreateColorBrush(Colors.Aqua);
            CompositionColorBrush black = compositor.CreateColorBrush(Colors.Black);
            */
            CompositionColorBrush colorOne = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#1a237e"));
            CompositionColorBrush colorTwo = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#283593"));
            CompositionColorBrush colorThree = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#303f9f"));
            CompositionColorBrush colorFour = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#3949ab"));
            CompositionColorBrush colorFive = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#3f51b5"));
            CompositionColorBrush colorSix = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#5c6bc0"));
            CompositionColorBrush colorSeven = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#7986cb"));
            CompositionColorBrush colorEight = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#9fa8da"));
            CompositionColorBrush colorNine = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#c5cae9"));
            CompositionColorBrush colorTen = compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e8eaf6"));

            int berechnungen = 0;
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

                            int periodizitaet = -1;
                            if (x - ursprungX < 5 && x - ursprungX > -5
                                && y - ursprungY < 5 && y - ursprungY > -5)
                            {
                                Complex complex = new Complex(x - ursprungX, y - ursprungY);
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
                                    rect.Brush = colorTen;
                                    break;
                                case 1:
                                    rect.Brush = colorOne;
                                    break;
                                case 2:
                                    rect.Brush = colorTwo;
                                    break;
                                case 3:
                                    rect.Brush = colorThree;
                                    break;
                                case 4:
                                    rect.Brush = colorFour;
                                    break;
                                case 5:
                                    rect.Brush = colorFive;
                                    break;
                                case 6:
                                    rect.Brush = colorSix;
                                    break;
                                case 7:
                                    rect.Brush = colorSeven;
                                    break;
                                case 8:
                                    rect.Brush = colorEight;
                                    break;
                                case 9:
                                    rect.Brush = colorNine;
                                    break;
                                default:
                                    rect.Brush = colorTen;
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
                    Debug.WriteLine(ursprungX + " " + ursprungY);
                    string elapsedTime = stopwatch.Elapsed.ToString();
                    isCalculating = false;

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

        private void ZoomIn(double x, double y)
        {
            // Get middle of canvas in pixel
            double xMitte = (width / faktor) / 2;
            double yMitte = (height / faktor) / 2;
            // Translate the clicked point into pixel
            double xPixel = x / faktor;
            double yPixel = y / faktor;

            Debug.WriteLine("Clicked point: " + xPixel/ ursprungX + " " + yPixel/ ursprungY);
            ursprungX = ursprungX + (xMitte-xPixel);
            ursprungY = ursprungY + (yMitte-yPixel);

            faktor = faktor + 200;
            //ursprungX = ursprungX - (ursprungX / 3);
            //ursprungY = ursprungY - (ursprungY / 3);
            Bindings.Update();
            CreateMap();
        }

        private void ZeichnenButton_Click(object sender, RoutedEventArgs e)
        {
            CreateMap();
        }

        private void ContentRoot_Tapped(object sender, TappedRoutedEventArgs e)
        {
            double x = e.GetPosition(ContentRoot).X;
            double y = e.GetPosition(ContentRoot).Y - ControlsGrid.ActualHeight;
            if(y > 0 && !isCalculating && !canvasIsEmpty)
            {
                ZoomIn(x, y);
            }
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
