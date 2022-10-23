using HTAlt;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Yorot
{
    /// <summary>
    /// Yorot App Manager
    /// </summary>
    public class AppManager : YorotManager
    {
        /// <summary>
        /// Creates a new App manager.
        /// </summary>
        /// <param name="configFile">Location of the configuration file on drive.</param>
        public AppManager(YorotMain main) : base(main.AppsConfig, main)
        {
            new DefaultApps.WebBrowser(this).Register(this);
            new DefaultApps.Downloads(this).Register(this);
            // TODO: Load Apps fromn here
            for (int i = 0; i < Apps.Count; i++) // Claims management for all applications that are loaded.
            {
                Apps[i].Manager = this;
                Apps[i].InitAppFolder();
            }
            UpdateCount++;
        }

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
                "<root>" + Environment.NewLine +
                "<!-- Yorot Apps Config File" + Environment.NewLine + Environment.NewLine +
                "This file is used to configure applications." + Environment.NewLine +
                "Editing this file might cause problems with apps." + Environment.NewLine +
                "-->" + Environment.NewLine +
                "<Apps>" + Environment.NewLine;
            for (int i = 0; i < AppInfos.Count; i++)
            {
                YorotAppInfo app = AppInfos[i];

                x += "<App CodeName=\"" + app.AppCodeName + "\" Enabled=\"" + (app.isEnabled ? "true" : "false") + "\" Origin=\"" + app.AppOrigin.ToString() + "\" OriginInfo=\"" + app.AppOriginInfo.Replace(Environment.NewLine, "[NEWLINE]") + "\" />" + Environment.NewLine;
            }
            return (x + "</Apps>" + Environment.NewLine + "</root>").BeautifyXML();
        }

        /// <summary>
        /// Serves information of update count. Used in App menu refreshment
        /// </summary>
        public int UpdateCount { get; set; } = 0;

        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="YorotApp"/>(s).
        /// </summary>
        public List<YorotApp> Apps { get; set; } = new List<YorotApp>();

        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="YorotAppInfo"/>(s).
        /// </summary>
        public List<YorotAppInfo> AppInfos { get; set; } = new List<YorotAppInfo>();

        /// <summary>
        /// Gets <see cref="YorotApp"/> by it's <see cref="YorotApp.AppCodeName"/>.
        /// </summary>
        /// <param name="appcn"><see cref="YorotApp.AppCodeName"/></param>
        /// <returns><see cref="YorotApp"/></returns>
        public YorotApp FindByAppCN(string appcn)
        {
            return Apps.Find(i => string.Equals(i.AppCodeName, appcn));
        }

        // TODO: DON'T actullay add apps from here, load DLLs and determine if they are enbaled or not here.
        // TODO: Do the same to extensions and add ability to inject JS too.
        public override void ExtractXml(XmlNode rootNode)
        {
            List<string> appliedSettings = new List<string>();
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                switch (node.Name)
                {
                    case "Apps":
                        if (appliedSettings.FindAll(it => it == node.Name).Count > 0)
                        {
                            Output.WriteLine("[AppMan] Threw away \"" + node.OuterXml + "\". Configurtion already applied.", LogLevel.Warning);
                            break;
                        }
                        appliedSettings.Add(node.Name);
                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                        {
                            XmlNode subnode = node.ChildNodes[ı];
                            if (subnode.Name == "App" && subnode.Attributes["CodeName"] != null)
                            {
                                if (Apps.FindAll(it => it.AppCodeName == subnode.Attributes["CodeName"].Value).Count > 0)
                                {
                                    Output.WriteLine("[AppMan] Threw away \"" + subnode.OuterXml + "\". App already installed.", LogLevel.Warning);
                                }
                                else
                                {
                                    YorotAppInfo app = new YorotAppInfo(subnode.Attributes["CodeName"].Value, this);
                                    if (subnode.Attributes["Pinned"] != null)
                                    {
                                        app.isPinned = subnode.Attributes["Pinned"].Value == "true";
                                    }
                                    if (subnode.Attributes["Origin"] != null && subnode.Attributes["OriginInfo"] != null)
                                    {
                                        app.AppOrigin = (YorotAppOrigin)(int.Parse(subnode.Attributes["Origin"].Value));
                                        app.AppOriginInfo = subnode.Attributes["OriginInfo"].Value.Replace("[NEWLINE]", Environment.NewLine);
                                    }
                                    else
                                    {
                                        app.AppOrigin = YorotAppOrigin.Unknown;
                                        app.AppOriginInfo = "Xml node responsible for this app did not include \"Origin\" and \"OriginInfo\" attributes in config file \"" + ConfigFile + "\".";
                                    }
                                    AppInfos.Add(app);
                                }
                            }
                            else
                            {
                                if (!subnode.NodeIsComment())
                                {
                                    Output.WriteLine("[AppMan] Threw away \"" + subnode.OuterXml + "\". unsupported.", LogLevel.Warning);
                                }
                            }
                        }
                        break;

                    default:
                        if (!node.NodeIsComment())
                        {
                            Output.WriteLine("[AppMan] Threw away \"" + node.OuterXml + "\". Invalid configurtion.", LogLevel.Warning);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Changes the pinned status of an app.
        /// </summary>
        /// <param name="value">Codename of the app.</param>
        /// <param name="v">A <see cref="bool"/> representing the isPinned status.</param>
        public void SetPinStatus(string value, bool v)
        {
            List<YorotAppInfo> l = AppInfos.FindAll(it => it.AppCodeName == value);
            if (l.Count > 0)
            {
                l[0].isPinned = v;
            }
            else
            {
                throw new ArgumentException("Cannot find \"" + value + "\" in apps list.");
            }
        }

        /// <summary>
        /// Enables an app.
        /// </summary>
        /// <param name="value">Codename of the app.</param>
        public void Enable(string value)
        {
            List<YorotAppInfo> l = AppInfos.FindAll(it => it.AppCodeName == value);
            if (l.Count > 0)
            {
                l[0].isEnabled = true;
            }
            else
            {
                throw new ArgumentException("Cannot find \"" + value + "\" in apps list.");
            }
        }

        /// <summary>
        /// Checks if an app is loaded into system.
        /// </summary>
        /// <param name="value">COde name of the app.</param>
        /// <returns><see cref="bool"/></returns>
        public bool AppExists(string value)
        {
            return Apps.FindAll(it => it.AppCodeName == value).Count > 0;
        }

        /// <summary>
        /// Unşnstalls the <paramref name="app"/>.
        /// </summary>
        /// <param name="app"><see cref="YorotApp"/></param>
        public void Uninstall(YorotApp app)
        {
            app.Reset();
            var folder = app.AppFolder;
            if (!string.IsNullOrWhiteSpace(folder) && System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.Delete(folder, true);
            }
            Apps.Remove(app);
        }
    }

    // TODO: Remove these except Yorot & Settings & Yopad, we will bundle the rest instead.

    /// <summary>
    /// This class contains default <see cref="YorotApp"/>s.
    /// </summary>
    public static class DefaultApps
    {
        /// <summary>
        /// Yorot
        /// </summary>
        public class WebBrowser : YorotApp
        {
            public WebBrowser(AppManager manager) : base("com.haltroy.yorot", manager)
            {
            }
        }

        /// <summary>
        /// The Downloads app
        /// </summary>
        public class Downloads : YorotApp
        {
            public Downloads(AppManager manager) : base("com.yorot.downloads", manager)
            {
            }
        }
    }

    public class YorotAppInfo
    {
        public YorotAppInfo(string appCodeName, AppManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            AppCodeName = appCodeName ?? throw new ArgumentNullException(nameof(appCodeName));
        }

        /// <summary>
        /// <see cref="AppManager"/>.
        /// </summary>
        public AppManager Manager { get; set; }

        /// <summary>
        /// Origin of aapp.
        /// </summary>
        public YorotAppOrigin AppOrigin { get; set; }

        /// <summary>
        /// Information about origin of this app.
        /// </summary>
        public string AppOriginInfo { get; set; }

        /// <summary>
        /// <see cref="true"/> if this app is pinned, otherwise <seealso cref="false"/>.
        /// </summary>
        public bool isPinned { get; set; } = false;

        /// <summary>
        /// Determines if the application is enabled.
        /// </summary>
        public bool isEnabled { get; set; } = false;

        /// <summary>
        /// Code Name of this App.
        /// </summary>
        public string AppCodeName { get; set; }

        /// <summary>
        /// Permissions of this application.
        /// </summary>
        public YorotAppPermissions Permissions { get; set; }

        /// <summary>
        /// <see cref="YorotApp"/>.
        /// </summary>
        public YorotApp App
        {
            get
            {
                var apps = Manager.Apps.FindAll(it => it.AppCodeName == AppCodeName);
                if (apps.Count > 0)
                {
                    return apps[0];
                }
                throw new Exception("Cannot find app \"" + AppCodeName + "\".");
            }
        }
    }

    // TODO: Make this into a GUI formed way where the Yorot flavor would parse them into the actual GUI stuff.
    // Ex. 1: YorotApps Button --[Yorot-Avalonia]--> AvaloniaUI Button
    // Ex. 2: YorotApps.Button --[Yorot flavor that uses Winforms]--> System.Windows.Forms.Button

    /// <summary>
    /// A Yorot App.
    /// </summary>
    public class YorotApp
    {
        /// <summary>
        /// Creates new <see cref="YorotApp"/>.
        /// </summary>
        /// <param name="xmlNode"><see cref="XmlNode"/> that contains details of <see cref="YorotApp"/>.</param>
        public YorotApp(string appCodeName, AppManager manager)
        {
            AppCodeName = appCodeName;
            Manager = manager;
            string configFile = Manager.Main.AppsFolder + appCodeName + System.IO.Path.DirectorySeparatorChar + "app.ycf";
            if (!string.IsNullOrWhiteSpace(configFile))
            {
                if (System.IO.File.Exists(configFile))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(HTAlt.Tools.ReadFile(configFile, System.Text.Encoding.Unicode));
                        XmlNode rootNode = HTAlt.Tools.FindRoot(doc.DocumentElement);
                        List<string> appliedSettings = new List<string>();
                        for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                        {
                            XmlNode node = rootNode.ChildNodes[i];
                            if (appliedSettings.Contains(node.Name.ToLowerEnglish()))
                            {
                                Output.WriteLine("[YorotApp] Threw away \"" + node.OuterXml + "\", configuration already applied.", LogLevel.Warning);
                                break;
                            }
                            appliedSettings.Add(node.Name.ToLowerEnglish());
                            switch (node.Name.ToLowerEnglish())
                            {
                                case "appicon":
                                    AppIcon = node.InnerXml.XmlToString();
                                    break;

                                case "author":
                                    Author = node.InnerXml.XmlToString();
                                    break;

                                case "version":
                                    Version = node.InnerXml.XmlToString();
                                    break;

                                case "versionno":
                                    VersionNo = int.Parse(node.InnerXml.XmlToString());
                                    break;

                                case "startfile":
                                    StartFile = node.InnerXml.XmlToString();
                                    break;

                                case "appcodename":
                                    AppCodeName = node.InnerXml.XmlToString();
                                    break;

                                case "appname":
                                    AppName = node.InnerXml.XmlToString();
                                    break;

                                case "multiplesession":
                                    MultipleSession = node.InnerXml.XmlToString() == "true";
                                    break;

                                default:
                                    Output.WriteLine("[YorotApp] Threw away \"" + node.OuterXml + "\", Invalid configuration.", LogLevel.Warning);
                                    break;
                            }
                        }
                    }
                    catch (XmlException xex)
                    {
                        Error = xex;
                    }
                    catch (Exception ex)
                    {
                        Error = ex;
                    }
                }
                else
                {
                    Error = new ArgumentException("Configuration file does not exists.");
                    Output.WriteLine("[YorotApp] Cannot load app \"" + appCodeName + "\", configuration file does not exists.", LogLevel.Error);
                }
            }
            else
            {
                Error = new ArgumentNullException("appCodeName");
                Output.WriteLine("[YorotApp] Cannot load app, codename was empty.", LogLevel.Error);
            }
            if (!System.IO.Directory.Exists(AppFolder)) { System.IO.Directory.CreateDirectory(AppFolder); }
        }

        /// <summary>
        /// Registers this app to <see cref="AppManager"/>.
        /// </summary>
        /// <param name="manager"><see cref="AppManager"/></param>
        public void Register(AppManager manager)
        {
            manager.Apps.Add(this);
            Manager = manager;
        }

        /// <summary>
        /// Resets an app by deleting all cache.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// The main form of this app.
        /// </summary>
        public AppForms.Form MainForm { get; set; }

        /// <summary>
        /// Determines if this application had error(s) while loading.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Creates new <see cref="YorotApp"/>.
        /// </summary>
        public YorotApp(AppManager manager)
        {
            Manager = manager;
        }

        /// <summary>
        /// Inits the app folders.
        /// </summary>
        public void InitAppFolder()
        {
            if (!System.IO.Directory.Exists(AppFolder)) { System.IO.Directory.CreateDirectory(AppFolder); }
        }

        /// <summary>
        /// gets the main manager of this app.
        /// </summary>
        public AppManager Manager { get; set; }

        /// <summary>
        /// Icon location of app.
        /// </summary>
        public string AppIcon { get; set; }

        /// <summary>
        /// Creator of this app.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Display version of this app.
        /// </summary>
        public string Version { get; set; } = "0";

        /// <summary>
        /// Actual version of this app. Used by Foster.
        /// </summary>
        public int VersionNo { get; set; } = 0;

        /// <summary>
        /// URL of Foster file for this app.
        /// </summary>
        public string FosterLink { get; set; }

        /// <summary>
        /// Name of file (or URL) when loaded while starting app.
        /// </summary>
        public string StartFile { get; set; }

        /// <summary>
        /// Codename of app.
        /// </summary>
        public string AppCodeName { get; set; }

        /// <summary>
        /// Display name of application.
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Determines if this application supports multiple sessions.
        /// </summary>
        public bool MultipleSession { get; set; } = false;

        /// <summary>
        /// Determines if this <see cref="YorotApp"/> is a system app.
        /// </summary>
        public bool isSystemApp { get; set; }

        /// <summary>
        /// Folder that hosts all application data.
        /// </summary>
        public string AppFolder => Manager.Main.AppsFolder + AppCodeName;

        /// <summary>
        /// Folder that hosts the DLL file of application.
        /// </summary>
        public string AppFile => Manager.Main.AppsFolder + AppCodeName + ".dll";

        /// <summary>
        /// Figures out this <see cref="YorotApp"/> has open session(s).
        /// </summary>
        /// <returns><see cref="true"/> if this <see cref="YorotApp"/> has open session(s), otherwise <see cref="false"/>.</returns>
        public bool hasSessions()
        {
            return Layouts.FindAll(i => i.hasSessions).Count > 0;
        }

        /// <summary>
        /// Returns app size in bytes.
        /// </summary>+
        public long AppSize => HTAlt.Tools.GetDirSize(AppFolder);

        /// <summary>
        /// Gets size of app.
        /// </summary>
        /// <param name="bytes">Translation of word "bytes".</param>
        /// <returns><see cref="string"/></returns>
        public string GetAppSizeInfo(string bytes)
        {
            long size = AppSize;
            if (size > 1099511627776F) //TiB
            {
                return (size / 1099511627776F) + " TiB (" + size + " " + bytes + ")";
            }
            else if (size > 1073741824F) //GiB
            {
                return (size / 1073741824F) + " GiB (" + size + " " + bytes + ")";
            }
            else if (size > 1048576F) //MiB
            {
                return (size / 1048576F) + " MiB (" + size + " " + bytes + ")";
            }
            else if (size > 1024F) // KiB
            {
                return (size / 1024F) + " KiB (" + size + " " + bytes + ")";
            }
            else
            {
                return size + " " + bytes;
            }
        }

        /// <summary>
        /// Creates a copy of this <see cref="YorotApp"/> excluding <see cref="YorotApp.AssocForm"/>,<see cref="YorotApp.AssocTab"/> and <see cref="YorotApp.AssocPB"/>.
        /// </summary>
        /// <returns><see cref="YorotApp"/></returns>
        public YorotApp CreateCarbonCopy(AppManager manager)
        {
            return new YorotApp(manager)
            {
                AppIcon = AppIcon,
                isSystemApp = isSystemApp,
                AppCodeName = AppCodeName,
                FosterLink = FosterLink,
                AppName = AppName,
                Author = Author,
                MultipleSession = MultipleSession,
                StartFile = StartFile,
                Version = Version,
                VersionNo = VersionNo,
                MainForm = MainForm,
            };
        }

        /// <summary>
        /// List of layouts (sessions) for this app.
        /// </summary>
        public List<YorotAppLayout> Layouts { get; set; } = new List<YorotAppLayout>();
    }

    public abstract class YorotAppLayout
    {
        /// <summary>
        /// Ownner of this app layout.
        /// </summary>
        public YorotApp Parent { get; set; }

        /// <summary>
        /// <see cref="true"/> to ask this layout to reload app, otherwise <see cref="false"/>
        /// </summary>
        public bool waitLayoutRestart { get; set; } = false;

        /// <summary>
        /// <see cref="true"/> if this layout has a session. This bool is mostly customized for each other layout that inherit this class.
        /// </summary>
        public bool hasSessions { get; set; } = false;

        /// <summary>
        /// Arguments of this layout.
        /// </summary>
        public string[] Args { get; set; } = new string[] { };
    }

    public enum YorotAppOrigin
    {
        /// <summary>
        /// App instalked from other software outside Yorot.
        /// </summary>
        Other,

        /// <summary>
        /// App installed using Yopad.
        /// </summary>
        Yopad,

        /// <summary>
        /// App downloaded and instaalled from store.
        /// </summary>
        Store,

        /// <summary>
        /// App embedded to Yorot.
        /// </summary>
        Embedded,

        /// <summary>
        /// No information given about this app origin.
        /// </summary>
        Unknown,
    }

    /// <summary>
    /// Permissions of a generic <see cref="YorotApp"/>.
    /// </summary>
    public class YorotAppPermissions
    {
        /// <summary>
        /// Creates a new <see cref="YorotAppPermissions"/> with default values.
        /// </summary>
        /// <param name="app"><see cref="YorotApp"/></param>
        public YorotAppPermissions(YorotApp app) : this(app, YorotPermissionMode.None, YorotPermissionMode.None, YorotPermissionMode.None, 0, false) { }

        /// <summary>
        /// Creates a new <see cref="YorotappPermissions"/> with custom values.
        /// </summary>
        /// <param name="app"><see cref="YorotApp"/></param>
        /// <param name="runInc">Determines if this app can be launched in Incognito mode.</param>
        /// <param name="runStart">Determines if this app will be launched on start.</param>
        /// <param name="allowNotif">Determines if this app can send notifications.</param>
        /// <param name="notifPriority">Determines the priority of the notifications coming from this webapp.
        /// <para></para>
        /// -1 = Prioritize others
        /// <para></para>
        /// 0 = Normal
        /// <para></para>
        /// 1 = Prioritize this</param>
        /// <param name="startNotifOnBoot">Dertermines if Yorot should start notification listener on start for this app.</param>
        public YorotAppPermissions(YorotApp app, YorotPermissionMode runInc, YorotPermissionMode runStart, YorotPermissionMode allowNotif, int notifPriority, bool startNotifOnBoot)
        {
            App = app;
            if (app.Manager == null || app.Manager.Main == null)
            {
            }
            else
            {
                this.runInc = new YorotPermission("runInc", app, app.Manager.Main, runInc);
                this.runStart = new YorotPermission("runStart", app, app.Manager.Main, runStart);
                this.allowNotif = new YorotPermission("allowNotif", app, app.Manager.Main, allowNotif);
                this.notifPriority = notifPriority;
                this.startNotifOnBoot = startNotifOnBoot;
            }
        }

        /// <summary>
        /// The app of these permissions.
        /// </summary>
        public YorotApp App { get; set; }

        /// <summary>
        /// Determines if this app can be launched in Incognito mode.
        /// </summary>
        public YorotPermission runInc { get; set; }

        /// <summary>
        /// Determines if this app will be launched on start.
        /// </summary>
        public YorotPermission runStart { get; set; }

        /// <summary>
        /// Determines if this app can send notifications.
        /// </summary>
        public YorotPermission allowNotif { get; set; }

        /// <summary>
        /// Determines the priority of the notifications coming from this webapp.
        /// <para></para>
        /// -1 = Prioritize others
        /// <para></para>
        /// 0 = Normal
        /// <para></para>
        /// 1 = Prioritize this
        /// </summary>
        public int notifPriority { get; set; } = 0;

        /// <summary>
        /// Dertermines if Yorot should start notification listener on start for this app.
        /// </summary>
        public bool startNotifOnBoot { get; set; } = false;
    }
}