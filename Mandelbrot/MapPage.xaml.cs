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
            CreateMap();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CreateMap()
        {
            _root = FileManager.GetVisual(ContentRoot);

            // Get the current compositor
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            CompositionColorBrush orange = compositor.CreateColorBrush(Colors.Orange);
            CompositionColorBrush red = compositor.CreateColorBrush(Colors.Red);
            CompositionColorBrush blue = compositor.CreateColorBrush(Colors.Blue);
            CompositionColorBrush darkBlue = compositor.CreateColorBrush(Colors.DarkBlue);
            CompositionColorBrush cyan = compositor.CreateColorBrush(Colors.Cyan);
            CompositionColorBrush aqua = compositor.CreateColorBrush(Colors.Aqua);

            int factor = 10;
            double pixelOverall = (Window.Current.Bounds.Width * Window.Current.Bounds.Height) / factor;
            double pixelWidth = Window.Current.Bounds.Width/factor;
            double pixelHeight = Window.Current.Bounds.Height/factor;
            Debug.WriteLine("Pixel Width: " + pixelWidth);


            // Ursprung = (x/1,5|y/2)
            // Oben links Ecke = (x-ursprungX|y-ursprungY)
            // Oben links Ecke + x1 = (x-ursprungX|y-ursprungY)
            double ursprungX = pixelWidth / 1.5;
            double ursprungY = pixelHeight / 2;
            Complex ursprung = new Complex(pixelWidth / 1.5, pixelHeight / 2);
            Debug.WriteLine("Ursprung X: " + ursprungX + " Ursprung Y: " + ursprungY);

            for (int y = 0; y < pixelHeight; y++)
            {
                for (int x = 0; x < pixelWidth; x++)
                {
                    SpriteVisual rect = compositor.CreateSpriteVisual();
                    //Debug.WriteLine(Math.Floor(Decimal.Ceiling(x / 10) / 2));

                    Complex complex = new Complex(x - ursprung.Real, y - ursprung.Imaginary);
                    int periodiziteat = FileManager.Berechne(complex.Real, complex.Imaginary);
                    Debug.WriteLine("Periodizität von " + complex + ": " + periodiziteat);

                    switch (periodiziteat)
                    {
                        case -1:
                            rect.Brush = red;
                            break;
                        default:
                            rect.Brush = blue;
                            break;
                    }

                    /*
                    switch (Math.Floor(Decimal.Ceiling(x / 10)/2))
                    {
                        case 0:
                            rect.Brush = orange;
                            break;
                        case 1:
                            rect.Brush = red;
                            break;
                        case 2:
                            rect.Brush = blue;
                            break;
                        case 3:
                            rect.Brush = darkBlue;
                            break;
                        case 4:
                            rect.Brush = cyan;
                            break;
                        case 5:
                            rect.Brush = aqua;
                            break;
                        default:
                            rect.Brush = red;
                            break;
                    }
                    */

                    rect.Size = new Vector2(factor, factor);
                    rect.Offset = new Vector3(x*factor, y*factor, 0);
                    _root.Children.InsertAtTop(rect);
                }
                Debug.WriteLine("Reihe " + y + " von " + pixelHeight);
            }
        }
    }
}
