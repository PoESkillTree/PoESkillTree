using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils.Extensions;
using POESKillTree.Utils.WikiApi;

namespace UpdateDB.DataLoading
{
    public class PropertyBuilder
    {
        private readonly JToken _printouts;
        private readonly List<XmlStat> _properties = new List<XmlStat>();

        public PropertyBuilder(JToken printouts)
        {
            _printouts = printouts;
        }

        public XmlStat[] ToArray()
        {
            return _properties.ToArray();
        }

        public void Add(string name, string rdfPredicate)
        {
            Add(name, rdfPredicate, rdfPredicate);
        }

        public void Add(string name, string rdfPredicateFrom, string rdfPredicateTo)
        {
            var from = WikiApiUtils.SingularValue<float>(_printouts, rdfPredicateFrom, 0);
            var to = WikiApiUtils.SingularValue<float>(_printouts, rdfPredicateTo, 0);
            if (from.AlmostEquals(0, 0.001) && to.AlmostEquals(0, 0.001)) // stats don't use many decimal places
                return;
            _properties.Add(new XmlStat
            {
                Name = name,
                From = new[] { @from },
                To = new[] { to }
            });
        }
    }
}