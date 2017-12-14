using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using DWARFLib;

namespace DWARFAppMan
{
    public class WSListener
    {
        public static WSListener instance;

        List<IWebSocketConnection> Connections = new List<IWebSocketConnection>();
        WebSocketServer Server = new WebSocketServer("ws://127.0.0.1:42368");

        public WSListener()
        {
            instance = this;
            Server.Start(Socket =>
            {
                Socket.OnOpen = () => Connections.Add(Socket);
                Socket.OnClose = () => Connections.Remove(Socket);
                Socket.OnMessage = Message =>
                {
                    HandleMessage(Socket, Message);
                };
            });
        }

        void HandleMessage(IWebSocketConnection Socket, string _message)
        {
            dynamic packet = DynamicJson.Parse(_message);

            string command = packet.command;
            dynamic data = packet.data;

            switch (command)
            {
                case Consts.Commands.INSTALL_APPLICATION:
                    ApplicationManager.instance.InstallApplication((int)data.ID);
                break;

                case Consts.Commands.INSTALL_PLUGIN:
                    ApplicationManager.instance.InstallPlugin((int)data.ID, false);
                break;

                case Consts.Commands.REMOVE_APPLICATION:
                    ApplicationManager.instance.RemoveApplication((int)data.ID);
                break;

                case Consts.Commands.REMOVE_PLUGIN:
                    ApplicationManager.instance.RemovePlugin((int)data.ID);
                break;

                case Consts.Commands.UPDATE_APPLICATION:
                    ApplicationManager.instance.UpdateApplication((int)data.ID);
                break;

                case Consts.Commands.UPDATE_PLUGIN:
                    ApplicationManager.instance.UpdatePlugin((int)data.ID);
                break;

                case Consts.Commands.CHECK_FOR_UPDATES:
                    ApplicationManager.instance.CheckForUpdates();
                break;

                default:

                break;
            }
        }

        void BroadcastToConnections(Packet packet)
        {
            string json = packet.ToJSONString();
            foreach (IWebSocketConnection con in Connections)
            {
                con.Send(json);
            }
        }

        public void OnApplicationInstalled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationInstalled(ID));
        }

        public void OnApplicationInstallCancelled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationInstallCancelled(ID));
        }

        public void OnApplicationRemoved(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationRemoved(ID));
        }

        public void OnApplicationRemoveCancelled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationRemoveCancelled(ID));
        }

        public void OnApplicationInstallProgressUpdate(int ID, int Progress)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationInstallProgressUpdate(ID, Progress));
        }

        public void OnApplicationUpdateAvailable(int ID, double NewVersion)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationUpdateAvailable(ID, NewVersion));
        }

        public void OnApplicationUpdateCancelled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationUpdateCancelled(ID));
        }

        public void OnApplicationUpdateProgressUpdate(int ID, int Progress)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationUpdateProgressUpdate(ID, Progress));
        }

        public void OnApplicationUpdated(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.ApplicationUpdated(ID));
        }

        public void OnPluginInstalled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginInstalled(ID));
        }

        public void OnPluginInstallCancelled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginInstallCancelled(ID));
        }

        public void OnPluginRemoved(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginRemoved(ID));
        }

        public void OnPluginRemoveCancelled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginRemoveCancelled(ID));
        }

        public void OnPluginInstallProgressUpdate(int ID, int Progress)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginInstallProgressUpdate(ID, Progress));
        }

        public void OnPluginUpdateProgressUpdate(int ID, int Progress)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginUpdateProgressUpdate(ID, Progress));
        }

        public void OnPluginUpdateCancelled(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginUpdateCancelled(ID));
        }

        public void OnPluginUpdateAvailable(int ID, double NewVersion)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginUpdateAvailable(ID, NewVersion));
        }

        public void OnPluginUpdated(int ID)
        {
            BroadcastToConnections(new DWARFLib.Packets.PluginUpdated(ID));
        }
    }
}
