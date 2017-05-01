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

            int factor = 8;
            double pixelOverall = (Window.Current.Bounds.Width * Window.Current.Bounds.Height) / factor;
            double pixelWidth = Window.Current.Bounds.Width/factor;
            double pixelHeight = Window.Current.Bounds.Height/factor;

            for (int y = 0; y < pixelHeight; y++)
            {
                for (int x = 0; x < pixelWidth; x++)
                {
                    SpriteVisual rect = compositor.CreateSpriteVisual();
                    rect.Brush = compositor.CreateColorBrush(Colors.Black);
                    rect.Size = new Vector2(factor, factor);
                    rect.Offset = new Vector3(x*factor, y*factor, 0);
                    _root.Children.InsertAtTop(rect);
                }
                Debug.WriteLine("Reihe " + y + " von " + pixelHeight);
            }
        }
    }
}
