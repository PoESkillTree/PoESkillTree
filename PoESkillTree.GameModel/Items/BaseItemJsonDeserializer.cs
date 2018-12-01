using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PoESkillTree.GameModel.Items
{
    public class BaseItemJsonDeserializer
    {
        public static async Task<BaseItemDefinitions> DeserializeAsync()
        {
            var jsonString = await DataUtils.LoadRePoEAsync("base_items").ConfigureAwait(false);
            var json = JObject.Parse(jsonString);
            return Deserialize(json);
        }

        public static BaseItemDefinitions Deserialize(JObject baseItemJson)
        {
            var definitions = baseItemJson.Properties().Select(Deserialize);
            return new BaseItemDefinitions(definitions.ToList());
        }

        private static BaseItemDefinition Deserialize(JProperty itemProperty)
        {
            return new BaseItemDefinition();
        }
    }
}