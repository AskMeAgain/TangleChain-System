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

            try
            {
                return jo.ToObject<SC_Int>();
            }
            catch (Exception)
            {
                return jo.ToObject<SC_String>();
            }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }


        public override bool CanConvert(Type objectType)
        {

            return (objectType == typeof(ISCType));
        }
    }

}
