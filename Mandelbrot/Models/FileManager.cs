using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace Mandelbrot
{
    public static class FileManager
    {
        public const double MAX_DIFFERENCE = 0.1;
        public const int MAX_PERIODIZITAET = 500;
        public const int ITERATIONEN = 10000;

        public static ContainerVisual GetVisual(this UIElement element)
        {
            var hostVisual = ElementCompositionPreview.GetElementVisual(element);
            var root = hostVisual.Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(element, root);
            return root;
        }

        public static Tuple<int, List<Complex>> Berechne(double x, double y)
        {
            List<Complex> liste = iterieren(x, y, ITERATIONEN);
            int periodizitaet = GetPeriodizitaet(liste);

            return Tuple.Create(periodizitaet, liste);
        }

        private static List<Complex> iterieren(double x, double y, int n)
        {
            Complex c = new Complex(x, y);     // Startwert

            List<Complex> complexNumbers = new List<Complex>();
            complexNumbers.Add(new Complex(0, 0));

            for (int i = 0; i < n; i++)
            {
                Complex z = complexNumbers.ElementAt(i) * complexNumbers.ElementAt(i) + c;
                complexNumbers.Add(z);
            }

            return complexNumbers;
        }

        private static int GetPeriodizitaet(List<Complex> complexList)
        {
            int periodizitaet = 1;

            for (int i = 0; i < FileManager.MAX_PERIODIZITAET; i++)
            {
                if (IsKonvergent(createListe(complexList, periodizitaet)))
                {
                    return periodizitaet;
                }
                else
                {
                    periodizitaet++;
                }
            }
            return -1;
        }

        private static bool IsKonvergent(List<Complex> complexList)
        {
            // Remove first 10 entries
            for (int i = 0; i < 10; i++)
            {
                complexList.RemoveAt(0);
            }
            
            for (int i = complexList.Count; i > complexList.Count - 10; i--)
            {
                if (complexList.Count > 10)
                {
                    if (!IstAehnlich(complexList.ElementAt(i - 1), complexList.ElementAt(i - 2)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IstAehnlich(Complex complex1, Complex complex2)
        {
            double imaginaryDifferenz = complex1.Imaginary - complex2.Imaginary;
            double realDifferenz = complex1.Real - complex2.Real;

            // Wenn Differenz zwischen komplexen Zahlen kleiner 0,1 ist
            return (imaginaryDifferenz < FileManager.MAX_DIFFERENCE && imaginaryDifferenz > -FileManager.MAX_DIFFERENCE &&
                    realDifferenz < FileManager.MAX_DIFFERENCE && realDifferenz > -FileManager.MAX_DIFFERENCE);
        }

        private static List<Complex> createListe(List<Complex> liste, int periodizitaet)
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
    }
}
