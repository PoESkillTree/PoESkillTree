using System;
using System.Windows.Media;
using POESKillTree.Utils;

namespace POESKillTree.Controls
{
    public class StashBookmark : Notifier
    {
        private static readonly Color DefaultColor = Color.FromRgb(98, 128, 0);

        [Obsolete("only for serialization")]
        public StashBookmark()
        {

        }
        public StashBookmark(string name, int positon) : this(name, positon, DefaultColor)
        { }

        public StashBookmark(string name, int positon, Color color)
        {
            _color = color;
            _name = name;
            _position = positon;
        }

        private int _position;
        private Color _color;
        private string _name;

        public int Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}