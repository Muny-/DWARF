﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DWARFSetup.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DWARFSetup.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function onAppLoaded()
        ///{
        ///    App.showForm(&quot;index&quot;);
        ///}
        ///
        ///function onAppClosing()
        ///{
        ///    
        ///}.
        /// </summary>
        public static string app {
            get {
                return ResourceManager.GetString("app", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///    &quot;name&quot;: &quot;DWARF Foundry&quot;,
        ///    &quot;author&quot;: &quot;Kevin Smith + Cameron Kelley // Azuru Networks&quot;,
        ///    &quot;debugging&quot;: true,
        ///    &quot;version&quot;: 0.1,
        ///    &quot;forms&quot;: [
        ///        {
        ///            &quot;name&quot;: &quot;index&quot;,
        ///            &quot;title&quot;: &quot;DWARF Foundry&quot;,
        ///            &quot;file&quot;: &quot;index.html&quot;,
        ///            &quot;width&quot;: 560,
        ///            &quot;height&quot;: 360,
        ///            &quot;startposition&quot;: &quot;center&quot;,
        ///            &quot;defaultwindowborder&quot;: true
        ///        }
        ///    ]
        ///}.
        /// </summary>
        public static string app_info {
            get {
                return ResourceManager.GetString("app_info", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] Elysium {
            get {
                object obj = ResourceManager.GetObject("Elysium", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        public static System.Drawing.Icon icon {
            get {
                object obj = ResourceManager.GetObject("icon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;html&gt;
        ///&lt;head&gt;
        ///	&lt;title&gt;DWARF Foundry&lt;/title&gt;
        ///	&lt;style type=&quot;text/css&quot;&gt;
        ///		body {
        ///			font-family: &quot;Segoe UI Light&quot;;
        ///		}
        ///
        ///		#head {
        ///			padding: 10px;
        ///		}
        ///
        ///		img, button {
        ///			vertical-align: middle;
        ///		}
        ///
        ///		button {
        ///			opacity: 0;
        ///			-webkit-transition: opacity 50ms linear;
        ///		}
        ///
        ///		button:hover {
        ///			opacity:1;
        ///		}
        ///	&lt;/style&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///	&lt;script type=&quot;text/javascript&quot;&gt;
        ///		function onFormLoaded() {
        ///			App.animateFormOpacity(&quot;index&quot;, 0.02, 1, EasingFunctions.QuinticEase, EasingModes.EaseOut); 
        ///		}
        ///
        ///		function onForm [rest of string was truncated]&quot;;.
        /// </summary>
        public static string index {
            get {
                return ResourceManager.GetString("index", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap shield {
            get {
                object obj = ResourceManager.GetObject("shield", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
