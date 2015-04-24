namespace POESKillTree.ViewModels
{
    public class Attribute
    {
        public string Text { get; set; }
        public float[] Deltas { get; set; }
        public float[] Values { get; set; }
        public bool Missing { get; set; }
        public Attribute(string text, float[] values=null)
        {
            Text = text;
            Values = values;
        }


        public override string ToString()
        {
            return Text;
        }


    }
}