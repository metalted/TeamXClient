using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamXClient
{
    public class BlockPropertyJSONX
    {
        public BlockPropertyJSON blockPropertyJSON;
        public ulong SteamID;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static BlockPropertyJSONX FromJson(string json)
        {
            return JsonConvert.DeserializeObject<BlockPropertyJSONX>(json);
        }
    }
}
