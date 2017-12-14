using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Awesomium.Core;

namespace DWARFLib
{
    public static class FileSystem
    {
        public static bool CreateDirectory(string name)
        {
            try
            {
                Directory.CreateDirectory(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CreateFile(string name)
        {
            try
            {
                File.Create(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool OverwriteToFile(string name, string value)
        {
            try
            {
                File.WriteAllText(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool AppendTextToFile(string name, string value)
        {
            try
            {
                File.AppendAllText(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Awesomium.Core.JSObject ReadFromFile(string name)
        {
            Awesomium.Core.JSObject returnObj = new Awesomium.Core.JSObject();
            try
            {
                string content = File.ReadAllText(name);
                returnObj["status"] = true;
                returnObj["text"] = content;
            }
            catch
            {
                returnObj["status"] = false;
            }
            return returnObj;
        }

        public static bool DeleteFile(string name)
        {
            try
            {
                File.Delete(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteDirectory(string name)
        {
            try
            {
                Directory.Delete(name, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
