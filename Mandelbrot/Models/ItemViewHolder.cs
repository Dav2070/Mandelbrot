using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Models
{
    public class ItemViewHolder : INotifyPropertyChanged
    {
        private bool _progressRingIsActive;
        private List<Complex> _complexList;
        private int _periodizitaet;
        private Type _page;
        private double _x;
        private double _y;


        public bool progressRingIsActive
        {
            get { return _progressRingIsActive; }

            set
            {
                _progressRingIsActive = value;
                NotifyPropertyChanged("progressRingIsActive");
            }
        }

        public List<Complex> complexList
        {
            get { return _complexList; }

            set
            {
                _complexList = value;
                NotifyPropertyChanged("complexList");
            }
        }

        public int periodizitaet
        {
            get { return _periodizitaet; }

            set
            {
                _periodizitaet = value;
                NotifyPropertyChanged("periodizitaet");
            }
        }

        public Type page
        {
            get { return _page; }

            set
            {
                _page = value;
                NotifyPropertyChanged("page");
            }
        }

        public double x
        {
            get { return _x; }

            set
            {
                _x = value;
                NotifyPropertyChanged("x");
            }
        }

        public double y
        {
            get { return _y; }

            set
            {
                _y = value;
                NotifyPropertyChanged("y");
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
