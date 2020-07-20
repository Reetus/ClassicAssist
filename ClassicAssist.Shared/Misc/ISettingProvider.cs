using ClassicAssist.Data;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Misc
{
    public interface ISettingProvider
    {
        void Serialize( JObject json );
        void Deserialize( JObject json, Options options );
    }
}