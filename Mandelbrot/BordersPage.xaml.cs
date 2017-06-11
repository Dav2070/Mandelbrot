using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
        ObservableCollection<int> periodicityList;

        public BordersPage()
        {
            this.InitializeComponent();
            periodicityList = new ObservableCollection<int>();
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


            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                async (workItem) =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                new DispatchedHandler(() =>
                                {
                                    ProgressRing.IsActive = true;
                                }));

                    // Return periodicityList (Item1) and the point of the periodicity in the corresponding List (Item2)
                    Tuple<List<int>, List<Complex>> periodicityTuple = FileManager.CalculatePeriodicityBetweenPoints(new Complex(startPointX, startPointY), new Complex(endPointX, endPointY), steps);

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                new DispatchedHandler(() =>
                                {

                                    periodicityList.Clear();
                                    foreach (int periodicity in periodicityTuple.Item1)
                                    {
                                        periodicityList.Add(periodicity);
                                    }

                                    createTable(periodicityTuple);
                                }));
                });
        }

        private void createTable(Tuple<List<int>, List<Complex>> periodicityTuple)
        {
            // Clear old table
            foreach (UIElement item in ResultTable.Children)
            {
                if(item.GetType() == typeof(TextBlock))
                {
                    ((TextBlock)item).Text = "";
                }
            }

            double headerFontSize = 25;
            double textFontSize = 22;

            // Add the header row
            // Add a new row
            RowDefinition headerRowDefinition = new RowDefinition();
            headerRowDefinition.Height = new GridLength(50, GridUnitType.Auto);
            ResultTable.RowDefinitions.Add(headerRowDefinition);

            TextBlock periodicityHeaderTextBlock = new TextBlock();
            periodicityHeaderTextBlock.Text = "Periodizität";
            periodicityHeaderTextBlock.FontSize = headerFontSize;
            Grid.SetRow(periodicityHeaderTextBlock, 0);
            Grid.SetColumn(periodicityHeaderTextBlock, 0);

            TextBlock XHeaderTextBlock = new TextBlock();
            XHeaderTextBlock.Text = "X";
            XHeaderTextBlock.FontSize = headerFontSize;
            Grid.SetRow(XHeaderTextBlock, 0);
            Grid.SetColumn(XHeaderTextBlock, 1);

            TextBlock YHeaderTextBlock = new TextBlock();
            YHeaderTextBlock.Text = "Y";
            YHeaderTextBlock.FontSize = headerFontSize;
            Grid.SetRow(YHeaderTextBlock, 0);
            Grid.SetColumn(YHeaderTextBlock, 2);

            ResultTable.Children.Add(periodicityHeaderTextBlock);
            ResultTable.Children.Add(XHeaderTextBlock);
            ResultTable.Children.Add(YHeaderTextBlock);

            // Create Table
            for (int i = 0; i < periodicityTuple.Item1.Count; i++)
            {
                // Add a new row
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = new GridLength(50, GridUnitType.Auto);
                ResultTable.RowDefinitions.Add(rowDefinition);

                // Add the data
                TextBlock periodicityTextBlock = new TextBlock();
                periodicityTextBlock.Text = periodicityTuple.Item1.ElementAt(i).ToString();
                periodicityTextBlock.FontSize = textFontSize;
                Grid.SetColumn(periodicityTextBlock, 0);
                Grid.SetRow(periodicityTextBlock, i + 1);

                TextBlock XTextBlock = new TextBlock();
                XTextBlock.Text = periodicityTuple.Item2.ElementAt(i).Real.ToString();
                XTextBlock.FontSize = textFontSize;
                Grid.SetColumn(XTextBlock, 1);
                Grid.SetRow(XTextBlock, i + 1);

                TextBlock YTextBlock = new TextBlock();
                YTextBlock.Text = periodicityTuple.Item2.ElementAt(i).Imaginary.ToString();
                YTextBlock.FontSize = textFontSize;
                Grid.SetColumn(YTextBlock, 2);
                Grid.SetRow(YTextBlock, i + 1);


                ResultTable.Children.Add(periodicityTextBlock);
                ResultTable.Children.Add(XTextBlock);
                ResultTable.Children.Add(YTextBlock);

                ProgressRing.IsActive = false;
            };
        }
    }
}