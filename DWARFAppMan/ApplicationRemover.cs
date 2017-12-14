using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.IO.Compression;
using DWARFLib;

namespace DWARFAppMan
{
    public class ApplicationRemover
    {
        public int ID;

        public ApplicationRemover(int ID, Action OnRemoved, Action OnCancelled)
        {
            this.ID = ID;

            if (!ApplicationManager.instance.InstalledApplications.ContainsKey(ID))
            {
                OnCancelled();
            }
            else
            {
                var asi = new AskAction("Remove", "Application", ApplicationManager.instance.InstalledApplications[ID].Name, "", "Removing this application will also delete any data stored by this application (in the 'data' folder).");
                asi.Visibility = System.Windows.Visibility.Visible;
                if (asi.ShowDialog().Value)
                {
                    ApplicationManager.instance.RemoveAppShortcut(ApplicationManager.instance.InstalledApplications[ID].Name);
                    Directory.Delete(Consts.AppData + "\\Applications\\" + ApplicationManager.instance.InstalledApplications[ID].Name, true);

                    OnRemoved();
                }
                else
                {
                    OnCancelled();
                }
            }
        }
    }
}
