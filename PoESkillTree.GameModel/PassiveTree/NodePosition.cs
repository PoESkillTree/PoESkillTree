namespace PoESkillTree.GameModel.PassiveTree
{
    public readonly struct NodePosition
    {
        public NodePosition(double x, double y)
            => (X, Y) = (x, y);

        public void Deconstruct(out double x, out double y)
            => (x, y) = (X, Y);

        public double X { get; }
        public double Y { get; }
    }
}