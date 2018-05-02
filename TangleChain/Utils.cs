using System;

namespace TangleChain {
    public static class Utils {

        public static Block GetBlockFromJSON(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Block>(json);
        }

        public static string GetStringFromBlock(Block block) {
            return Newtonsoft.Json.JsonConvert.SerializeObject(block);
        }

    }
}
