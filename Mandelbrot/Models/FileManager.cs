using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

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

        public static Tuple<int, List<Complex>> Berechne(double x, double y, int iterationen)
        {
            int periodizitaet = -1;

            Tuple<List<Complex>, bool> iterationenTuple = iterieren(x, y, iterationen);
            if (!iterationenTuple.Item2)
            {
                periodizitaet = GetPeriodizitaet(iterationenTuple.Item1);
            }

            return Tuple.Create(periodizitaet, iterationenTuple.Item1);
        }

        private static Tuple<List<Complex>, bool> iterieren(double x, double y, int n)
        {
            Complex c = new Complex(x, y);     // Startwert

            List<Complex> complexNumbers = new List<Complex>();
            complexNumbers.Add(new Complex(0, 0));
            bool isDivergent = false;

            for (int i = 0; i < n; i++)
            {
                Complex z = complexNumbers.ElementAt(i) * complexNumbers.ElementAt(i) + c;
                if((double.IsNaN(z.Imaginary) || double.IsNaN(z.Real)) ||
                    (z.Imaginary > 5 || z.Imaginary < -5) ||
                    (z.Real > 5 || z.Real < -5))
                {
                    isDivergent = true;
                    break;
                }

                complexNumbers.Add(z);
            }

            return Tuple.Create(complexNumbers, isDivergent);
        }

        private static int GetPeriodizitaet(List<Complex> complexList)
        {
            int periodizitaet = 1;

            if(complexList.Count < 15)
            {
                return -1;
            }

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
            if(complexList.Count > 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    complexList.RemoveAt(0);
                }
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
            if (periodizitaet == 1)
            {   // Gib alte Liste zurück, da diese identisch mit der neuen Liste ist
                return liste;
            }
            
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

        // Methods not specific to this project
        public static Color GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            //byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            return Color.FromArgb(255, r, g, b);
        }
    }
}
