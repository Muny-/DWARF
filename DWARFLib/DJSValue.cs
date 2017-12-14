using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWARFLib
{
    public class DJSValue
    {
        private object obj;

        public DJSValue(object obj)
        {
            this.obj = obj;
        }

        public string ToJSONString()
        {
            string json = "";

            Console.WriteLine(obj.GetType().ToString());

            switch(obj.GetType().ToString())
            {
                case "System.String":
                    json = "\"" + obj + "\"";
                break;

                case "System.Int32":
                    json = obj.ToString();
                break;

                case "System.Double":
                    json = obj.ToString();
                break;

                case "System.Collections.Generic.Dictionary`2[System.Int32,DWARFLib.Application]":
                    json = "[";

                    Dictionary<int, Application> apps = (Dictionary<int, Application>)obj;

                    foreach (KeyValuePair<int, Application> app in apps)
                    {
                        json += app.Value.ToMinJSONString() + ",";
                    }

                    if (json.Length > 1) json = json.Remove(json.Length - 1, 1);

                    json += "]";
                break;

                case "System.Collections.Generic.Dictionary`2[System.Int32,DWARFLib.Plugin]":
                    json = "[";

                    Dictionary<int, Plugin> plugins = (Dictionary<int, Plugin>)obj;

                    foreach (KeyValuePair<int, Plugin> plugin in plugins)
                    {
                        json += plugin.Value.ToMinJSONString() + ",";
                    }

                    if (json.Length > 1) json = json.Remove(json.Length - 1, 1);

                    json += "]";
                break;

                default:
                    json = "\"" + obj.ToString() + "\"";
                break;
            }

            return json;
        }
    }
}
