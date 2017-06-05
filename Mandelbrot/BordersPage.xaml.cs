using System;
using System.Collections.Generic;
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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Mandelbrot
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class BordersPage : Page
    {
        public BordersPage()
        {
            this.InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            double startPointX = 0;
            double startPointY = 0;
            double endPointX = 0;
            double endPointY = 0;
            double steps = 0;

            double.TryParse(StartPointXTextBox.Text, out startPointX);
            double.TryParse(StartPointYTextBox.Text, out startPointY);
            double.TryParse(EndPointXTextBox.Text, out endPointX);
            double.TryParse(EndPointYTextBox.Text, out endPointY);
            double.TryParse(StepsTextBox.Text, out steps);

            List<int> periodicityList = FileManager.CalculatePeriodicityBetweenPoints(new Complex(startPointX, startPointY), new Complex(endPointX, endPointY), steps);

            ResultTextBlock.Text = "";
            foreach(int periodicity in periodicityList)
            {
                ResultTextBlock.Text += periodicity.ToString() + "\n";
            }
        }
    }
}
