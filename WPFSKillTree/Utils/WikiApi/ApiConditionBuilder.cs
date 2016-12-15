using System.Collections;
using System.Collections.Generic;

namespace POESKillTree.Utils.WikiApi
{
    public class ConditionBuilder : IEnumerable<string>
    {
        private readonly List<string> _conditions = new List<string>();

        public void Add(string rdfPredicate, string rdfObject)
        {
            _conditions.Add(rdfPredicate + "::" + rdfObject);
        }

        public override string ToString()
        {
            return string.Join("|", _conditions);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _conditions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}