using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using Alchemy;
using DWARFLib;

namespace DWARF
{
    public interface IPluginHandler
    {
        Object GetData(String PluginName, List<string> args, ApplicationForm form);

        Boolean PluginExists(String PluginName);

        int PluginCount();

        string[] GetPluginNames();

        void EnablePlugins();

        void DisablePlugins();

        void DisablePlugin(String PluginName);

        void EnablePlugin(String PluginName);
    }

    public interface IPlugin
    {
        object GetData(List<string> args, ApplicationForm form);

        void OnEnabled();

        void OnDisabled();
    }

    public interface IPluginIdentifier
    {
        String Identifier { get; }
    }

    [Export(typeof(IPluginHandler))]
    public class PluginHandler : IPluginHandler
    {
        [ImportMany]
        IEnumerable<Lazy<IPlugin, IPluginIdentifier>> plugins;

        public Object GetData(string PluginName, List<string> args, ApplicationForm form)
        {
            return plugins.Where(val => val.Metadata.Identifier.Equals(PluginName)).First().Value.GetData(args, form);
        }

        public Boolean PluginExists(string PluginName)
        {
            return (plugins.Where(value => value.Metadata.Identifier == PluginName).Count() > 0);
        }

        public int PluginCount()
        {
            return plugins.Count();
        }

        public string[] GetPluginNames()
        {
            return plugins.ToDictionary(val => val.Metadata.Identifier).Keys.ToArray();
        }

        public void EnablePlugins()
        {
            foreach (IPlugin plugin in plugins)
            {
                plugin.OnEnabled();
            }
        }

        public void DisablePlugins()
        {
            foreach (IPlugin plugin in plugins)
            {
                plugin.OnDisabled();
            }
        }

        public void EnablePlugin(string plugin)
        {
            plugins.Where(val => val.Metadata.Identifier == plugin).First().Value.OnEnabled();
        }

        public void DisablePlugin(string plugin)
        {
            plugins.Where(val => val.Metadata.Identifier == plugin).First().Value.OnDisabled();
        }
    }

    /// <summary>
    /// Interaction logic for DWARFLoader.xaml
    /// </summary>
    public partial class DWARFLoader : Window
    {

        public Dictionary<string, ApplicationForm> ChildForms = new Dictionary<string, ApplicationForm>();
        public Dictionary<string, object> GlobalVars = new Dictionary<string, object>();

        public static DWARFLoader instance;

        public string AppName;
        public string Author;
        public string WindowControlsHTML;
        public string WindowControlsCSS;

        public bool DebugEnabled = false;

        public ApplicationForm AppForm;

        public string dwarf_conf_file;

        public int windowControlsBackgroundColorRed = 34;
        public int windowControlsBackgroundColorGreen = 136;
        public int windowControlsBackgroundColorBlue = 204;

        public string PluginJavascript = "";

        private CompositionContainer _container;

        [Import(typeof(IPluginHandler))]
        public IPluginHandler ph;

        DrawingImage[] NotificationOverlays = new DrawingImage[10];

        public Dictionary<string, Dictionary<string, Awesomium.Core.JSObject>> GlobalObjects = new Dictionary<string, Dictionary<string, Awesomium.Core.JSObject>>();

        public DWARFLoader()
        {

            List<String> args = Environment.GetCommandLineArgs().ToList();

            //args.Add("C:\\Users\\Kevin\\Documents\\Visual Studio 2013\\Projects\\DWARF\\DWARF\\examples\\Hello World\\helloworld.dwarf");
            //args.Add("C:\\Users\\Kevin\\Desktop\\Projects\\syndycate\\syndycate.dwarf");
            //args.Add("C:\\Users\\Kevin\\AppData\\Roaming\\Azuru\\DWARF\\Applications\\Solid Design\\app_info.dwarf");
            //args.Add(@"C:\Users\Kevin\AppData\Roaming\Azuru\DWARF\Applications\Foundry\app_info.dwarf");
            //args.Add(@"C:\Users\Kevin\Desktop\FileTrans\app_info.dwarf");
            

            if (args.Count < 2)
            {
                MessageBox.Show("Error: Please specify a DWARF project configuration file path.");
                Environment.Exit(0);
            }
            else if (args.Count > 2)
            {
                MessageBox.Show("Error: Only 1 argument is required.  If the path includes spaces, please encase it in double quotes.");
                Environment.Exit(0);
            }

            this.dwarf_conf_file = args[1];

            dynamic appInfo = DynamicJson.Parse(File.ReadAllText(dwarf_conf_file));

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the ApplicationForm class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ApplicationForm).Assembly));

            if (appInfo.IsDefined("plugins"))
            {
                string[] pluginList = appInfo.plugins;

                foreach (string pluginn in pluginList)
                {
                    try
                    {
                        // TODO: finish this
                        if (File.Exists(Consts.AppData + "Plugins\\" + pluginn + "\\" + pluginn + ".dll"))
                            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFile(Consts.AppData + "Plugins\\" + pluginn + "\\" + pluginn + ".dll")));
                        else
                        {
                            MessageBox.Show("Error!  Missing plugin: " + pluginn + "\n\nPlease install that plugin before running this application!\n\nError code 0x0001");
                            Environment.Exit(0);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Unable to load plugin: " + pluginn + "\n\n0x0003");
                    }

                    try
                    {
                        if (File.Exists(Consts.AppData + "Plugins\\" + pluginn + "\\" + pluginn + ".js"))
                            this.PluginJavascript += File.ReadAllText(Consts.AppData + "Plugins\\" + pluginn + "\\" + pluginn + ".js") + "\n";
                        else
                        {
                            MessageBox.Show("Error!  Missing plugin: " + pluginn + "\n\nPlease install that plugin before running this application!\n\nError code 0x0002");
                            Environment.Exit(0);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Unable to load plugin: " + pluginn + "\n\nError code 0x0004");
                    }
                }
            }

            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }

            Directory.SetCurrentDirectory(new FileInfo(dwarf_conf_file).DirectoryName);

            AppName = appInfo.name;
            Author = appInfo.author;

            WebConfig wc = new WebConfig();

            int frmlen = ((dynamic[])appInfo.forms).Length;

            if (appInfo.IsDefined("debugging") && ((bool)appInfo.debugging) == true)
            {
                DebugEnabled = true;

                appInfo.forms[frmlen] = new { name = "debug", title = "DWARF Debug Tool", file = "dwarf://debug", width = 780, height = 467, defaultwindowborder = true, canresize = true};
                frmlen++;

                //formsjson = newfrmarr;

                if (appInfo.IsDefined("debughost"))
                    wc.RemoteDebuggingHost = appInfo.debughost;
                else
                    wc.RemoteDebuggingHost = "127.0.0.1";

                if (appInfo.IsDefined("debugport"))
                    wc.RemoteDebuggingPort = Convert.ToInt32(appInfo.debugport);
                else
                    wc.RemoteDebuggingPort = 7778;

                //wc.AutoUpdatePeriod = 1;
            }
            else
            {
                wc.RemoteDebuggingHost = "";
                wc.RemoteDebuggingPort = 0;
            }

            //wc.UserAgent += " Azuru DWARF (AppName=" + AppName + ", v=" + System.Windows.Application.ResourceAssembly.ImageRuntimeVersion + ")";
            this.Dispatcher.Invoke(() => {
                WebCore.Initialize(wc);
            });
            

            if (File.Exists(new FileInfo(dwarf_conf_file).DirectoryName + "/forms/dwarf.assets/dwarf.controls.html"))
            {
                WindowControlsHTML = File.ReadAllText("forms/dwarf.assets/dwarf.controls.html").Replace("\n", "");
            }

            if (File.Exists(new FileInfo(dwarf_conf_file).DirectoryName + "/forms/dwarf.assets/dwarf.controls.css"))
            {
                WindowControlsCSS = File.ReadAllText("forms/dwarf.assets/dwarf.controls.css");
            }

            instance = this;

            //DynamicJson test = appInfo;

            for (int i = 0; i < frmlen; i++)
            {
                string name = appInfo.forms[i].name;
                string title = appInfo.forms[i].title;
                string file = appInfo.forms[i].file;

                bool windowctrls = true;

                if (appInfo.forms[i].IsDefined("windowcontrols"))
                    windowctrls = appInfo.forms[i].windowcontrols;

                ApplicationForm form = new ApplicationForm(file, false, windowctrls);

                if (appInfo.forms[i].IsDefined("defaultwindowborder") && (bool)(appInfo.forms[i].defaultwindowborder) == true)
                {
                    form.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                    form.webView.ViewType = WebViewType.Window;
                    form.AllowsTransparency = false;
                    form.UsesWindowControls = false;
                    form.border1.Margin = new Thickness(0);
                    form.ResizeMode = System.Windows.ResizeMode.CanMinimize;
                }

                if (appInfo.forms[i].IsDefined("startposition"))
                {
                    if (appInfo.forms[i].startposition == "center")
                    {
                        form.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    }
                    else if (appInfo.forms[i].startposition == "centerowner")
                    {
                        form.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    }
                    else if (appInfo.forms[i].startposition == "manual")
                    {
                        form.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                    }
                }

                if (appInfo.forms[i].IsDefined("canresize"))
                {
                    if (!(bool)appInfo.forms[i].canresize)
                    {
                        if (form.ResizeMode != System.Windows.ResizeMode.CanMinimize)
                            form.ResizeMode = System.Windows.ResizeMode.NoResize;
                        form.Left_Resize.Visibility = System.Windows.Visibility.Hidden;
                        form.Right_Resize.Visibility = System.Windows.Visibility.Hidden;
                        form.Bottom_Resize.Visibility = System.Windows.Visibility.Hidden;
                        form.Top_Resize.Visibility = System.Windows.Visibility.Hidden;
                        form.BottomLeft_Resize.Visibility = System.Windows.Visibility.Hidden;
                        form.BottomRight_Resize.Visibility = System.Windows.Visibility.Hidden;
                        form.TopLeft_Resize.Visibility = System.Windows.Visibility.Hidden;
                        form.TopRight_Resize.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        form.ResizeMode = System.Windows.ResizeMode.CanResize;
                    }
                }

                if (appInfo.forms[i].IsDefined("width"))
                {
                    form.Width = appInfo.forms[i].width + 40;
                }

                if (appInfo.forms[i].IsDefined("height"))
                {
                    form.Height = appInfo.forms[i].height + 40;
                }

                if (appInfo.forms[i].IsDefined("position"))
                {
                    if (form.WindowStartupLocation == System.Windows.WindowStartupLocation.Manual)
                    {
                        form.Left = appInfo.forms[i].position[0];
                        form.Top = appInfo.forms[i].position[1];
                    }
                }

                if (appInfo.forms[i].IsDefined("startupstate"))
                {
                    if (appInfo.forms[i].startupstate == "normal")
                    {
                        form.WindowState = System.Windows.WindowState.Normal;
                    }
                    else if (appInfo.forms[i].startupstate == "maximized")
                    {
                        form.WindowState = System.Windows.WindowState.Maximized;
                    }
                    else if (appInfo.forms[i].startupstate == "minimized")
                    {
                        form.WindowState = System.Windows.WindowState.Minimized;
                    }
                }

                if (appInfo.forms[i].IsDefined("opacity"))
                    form.window.Opacity = appInfo.forms[i].opacity;
                else
                    form.window.Opacity = 1;

                form.Title = title;
                form.FormName = name;
                ChildForms.Add(name, form);
            }

            AppForm = new ApplicationForm("", true, false);
            AppForm.FormName = AppName;
            AppForm.Title = AppName;
            AppForm.window.Opacity = 0;
            AppForm.Title = "";
            AppForm.ShowInTaskbar = false;
            AppForm.Visibility = System.Windows.Visibility.Hidden;
            AppForm.WindowState = WindowState.Minimized;
            AppForm.WindowStyle = WindowStyle.None;

            AppForm.Show();

            if (DebugEnabled)
                ChildForms["debug"].Show();
        }

        public void CloseApp()
        {
            foreach (KeyValuePair<string, ApplicationForm> pair in ChildForms)
            {
                if (!pair.Value.webView.IsDisposed)
                {
                    try
                    {
                        pair.Value.webView.ExecuteJavascriptWithResult("if (typeof(onFormClosing) == typeof(Function)) {onFormClosing();}");
                    }
                    catch { }
                }
                try
                {
                    pair.Value.Close();
                }
                catch { }
            }

            AppForm.webView.ExecuteJavascriptWithResult("if (typeof(onAppClosing) == typeof(Function)) {onAppClosing();}");
            AppForm.Close();

            System.Windows.Application.Current.Shutdown();
        }
    }
}
