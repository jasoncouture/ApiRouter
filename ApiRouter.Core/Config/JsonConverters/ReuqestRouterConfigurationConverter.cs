using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiRouter.Core.Config.JsonConverters
{
    public class ReuqestRouterConfigurationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined) return null;
            var obj = (JObject)token;
            string key = null;
            if (obj.Properties().Any(x => x.Name == "type"))
            {
                key = obj["type"].Value<string>();
            }
            else
            {
                key = "__default";
            }

            if (key == null) throw new JsonSerializationException("Unable to determine configuration type.");

            Type type = null;
            if (!RequestRouterConfigurationBase.KeyTypeMaps.TryGetValue(key, out type)) throw new JsonSerializationException($"The type: {key} is not known.");
            return obj.ToObject(type, serializer);
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RequestRouterConfigurationBase);
        }
    }
}