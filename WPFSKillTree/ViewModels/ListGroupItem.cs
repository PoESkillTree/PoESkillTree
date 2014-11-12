namespace POESKillTree.ViewModels
{
    internal class ListGroupItem
    {
        public string Text { get; set; }
        public AttributeGroup Group { get; set; }

        public ListGroupItem(string text, AttributeGroup attributeGroup)
        {
            Text = text;
            Group = attributeGroup;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
