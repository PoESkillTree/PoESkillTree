using System.ComponentModel;
using System.Windows.Media;

namespace POESKillTree.Controls
{
    public class StashBookmark : INotifyPropertyChanged
    {
        static Color DefaultColor = Color.FromRgb(98, 128, 0);
        public StashBookmark(string name, int positon) : this(name, positon, DefaultColor)
        { }

        public StashBookmark(string name, int positon, Color color)
        {
            _color = color;
            _name = name;
            _position = positon;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        int _position = 0;
        Color _color = DefaultColor;
        string _name = "";

        public int Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        public Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
    }
}