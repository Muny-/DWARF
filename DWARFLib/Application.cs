using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWARFLib
{
    public class Application
    {
        public string Name;
        public string Author;
        public int ID;
        public double Version;
        public List<int> Plugins;
        public bool HasDesktopIcon;

        public Application(string Name, string Author, int ID, double Version, List<int> Plugins, bool HasDesktopIcon)
        {
            this.Name = Name;
            this.Author = Author;
            this.ID = ID;
            this.Version = Version;
            this.Plugins = Plugins;
            this.HasDesktopIcon = HasDesktopIcon;
        }

        public string ToMinJSONString()
        {
            return "{\"id\": " + ID + ",\"name\": \"" + Name + "\", \"hasdesktopicon\": " + HasDesktopIcon.ToString().ToLower() + "}";
        }

        public string ToJSONString_notdone()
        {
            string json = "{\"id\": ";

            // NOT DONE

            return json;
        }
    }
}
