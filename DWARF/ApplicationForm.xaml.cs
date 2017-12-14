using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Awesomium.Core;
using Awesomium.Core.Data;
using Microsoft.Win32;
using DWARFLib;

namespace DWARF
{

    public class NativeMethods
    {
        public const int WM_NCHITTEST = 0x84;
        public const int HTCAPTION = 2;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
    }

    /// <summary>
    /// Interaction logic for ApplicationForm.xaml
    /// </summary>
    public partial class ApplicationForm : Window
    {
        public string File;
        public string FormName;
        public bool IsAppContainer = false;
        public bool UsesWindowControls = false;
        public bool IsFullscreen = false;
        double animateLocationSpeed = 0.02;
        double animateSizeSpeed = 0.02;
        double animateOpacitySpeed = 0.02;
        Point animateLocationTargetLocation;
        Point animateLocationOriginalLocation;
        double animateOpacityTargetOpacity;
        double animateOpacityOriginalOpacity;
        Size animateSizeTargetSize;
        Size animateSizeOriginalSize;
        double animateLocationDuration = 0;
        double animateOpacityDuration = 0;
        double animateSizeDuration = 0;
        EasingFunctionBase animateLocationEase;
        EasingFunctionBase animateOpacityEase;
        EasingFunctionBase animateSizeEase;
        bool canClose = false;
        bool hasLoaded = false;
        double old_left = 0;
        double old_top = 0;
        double old_bottom = 0;
        double old_right = 0;
        Resize resize;

        Timer animateLocationTimer = new Timer();
        Timer animateOpacityTimer = new Timer();
        Timer animateSizeTimer = new Timer();
        Timer fixWindowControls = new Timer();
        Timer formSizeFix = new Timer();

        string ResizeTitleJS = "document.getElementById('dwarf-window-controls-title').style.width = (document.getElementById('dwarf-window-controls').offsetWidth - 85) + 'px';";

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        double LastWidth = 0;
        double LastHeight = 0;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        JSObject App;
        JSObject FileSystemObj;
        JSObject Animations;
        JSObject EaseFunctions;
        JSObject EasingModes;
        JSObject TaskbarPositions;
        JSObject FormStates;
        JSObject ResizeModes;
        JSObject Plugins;

        public ApplicationForm(string file, bool isAppContainer, bool uses_windowcontrols)
        {
            resize = new Resize(this);
            this.UsesWindowControls = uses_windowcontrols;
            this.File = file;
            this.IsAppContainer = isAppContainer;
            InitializeComponent();

            string customcss = ":-webkit-any(input[type=submit]):not(.custom-appearance):not(.link-button),input[type=checkbox],input[type=radio]{-webkit-appearance:none;-webkit-user-select:none;background-image:-webkit-linear-gradient(#ededed,#ededed 38%,#dedede);border:1px solid rgba(0,0,0,.25);border-radius:2px;box-shadow:0 1px 0 rgba(0,0,0,.08),inset 0 1px 2px rgba(255,255,255,.75);color:#444;font:inherit;margin:0 1px 0 0;text-shadow:0 1px 0 #f0f0f0}:-webkit-any(input[type=submit]):not(.custom-appearance):not(.link-button),:-webkit-any(input[type=submit]):not(.custom-appearance):not(.link-button){-webkit-padding-end:10px;-webkit-padding-start:10px}input[type=checkbox]{bottom:2px;height:13px;position:relative;vertical-align:middle;width:13px}input[type=radio]{border-radius:100%;bottom:3px;height:15px;position:relative;vertical-align:middle;width:15px} input[type=search]{-webkit-appearance:textfield;min-width:160px}input[type=search]::-webkit-textfield-decoration-container{direction:inherit}input[type=checkbox]:checked::before{-webkit-user-select:none;background-image:url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAALCAQAAAADpb+tAAAAaElEQVR4Xl3PIQoCQQCF4Y8JW42D1bDZ4iVEjDbxFpstYhC7eIVBZHkXFGw734sv/TqDQQ8Xb1udja/I8igeIm7Aygj2IpoKTGZnVRNxAHYi4iPiDlA9xX+aNQDFySziqDN6uSp6y7ofEMwZ05uUZRkAAAAASUVORK5CYII=);background-size:100% 100%;content:'';display:block;height:100%;width:100%}input[type=radio]:checked::before{background-color:#666;border-radius:100%;bottom:3px;content:'';display:block;left:3px;position:absolute;right:3px;top:3px}:-webkit-any(input[type=submit]):not(.custom-appearance):not(.link-button)),:enabled:hover:-webkit-any(input[type=checkbox],input[type=radio]{background-image:-webkit-linear-gradient(#f0f0f0,#f0f0f0 38%,#e0e0e0);border-color:rgba(0,0,0,.3);box-shadow:0 1px 0 rgba(0,0,0,.12),inset 0 1px 2px rgba(255,255,255,.95);color:#000}:enabled:hover::-webkit-any(input[type=submit]):not(.custom-appearance):not(.link-button)),:enabled:active:-webkit-any(input[type=checkbox],input[type=radio]{background-image:-webkit-linear-gradient(#e7e7e7,#e7e7e7 38%,#d7d7d7);box-shadow:none;text-shadow:none}:disabled:-webkit-any(input[type=submit]):not(.custom-appearance):not(.link-button),[type=radio]),input:disabled:-webkit-any([type=checkbox]{opacity:.75}:not([type])),[type=search],[type=text],[type=url],input:disabled:-webkit-any([type=password]{color:#999}:-webkit-any(button,:enabled:focus:-webkit-any(input:not([type]),input[type=button],input[type=checkbox],input[type=number],input[type=password],input[type=radio],input[type=search],input[type=submit]):not(.custom-appearance):not(.link-button)),input[type=text],input[type=url]{-webkit-transition:border-color 200ms;border-color:#4d90fe;outline:0;--webkit-outline:0}.link-button{-webkit-box-shadow:none;background:transparent none;border:0;color:#15c;cursor:pointer;font:inherit;margin:0;padding:0 4px}.link-button:hover{text-decoration:underline}.link-button:active{color:#052577;text-decoration:underline}.link-button[disabled]{color:#999;cursor:default;text-decoration:none}.radio) label,:-webkit-any(.checkbox{display:-webkit-inline-box;padding-bottom:7px;padding-top:7px}.radio) label input~span,:-webkit-any(.checkbox{-webkit-margin-start:.6em;display:block}.radio) label:hover,:-webkit-any(.checkbox{color:#000}[type=radio])~span,label&gt;input:disabled:-webkit-any([type=checkbox]{color:#999}[hidden]{display:none!important}html.loading *{-webkit-transition-delay:0!important;-webkit-transition-duration:0!important}body{cursor:default;margin:0}p{line-height:1.8em}h1,h2,h3{-webkit-user-select:none;font-weight:400;line-height:1}h1{font-size:1.5em}h2{font-size:1.3em;margin-bottom:.4em}h3{color:#000;font-size:1.2em;margin-bottom:.8em}a{color:#15c;text-decoration:underline}a:active{color:#052577}html[dir=rtl] .weakrtl,html[dir=rtl] div.weakrtl input{direction:ltr;text-align:right}html[dir=rtl] .favicon-cell.weakrtl{-webkit-padding-end:22px;-webkit-padding-start:0}html[dir=rtl] select.weakrtl{direction:rtl}html[dir=rtl] select.weakrtl option{direction:ltr}html[dir=rtl] .weakrtl input::-webkit-input-placeholder,html[dir=rtl] input.weakrtl::-webkit-input-placeholder{direction:rtl} ::-webkit-scrollbar-track{-webkit-box-shadow:inset 0 0 1px rgba(0,0,0,0.5);background-color:#f5f5f5;}::-webkit-scrollbar-track:hover{-webkit-box-shadow:inset 0 0 3px rgba(0,0,0,0.5);background-color:#ddd}::-webkit-scrollbar{width:10px;height:10px;}::-webkit-scrollbar-thumb{-webkit-box-shadow:inset 0 0 3px rgba(0,0,0,0.5);background-color:#bababa;}::-webkit-scrollbar-thumb:hover{background-color:#9b9b9b;}::-webkit-scrollbar-thumb:active{-webkit-box-shadow:inset 0 0 3px rgba(0,0,0,0.7);background-color:#7a7a7a;}::-webkit-scrollbar-corner{height:0px;width:0px;}";

            if (uses_windowcontrols && DWARFLoader.instance != null)
            {
                customcss += DWARFLoader.instance.WindowControlsCSS;
            }

            webView.WebSession = WebCore.CreateWebSession(new WebPreferences()
            {
                CanScriptsAccessClipboard = true,
                CanScriptsCloseWindows = true,
                CanScriptsOpenWindows = true,
                CustomCSS = customcss,
                EnableGPUAcceleration = true,
                SmoothScrolling = true,
                WebGL = true,
                WebSecurity = false
            });

            animateLocationTimer.Interval = 1;
            animateOpacityTimer.Interval = 1;
            animateSizeTimer.Interval = 1;
            fixWindowControls.Interval = 500;
            formSizeFix.Interval = 500;
            animateLocationTimer.Tick += animateLocationTimer_Tick;
            animateOpacityTimer.Tick += animateOpacityTimer_Tick;
            animateSizeTimer.Tick += animateSizeTimer_Tick;
            fixWindowControls.Tick += fixWindowControls_Tick;
            formSizeFix.Tick += formSizeFix_Tick;
            //formSizeFix.Start();
        }

        void formSizeFix_Tick(object sender, EventArgs e)
        {
            if (this.webView.Width != LastWidth || this.webView.Height != LastHeight)
            {
                SendSize(this.webView.Width, this.webView.Height);
            }
        }

        void fixWindowControls_Tick(object sender, EventArgs e)
        {
            if (UsesWindowControls)
            {
                try
                {
                    webView.ExecuteJavascript(ResizeTitleJS);
                }
                catch { }
            }
        }

        

        void webView_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            /*try
            {
                webView.ExecuteJavascript("if (typeof(onConsoleMessage) != \"undefined\") {onConsoleMessage(\"" + e.EventName + "\", " + e.LineNumber + ", \"" + e.Message + "\", \"" + e.Source + "\");}");
            }
            catch { }*/

            string msg_type = "message";

            if (e.Message.Contains(":"))
            {
                if (e.Message.Split(':')[0].Contains("Error") || e.Message.Split(':')[0].Contains("error"))
                {
                    msg_type = "error";
                }
                else if (e.Message.Split(':')[0].Contains("Warning") || e.Message.Split(':')[0].Contains("Warning"))
                {
                    msg_type = "warning";
                }
            }

            string source = "";

            string responseType = "&laquo;";

            string line = e.LineNumber.ToString();

            if (e.Source == "")
            {
                line += ":DBG";
                source = "VM000";
                responseType = "&raquo;";
            }
            else
            {
                if (e.Source.Length > 14)
                {
                    source = Directory.GetCurrentDirectory().Replace("\\", "/") + "/forms/" + e.Source.Remove(0, 14);
                }
                else
                    source = Directory.GetCurrentDirectory().Replace("\\", "/") + "/forms/" + e.Source;
            }

            string code = "if (typeof(DebugHelper) != \"undefined\") { DebugHelper.notifyConsoleMessage('" + FormName.CleanForJavascript("'", "<br>") + "', '" + e.EventName.CleanForJavascript("'", "<br>") + "', '" + line + "', '" + e.Message.CleanForJavascript("'", "<br>") + "', '" + source.CleanForJavascript("'", "<br>") + "', '" + responseType + "', '" + msg_type + "'); }";

            webView.ExecuteJavascript(code);
        }

        JSObject console;

        void bindFormFunctions(JSObject obj)
        {
            

            fixWindowControls.Start();
            obj.Bind("getForms", true, (s, ee) =>
            {
                List<JSValue> jsforms = new List<JSValue>();

                foreach (KeyValuePair<string, ApplicationForm> pair in DWARFLoader.instance.ChildForms)
                {
                    jsforms.Add((JSValue)pair.Value.FormName);
                }

                ee.Result = new JSValue(jsforms.ToArray());
            });

            obj.Bind("showForm", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    try
                    {
                        form.Show();
                        form.Activate();
                        ee.Result = true;
                    }
                    catch
                    {
                        ee.Result = false;
                    }
                    
                }
            });

            obj.Bind("hideForm", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.Hide();
                    ee.Result = true;
                }
            });

            obj.Bind("minimizeForm", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.WindowState = System.Windows.WindowState.Minimized;
                    IWebView view = (IWebView)webView;

                    view.InjectMouseUp(Awesomium.Core.MouseButton.Left);
                    ee.Result = true;
                }
            });

            obj.Bind("duplicateForm", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    if (!DWARFLoader.instance.ChildForms.ContainsKey(ee.Arguments[1]))
                    {
                        ApplicationForm newform = new ApplicationForm(form.File, false, form.UsesWindowControls);
                        newform.Title = form.Title;

                        DWARFLoader.instance.ChildForms.Add(ee.Arguments[1], newform);

                        ee.Result = true;
                    }
                    else
                        ee.Result = false;
                }
            });

            obj.Bind("close", false, (s, ee) =>
            {
                DWARFLoader.instance.CloseApp();
            });

            obj.Bind("closeForm", false, (s, ee) =>
            {
                this.Close();
            });

            obj.Bind("getCurrentFormName", true, (s, ee) =>
            {
                ee.Result = FormName;
            });

            obj.Bind("getFormSize", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    JSObject size = new JSObject();
                    size["width"] = form.Width - 40;
                    size["height"] = form.Height - 40;
                    ee.Result = size;
                }
            });

            obj.Bind("setFormSize", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.Width = (int)ee.Arguments[1] + 40;
                    form.Height = (int)ee.Arguments[2] + 40;
                    ee.Result = true;
                }
            });

            obj.Bind("getFormLocation", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    JSObject loc = new JSObject();
                    loc["x"] = form.Left + 20;
                    loc["y"] = form.Top + 20;
                    ee.Result = loc;
                }
            });

            obj.Bind("setFormLocation", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.Left = (int)ee.Arguments[1] - 20;
                    form.Top = (int)ee.Arguments[2] - 20;
                    ee.Result = true;
                }
            });

            obj.Bind("setFormOpacity", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.window.Opacity = (double)ee.Arguments[1];
                    ee.Result = true;
                }
            });

            obj.Bind("getFormOpacity", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    ee.Result = form.window.Opacity;
                }
            });

            obj.Bind("setFormTopMost", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.Topmost = (bool)ee.Arguments[1];
                    ee.Result = true;
                }
            });

            obj.Bind("getFormTopMost", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    ee.Result = form.Topmost;
                }
            });

            obj.Bind("setActiveForm", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.Activate();
                    ee.Result = true;
                }
            });

            obj.Bind("setGlobalVar", true, (s, ee) =>
            {
                if (!DWARFLoader.instance.GlobalVars.ContainsKey(ee.Arguments[0]))
                    DWARFLoader.instance.GlobalVars.Add(ee.Arguments[0], ee.Arguments[1]);
                else
                    DWARFLoader.instance.GlobalVars[ee.Arguments[0]] = ee.Arguments[1];

                ee.Result = ee.Arguments[1];
            });

            obj.Bind("getGlobalVar", true, (s, ee) =>
            {
                if (DWARFLoader.instance.GlobalVars.ContainsKey(ee.Arguments[0]))
                    ee.Result = (JSValue)DWARFLoader.instance.GlobalVars[ee.Arguments[0]];
            });

            obj.Bind("unsetGlobalVar", true, (s, ee) =>
            {
                if (DWARFLoader.instance.GlobalVars.ContainsKey(ee.Arguments[0]))
                {
                    DWARFLoader.instance.GlobalVars.Remove(ee.Arguments[0]);
                    ee.Result = true;
                }
            });

            obj.Bind("getTaskbarPosition", true, (s, ee) =>
            {
                Taskbar tb = new Taskbar();

                if (tb.Position == TaskbarPosition.Bottom)
                    ee.Result = 0;
                else if (tb.Position == TaskbarPosition.Left)
                    ee.Result = 1;
                else if (tb.Position == TaskbarPosition.Right)
                    ee.Result = 2;
                else if (tb.Position == TaskbarPosition.Top)
                    ee.Result = 3;
                else if (tb.Position == TaskbarPosition.Unknown)
                    ee.Result = 4;
            });


            obj.Bind("getTaskbarWidth", true, (s, ee) =>
            {
                Taskbar tb = new Taskbar();
                ee.Result = tb.Size.Width;
            });

            obj.Bind("getTaskbarHeight", true, (s, ee) =>
            {
                Taskbar tb = new Taskbar();
                ee.Result = tb.Size.Height;
            });

            obj.Bind("animateFormLocation", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    int easingfunction = (int)ee.Arguments[4];

                    if (easingfunction == 0)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 1)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], ee.Arguments[7]);
                        ee.Result = true;
                    }
                    else if (easingfunction == 2)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 3)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 4)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], ee.Arguments[7]);
                        ee.Result = true;
                    }
                    else if (easingfunction == 5)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 6)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 7)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 8)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 9)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 10)
                    {
                        form.AnimateLocation((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else
                        ee.Result = false;
                }
            });

            obj.Bind("animateFormSize", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    int easingfunction = (int)ee.Arguments[4];

                    if (easingfunction == 0)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 1)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], ee.Arguments[7]);
                        ee.Result = true;
                    }
                    else if (easingfunction == 2)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 3)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 4)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], ee.Arguments[7]);
                        ee.Result = true;
                    }
                    else if (easingfunction == 5)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 6)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), ee.Arguments[6], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 7)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 8)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 9)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 10)
                    {
                        form.AnimateSize((double)ee.Arguments[1], (int)ee.Arguments[2], (int)ee.Arguments[3], easingfunction, ((int)ee.Arguments[5]), null, null);
                        ee.Result = true;
                    }
                    else
                        ee.Result = false;
                }
            });

            obj.Bind("animateFormOpacity", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    int easingfunction = (int)ee.Arguments[3];

                    if (easingfunction == 0)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), ee.Arguments[5], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 1)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), ee.Arguments[5], ee.Arguments[6]);
                        ee.Result = true;
                    }
                    else if (easingfunction == 2)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 3)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 4)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), ee.Arguments[5], ee.Arguments[6]);
                        ee.Result = true;
                    }
                    else if (easingfunction == 5)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[2]), ee.Arguments[5], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 6)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[2]), ee.Arguments[5], null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 7)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 8)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 9)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), null, null);
                        ee.Result = true;
                    }
                    else if (easingfunction == 10)
                    {
                        form.AnimateOpacity((double)ee.Arguments[1], (double)ee.Arguments[2], easingfunction, ((int)ee.Arguments[4]), null, null);
                        ee.Result = true;
                    }
                    else
                        ee.Result = false;
                }
            });

            obj.Bind("getFormTitle", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    ee.Result = form.Title;
                }
            });

            obj.Bind("setFormTitle", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    form.Title = ee.Arguments[1];
                    if (form.UsesWindowControls)
                        form.webView.ExecuteJavascriptWithResult("document.getElementById('dwarf-window-controls-title').innerText = '" + form.Title + "';");
                    ee.Result = true;
                }
            });

            obj.Bind("getPrimaryScreen", true, (s, ee) =>
            {
                ee.Result = getScreenObj(Screen.PrimaryScreen);
            });

            obj.Bind("getScreens", true, (s, ee) =>
            {
                JSValue[] screens = new JSValue[Screen.AllScreens.Length];

                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    screens[i] = getScreenObj(Screen.AllScreens[i]);
                }

                ee.Result = new JSValue(screens);
            });

            obj.Bind("onMouseFormDrag", false, (s, ee) =>
            {
                webView.CaptureMouse();
                ReleaseCapture();
                SendMessage(new WindowInteropHelper(this).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                IWebView view = (IWebView)webView;

                view.InjectMouseUp(Awesomium.Core.MouseButton.Left);

                if (UsesWindowControls)
                {
                    webView.ExecuteJavascript(ResizeTitleJS);
                }
                webView.ReleaseMouseCapture();
            });

            obj.Bind("centerForm", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    Rect workArea = System.Windows.SystemParameters.WorkArea;
                    this.Left = (workArea.Width - this.Width) / 2 + workArea.Left;
                    this.Top = (workArea.Height - this.Height) / 2 + workArea.Top;
                    ee.Result = true;
                }
                else
                    ee.Result = false;
            });

            obj.Bind("invokeOnFormAsync", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    try
                    {
                        form.webView.ExecuteJavascript(ee.Arguments[1]);
                        ee.Result = true;
                    }
                    catch
                    {
                        ee.Result = false;
                    }
                }
                else
                    ee.Result = false;
            });

            obj.Bind("invokeOnForm", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    ee.Result = form.webView.ExecuteJavascriptWithResult(ee.Arguments[1]);
                }
            });

            obj.Bind("writeLine", false, (s, ee) =>
            {
                Console.WriteLine(ee.Arguments[0].ToString());
            });

            obj.Bind("invokeOnFormDebugCallback", false, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    try
                    {
                        string final_code = ee.Arguments[1].ToString().CleanForJavascript("'", "\n");

                        JSValue result = form.webView.ExecuteJavascriptWithResult("var ______d____cod = '" + final_code + "'; console.log(eval(______d____cod));");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            });

            obj.Bind("getFormFilePath", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    ee.Result = Directory.GetCurrentDirectory().Replace("\\", "/") + "/forms/" + form.File;
                }
            });

            obj.Bind("openURL", true, (s, ee) =>
            {
                OpenWebsite(ee.Arguments[0]);
            });

            obj.Bind("getUsesWindowControls", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    ee.Result = form.UsesWindowControls;
                }
            });

            obj.Bind("setUsesWindowControls", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    bool oldvalue = form.UsesWindowControls;

                    form.UsesWindowControls = ee.Arguments[1];

                    if (UsesWindowControls && !oldvalue)
                    {
                        webView.ExecuteJavascript("if(typeof document.body != \"undefined\") { var frag = document.createDocumentFragment(), temp = document.createElement('div'); temp.innerHTML = \"" + DWARFLoader.instance.WindowControlsHTML.Replace("\"", "\\") + "\"; while (temp.firstChild) { frag.appendChild(temp.firstChild);} document.body.insertBefore(frag, document.body.childNodes[0]);}");
                    }
                    else if (!UsesWindowControls && oldvalue)
                    {
                        webView.ExecuteJavascript("if(typeof document.body != \"undefined\") { document.body.removeChild(document.getElementById('dwarf-window-controls')); }");
                    }

                    ee.Result = true;
                }
            });

            obj.Bind("setFormBorder", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    JSValue[] border = (JSValue[])ee.Arguments[1];
                    JSValue[] borderthick;
                    int outthick;
                    if (Int32.TryParse(border[0], out outthick))
                    {
                        borderthick = new JSValue[] { outthick, outthick, outthick, outthick };
                    }
                    else
                    {
                        borderthick = (JSValue[])border[0];
                    }
                    JSValue[] bordercolor = (JSValue[])border[1];

                    border1.BorderThickness = new Thickness((double)borderthick[0], (double)borderthick[1], (double)borderthick[2], (double)borderthick[3]);
                    border1.BorderBrush = new SolidColorBrush(Color.FromArgb((byte)bordercolor[3], (byte)bordercolor[0], (byte)bordercolor[1], (byte)bordercolor[2]));

                    DropShadowEffect effect = new DropShadowEffect();

                    JSValue[] dropshadow = (JSValue[])ee.Arguments[2];
                    JSValue[] dscolor = (JSValue[])dropshadow[0];

                    effect.Color = Color.FromArgb((byte)dscolor[3], (byte)dscolor[0], (byte)dscolor[1], (byte)dscolor[2]);
                    effect.Direction = (double)dropshadow[1];
                    effect.RenderingBias = RenderingBias.Quality;
                    effect.Opacity = (double)dscolor[3] / 255;
                    effect.ShadowDepth = (double)dropshadow[2];
                    effect.BlurRadius = (double)dropshadow[3];

                    border1.BitmapEffect = null;

                    border1.Effect = effect;

                    ee.Result = true;
                }


            });

            obj.Bind("setWindowControlsBackColor", true, (s, ee) =>
            {
                DWARFLoader.instance.windowControlsBackgroundColorRed = (int)ee.Arguments[0];
                DWARFLoader.instance.windowControlsBackgroundColorGreen = (int)ee.Arguments[1];
                DWARFLoader.instance.windowControlsBackgroundColorBlue = (int)ee.Arguments[2];

                bindMouseEventsWindow();

                ee.Result = true;
            });

            obj.Bind("getFormState", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    if (form.WindowState == System.Windows.WindowState.Normal)
                        ee.Result = 0;
                    else if (form.WindowState == System.Windows.WindowState.Maximized)
                        ee.Result = 1;
                    else if (form.WindowState == WindowState.Minimized)
                        ee.Result = 2;
                }
            });

            obj.Bind("setFormState", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    if ((int)ee.Arguments[1] == 0)
                        form.WindowState = System.Windows.WindowState.Normal;
                    else if ((int)ee.Arguments[1] == 1)
                        form.WindowState = System.Windows.WindowState.Maximized;
                    else if ((int)ee.Arguments[1] == 2)
                        form.WindowState = System.Windows.WindowState.Minimized;

                    ee.Result = true;
                }
            });

            obj.Bind("getFormResizeMode", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    if (form.ResizeMode == System.Windows.ResizeMode.CanMinimize)
                        ee.Result = 0;
                    else if (form.ResizeMode == System.Windows.ResizeMode.CanResize)
                        ee.Result = 1;
                    else if (form.ResizeMode == System.Windows.ResizeMode.CanResizeWithGrip)
                        ee.Result = 2;
                    else if (form.ResizeMode == System.Windows.ResizeMode.NoResize)
                        ee.Result = 3;
                }
            });

            obj.Bind("setFormResizeMode", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    if ((int)ee.Arguments[1] == 0)
                        form.ResizeMode = System.Windows.ResizeMode.CanMinimize;
                    else if ((int)ee.Arguments[1] == 1)
                        form.ResizeMode = System.Windows.ResizeMode.CanResize;
                    else if ((int)ee.Arguments[1] == 2)
                        form.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
                    else if ((int)ee.Arguments[1] == 3)
                        form.ResizeMode = System.Windows.ResizeMode.NoResize;

                    ee.Result = true;
                }
            });

            obj.Bind("setFormFullscreen", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    if ((bool)ee.Arguments[1] && !IsFullscreen)
                    {
                        old_left = form.Left;
                        old_top = form.Top;
                        old_right = form.Width;
                        old_bottom = form.Height;

                        form.Left = -20;
                        form.Top = -20;

                        int max_bottom = 0;
                        int max_right = 0;

                        foreach (Screen screen in Screen.AllScreens)
                        {
                            if (screen.Bounds.Bottom > max_bottom)
                                max_bottom = screen.Bounds.Bottom;

                            if (screen.Bounds.Right > max_right)
                                max_right = screen.Bounds.Right;
                        }

                        form.Height = max_bottom;
                        form.Width = max_right;
                        form.IsFullscreen = true;
                    }
                    else if (!(bool)ee.Arguments[1])
                    {
                        form.Left = old_left;
                        form.Top = old_top;
                        form.Width = old_right;
                        form.Height = old_bottom;
                        form.IsFullscreen = false;
                    }

                    ee.Result = true;
                }
            });

            obj.Bind("getFormFullscreen", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    ee.Result = form.IsFullscreen;
                }
            });

            /*obj.Bind("setAppIcon", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    

                    ee.Result = true;
                }
            });
             
            obj.Bind("setFormIcon", true, (s, ee) =>
            {
                ApplicationForm form;

                if (DWARFLoader.instance.ChildForms.TryGetValue(ee.Arguments[0], out form))
                {
                    

                    ee.Result = true;
                }
            });*/
        }

        void bindMouseEventsWindow()
        {
            if (UsesWindowControls)
            {
                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-minimize').onmouseover = function() { document.getElementById('dwarf-window-controls-minimize').style.backgroundColor = 'rgb(" + DWARFLoader.instance.windowControlsBackgroundColorRed + ", " + DWARFLoader.instance.windowControlsBackgroundColorGreen + ", " + DWARFLoader.instance.windowControlsBackgroundColorBlue + ")'; }");
                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-close').onmouseover = function() { document.getElementById('dwarf-window-controls-close').style.backgroundColor = 'rgb(" + DWARFLoader.instance.windowControlsBackgroundColorRed + ", " + DWARFLoader.instance.windowControlsBackgroundColorGreen + ", " + DWARFLoader.instance.windowControlsBackgroundColorBlue + ")'; }");

                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-minimize').onmousedown = function() { document.getElementById('dwarf-window-controls-minimize').style.border = '1px solid rgba(0,0,0,0.3)'; }");
                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-close').onmousedown = function() { document.getElementById('dwarf-window-controls-close').style.border = '1px solid rgba(0,0,0,0.3)'; }");

                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-minimize').onmouseup = function() { document.getElementById('dwarf-window-controls-minimize').style.border = '1px solid rgba(0,0,0,0)'; }");
                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-close').onmouseup = function() { document.getElementById('dwarf-window-controls-close').style.border = '1px solid rgba(0,0,0,0)'; }");

                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-minimize').onmouseout = function() { document.getElementById('dwarf-window-controls-minimize').style.backgroundColor = 'transparent'; document.getElementById('dwarf-window-controls-minimize').style.border = '1px solid rgba(0,0,0,0)'; }");
                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-close').onmouseout = function() { document.getElementById('dwarf-window-controls-close').style.backgroundColor = 'transparent'; document.getElementById('dwarf-window-controls-close').style.border = '1px solid rgba(0,0,0,0)'; }");

                webView.ExecuteJavascript("document.getElementById('dwarf-window-controls-minimize').onmouseup(); document.getElementById('dwarf-window-controls-close').onmouseup();");
            }
        }

        public static void OpenWebsite(string url)
        {
            Process.Start(GetDefaultBrowserPath(), url);
        }

        private static string GetDefaultBrowserPath()
        {
            /*try
            {
                string key = @"http\shell\open\command";
                RegistryKey registryKey =
                Registry.ClassesRoot.OpenSubKey(key, false);
                return ((string)registryKey.GetValue(null, null)).Split('"')[1];
            }
            catch
            {
                return "";
            }*/

            string browserName = "iexplore.exe";
            using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                if (userChoiceKey != null)
                {
                    object progIdValue = userChoiceKey.GetValue("Progid");
                    if (progIdValue != null)
                    {
                        if (progIdValue.ToString().ToLower().Contains("chrome"))
                            browserName = "chrome.exe";
                        else if (progIdValue.ToString().ToLower().Contains("firefox"))
                            browserName = "firefox.exe";
                        else if (progIdValue.ToString().ToLower().Contains("safari"))
                            browserName = "safari.exe";
                        else if (progIdValue.ToString().ToLower().Contains("opera"))
                            browserName = "opera.exe";
                    }
                }
            }

            return browserName;
        }

        void bindFileSystemFunctions(JSObject obj)
        {
            obj.Bind("createDirectory", true, (s, ee) =>
            {
                ee.Result = FileSystem.CreateDirectory(ee.Arguments[0]);
            });

            obj.Bind("createFile", true, (s, ee) =>
            {
                ee.Result = FileSystem.CreateFile(ee.Arguments[0]);
            });

            obj.Bind("overwriteToFile", true, (s, ee) =>
            {
                ee.Result = FileSystem.OverwriteToFile(ee.Arguments[0], ee.Arguments[1]);
            });

            obj.Bind("appendTextToFile", true, (s, ee) =>
            {
                ee.Result = FileSystem.AppendTextToFile(ee.Arguments[0], ee.Arguments[1]);
            });

            obj.Bind("readFromFile", true, (s, ee) =>
            {
                ee.Result = FileSystem.ReadFromFile(ee.Arguments[0]);
            });

            obj.Bind("deleteFile", true, (s, ee) =>
            {
                ee.Result = FileSystem.DeleteFile(ee.Arguments[0]);
            });

            obj.Bind("deleteDirectory", true, (s, ee) =>
            {
                ee.Result = FileSystem.DeleteDirectory(ee.Arguments[0]);
            });
        }

        void bindAnimationFunctions(JSObject obj)
        {
            obj.Bind("ease", true, (s, ee) =>
            {
                if (((int)ee.Arguments[0]) == 0)
                {
                    BackEase ease = new BackEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ease.Amplitude = (double)ee.Arguments[3];
                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 1)
                {
                    BounceEase ease = new BounceEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ease.Bounces = (int)ee.Arguments[3];
                    ease.Bounciness = (double)ee.Arguments[4];
                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 2)
                {
                    CircleEase ease = new CircleEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 3)
                {
                    CubicEase ease = new CubicEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 4)
                {
                    ElasticEase ease = new ElasticEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ease.Oscillations = (int)ee.Arguments[3];
                    ease.Springiness = (double)ee.Arguments[4];

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 5)
                {
                    ExponentialEase ease = new ExponentialEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ease.Exponent = (double)ee.Arguments[3];

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 6)
                {
                    PowerEase ease = new PowerEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ease.Power = (double)ee.Arguments[3];

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 7)
                {
                    QuadraticEase ease = new QuadraticEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 8)
                {
                    QuarticEase ease = new QuarticEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 9)
                {
                    QuinticEase ease = new QuinticEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
                else if (((int)ee.Arguments[0]) == 10)
                {
                    SineEase ease = new SineEase();

                    ease.EasingMode = getEasingMode(((int)ee.Arguments[2]));

                    ee.Result = ease.Ease((double)ee.Arguments[1]);
                }
            });
        }

        void bindPluginsFunctions(JSObject obj)
        {
            obj.Bind("getData", true, (s, ee) =>
            {
                /*try
                {*/
                if (DWARFLoader.instance.ph.PluginExists(ee.Arguments[0]))
                {
                    List<string> args = new List<string>();

                    if (ee.Arguments.Length > 1)
                    {
                        for (int i = 1; i < ee.Arguments.Length; i++)
                        {
                            args.Add(ee.Arguments[i].ToString());
                        }
                    }

                    ee.Result = new JSValue(DWARFLoader.instance.ph.GetData(ee.Arguments[0], args, this).ToString());
                }
                /*}
                catch
                {

                }*/
            });

            obj.Bind("pluginExists", true, (s, ee) =>
            {
                ee.Result = DWARFLoader.instance.ph.PluginExists(ee.Arguments[0]);
            });

            obj.Bind("getPlugins", true, (s, ee) =>
            {
                JSValue[] plugins = new JSValue[DWARFLoader.instance.ph.PluginCount()];

                string[] plugins2 = DWARFLoader.instance.ph.GetPluginNames();

                for (int i = 0; i < plugins2.Length; i++)
                {
                    plugins[i] = plugins2[i];
                }

                ee.Result = plugins;
            });
        }

        public void JS(string js)
        {
            if (webView.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                webView.Dispatcher.Invoke(() => JS(js));
            }
            else
            {
                webView.ExecuteJavascript(js);
            }
        }

        public JSValue JSA(string js)
        {
            if (webView.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                return webView.Dispatcher.Invoke(() => JSA(js));
            }
            else
            {
                return webView.ExecuteJavascriptWithResult(js);
            }
        }

        private void Awesomium_Windows_Forms_WebControl_DocumentReady(object sender, Awesomium.Core.UrlEventArgs e)
        {
            try
            {
                if (!hasLoaded)
                {
                    if (webView.Source.AbsoluteUri == "data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==" && File == "dwarf://debug")
                    {
                        webView.LoadHTML(DWARF.Properties.Resources.debug_html);
                    }
                    else
                    {

                        App = webView.CreateGlobalJavascriptObject("App");
                        bindFormFunctions(App);

                        /*console = webView.CreateGlobalJavascriptObject("console");
                        bindConsoleFunctions(console);*/

                        using (FileSystemObj = webView.CreateGlobalJavascriptObject("FileSystem"))
                        {
                            bindFileSystemFunctions(FileSystemObj);
                        }

                        using (Animations = webView.CreateGlobalJavascriptObject("Animations"))
                        {
                            bindAnimationFunctions(Animations);
                        }

                        Plugins = webView.CreateGlobalJavascriptObject("Plugins");
                        bindPluginsFunctions(Plugins);

                        using (EaseFunctions = webView.CreateGlobalJavascriptObject("EasingFunctions"))
                        {
                            EaseFunctions["BackEase"] = 0;
                            EaseFunctions["BounceEase"] = 1;
                            EaseFunctions["CircleEase"] = 2;
                            EaseFunctions["CubicEase"] = 3;
                            EaseFunctions["ElasticEase"] = 4;
                            EaseFunctions["ExponentialEase"] = 5;
                            EaseFunctions["PowerEase"] = 6;
                            EaseFunctions["QuadraticEase"] = 7;
                            EaseFunctions["QuarticEase"] = 8;
                            EaseFunctions["QuinticEase"] = 9;
                            EaseFunctions["SineEase"] = 10;
                        }

                        using (EasingModes = webView.CreateGlobalJavascriptObject("EasingModes"))
                        {
                            EasingModes["EaseIn"] = 0;
                            EasingModes["EaseInOut"] = 1;
                            EasingModes["EaseOut"] = 2;
                        }

                        using (TaskbarPositions = webView.CreateGlobalJavascriptObject("TaskbarPositions"))
                        {
                            TaskbarPositions["Bottom"] = 0;
                            TaskbarPositions["Left"] = 1;
                            TaskbarPositions["Right"] = 2;
                            TaskbarPositions["Top"] = 3;
                            TaskbarPositions["Unknown"] = 4;
                        }

                        using (FormStates = webView.CreateGlobalJavascriptObject("FormStates"))
                        {
                            FormStates["Normal"] = 0;
                            FormStates["Maximized"] = 1;
                            FormStates["Minimized"] = 2;
                        }

                        using (ResizeModes = webView.CreateGlobalJavascriptObject("ResizeModes"))
                        {
                            ResizeModes["CanMinimize"] = 0;
                            ResizeModes["CanResize"] = 1;
                            ResizeModes["CanResizeWithGrip"] = 2;
                            ResizeModes["NoResize"] = 3;
                        }

                        webView.ExecuteJavascript(DWARFLoader.instance.PluginJavascript);

                        if (DWARFLoader.instance.DebugEnabled)
                        {
                            webView.ExecuteJavascriptWithResult(DWARF.Properties.Resources.debug_js);
                        }

                        console = (JSObject)webView.ExecuteJavascriptWithResult("console");

                        if (File == "dwarf://debug")
                        {
                            webView.ExecuteJavascript("if (typeof(onFormLoaded) != \"undefined\") {onFormLoaded();}");
                        }
                        else
                        {
                            if (!IsAppContainer)
                            {
                                webView.ExecuteJavascript("if (typeof(onFormLoaded) != \"undefined\") {onFormLoaded();}");
                                if (UsesWindowControls)
                                {
                                    webView.ExecuteJavascript("if(typeof document.body != \"undefined\") { if (document.getElementById('dwarf-window-controls') != undefined) {document.body.removeChild(document.getElementById('dwarf-window-controls'))} var frag = document.createDocumentFragment(), temp = document.createElement('div'); temp.setAttribute('id', 'dwarf-window-controls'); temp.innerHTML = \"" + DWARFLoader.instance.WindowControlsHTML.Replace("\"", "\\") + "\"; while (temp.firstChild) { frag.appendChild(temp.firstChild);} document.body.insertBefore(frag, document.body.childNodes[0]); document.getElementById('dwarf-window-controls-title').innerText = '" + Title + "'; " + ResizeTitleJS + "}");
                                    bindMouseEventsWindow();
                                }
                            }
                            else
                            {
                                webView.ExecuteJavascriptWithResult(System.IO.File.ReadAllText(new FileInfo(DWARFLoader.instance.dwarf_conf_file).DirectoryName + "/app.js"));
                                webView.ExecuteJavascript("if (typeof(onAppLoaded) != \"undefined\") {onAppLoaded();}");
                            }
                        }
                    }
                }
                else
                {
                    hasLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ex. in ln 1334:\n\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        JSObject getScreenObj(Screen screen)
        {
            JSObject screen_obj = new JSObject();
            JSObject bounds = new JSObject();
            JSObject bounds_location = new JSObject();
            JSObject bounds_size = new JSObject();
            JSObject working_area = new JSObject();
            JSObject working_area_location = new JSObject();
            JSObject working_area_size = new JSObject();

            bounds_location["X"] = screen.Bounds.Location.X;
            bounds_location["Y"] = screen.Bounds.Location.Y;

            bounds_size["Width"] = screen.Bounds.Size.Width;
            bounds_size["Height"] = screen.Bounds.Size.Height;

            bounds["Bottom"] = screen.Bounds.Bottom;
            bounds["Height"] = screen.Bounds.Height;
            bounds["Left"] = screen.Bounds.Left;
            bounds["Location"] = bounds_location;
            bounds["Right"] = screen.Bounds.Right;
            bounds["Size"] = bounds_size;
            bounds["Top"] = screen.Bounds.Top;
            bounds["Width"] = screen.Bounds.Width;
            bounds["X"] = screen.Bounds.X;
            bounds["Y"] = screen.Bounds.Y;

            working_area_location["X"] = screen.WorkingArea.Location.X;
            working_area_location["Y"] = screen.WorkingArea.Location.Y;

            working_area_size["Width"] = screen.WorkingArea.Size.Width;
            working_area_size["Height"] = screen.WorkingArea.Size.Height;

            working_area["Bottom"] = screen.WorkingArea.Bottom;
            working_area["Height"] = screen.WorkingArea.Height;
            working_area["Left"] = screen.WorkingArea.Left;
            working_area["Location"] = working_area_location;
            working_area["Right"] = screen.WorkingArea.Right;
            working_area["Size"] = working_area_size;
            working_area["Top"] = screen.WorkingArea.Top;
            working_area["Width"] = screen.WorkingArea.Width;
            working_area["X"] = screen.WorkingArea.X;
            working_area["Y"] = screen.WorkingArea.Y;

            screen_obj["Bounds"] = bounds;
            screen_obj["DeviceName"] = screen.DeviceName;
            screen_obj["Primary"] = screen.Primary;
            screen_obj["WorkingArea"] = working_area;

            return screen_obj;
        }

        public void AnimateLocation(double speed, int x, int y, int easingfunction, int easingmode, dynamic var1, dynamic var2)
        {
            if (easingfunction == 0)
            {
                BackEase ease = new BackEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Amplitude = (double)var1;
                animateLocationEase = ease;
            }
            else if (easingfunction == 1)
            {
                BounceEase ease = new BounceEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Bounces = (int)var1;
                ease.Bounciness = (double)var2;
                animateLocationEase = ease;
            }
            else if (easingfunction == 2)
            {
                CircleEase ease = new CircleEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateLocationEase = ease;
            }
            else if (easingfunction == 3)
            {
                CubicEase ease = new CubicEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateLocationEase = ease;
            }
            else if (easingfunction == 4)
            {
                ElasticEase ease = new ElasticEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Oscillations = (int)var1;
                ease.Springiness = (double)var2;

                animateLocationEase = ease;
            }
            else if (easingfunction == 5)
            {
                ExponentialEase ease = new ExponentialEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Exponent = (double)var1;

                animateLocationEase = ease;
            }
            else if (easingfunction == 6)
            {
                PowerEase ease = new PowerEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Power = (double)var1;

                animateLocationEase = ease;
            }
            else if (easingfunction == 7)
            {
                QuadraticEase ease = new QuadraticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateLocationEase = ease;
            }
            else if (easingfunction == 8)
            {
                QuarticEase ease = new QuarticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateLocationEase = ease;
            }
            else if (easingfunction == 9)
            {
                QuinticEase ease = new QuinticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateLocationEase = ease;
            }
            else if (easingfunction == 10)
            {
                SineEase ease = new SineEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateLocationEase = ease;
            }

            this.animateLocationSpeed = speed;
            animateLocationDuration = 0;
            animateLocationOriginalLocation = new Point(Left, Top);
            animateLocationTargetLocation = new Point(x-20, y-20);
            animateLocationTimer.Start();
        }

        public void AnimateSize(double speed, int width, int height, int easingfunction, int easingmode, dynamic var1, dynamic var2)
        {
            if (easingfunction == 0)
            {
                BackEase ease = new BackEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Amplitude = (double)var1;
                animateSizeEase = ease;
            }
            else if (easingfunction == 1)
            {
                BounceEase ease = new BounceEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Bounces = (int)var1;
                ease.Bounciness = (double)var2;
                animateSizeEase = ease;
            }
            else if (easingfunction == 2)
            {
                CircleEase ease = new CircleEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateSizeEase = ease;
            }
            else if (easingfunction == 3)
            {
                CubicEase ease = new CubicEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateSizeEase = ease;
            }
            else if (easingfunction == 4)
            {
                ElasticEase ease = new ElasticEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Oscillations = (int)var1;
                ease.Springiness = (double)var2;

                animateSizeEase = ease;
            }
            else if (easingfunction == 5)
            {
                ExponentialEase ease = new ExponentialEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Exponent = (double)var1;

                animateSizeEase = ease;
            }
            else if (easingfunction == 6)
            {
                PowerEase ease = new PowerEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Power = (double)var1;

                animateSizeEase = ease;
            }
            else if (easingfunction == 7)
            {
                QuadraticEase ease = new QuadraticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateSizeEase = ease;
            }
            else if (easingfunction == 8)
            {
                QuarticEase ease = new QuarticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateSizeEase = ease;
            }
            else if (easingfunction == 9)
            {
                QuinticEase ease = new QuinticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateSizeEase = ease;
            }
            else if (easingfunction == 10)
            {
                SineEase ease = new SineEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateSizeEase = ease;
            }

            this.animateSizeSpeed = speed;
            animateSizeDuration = 0;
            animateSizeOriginalSize = new Size(Width, Height);
            animateSizeTargetSize = new Size(width+20, height+20);
            animateSizeTimer.Start();
        }

        public void AnimateOpacity(double speed, double targetValue, int easingfunction, int easingmode, dynamic var1, dynamic var2)
        {
            if (easingfunction == 0)
            {
                BackEase ease = new BackEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Amplitude = (double)var1;
                animateOpacityEase = ease;
            }
            else if (easingfunction == 1)
            {
                BounceEase ease = new BounceEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Bounces = (int)var1;
                ease.Bounciness = (double)var2;
                animateOpacityEase = ease;
            }
            else if (easingfunction == 2)
            {
                CircleEase ease = new CircleEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateOpacityEase = ease;
            }
            else if (easingfunction == 3)
            {
                CubicEase ease = new CubicEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateOpacityEase = ease;
            }
            else if (easingfunction == 4)
            {
                ElasticEase ease = new ElasticEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Oscillations = (int)var1;
                ease.Springiness = (double)var2;

                animateOpacityEase = ease;
            }
            else if (easingfunction == 5)
            {
                ExponentialEase ease = new ExponentialEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Exponent = (double)var1;

                animateOpacityEase = ease;
            }
            else if (easingfunction == 6)
            {
                PowerEase ease = new PowerEase();

                ease.EasingMode = getEasingMode(easingmode);

                ease.Power = (double)var1;

                animateOpacityEase = ease;
            }
            else if (easingfunction == 7)
            {
                QuadraticEase ease = new QuadraticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateOpacityEase = ease;
            }
            else if (easingfunction == 8)
            {
                QuarticEase ease = new QuarticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateOpacityEase = ease;
            }
            else if (easingfunction == 9)
            {
                QuinticEase ease = new QuinticEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateOpacityEase = ease;
            }
            else if (easingfunction == 10)
            {
                SineEase ease = new SineEase();

                ease.EasingMode = getEasingMode(easingmode);

                animateOpacityEase = ease;
            }

            this.animateOpacityTargetOpacity = targetValue;
            this.animateOpacityOriginalOpacity = window.Opacity;
            this.animateOpacityDuration = 0;
            this.animateOpacitySpeed = speed;
            animateOpacityTimer.Start();
        }

        private void animateLocationTimer_Tick(object sender, EventArgs e)
        {
            if (animateLocationDuration < 1)
            {
                animateLocationDuration += animateLocationSpeed;

                double newX = 0;
                double newY = 0;

                if (animateLocationTargetLocation.Y > animateLocationOriginalLocation.Y)
                {
                    newY = animateLocationEase.Ease(animateLocationDuration) * (animateLocationTargetLocation.Y - animateLocationOriginalLocation.Y) + animateLocationOriginalLocation.Y;
                }
                else
                {
                    newY = animateLocationOriginalLocation.Y - animateLocationEase.Ease(animateLocationDuration) * (animateLocationOriginalLocation.Y - animateLocationTargetLocation.Y);
                }

                if (animateLocationTargetLocation.X > animateLocationOriginalLocation.X)
                {
                    newX = animateLocationEase.Ease(animateLocationDuration) * (animateLocationTargetLocation.X - animateLocationOriginalLocation.X) + animateLocationOriginalLocation.X;
                }
                else
                {
                    newX = animateLocationOriginalLocation.X - animateLocationEase.Ease(animateLocationDuration) * (animateLocationOriginalLocation.X - animateLocationTargetLocation.X);
                }

                Left = newX;
                Top = newY;
            }
            else
            {
                animateLocationTimer.Stop();
                animateLocationDuration = 0;
            }
        }

        private void animateSizeTimer_Tick(object sender, EventArgs e)
        {
            if (animateSizeDuration < 1)
            {
                animateSizeDuration += animateSizeSpeed;

                double newWidth = 0;
                double newHeight = 0;

                if (animateSizeTargetSize.Height > animateSizeOriginalSize.Height)
                {
                    newHeight = animateSizeEase.Ease(animateSizeDuration) * ((animateSizeTargetSize.Height + 20) - animateSizeOriginalSize.Height) + animateSizeOriginalSize.Height;
                }
                else
                {
                    newHeight = animateSizeOriginalSize.Height - animateSizeEase.Ease(animateSizeDuration) * (animateSizeOriginalSize.Height - (animateSizeTargetSize.Height + 20));
                }

                if (animateSizeTargetSize.Width > animateSizeOriginalSize.Width)
                {
                    newWidth = animateSizeEase.Ease(animateSizeDuration) * ((animateSizeTargetSize.Width + 20) - animateSizeOriginalSize.Width) + animateSizeOriginalSize.Width;
                }
                else
                {
                    newWidth = animateSizeOriginalSize.Width - animateSizeEase.Ease(animateSizeDuration) * (animateSizeOriginalSize.Width - (animateSizeTargetSize.Width + 20));
                }

                Width = newWidth;
                Height = newHeight;
            }
            else
            {
                animateSizeTimer.Stop();
                animateSizeDuration = 0;
            }
        }

        private void animateOpacityTimer_Tick(object sender, EventArgs e)
        {
            if (animateOpacityDuration < 1)
            {
                animateOpacityDuration += animateOpacitySpeed;

                if (animateOpacityTargetOpacity > animateOpacityOriginalOpacity)
                {
                    window.Opacity = animateOpacityEase.Ease(animateOpacityDuration) * (animateOpacityTargetOpacity - animateOpacityOriginalOpacity) + animateOpacityOriginalOpacity;
                }
                else if (animateOpacityTargetOpacity < animateOpacityOriginalOpacity)
                {
                    window.Opacity = animateOpacityOriginalOpacity - (animateOpacityEase.Ease(animateOpacityDuration) * (animateOpacityOriginalOpacity - animateOpacityTargetOpacity));
                }
            }
            else
            {
                animateOpacityTimer.Stop();
                animateOpacityDuration = 0;
            }
        }

        EasingMode getEasingMode(int easingmode)
        {
            if (easingmode == 0)
                return EasingMode.EaseIn;
            else if (easingmode == 1)
                return EasingMode.EaseInOut;
            else
                return EasingMode.EaseOut;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canClose)
            {
                e.Cancel = true;
                try
                {
                    JSValue result = webView.ExecuteJavascriptWithResult("if (typeof(onFormClosing) != \"undefined\") {onFormClosing();}");

                    if ((int)result == 1)
                    {
                        canClose = true;
                        DWARFLoader.instance.CloseApp();
                    }
                    else if ((int)result == 2)
                    {
                        canClose = false;
                        e.Cancel = true;
                        this.Hide();
                    }
                    else if (result.IsUndefined || (int)result == 0)
                    {
                        canClose = true;
                        e.Cancel = false;
                    }
                    else
                    {
                        webView.ExecuteJavascript("alert('Unknown exit code: " + result.ToString().Replace("'", "") + "');");
                    }
                }
                catch
                {

                }
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (webView.IsDocumentReady)
            {
                try
                {
                    int left = (int)Math.Round(this.Left, 0);
                    int top = (int)Math.Round(this.Top, 0);

                    if (this.WindowStyle == System.Windows.WindowStyle.None)
                    {
                        left += 40;
                        top += 40;
                    }

                    webView.ExecuteJavascript("if (typeof(onFormLocationChanged) != \"undefined\") {onFormLocationChanged(" + left + ", " + top + ");}");
                }
                catch { }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*if (webView.IsDocumentReady)
            {
                try
                {
                    int width = (int)Math.Round(this.webView.Width, 0);
                    int height = (int)Math.Round(this.webView.Height, 0);

                    if (this.WindowStyle == System.Windows.WindowStyle.None)
                    {
                        width -= 40;
                        height -= 40;
                    }

                    SendSize(width, height);
                }
                catch { }
            }*/
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (webView.IsDocumentReady)
            {
                try
                {
                    webView.ExecuteJavascript("if (typeof(onFormDeactivated) != \"undefined\") {onFormDeactivated();}");
                }
                catch { }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (webView.IsDocumentReady)
            {
                try
                {
                    webView.ExecuteJavascript("if (typeof(onFormActivated) != \"undefined\") {onFormActivated();}");
                }
                catch { }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            webView.WebSession.AddDataSource("dwarf", new DirectoryDataSource(Directory.GetCurrentDirectory() + "/forms"));
            webView.ConsoleMessage += webView_ConsoleMessage;

            if (!IsAppContainer && File != "dwarf://debug")
            {
                webView.Source = new Uri("asset://dwarf/" + File);
            }
            else
            {
                webView.Source = new Uri("data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private bool isResizing = false;

        private void Resize_Init(object sender, MouseButtonEventArgs e)
        {
            if (this.ResizeMode != System.Windows.ResizeMode.NoResize)
            {
                isResizing = true;
                Canvas senderC = sender as Canvas;

                senderC.CaptureMouse();
            }
        }

        private void Resize_Move(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isResizing)
            {
                Canvas senderC = sender as Canvas;

                if (senderC != null)
                {
                    double width = e.GetPosition(this).X;
                    double height = e.GetPosition(this).Y;

                    width += 21;
                    height += 21;

                    senderC.CaptureMouse();

                    if (senderC.Name == "Right_Resize")
                    {
                        resize.Right(width);
                    }
                    else if (senderC.Name == "Bottom_Resize")
                    {
                        resize.Bottom(height);
                    }
                    else if (senderC.Name == "BottomRight_Resize")
                    {
                        resize.Bottom(height);
                        resize.Right(width);
                    }
                    else if (senderC.Name == "Left_Resize")
                    {
                        resize.Left(width);
                    }
                    else if (senderC.Name == "Top_Resize")
                    {
                        resize.Top(height);
                    }
                    else if (senderC.Name == "TopLeft_Resize")
                    {
                        resize.Top(height);
                        resize.Left(width);
                    }
                    else if (senderC.Name == "TopRight_Resize")
                    {
                        resize.Top(height);
                        resize.Right(width);
                    }
                    else if (senderC.Name == "BottomLeft_Resize")
                    {
                        resize.Bottom(height);
                        resize.Left(width);
                    }
                }
            }
        }

        private void Resize_Mouseup(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;
            Canvas senderC = sender as Canvas;

            senderC.ReleaseMouseCapture();
        }

        public void SendSize(double width, double height)
        {
            if (webView.IsDocumentReady)
            {
                webView.ExecuteJavascript("if (typeof(onFormSizeChanged) != \"undefined\") {onFormSizeChanged(" + Math.Round(width, 0) + ", " + Math.Round(height, 0) + ");} " + ResizeTitleJS);
                LastWidth = width;
                LastHeight = height;
            }
        }

        private void webView_LoginRequest(object sender, LoginRequestEventArgs e)
        {
            /*e.Username = "admin";
            e.Password = Prompt2.ShowDialog("This page is requesting authentication").Password;
            if (e.Password == "")
                e.Handled = EventHandling.Default;
            else
                e.Handled = EventHandling.Modal;*/
        }
    }

    public class AuthenticationResult
    {
        public bool Cancelled;
        public string Username;
        public string Password;

        public AuthenticationResult(bool Cancelled, string Username, string Password)
        {
            this.Cancelled = Cancelled;
            this.Username = Username;
            this.Password = Password;
        }
    }

    public static class Prompt2
    {
        public static AuthenticationResult ShowDialog(string text)
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 150;
            prompt.Text = text;
            System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label() { Left = 50, Top = 20, Text = "Password: " };
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 50, Top = 50, Width = 400 };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Ok", Left = 300, Width = 100, Top = 140 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            textBox.PasswordChar = '*';
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            if (prompt.ShowDialog() == DialogResult.Cancel)
                return new AuthenticationResult(true, "", "");
            else
                return new AuthenticationResult(false, textBox.Text, "");
        }
    }

    public class Resize
    {
        private ApplicationForm form;

        public Resize(ApplicationForm form)
        {
            this.form = form;
        }

        public void Right(double width)
        {
            if (width > 0)
                form.Width = width;
        }

        public void Left(double width)
        {
            double oldLeft = form.Left;

            form.Left += (width - form.Width) + form.Width - 43;

            if (form.Width + (oldLeft - form.Left) > 0)
                form.Width += oldLeft - form.Left;
        }

        public void Bottom(double height)
        {
            if (height > 0)
                form.Height = height;
        }

        public void Top(double height)
        {
            double oldTop = form.Top;

            form.Top += (height - form.Height) + form.Height - 43;

            if (form.Height + (oldTop - form.Top) > 0)
                form.Height += oldTop - form.Top;
        }
    }

    public static class StringExtensions
    {
        public static string CleanForJavascript(this String instr, string escapeChar, string newLine)
        {
            //string final = instr.Replace("\\\\n", "\\\\\\n").Replace("\\n", "\\\\n").Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\\" + escapeChar, "\\\\" + escapeChar).Replace(escapeChar, "\\" + escapeChar);
            string final = instr.Replace("\\", "\\\\").Replace(escapeChar, "\\" + escapeChar).Replace("\r\n", "\\n").Replace("\n", "\\n");
            //Console.WriteLine(final);
            return final;
        }
    }
}
