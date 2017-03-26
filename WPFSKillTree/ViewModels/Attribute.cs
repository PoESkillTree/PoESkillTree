namespace POESKillTree.ViewModels
{
	using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    public class Attribute
    {
        public string Text { get; set; }
#if (PoESkillTree_UseSmallDec_ForAttributes)
		public SmallDec[] Deltas { get; set; }
#else
		public float[] Deltas { get; set; }
#endif
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