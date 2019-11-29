using System;

namespace PoESkillTree.ViewModels
{
    public class Attribute
    {
        public string Text { get; }
        public float[] Deltas { get; set; } = Array.Empty<float>();
        public bool Missing { get; set; }

        public Attribute(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}