using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWARFLib
{
    public class Plugin
    {
        public string Name;
        public string Author;
        public int ID;
        public double Version;

        public Plugin(string Name, string Author, int ID, double Version)
        {
            this.Name = Name;
            this.Author = Author;
            this.ID = ID;
            this.Version = Version;
        }

        public string ToMinJSONString()
        {
            return "{\"id\": " + ID + ",\"name\": \"" + Name + "\"}";
        }

        public string ToJSONString_notdone()
        {
            string json = "{\"id\": ";

            // NOT DONE

            return json;
        }
    }
}
