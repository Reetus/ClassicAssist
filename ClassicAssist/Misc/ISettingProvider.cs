using ClassicAssist.Data;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Misc
{
    public interface ISettingProvider
    {
        void Serialize( JObject json, bool global = false );
        void Deserialize( JObject json, Options options, bool global = false );
    }
}