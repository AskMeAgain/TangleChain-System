using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.Smartcontracts
{
    public class CustomJsonConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            var ss = reader.Value.ToString();

            var list = new List<Expression>();

            foreach (var exp in ss.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var arr = exp.Split('.');
                Expression tempExp = null;
                switch (arr.Length)
                {
                    case 1:
                        tempExp = new Expression(int.Parse(arr[0]));
                        break;
                    case 2:
                        tempExp = new Expression(int.Parse(arr[0]), arr[1]);
                        break;
                    case 3:
                        tempExp = new Expression(int.Parse(arr[0]), arr[1], arr[2]);
                        break;
                    case 4:
                        tempExp = new Expression(int.Parse(arr[0]), arr[1], arr[2], arr[3]);
                        break;
                }

                list.Add(tempExp);

            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            var list = (List<Expression>)value;
            writer.WriteValue(list.ToFlatList());
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<Expression>));

        }
    }
}
