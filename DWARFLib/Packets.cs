using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWARFLib.Packets
{
    public class ApplicationInstalled : Packet
    {
        public ApplicationInstalled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_INSTALLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class ApplicationInstallCancelled : Packet
    {
        public ApplicationInstallCancelled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_INSTALL_CANCELLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class ApplicationRemoveCancelled : Packet
    {
        public ApplicationRemoveCancelled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_REMOVE_CANCELLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class ApplicationRemoved : Packet
    {
        public ApplicationRemoved(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_REMOVED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class ApplicationInstallProgressUpdate : Packet
    {
        public ApplicationInstallProgressUpdate(int ID, int Progress)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_INSTALL_PROGRESS_UPDATE) }, { "ID", new DJSValue(ID) }, { "Progress", new DJSValue(Progress) } })
        {

        }
    }

    public class ApplicationUpdateCancelled : Packet
    {
        public ApplicationUpdateCancelled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_UPDATE_CANCELLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class ApplicationUpdateAvailable : Packet
    {
        public ApplicationUpdateAvailable(int ID, double NewVersion)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_UPDATE_AVAILABLE) }, { "ID", new DJSValue(ID) }, {"NewVersion", new DJSValue(NewVersion) } })
        {

        }
    }

    public class ApplicationUpdateProgressUpdate : Packet
    {
        public ApplicationUpdateProgressUpdate(int ID, int Progress)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_UPDATE_PROGRESS_UPDATE) }, { "ID", new DJSValue(ID) }, { "Progress", new DJSValue(Progress) } })
        {

        }
    }

    public class ApplicationUpdated : Packet
    {
        public ApplicationUpdated(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.APPLICATION_UPDATED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class PluginInstalled : Packet
    {
        public PluginInstalled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_INSTALLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class PluginInstallCancelled : Packet
    {
        public PluginInstallCancelled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_INSTALL_CANCELLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class PluginRemoveCancelled : Packet
    {
        public PluginRemoveCancelled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_REMOVE_CANCELLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class PluginRemoved : Packet
    {
        public PluginRemoved(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_REMOVED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class PluginUpdateProgressUpdate : Packet
    {
        public PluginUpdateProgressUpdate(int ID, int Progress)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_UPDATE_PROGRESS_UPDATE) }, { "ID", new DJSValue(ID) }, { "Progress", new DJSValue(Progress) } })
        {

        }
    }

    public class PluginInstallProgressUpdate : Packet
    {
        public PluginInstallProgressUpdate(int ID, int Progress)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_INSTALL_PROGRESS_UPDATE) }, { "ID", new DJSValue(ID) }, { "Progress", new DJSValue(Progress) } })
        {

        }
    }

    public class PluginUpdateCancelled : Packet
    {
        public PluginUpdateCancelled(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_UPDATE_CANCELLED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class PluginUpdated : Packet
    {
        public PluginUpdated(int ID)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_UPDATED) }, { "ID", new DJSValue(ID) } })
        {

        }
    }

    public class PluginUpdateAvailable : Packet
    {
        public PluginUpdateAvailable(int ID, double NewVersion)
            : base(Consts.Commands.EVENT, new Dictionary<string, DJSValue>() { { "ident", new DJSValue(Consts.Events.PLUGIN_UPDATE_AVAILABLE) }, { "ID", new DJSValue(ID) }, { "NewVersion", new DJSValue(NewVersion) } })
        {

        }
    }
}
