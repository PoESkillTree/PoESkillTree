using System.Collections;
using System.Collections.Generic;

namespace POESKillTree.Utils.WikiApi
{
    /// <summary>
    /// Can be used to build conditions for <see cref="ApiAccessor.Ask"/> and <see cref="ApiAccessor.AskArgs"/>
    /// on RDF predicates.
    /// </summary>
    public class ConditionBuilder : IEnumerable<string>
    {
        private readonly List<string> _conditions = new List<string>();

        /// <summary>
        /// Adds a condition which is satisfied iff the given predicate on the evaluated subject has the given object
        /// associated with it.
        /// </summary>
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