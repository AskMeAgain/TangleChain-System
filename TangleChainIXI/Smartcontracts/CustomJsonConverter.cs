using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.Smartcontracts
{
    public class CustomJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            var token = JToken.FromObject(value);

            JObject o = (JObject)token;

            var reducedCode = ((Smartcontract)value).Code.ToReducedString();

            o.AddFirst(new JProperty("ReducedCode", reducedCode));
            o.AddFirst(new JProperty("ReducedVariables", "LOL"));

            o.Remove("Code");

            o.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            var code = (string)jObject["ReducedCode"];

            var resultJSON = Code.FromReducedString(code);

            var obj = new Code();
            obj.Expressions = resultJSON;
            obj.Variables = new Dictionary<string, ISCType>() {
                {"test", new SC_Int()}
            };

            jObject.Remove("ReducedCode");
            jObject.Remove("ReducedVariables");

            jObject.Add("Code", JObject.FromObject(obj));

            string json = jObject.ToString();

            return JsonConvert.DeserializeObject<Smartcontract>(jObject.ToString(), new JsonISCTypeConverter(), new JsonISCTypeConverter());

        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType != typeof(ISCType));

        }
    }
}
