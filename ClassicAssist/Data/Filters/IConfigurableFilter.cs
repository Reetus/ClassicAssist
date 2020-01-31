using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Filters
{
    public interface IConfigurableFilter
    {
        void Configure();
        void Deserialize( JToken token );
        JObject Serialize();
        void ResetOptions();
    }
}