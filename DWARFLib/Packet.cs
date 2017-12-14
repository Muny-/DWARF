using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWARFLib
{
    public class Packet
    {
        public string ID;
        public Dictionary<string, DJSValue> Data = new Dictionary<string, DJSValue>();

        public Packet(string ID) : this(ID, new Dictionary<string, DJSValue>()) { }

        public Packet(string ID, Dictionary<string, DJSValue> Data)
        {
            this.ID = ID;
            this.Data = Data;
        }

        public string ToJSONString()
        {
            string json = "{\"command\": " + ID + ", \"data\": {";

            foreach (KeyValuePair<string, DJSValue> pair in Data)
            {
                json += "\"" + pair.Key + "\": " + pair.Value.ToJSONString() + ",";
            }

            json = json.Remove(json.Length - 1, 1);

            json += "}}";

            return json;
        }
    }
}
