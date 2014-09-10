namespace POESKillTree.ViewModels
{
    internal class ListGroupItem
    {
        public string Text { get; set; }
        public string GroupName { get; set; }

        public ListGroupItem(string text, string groupName)
        {
            Text = text;
            GroupName = groupName;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
