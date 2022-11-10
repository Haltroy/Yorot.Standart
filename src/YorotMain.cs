﻿using HTAlt;
using LibFoster;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Yorot
{
    /// <summary>
    /// Main class for Yorot.
    /// <para></para>Suggestion: Create a new instance of this class in your project's one of the public static classes for easy access and avoid duplicates. See Yorot-Win32 project for simple implementation.
    /// </summary>
    public abstract class YorotMain
    {
        /// <summary>
        /// Creates a new Yorot Main.
        /// </summary>
        /// <param name="appPath">Path of the application, mostly &quot;.yorot&quot; folder</param>
        /// <param name="codename">Codename of the Yorot or the flavor.</param>
        /// <param name="isIncognito"><see cref="true"/> to start Yorot in incognito mode, otherwise <seealso cref="false"/>.</param>
        /// <param name="version">Version of the Yorot or the flavor.</param>
        /// <param name="name">Name of the Yorot or the flavor.</param>
        /// <param name="branch">Name of the branch app.</param>
        /// <param name="devMode">Enables Developer mode.</param>
        /// <param name="verno">Version number of the Yorot or the flavor.</param>
        public YorotMain(string appPath, string name, string codename, string version, int verno, string branch, bool devMode = false, bool isIncognito = false)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("\"name\" caanot be empty."); }
            Name = name;
            if (string.IsNullOrWhiteSpace(codename)) { throw new ArgumentException("\"codename\" caanot be empty."); }
            CodeName = codename;
            if (string.IsNullOrWhiteSpace(version)) { throw new ArgumentException("\"version\" caanot be empty."); }
            VersionText = version;
            if (string.IsNullOrWhiteSpace(branch)) { throw new ArgumentException("\"branch\" caanot be empty."); }
            YorotBranch = branch;
            if (verno <= 0) { throw new ArgumentException("\"verno\" must be bigger than zero."); }
            Version = verno;
            if (string.IsNullOrWhiteSpace(appPath)) { throw new ArgumentNullException("\"appPath\" cannot be empty."); };
            if (!System.IO.Directory.Exists(appPath)) { System.IO.Directory.CreateDirectory(appPath); }
            if (!appPath.HasWriteAccess()) { throw new System.IO.FileLoadException("Cannot access to path \"" + appPath + "\"."); }
            Incognito = isIncognito;
            AppPath = appPath;
            Cleanup = new Cleanup(this);

#if DEBUG
            DevMode = true;
#endif

            DevMode = devMode;
            if (!System.IO.Directory.Exists(AppPath + "var" + System.IO.Path.DirectorySeparatorChar)) { System.IO.Directory.CreateDirectory(AppPath + "var" + System.IO.Path.DirectorySeparatorChar); }
            LangConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "lang.ycf";
            EngineFolder = AppPath + "cef";
            LangFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "lang" + System.IO.Path.DirectorySeparatorChar;
            ExtFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "ext" + System.IO.Path.DirectorySeparatorChar;
            ThemesFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "themes" + System.IO.Path.DirectorySeparatorChar;
            ThemeConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "themes.ycf";
            ExtConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "ext.ycf";
            AppsConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "apps.ycf";
            AppsFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "apps" + System.IO.Path.DirectorySeparatorChar;
            ProfilesFolder = AppPath + "user" + System.IO.Path.DirectorySeparatorChar;
            ProfileConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "users.ycf";
            SiteIconCache = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "icons" + System.IO.Path.DirectorySeparatorChar;
            LogFolder = AppPath + "logs" + System.IO.Path.DirectorySeparatorChar;
            TempFolder = AppPath + "temp" + System.IO.Path.DirectorySeparatorChar;
            WHFolder = AppPath + "temp" + System.IO.Path.DirectorySeparatorChar + "wh" + System.IO.Path.DirectorySeparatorChar;
            YopadConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "yopad.ycf";
            HTAlt.Output.LogDirPath = LogFolder;
            if (DevMode) { Output.WriteLine("[Main] Developer mode activated."); }
            if (!System.IO.Directory.Exists(LogFolder)) { System.IO.Directory.CreateDirectory(LogFolder); }
            if (System.IO.File.Exists(appPath + System.IO.Path.DirectorySeparatorChar + "yorot.moved")) // Detect if Yorot Users folder is moved. If then, move info to new app path.
            {
                string MOVED = HTAlt.Tools.ReadFile(appPath + System.IO.Path.DirectorySeparatorChar + "yorot.moved", Encoding.Unicode);
                if (!string.IsNullOrWhiteSpace(MOVED))
                {
                    if (MOVED.HasWriteAccess())
                    {
                        AppPath = MOVED;
                        EngineFolder = AppPath + "cef";
                        LangConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "lang.ycf";
                        LangFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "lang" + System.IO.Path.DirectorySeparatorChar;
                        ExtFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "ext" + System.IO.Path.DirectorySeparatorChar;
                        ThemesFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "themes" + System.IO.Path.DirectorySeparatorChar;
                        ThemeConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "themes.ycf";
                        ExtConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "ext.ycf";
                        AppsConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "apps.ycf";
                        AppsFolder = AppPath + "addons" + System.IO.Path.DirectorySeparatorChar + "apps" + System.IO.Path.DirectorySeparatorChar;
                        ProfilesFolder = AppPath + "user" + System.IO.Path.DirectorySeparatorChar;
                        ProfileConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "users.ycf";
                        LogFolder = AppPath + "logs" + System.IO.Path.DirectorySeparatorChar;
                        TempFolder = AppPath + "temp" + System.IO.Path.DirectorySeparatorChar;
                        WHFolder = AppPath + "temp" + System.IO.Path.DirectorySeparatorChar + "wh" + System.IO.Path.DirectorySeparatorChar;
                        SiteIconCache = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "icons" + System.IO.Path.DirectorySeparatorChar;
                        YopadConfig = AppPath + "var" + System.IO.Path.DirectorySeparatorChar + "yopad.ycf";
                        Output.LogDirPath = LogFolder;
                        if (!System.IO.Directory.Exists(LogFolder)) { System.IO.Directory.CreateDirectory(LogFolder); }
                    }
                    else
                    {
                        Output.WriteLine("[YorotMain] Ignoring yorot.moved file. Cannot use path given in this file.", HTAlt.LogLevel.Warning);
                    }
                }
                else
                {
                    Output.WriteLine("[YorotMain] Ignoring yorot.moved file. File does not contains suitable path data.", HTAlt.LogLevel.Warning);
                }
            }

            if (!System.IO.Directory.Exists(EngineFolder)) { System.IO.Directory.CreateDirectory(EngineFolder); }
            if (!System.IO.Directory.Exists(LangFolder)) { System.IO.Directory.CreateDirectory(LangFolder); }
            if (!System.IO.Directory.Exists(ExtFolder)) { System.IO.Directory.CreateDirectory(ExtFolder); }
            if (!System.IO.Directory.Exists(ThemesFolder)) { System.IO.Directory.CreateDirectory(ThemesFolder); }
            if (!System.IO.Directory.Exists(AppsFolder)) { System.IO.Directory.CreateDirectory(AppsFolder); }
            if (!System.IO.Directory.Exists(ProfilesFolder)) { System.IO.Directory.CreateDirectory(ProfilesFolder); }
            if (!System.IO.Directory.Exists(TempFolder)) { System.IO.Directory.CreateDirectory(TempFolder); }
            if (!System.IO.Directory.Exists(WHFolder)) { System.IO.Directory.CreateDirectory(WHFolder); }
            BeforeInit();
            Yopad = new Yopad(this);
            AppMan = new AppManager(this);
            ThemeMan = new ThemeManager(this);
            LangMan = new YorotLangManager(this);
            Extensions = new ExtensionManager(this);
            Profiles = new ProfileManager(this);
            Wolfhook = new Wolfhook(WHFolder);
            if (Profiles.Profiles.Count < 2 && Profiles.Profiles.FindAll(it => it.Name == "root").Count > 0)
            {
                OOBE = true;
            }
            AfterInit();
        }

        /// <summary>
        /// Determines if Out-of-Box-Experience should be displayed, or in laymans term: user's first time running Yorot.
        /// </summary>
        public bool OOBE { get; set; } = false;

        /// <summary>
        /// Gets the user agent string for your application with specific <paramref name="engineversion"/>.
        /// </summary>
        /// <param name="engineName">Name of the engine. Ex.: Chrome</param>
        /// <param name="engineversion">Version of the engine used.</param>
        /// <returns><see cref="string"/></returns>
        public string GetUserAgent(string engineName, string engineVersion)
        {
            string osInfo = "";

            var os = System.Environment.OSVersion;

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                osInfo = "Windows NT " + os.Version.Major + "." + os.Version.Minor + "; [WINPROC]; [PROC]";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                // TODO: (LONG-TERM) If Avalonia gets a Waylanbd support, figure out which one we use.
                // Currently, Avalonia is X11 only so this is not a problem.
                osInfo = "X11; Linux [XPROC]";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                osInfo = "Macintosh; [OSXPROC] Mac OS X" + os.Version.ToString().Replace(".", "_").Replace(" ", "_");
            }
            switch (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture)
            {
                case System.Runtime.InteropServices.Architecture.X86:
                    osInfo = osInfo.Replace("[PROC]", "x86").Replace("[WINPROC]", "Win32").Replace("[OSXPROC]", "Intel").Replace("[XPROC]", "i386");
                    break;

                case System.Runtime.InteropServices.Architecture.X64:
                    osInfo = osInfo.Replace("[PROC]", "x64").Replace("[WINPROC]", "Win64").Replace("[OSXPROC]", "Intel").Replace("[XPROC]", "x86_64");
                    break;

                case System.Runtime.InteropServices.Architecture.Arm:
                    osInfo = osInfo.Replace("[PROC]", "ARM").Replace("[WINPROC]", "WinARM").Replace("[OSXPROC]", "M1").Replace("[XPROC]", "ARM");
                    break;

                case System.Runtime.InteropServices.Architecture.Arm64:
                    osInfo = osInfo.Replace("[PROC]", "Aarch64").Replace("[WINPROC]", "WinARM64").Replace("[OSXPROC]", "M1").Replace("[XPROC]", "ARM64");
                    break;
            }
            CurrentUserAgent = $"Mozilla/5.0 ({osInfo}) AppleWebKit/537.36 (KHTML, like Gecko) {engineName}/{engineVersion} Safari/537.36 {Name}/{VersionText}";
            return CurrentUserAgent;
        }

        private System.Collections.Generic.List<YorotBrowserWebSource> _sources = new System.Collections.Generic.List<YorotBrowserWebSource>();

        public void RegisterWebSource(string url, object data, string type, bool takesArguments = false, bool ignoredBySessionMan = false)
        {
            _sources.Add(new YorotBrowserWebSource
            {
                Data = data,
                Url = url,
                Type = type,
                IgnoreOnSessionList = ignoredBySessionMan,
                TakesArguments = takesArguments
            });
        }

        public void UnregisterWebSource(string url)
        {
            var sources = _sources.FindAll(x => x.Url == url);
            if (sources.Count > 0)
            {
                for (int i = 0; i < sources.Count; i++)
                {
                    _sources.Remove(sources[i]);
                }
            }
        }

        public YorotBrowserWebSource GetWebSource(string url)
        {
            var sources = _sources.FindAll(x => x.Url == url);
            if (sources.Count > 0)
            {
                return sources[0];
            }
            else
            {
                return null;
            }
        }

        public string[] GetWebSource(string url, List<YorotBrowserWebSource.Argument> args = null)
        {
            var sources = _sources.FindAll(x => x.Url == url);
            if (sources.Count > 0 && sources[0].Data is string str)
            {
                string newstr = str;
                string[,] itArgs = new string[,]
                {
                    { "Theme.BackColor", CurrentTheme.BackColor.ToHex() },
                    { "Theme.BackColor2", CurrentTheme.BackColor2.ToHex() },
                    { "Theme.BackColor3", CurrentTheme.BackColor3.ToHex() },
                    { "Theme.BackColor4", CurrentTheme.BackColor4.ToHex() },
                    { "Theme.ForeColor", CurrentTheme.ForeColor.ToHex() },
                    { "Theme.OverlayColor", CurrentTheme.OverlayColor.ToHex() },
                    { "Theme.OverlayColor2", CurrentTheme.OverlayColor2.ToHex() },
                    { "Theme.OverlayColor3", CurrentTheme.OverlayColor3.ToHex() },
                    { "Theme.OverlayColor4", CurrentTheme.OverlayColor4.ToHex() },
                    { "Theme.OverlayForeColor", CurrentTheme.OverlayForeColor.ToHex() },
                    { "Theme.OverlayForeColor2", CurrentTheme.OverlayForeColor2.ToHex() },
                    { "Theme.OverlayForeColor3", CurrentTheme.OverlayForeColor3.ToHex() },
                    { "Theme.OverlayForeColor4", CurrentTheme.OverlayForeColor4.ToHex() },
                    { "Theme.ArtForeColor", CurrentTheme.ArtForeColor.ToHex() },
                    { "Theme.ArtForeColor2", CurrentTheme.ArtForeColor2.ToHex() },
                    { "Theme.ArtForeColor3", CurrentTheme.ArtForeColor3.ToHex() },
                    { "Theme.ArtForeColor4", CurrentTheme.ArtForeColor4.ToHex() },
                    { "Theme.ArtColor", CurrentTheme.ArtColor.ToHex() },
                    { "Theme.ArtColor2", CurrentTheme.ArtColor2.ToHex() },
                    { "Theme.ArtColor3", CurrentTheme.ArtColor3.ToHex() },
                    { "Theme.ArtColor4", CurrentTheme.ArtColor4.ToHex() },
                    { "Info.YorotVerText", VersionText },
                    { "Info.YorotVer", "" + Version },
                    { "Info.OperatingSystem", System.Runtime.InteropServices.RuntimeInformation.OSDescription },
                    { "Info.Arch", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString() },
                    { "Info.CommandLineArgs", Environment.CommandLine },
                    { "Info.UserAgent", CurrentUserAgent },
                    { "Info.AppExePath", System.Reflection.Assembly.GetExecutingAssembly().Location },
                    { "Info.ProfilePath", Profiles.Current.Path },
                    { "Info.Language", CurrentLanguage.Name },
                    { "Info.HTAltVer", HTAlt.HTInfo.ProjectVersion },
                    { "Info.EngineVer", CurrentEngineVer },
                    { "Info.DotNetVer", Environment.Version.ToString() },
                    { "Info.FosterVer", FosterSettings.FosterVersion + "" },
                    { "Info.FostrianVer", Assembly.GetAssembly(typeof(Fostrian.FostrianNode)).GetName().Version.ToString() },
                    { "Info.Homepage", CurrentSettings.HomePage },
                };
                for (int i = 0; i < itArgs.Length / 2; i++)
                {
                    newstr = newstr.Replace("[" + itArgs[i, 0] + "]", itArgs[i, 1]);
                }

                if (args != null)
                {
                    for (int i = 0; i < args.Count; i++)
                    {
                        newstr = newstr.Replace("[Parameter." + args[i].Name + "]", sources[0].Url == "yorot://search" ? (CurrentSettings.SearchEngine.Search(args[i].Value)) : args[i].Value);
                    }
                }
                newstr = CurrentLanguage.Prepare(newstr);
                return new string[] { newstr, sources[0].Type };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Current User Agent string, generated after <see cref="GetUserAgent(string, string)"/>.
        /// </summary>
        public string CurrentUserAgent { get; set; }

        /// <summary>
        /// Gets/sets current engine version.
        /// </summary>
        public virtual string CurrentEngineVer { get; set; }

        /// <summary>
        /// Current Branch of Yorot. (ex. Yorot-Avalonia)
        /// </summary>
        public string YorotBranch { get; set; }

        /// <summary>
        /// Determines if this app is in Development Mode.
        /// </summary>
        public bool DevMode { get; set; }

        /// <summary>
        /// The cleanup service.
        /// </summary>
        public Cleanup Cleanup { get; set; }

        /// <summary>
        /// Event raised by permissions on a permission change.
        /// </summary>
        public abstract YorotPermissionMode OnPermissionRequest(YorotPermission permission, YorotPermissionMode requested);

        /// <summary>
        /// Event raised before launching all managers.
        /// </summary>
        public abstract void BeforeInit();

        /// <summary>
        /// Event raised after launching all managers.
        /// </summary>
        public abstract void AfterInit();

        /// <summary>
        /// Yorot Package Distribution Service.
        /// </summary>
        public Yopad Yopad { get; set; }

        /// <summary>
        /// Gets the current theme applied by user.
        /// </summary>
        public YorotTheme CurrentTheme => Profiles.Current.Settings.CurrentTheme;

        /// <summary>
        /// Gets the current settings applied by user.
        /// </summary>
        public Settings CurrentSettings => Profiles.Current.Settings;

        /// <summary>
        /// Gets the current language applied by user.
        /// </summary>
        public YorotLanguage CurrentLanguage => Profiles.Current.Settings.CurrentLanguage;

        /// <summary>
        /// Determines if this session is Incognito mode.
        /// </summary>
        public bool Incognito { get; set; }

        /// <summary>
        /// Profiles Manager
        /// </summary>
        public ProfileManager Profiles { get; set; }

        /// <summary>
        /// Application Manager
        /// </summary>
        public AppManager AppMan { get; set; }

        /// <summary>
        /// Theme Manager
        /// </summary>
        public ThemeManager ThemeMan { get; set; }

        /// <summary>
        /// Wolfhook folder.
        /// </summary>
        public string WHFolder { get; set; }

        /// <summary>
        /// The folder that contains the engine used by Yorot. Mostly hosts libCEF and other Chromium Embedded Framework related files.
        /// </summary>
        public string EngineFolder { get; set; }

        /// <summary>
        /// The folder that hosts the locale files used by the engine.
        /// </summary>
        public string EngineLocaleFolder => System.IO.Path.Combine(EngineFolder, "locales");

        /// <summary>
        /// Language Manager
        /// </summary>
        public YorotLangManager LangMan { get; set; }

        /// <summary>
        /// Extension Manager
        /// </summary>
        public ExtensionManager Extensions { get; set; }

        /// <summary>
        /// Folder that contains the site icons.
        /// </summary>
        public string SiteIconCache { get; set; }

        /// <summary>
        /// Wolfhook Content Delivery System
        /// </summary>
        public Wolfhook Wolfhook { get; set; }

        /// <summary>
        /// Saves configuration and shuts down.
        /// </summary>
        public void Shutdown(bool forceSave = false)
        {
            if (!Incognito || forceSave)
            {
                Profiles.Current.Settings.Save();
                Profiles.Current.Settings.FavManager.Save();
                Profiles.Current.Settings.SessionManager.Shutdown();
                Profiles.Current.Settings.DownloadManager.Save();
                Profiles.Save();
                AppMan.Save();
                ThemeMan.Save();
                LangMan.Save();
                Extensions.Save();
                Wolfhook.StopSearch();
            }
        }

        /// <summary>
        /// Location of application files.
        /// </summary>
        public string AppPath { get; set; }

        /// <summary>
        /// Language configuration file location.
        /// </summary>
        public string LangConfig { get; set; }

        /// <summary>
        /// Language folder
        /// </summary>
        public string LangFolder { get; set; }

        /// <summary>
        /// Extensions folder
        /// </summary>
        public string ExtFolder { get; set; }

        /// <summary>
        /// Themes location.
        /// </summary>
        public string ThemesFolder { get; set; }

        /// <summary>
        /// Themes Manager configuration file location.
        /// </summary>
        public string ThemeConfig { get; set; }

        /// <summary>
        /// Extension Manager configuration file location.
        /// </summary>
        public string ExtConfig { get; set; }

        /// <summary>
        /// Yorot App Manager configuration file location.
        /// </summary>
        public string AppsConfig { get; set; }

        /// <summary>
        /// Yorot App Manager Application storage.
        /// </summary>
        public string AppsFolder { get; set; }

        /// <summary>
        /// User profiles folder.
        /// </summary>
        public string ProfilesFolder { get; set; }

        /// <summary>
        /// Temporary Folder
        /// </summary>
        public string TempFolder { get; set; }

        /// <summary>
        /// Yopad COnfig File
        /// </summary>
        public string YopadConfig { get; set; }

        /// <summary>
        /// User profiles configuration file.
        /// </summary>
        public string ProfileConfig { get; set; }

        /// <summary>
        /// Logs folder.
        /// </summary>
        public string LogFolder { get; set; }

        /// <summary>
        /// Name of your application. Ex.: Yorot
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Codename of your application. Ex.: indev1
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Version text of your application. Ex.: 1.0.0.0
        /// </summary>
        public string VersionText { get; set; }

        /// <summary>
        /// Version number of your application. Ex.: 1
        /// </summary>
        public int Version { get; set; }
    }
}