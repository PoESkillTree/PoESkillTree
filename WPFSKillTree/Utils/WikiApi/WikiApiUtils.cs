using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace POESKillTree.Utils.WikiApi
{
    public static class WikiApiUtils
    {
        /// <summary>
        /// The factor by which item images from the Wiki have to be resized to fit into the inventory/stash slots.
        /// </summary>
        public const double ItemImageResizeFactor = 0.6;

        public static T SingularValue<T>(JToken printouts, string rdfPredicate)
        {
            return printouts[rdfPredicate].First.Value<T>();
        }

        public static T SingularValue<T>(JToken printouts, string rdfPredicate, T defaultValue)
        {
            var token = printouts[rdfPredicate];
            return token.HasValues ? token.First.Value<T>() : defaultValue;
        }

        public static IEnumerable<T> PluralValue<T>(JToken printouts, string rdfPredicate)
        {
            return printouts[rdfPredicate].Values<T>();
        }
    }
}