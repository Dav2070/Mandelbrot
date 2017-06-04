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
            GetHeightAndWidth();
            CreateUrsprung();
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
            SetColorPalette(compositor);

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
                            // Set the color for the given periodicity
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                new DispatchedHandler(() =>
                                {
                                    List<CompositionColorBrush> brushList = SetColorPalette(compositor);

                                    if (periodizitaet < 1)
                                    {
                                        rect.Brush = compositor.CreateColorBrush(Colors.White);
                                    }
                                    else if (periodizitaet >= brushList.Count)
                                    {   // Last color of the list is the color for high periodicity
                                        rect.Brush = brushList.Last();
                                    }
                                    else
                                    {
                                        rect.Brush = brushList.ElementAt(periodizitaet);
                                    }
                                }));
                            
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

        private List<CompositionColorBrush> SetColorPalette(Compositor compositor)
        {
            List<CompositionColorBrush> brushList = new List<CompositionColorBrush>();

            // https://material.io/guidelines/style/color.html#color-color-palette
            if (ColorPaletteComboBox.SelectedItem == StandardColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(Colors.Orange));
                brushList.Add(compositor.CreateColorBrush(Colors.Red));
                brushList.Add(compositor.CreateColorBrush(Colors.Blue));
                brushList.Add(compositor.CreateColorBrush(Colors.DarkBlue));
                brushList.Add(compositor.CreateColorBrush(Colors.Cyan));
                brushList.Add(compositor.CreateColorBrush(Colors.Aqua));
                brushList.Add(compositor.CreateColorBrush(Colors.Black));
                brushList.Add(compositor.CreateColorBrush(Colors.LightGray));
            }
            else if(ColorPaletteComboBox.SelectedItem == RedColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#b71c1c")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#c62828")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#d32f2f")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e53935")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#f44336")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ef5350")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e57373")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ef9a9a")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ffcdd2")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ffebee")));
            }
            else if (ColorPaletteComboBox.SelectedItem == IndigoColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#1a237e")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#283593")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#303f9f")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#3949ab")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#3f51b5")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#5c6bc0")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#7986cb")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#9fa8da")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#c5cae9")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e8eaf6")));
            }
            else if (ColorPaletteComboBox.SelectedItem == BlueColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#0d47a1")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#1565c0")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#1976d2")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#1e88e5")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#2196f3")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#42a5f5")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#64b5f6")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#90caf9")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#bbdefb")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e3f2fd")));
            }
            else if (ColorPaletteComboBox.SelectedItem == CyanColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#006064")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#00838f")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#0097a7")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#00acc1")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#00bcd4")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#26c6da")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#4dd0e1")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#80deea")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#b2ebf2")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e0f7fa")));
            }
            else if (ColorPaletteComboBox.SelectedItem == TealColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#004d40")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#00695c")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#00796b")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#00897b")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#009688")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#26a69a")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#4db6ac")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#80cbc4")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#b2dfdb")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e0f2f1")));
            }
            else if (ColorPaletteComboBox.SelectedItem == OrangeColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e65100")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ef6c00")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#f57c00")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#fb8c00")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ff9800")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ffa726")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ffb74d")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ffcc80")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#ffe0b2")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#fff3e0")));
            }
            else if (ColorPaletteComboBox.SelectedItem == GreyColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#212121")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#424242")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#616161")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#757575")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#9e9e9e")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#bdbdbd")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#e0e0e0")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#eeeeee")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#f5f5f5")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#fafafa")));
            }
            else if (ColorPaletteComboBox.SelectedItem == BlueGreyColorComboBoxItem)
            {
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#263238")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#37474f")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#455a64")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#546e7a")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#607d8b")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#78909c")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#90a4ae")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#b0bec5")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#cfd8dc")));
                brushList.Add(compositor.CreateColorBrush(FileManager.GetSolidColorBrush("#eceff1")));
            }

            return brushList;
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
