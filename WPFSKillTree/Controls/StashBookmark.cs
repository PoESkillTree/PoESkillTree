using System;
using System.Windows.Media;
using PoESkillTree.Utils;

namespace PoESkillTree.Controls
{
    public class StashBookmark : Notifier
    {
        private static readonly Color DefaultColor = Color.FromRgb(98, 128, 0);

        [Obsolete("only for serialization")]
        public StashBookmark()
        {
            _name = "";
        }

        public StashBookmark(string name, int position) : this(name, position, DefaultColor)
        {
        }

        public StashBookmark(string name, int position, Color color)
        {
            _color = color;
            _name = name;
            _position = position;
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