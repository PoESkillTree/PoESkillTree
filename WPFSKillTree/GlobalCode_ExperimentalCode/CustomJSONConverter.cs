using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Markup;

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    [ValueConversion(typeof(string), typeof(SmallDec))]
    public class StringToSmallDec : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (SmallDec) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value;
        }
    }
    //based on http://michaelcummings.net/mathoms/using-a-custom-jsonconverter-to-fix-bad-json-results/
    //and http://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm
    //and http://www.jerriepelser.com/blog/custom-converters-in-json-net-case-study-1/
    //Place [JsonConverter(typeof(CustomJSONConverter))] before variable to use
    // or place
    //    JsonConvert.DefaultSettings = () => new JsonSerializerSettings
    //{
    //    Converters = new System.Collections.Generic.List<JsonConverter> { new CustomJSONConverter() }
    //};
    //in main code block for global converter applied
    public partial class CustomJSONConverter : Newtonsoft.Json.JsonConverter
    {
        private static readonly Type[] SupportedTypes = { typeof(string), typeof(SmallDec), typeof(double), typeof(float), typeof(decimal),
        typeof(byte),typeof(sbyte),typeof(int),typeof(uint),typeof(short),typeof(ushort),typeof(ulong),typeof(long)};
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken TokenVal = JToken.FromObject(value);
            if (value is SmallDec)
            {
                Console.WriteLine("SmallDec WriteJSon detected.");
            }
            else
            {

            }
            if (TokenVal.Type != JTokenType.Object)
            {
                TokenVal.WriteTo(writer);
            }
            else
            {
                JObject ObjVal = (JObject)TokenVal;
                foreach (var ValElement in ObjVal.Values())
                {
                    ValElement.WriteTo(writer);
                }
                //IList<string> propertyNames = ObjVal.Properties().Select(p => p.Name).ToList();
                //ObjVal.AddFirst(new JProperty("Keys", new JArray(propertyNames)));
                //ObjVal.WriteTo(writer);
            }
            //throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = new Object();
            //Console.WriteLine("JSON objectType:" + objectType.ToString() + " Existing ObjectValue:" + (string)existingValue);
            //string ReaderValue = reader.ReadAsString();
            string ObjectTypeString = objectType.ToString();
            //Console.Write("JSon ReaderValue:" + ReaderValue);
            dynamic ObjectAsType;
            if (existingValue is SmallDec)
            {
                ObjectAsType = (SmallDec)existingValue;
            }
            else if (existingValue is float)
            {
                ObjectAsType = (float)existingValue;
            }
            else
            {
                ObjectAsType = (dynamic)existingValue;
            }
            if (reader.TokenType == JsonToken.StartObject)
            {
                //T instance = (T)serializer.Deserialize(reader, typeof(T));
                //retVal = new List<T>() { instance };
                retVal = serializer.Deserialize(reader, objectType);
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                JArray ArrayValue = JArray.Load(reader);

                ////var users = ArrayValue.ToObject<IList<User>>();
                //return new PagedList<User>(users);
                retVal = serializer.Deserialize(reader, objectType);
            }
            else
            {
                retVal = serializer.Deserialize(reader, objectType);
            }
            return retVal;
        }

        public override bool CanConvert(Type objectType)
        {
            //return SupportedTypes.Any(t => t == objectType);
            return true;
        }

        /// <summary>
        /// Loads this component's values from <paramref name="jObject"/>.
        /// </summary>
        public void LoadFrom(JObject jObject)
        {
            try
            {
                //AbstractCompositeSetting
                //                JToken token;
                //#if (DEBUG)
                //                string DebugTest = jObject.ToString();
                //                if (DebugTest != null) { System.Console.WriteLine("Loaded jObject value " + DebugTest); }
                //#endif
                //                if (!jObject.TryGetValue(Key, out token) || !(token is JObject))
                //                {
                //                    Reset();
                //                    return;
                //                }
                //                SubSettings.ForEach(s => s.LoadFrom((JObject)token));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Loaded CustomJSONConverter JObject Exception of " + ex.ToString());
            }
        }

        /// <summary>
        /// Saves this component's values to <paramref name="jObject"/>.
        /// </summary>
        /// <returns>True if this operation changed <paramref name="jObject"/></returns>
        public bool SaveTo(JObject jObject)
        {
            try
            {
                //AbstractCompositeSetting
                //                JToken token;
                //#if (DEBUG)
                //                string DebugTest = jObject.ToString();
                //                if (DebugTest != null) { System.Console.WriteLine("Saved jObject value" + DebugTest); }
                //#endif
                //                if (!jObject.TryGetValue(Key, out token) || !(token is JObject))
                //                {
                //                    jObject[Key] = token = new JObject();
                //                }
                //                if (!SubSettings.Any())
                //                    return false;
                //                var obj = (JObject)token;
                //                var changed = false;
                //                foreach (var s in SubSettings)
                //                {
                //                    if (s.SaveTo(obj))
                //                        changed = true;
                //                }
                //                return changed;
                //LeafSetting
                //                string DebugTest;
                //#if (DEBUG)
                //                DebugTest = jObject.ToString();
                //                if (DebugTest != null) { Console.WriteLine("Saved jObject value " + DebugTest); }
                //#endif
                //                var newToken = new JValue(Value);
                //#if (DEBUG)
                //                DebugTest = newToken.ToString();
                //                if (DebugTest != null) { Console.WriteLine("newToken value " + DebugTest); }
                //                DebugTest = newToken.GetType().ToString();
                //                if (DebugTest != null) { Console.WriteLine("newToken has Type of " + DebugTest); }
                //#endif
                //                DebugTest = newToken.Type.ToString();
                //#if (DEBUG)
                //                if (DebugTest != null) { Console.WriteLine("newToken has JType of " + DebugTest); }
                //#endif
                //                //if(DebugTest!="Integer"&&DebugTest!="Boolean")//Attempt to convert SmallDec etc
                //                //{
                //                //    Console.WriteLine("Value has type:"+Value.GetType().ToString()+" newToken has value string of "+newToken.Value.ToString());

                //                //    var changed = !Equals(Value, _defaultValue);
                //                //    JToken oldToken;
                //                //    if (jObject.TryGetValue(_key, out oldToken))
                //                //    {
                //                //        changed = !JToken.DeepEquals(newToken, oldToken);
                //                //    }
                //                //    jObject[_key] = newToken;
                //                //    return changed;
                //                //}
                //                //else
                //                //{
                //                var changed = !Equals(Value, _defaultValue);
                //                JToken oldToken;
                //                if (jObject.TryGetValue(_key, out oldToken))
                //                {
                //                    changed = !JToken.DeepEquals(newToken, oldToken);
                //                }
                //                jObject[_key] = newToken;
                //                return changed;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Saved CustomJSONConverter JObject Exception of " + ex.ToString());
            }
            return false;
        }
    }
}
