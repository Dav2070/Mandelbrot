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

        private void setDataContext()
        {
            ContentRoot.DataContext = (App.Current as App)._itemViewHolder;
        }

        private void SendNumberTextBox_Click(object sender, RoutedEventArgs e)
        {
            double x = -0.5;
            Double.TryParse(XTextBox.Text, out x);
            double y = 0.5;
            Double.TryParse(YTextBox.Text, out y);
            OutputTextBlock.Text = "";

            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
            (workItem) => {
                Berechne(x, y);
            });
        }

        private async void Berechne(double x, double y)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 (App.Current as App)._itemViewHolder.progressRingIsActive = true;
             });

            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
            async (workItem) => {

                await iterieren(x, y, FileManager.ITERATIONEN);

                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (Complex z in (App.Current as App)._itemViewHolder.complexList)
                    {
                        OutputTextBlock.Text += z.ToString() + "\n";
                    }
                });

                await GetPeriodizitaet((App.Current as App)._itemViewHolder.complexList);

                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PeriodizitaetTextBlock.Text = (App.Current as App)._itemViewHolder.periodizitaet.ToString();
                    (App.Current as App)._itemViewHolder.progressRingIsActive = false;
                });
            });
        }

        private async Task<int> GetPeriodizitaet(List<Complex> complexList)
        {
            int periodizitaet = 1;
            
            for (int i = 0; i < FileManager.MAX_PERIODIZITAET; i++)
            {
                if(IsKonvergent(createListe(complexList, periodizitaet)))
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        (App.Current as App)._itemViewHolder.periodizitaet = periodizitaet;
                    });
                    return periodizitaet;
                }
                else
                {
                    periodizitaet++;
                }
            }
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                (App.Current as App)._itemViewHolder.periodizitaet = -1;
            });
            return -1;
        }

        private List<Complex> createListe(List<Complex> liste, int periodizitaet)
        {
            List<Complex> newList = new List<Complex>();
            for (int i = 0; i < liste.Count; i++)
            {
                if (i % periodizitaet == 0)
                {
                    newList.Add(liste.ElementAt(i));
                }
            }
            return newList;
        }

        private bool IsKonvergent(List<Complex> complexList)
        {
            // Remove first 10 entries
            for (int i = 0; i < 10; i++)
            {
                complexList.RemoveAt(0);
            }
            Debug.WriteLine("Größe der Liste: " + complexList.Count);
            for (int i = complexList.Count; i > complexList.Count-10; i--)
            {
                if(complexList.Count > 10)
                {
                    if (!IstAehnlich(complexList.ElementAt(i - 1), complexList.ElementAt(i - 2)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IstAehnlich(Complex complex1, Complex complex2)
        {
            double imaginaryDifferenz = complex1.Imaginary - complex2.Imaginary;
            double realDifferenz = complex1.Real - complex2.Real;

            Debug.WriteLine("imaginäre Differenz: " + imaginaryDifferenz);
            Debug.WriteLine("reale Differenz: " + realDifferenz);

            // Wenn Differenz zwischen komplexen Zahlen kleiner 0,1 ist
            return (imaginaryDifferenz < FileManager.MAX_DIFFERENCE && imaginaryDifferenz > -FileManager.MAX_DIFFERENCE &&
                    realDifferenz < FileManager.MAX_DIFFERENCE && realDifferenz > -FileManager.MAX_DIFFERENCE);
        }

        private async Task<List<Complex>> iterieren(double x, double y, int n)
        {
            Complex c = new Complex(x, y);     // Startwert

            List<Complex> complexNumbers = new List<Complex>();
            complexNumbers.Add(new Complex(0, 0));

            for (int i = 0; i < n; i++)
            {
                Complex z = complexNumbers.ElementAt(i) * complexNumbers.ElementAt(i) + c;
                complexNumbers.Add(z);
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                (App.Current as App)._itemViewHolder.complexList = complexNumbers;
            });
            
            return complexNumbers;
        }
    }
}
