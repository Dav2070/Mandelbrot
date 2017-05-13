using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
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

        public MapPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ZeichnenButton_Click(object sender, RoutedEventArgs e)
        {
            int faktor = int.Parse(FaktorTextBox.Text);
            double genauigkeit = double.Parse(GenauigkeitTextBox.Text);

            CreateMap(faktor, genauigkeit);
        }

        private void CreateMap(int faktor, double genauigkeit)
        {
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
            Debug.WriteLine("Options Height: " + OptionsStackPanel.Height);

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

            for (double y = 0; y < pixelHeight; y = y+genauigkeit)
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
                        Tuple<int, List<Complex>> tuple = FileManager.Berechne(complex.Real, complex.Imaginary);
                        periodizitaet = tuple.Item1;
                        berechnungen++;
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

                    rect.Size = new Vector2(faktor, faktor);
                    rect.Offset = new Vector3(float.Parse((x * faktor).ToString()), float.Parse((y * faktor).ToString()), 0);
                    _root.Children.InsertAtTop(rect);
                }
                Debug.WriteLine("Fortschritt: " + 100 / pixelHeight * y);
            }
            Debug.WriteLine("Berechnungen: " + berechnungen);
        }
    }
}
