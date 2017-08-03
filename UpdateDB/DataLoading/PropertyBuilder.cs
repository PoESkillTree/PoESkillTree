using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils.Extensions;
using POESKillTree.Utils.WikiApi;

namespace UpdateDB.DataLoading
{
    public class PropertyBuilder
    {
        private readonly JToken _printouts;
        private readonly List<string> _properties = new List<string>();

        public PropertyBuilder(JToken printouts)
        {
            _printouts = printouts;
        }

        public string[] ToArray()
        {
            return _properties.ToArray();
        }

        public void Add(string format, string rdfPredicate)
        {
            var from = WikiApiUtils.SingularValue<float>(_printouts, rdfPredicate, 0);
            if (from.AlmostEquals(0, 0.001)) // stats don't use many decimal places
                return;
            _properties.Add(string.Format(CultureInfo.InvariantCulture, format, from));
        }

        public void Add(string format, string rdfPredicateFrom, string rdfPredicateTo)
        {
            var from = WikiApiUtils.SingularValue<float>(_printouts, rdfPredicateFrom, 0);
            var to = WikiApiUtils.SingularValue<float>(_printouts, rdfPredicateTo, 0);
            if (from.AlmostEquals(0, 0.001) && to.AlmostEquals(0, 0.001)) // stats don't use many decimal places
                return;
            _properties.Add(string.Format(CultureInfo.InvariantCulture, format, from, to));
        }
    }
}