using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils.Extensions;

using static POESKillTree.Utils.WikiApi.WikiApiUtils;

namespace POESKillTree.Utils.WikiApi
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
            return _properties.Any() ? _properties.ToArray() : null;
        }

        public void Add(string name, string rdfPredicate)
        {
            var value = SingularValue<float>(_printouts, rdfPredicate, 0);
            if (value.AlmostEquals(0))
                return;
            _properties.Add(new XmlStat
            {
                Name = name,
                From = value,
                To = value
            });
        }

        public void Add(string name, string rdfPredicateFrom, string rdfPredicateTo)
        {
            var from = SingularValue<float>(_printouts, rdfPredicateFrom, 0);
            var to = SingularValue<float>(_printouts, rdfPredicateTo, 0);
            if (from.AlmostEquals(0) && to.AlmostEquals(0))
                return;
            _properties.Add(new XmlStat
            {
                Name = name,
                From = from,
                To = to
            });
        }
    }
}