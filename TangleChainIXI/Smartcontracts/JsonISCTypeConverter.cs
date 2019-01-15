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
    public class JsonISCTypeConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            JObject jo = JObject.Load(reader);

            var scString = jo.ToObject<SC_Int>();
            if (scString != null) {
                jo = JObject.Parse(scString.GetValueAs<string>());
            }
            else
            {
               // jo = JObject.FromObject(jo.ToObject<SC_String>().GetValueAs<string>());
                jo =JObject.Parse(jo.ToObject<SC_String>().GetValueAs<string>());

            }

            return null;
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            var token = JToken.FromObject(value);

            JObject o = (JObject)token;

            try
            {
                var casted = (SC_Int)value;
                o["value"] = casted.GetValueAsStringWithPrefix();
                o.WriteTo(writer);
                return;
            }
            catch (Exception)
            {
                //nothing
            }

            var casted2 = (SC_String)value;
            o["value"] = casted2.GetValueAsStringWithPrefix();
            o.WriteTo(writer);


        }


        public override bool CanConvert(Type objectType)
        {

            return (objectType == typeof(ISCType));
        }
    }

}
