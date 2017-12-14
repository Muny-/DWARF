using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Fleck;

namespace DWARFAppMan
{
    class DWARFAppMan
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "updated")
                {
                    try
                    {
                        System.IO.File.Delete(args[1]);
                    }
                    catch
                    {

                    }
                }
            }

            ApplicationManager appman = new ApplicationManager();
            WSListener dam = new WSListener();
            System.Windows.Forms.Application.Run();
        }
    }
}
