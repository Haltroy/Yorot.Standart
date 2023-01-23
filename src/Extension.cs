using HTAlt;
using System;
using System.Collections.Generic;
using System.Xml;
using Yorot.AppForms;

namespace Yorot
{
    /// <summary>
    /// Yorot Extension Manager
    /// </summary>
    public class ExtensionManager : YorotManager
    {
        private List<YorotExtension> extensions = new List<YorotExtension>();

        public ExtensionManager(YorotMain main) : base(main.ExtConfig, main)
        { }

        /// <summary>
        /// A list of loaded Yorot extensions.
        /// </summary>
        public List<YorotExtension> Extensions
        { get => extensions; set { Main.OnExtListChanged(); extensions = value; } }

        public override void ExtractXml(XmlNode rootNode)
        {
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                if (node.Name.ToLowerEnglish() == "extension")
                {
                    if (node.Attributes["CodeName"] != null)
                    {
                        try
                        {
                            YorotExtension ext = new YorotExtension(node.Attributes["CodeName"].Value.XmlToString(), this);
                            Extensions.Add(ext);
                        }
                        catch (Exception e)
                        {
                            Output.WriteLine("[ExtMan] Threw away \"" + node.OuterXml + "\", exception caught: " + e.ToString(), LogLevel.Warning);
                        }
                    }
                    else
                    {
                        Output.WriteLine("[ExtMan] Threw away \"" + node.OuterXml + "\", configuration does not includes \"CodeName\" attribute.", LogLevel.Warning);
                    }
                }
                else
                {
                    if (!node.NodeIsComment())
                    {
                        Output.WriteLine("[ExtMan] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning);
                    }
                }
            }
        }

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
                "<root>" + Environment.NewLine +
                "<!-- Yorot Extensions Config File" + Environment.NewLine + Environment.NewLine +
                "This file is used to save browser extensions." + Environment.NewLine +
                "Editing this file might cause problems with Yorot and/or extensions." + Environment.NewLine +
                "-->" + Environment.NewLine;
            for (int i = 0; i < Extensions.Count; i++)
            {
                YorotExtension ext = Extensions[i];
                x += "<Extension CodeName=\"" + ext.CodeName.ToXML() + "\" />" + Environment.NewLine;
            }
            return (x + "</root>").BeautifyXML();
        }

        /// <summary>
        /// Checks if extension exists.
        /// </summary>
        /// <param name="value">Code name of the extension.</param>
        /// <returns><see cref="bool"/></returns>
        public bool ExtExists(string value)
        {
            return Extensions.FindAll(i => i.CodeName == value).Count > 0;
        }

        /// <summary>
        /// Enables extension.
        /// </summary>
        /// <param name="value"></param>
        public void Enable(string value)
        {
            List<YorotExtension> l = Extensions.FindAll(i => i.CodeName == value);
            if (l.Count > 0)
            {
                l[0].Enabled = true;
            }
            else
            {
                throw new ArgumentException("Cannot find extension with code name \"" + value + "\".");
            }
        }

        /// <summary>
        /// Finds extension by code name.
        /// </summary>
        /// <param name="codeName">Code name of the extension.</param>
        /// <returns><see cref="YorotExtension"/></returns>
        public YorotExtension GetExtByCN(string codeName)
        {
            List<YorotExtension> l = Extensions.FindAll(i => i.CodeName == codeName);
            if (l.Count > 0)
            {
                return l[0];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Yorot Extension.
    /// </summary>
    public class YorotExtension
    {
        private bool isPinned1;
        private string manifestFile;
        private string codeName;
        private string name;
        private string hTU_Url;
        private string author;
        private int version;
        private string ıcon;
        private YorotRectangle size;
        private string popup;
        private YorotExtensionLaunchMode popupLaunchMode;
        private string background;
        private List<string> files = new List<string>();
        private List<string> pageList = new List<string>();
        private YorotExtensionSettings settings = new YorotExtensionSettings();
        private List<YorotExtensionRCOption> rCOptions = new List<YorotExtensionRCOption>();
        private bool enabled;

        /// <summary>
        /// Determines if this extension comes with this Yorot flavor.
        /// </summary>
        public bool isSystemExt { get; set; } = false;

        /// <summary>
        /// Creates a new yorot Extension.
        /// </summary>
        /// <param name="manifestFile">Location of the manifest file for this extension on drive.</param>
        public YorotExtension(string codeName, ExtensionManager extman)
        {
            if (extman == null)
            {
                throw new ArgumentNullException("extman");
            }
            Permissions = new YorotExtPermissions(this);
            Manager = extman;
            if (!string.IsNullOrWhiteSpace(codeName))
            {
                CodeName = codeName;
                ManifestFile = extman.Main.ExtFolder + codeName + System.IO.Path.DirectorySeparatorChar + "ext.yem";
                if (System.IO.File.Exists(ManifestFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(HTAlt.Tools.ReadFile(ManifestFile, System.Text.Encoding.Unicode));
                    List<string> appliedConfig = new List<string>();
                    XmlNode rootNode = HTAlt.Tools.FindRoot(doc.DocumentElement);
                    for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                    {
                        XmlNode node = rootNode.ChildNodes[i];
                        string nodeName = node.Name.ToLowerEnglish();
                        if (appliedConfig.Contains(nodeName))
                        {
                            Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + node.OuterXml + "\", configuration already applied.");
                            break;
                        }
                        appliedConfig.Add(nodeName);
                        switch (nodeName)
                        {
                            case "name":
                                Name = node.InnerXml.XmlToString();
                                break;

                            case "author":
                                Author = node.InnerXml.XmlToString();
                                break;

                            case "version":
                                Version = int.Parse(node.InnerXml.XmlToString());
                                break;

                            case "icon":
                                Icon = node.InnerXml.XmlToString();
                                break;

                            case "size":
                                string innertext = node.InnerXml.XmlToString();
                                string w = innertext.Substring(0, innertext.IndexOf(';'));
                                string h = innertext.Substring(innertext.IndexOf(';'), innertext.Length - innertext.IndexOf(';'));
                                Size = new YorotRectangle(0, 0, int.Parse(w), int.Parse(h));
                                break;

                            case "popup":
                                Popup = node.InnerXml.XmlToString();
                                break;

                            case "launch":
                                switch (node.InnerXml.XmlToString().ToLowerEnglish())
                                {
                                    case "raw":
                                        PopupLaunchMode = YorotExtensionLaunchMode.Raw;
                                        break;

                                    case "file":
                                        PopupLaunchMode = YorotExtensionLaunchMode.FromFile;
                                        break;

                                    case "raw-tab":
                                        PopupLaunchMode = YorotExtensionLaunchMode.TabRaw;
                                        break;

                                    case "file-tab":
                                        PopupLaunchMode = YorotExtensionLaunchMode.TabFile;
                                        break;

                                    case "raw-popup":
                                        PopupLaunchMode = YorotExtensionLaunchMode.PopupRaw;
                                        break;

                                    case "file-popup":
                                        PopupLaunchMode = YorotExtensionLaunchMode.PopupFile;
                                        break;

                                    case "appforms":
                                        PopupLaunchMode = YorotExtensionLaunchMode.AppForms;
                                        break;
                                }
                                break;

                            case "startup":
                                Startup = node.InnerXml.XmlToString();
                                break;

                            case "background":
                                Background = node.InnerXml.XmlToString();
                                break;

                            case "files":
                                for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                                {
                                    XmlNode subnode = node.ChildNodes[ı];
                                    if (subnode.Name.ToLowerEnglish() == "file")
                                    {
                                        Files.Add(subnode.InnerXml.XmlToString());
                                    }
                                    else
                                    {
                                        Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + subnode.OuterXml + "\", unsupported.");
                                    }
                                }
                                break;

                            case "settings":
                                for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                                {
                                    XmlNode subnode = node.ChildNodes[ı];
                                    switch (subnode.Name)
                                    {
                                        case "autoLoad":
                                            if (appliedConfig.FindAll(it => it == subnode.Name).Count > 0)
                                            {
                                                Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + subnode.OuterXml + "\", configuration already applied.");
                                                break;
                                            }
                                            appliedConfig.Add(subnode.Name);
                                            Settings.autoLoad = subnode.InnerXml.XmlToString() == "true";
                                            break;

                                        case "showPopupMenu":
                                            if (appliedConfig.FindAll(it => it == subnode.Name).Count > 0)
                                            {
                                                Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + subnode.OuterXml + "\", configuration already applied.");
                                                break;
                                            }
                                            appliedConfig.Add(subnode.Name);
                                            Settings.showPopupMenu = subnode.InnerXml.XmlToString() == "true";
                                            break;

                                        case "hasProxy":
                                            if (appliedConfig.FindAll(it => it == subnode.Name).Count > 0)
                                            {
                                                Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + subnode.OuterXml + "\", configuration already applied.");
                                                break;
                                            }
                                            appliedConfig.Add(subnode.Name);
                                            Settings.hasProxy = subnode.InnerXml.XmlToString() == "true";
                                            break;

                                        default:
                                            if (!subnode.NodeIsComment())
                                            {
                                                Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + subnode.OuterXml + "\", unsupported.");
                                            }
                                            break;
                                    }
                                }
                                break;

                            case "pagelist":
                                for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                                {
                                    XmlNode subnode = node.ChildNodes[ı];
                                    if (subnode.Name.ToLowerEnglish() == "page")
                                    {
                                        PageList.Add(subnode.InnerXml.XmlToString());
                                    }
                                    else
                                    {
                                        Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + subnode.OuterXml + "\", unsupported.");
                                    }
                                }
                                break;

                            case "rcoptions":
                            case "rightclickoptions":
                                for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                                {
                                    XmlNode subnode = node.ChildNodes[ı];
                                    if (subnode.Name.ToLowerEnglish() == "option")
                                    {
                                        if (subnode.Attributes["Script"] != null && subnode.Attributes["Type"] != null)
                                        {
                                            YorotExtensionRCOption rcoption = new YorotExtensionRCOption()
                                            {
                                                Script = subnode.Attributes["Script"].Value.XmlToString(),
                                                Text = subnode.InnerXml.XmlToString(),
                                            };
                                            switch (subnode.Attributes["Type"].Value.ToLowerEnglish())
                                            {
                                                case "none":
                                                    rcoption.Option = RightClickOptionStyle.None;
                                                    break;

                                                case "link":
                                                    rcoption.Option = RightClickOptionStyle.Link;
                                                    break;

                                                case "image":
                                                    rcoption.Option = RightClickOptionStyle.Image;
                                                    break;

                                                case "text":
                                                    rcoption.Option = RightClickOptionStyle.Text;
                                                    break;

                                                case "edit":
                                                    rcoption.Option = RightClickOptionStyle.Edit;
                                                    break;

                                                case "always":
                                                    rcoption.Option = RightClickOptionStyle.Always;
                                                    break;

                                                default:
                                                    Output.WriteLine("[Extensions] Threw away \"" + subnode.OuterXml + "\", unsupported Right-Click Option.");
                                                    continue;
                                            }
                                            switch (subnode.Attributes["Launch"].Value.ToLowerEnglish())
                                            {
                                                case "raw":
                                                    rcoption.LaunchMode = YorotExtensionLaunchMode.Raw;
                                                    break;

                                                case "file":
                                                    rcoption.LaunchMode = YorotExtensionLaunchMode.FromFile;
                                                    break;

                                                case "raw-tab":
                                                    rcoption.LaunchMode = YorotExtensionLaunchMode.TabRaw;
                                                    break;

                                                case "file-tab":
                                                    rcoption.LaunchMode = YorotExtensionLaunchMode.TabFile;
                                                    break;

                                                case "raw-popup":
                                                    rcoption.LaunchMode = YorotExtensionLaunchMode.PopupRaw;
                                                    break;

                                                case "file-popup":
                                                    rcoption.LaunchMode = YorotExtensionLaunchMode.PopupFile;
                                                    break;

                                                case "appforms":
                                                    rcoption.LaunchMode = YorotExtensionLaunchMode.AppForms;
                                                    break;
                                            }
                                            if (subnode.Attributes["Icon"] != null)
                                            {
                                                rcoption.Icon = subnode.Attributes["Icon"].Value.XmlToString().GetPath(Manager.Main);
                                            }
                                            RCOptions.Add(rcoption);
                                        }
                                    }
                                    else
                                    {
                                        Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + subnode.OuterXml + "\", unsupported.");
                                    }
                                }
                                break;

                            default:
                                if (!node.NodeIsComment())
                                {
                                    Output.WriteLine("[Extension:\"" + CodeName + "\"] Threw away \"" + node.OuterXml + "\", unsupported.");
                                }
                                break;
                        }
                    }
                }
                else
                {
                    throw new Exception("Manifest file does not exists.");
                }
            }
            else
            {
                throw new Exception("Extension code name was empty.");
            }
        }

        /// <summary>
        /// Associated extension manager for this extension.
        /// </summary>
        public ExtensionManager Manager { get; set; }

        /// <summary>
        /// Determines if this extension is pinned to navigation bar.
        /// </summary>
        public bool isPinned
        { get => isPinned1; set { Manager.Main.OnExtListChanged(); isPinned1 = value; } }

        /// <summary>
        /// Location fo the manifest file for this extension on drive.
        /// </summary>
        public string ManifestFile
        { get => manifestFile; set { Manager.Main.OnExtListChanged(); manifestFile = value; } }

        /// <summary>
        /// Code Name of the extension.
        /// </summary>
        public string CodeName
        { get => codeName; set { Manager.Main.OnExtListChanged(); codeName = value; } }

        /// <summary>
        /// Display name of the extension.
        /// </summary>
        public string Name
        { get => name; set { Manager.Main.OnExtListChanged(); name = value; } }

        /// <summary>
        /// HTUPDATE URL of this extension.
        /// </summary>
        public string HTU_Url
        { get => hTU_Url; set { Manager.Main.OnExtListChanged(); hTU_Url = value; } }

        /// <summary>
        /// Author of the extension.
        /// </summary>
        public string Author
        { get => author; set { Manager.Main.OnExtListChanged(); author = value; } }

        /// <summary>
        /// Version of the extension.
        /// </summary>
        public int Version
        { get => version; set { Manager.Main.OnExtListChanged(); version = value; } }

        /// <summary>
        /// Display Icon of the extension.
        /// </summary>
        public string Icon
        { get => ıcon; set { Manager.Main.OnExtListChanged(); ıcon = value; } }

        /// <summary>
        /// Size of the Pop-up menu used in this extension.
        /// </summary>
        public YorotRectangle Size
        { get => size; set { Manager.Main.OnExtListChanged(); size = value; } }

        /// <summary>
        /// Document to load as pop-up menu.
        /// </summary>
        public string Popup
        { get => popup; set { Manager.Main.OnExtListChanged(); popup = value; } }

        /// <summary>
        /// Determines the launch mode for <see cref="Popup"/>.
        /// </summary>
        public YorotExtensionLaunchMode PopupLaunchMode
        { get => popupLaunchMode; set { Manager.Main.OnExtListChanged(); popupLaunchMode = value; } }

        /// <summary>
        /// Script to run on Yorot startup.
        /// </summary>
        public string Startup { get; set; }

        /// <summary>
        /// Script to run on background for allowed pages.
        /// </summary>
        public string Background
        { get => background; set { Manager.Main.OnExtListChanged(); background = value; } }

        /// <summary>
        /// List of locations to files that used in this extension.
        /// </summary>
        public List<string> Files
        { get => files; set { Manager.Main.OnExtListChanged(); files = value; } }

        /// <summary>
        /// Settings for this extension.
        /// </summary>
        public YorotExtensionSettings Settings
        { get => settings; set { Manager.Main.OnExtListChanged(); settings = value; } }

        /// <summary>
        /// List of pages that user allowed this extension to run.
        /// </summary>
        public List<string> PageList
        { get => pageList; set { Manager.Main.OnExtListChanged(); pageList = value; } }

        /// <summary>
        /// Right-click options for this extension.
        /// </summary>
        public List<YorotExtensionRCOption> RCOptions
        { get => rCOptions; set { Manager.Main.OnExtListChanged(); rCOptions = value; } }

        /// <summary>
        /// Determines if this extension is enabled or not.
        /// </summary>
        public bool Enabled
        { get => enabled; set { Manager.Main.OnExtListChanged(); enabled = value; } }

        /// <summary>
        /// Returns extension size in bytes.
        /// </summary>+
        public long ExtSize => (Manager.Main.AppsFolder + CodeName).GetDirSize();

        /// <summary>
        /// Gets size of extension.
        /// </summary>
        /// <param name="bytes">Translation of word "bytes".</param>
        /// <returns><see cref="string"/></returns>
        public string GetSizeOnDisk(string bytes)
        {
            long size = ExtSize;
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
        /// PErmissions of this extension.
        /// </summary>
        public YorotExtPermissions Permissions { get; set; }
    }

    /// <summary>
    /// Modes that can be used to launch Extension Pop-up menu or Right-click options.
    /// </summary>
    public enum YorotExtensionLaunchMode
    {
        /// <summary>
        /// Runs raw JS code in current tab or launches raw HTML page in extension menu.
        /// </summary>
        Raw,

        /// <summary>
        /// Runs code from file in tab or launches the file in extension menu.
        /// </summary>
        FromFile,

        /// <summary>
        /// Runs raw JS code in a new tab or launches raw HTML page in a new tab.
        /// </summary>
        TabRaw,

        /// <summary>
        /// Runs JS code from file in a new tab or launches HTML page from file in a new tab.
        /// </summary>
        TabFile,

        /// <summary>
        /// Runs raw JS code in a pop-up window or launches raw HTML page in a pop-up window.
        /// </summary>
        PopupRaw,

        /// <summary>
        /// Runs JS code from file in a pop-up window or launches HTML page from file in a pop-up window.
        /// </summary>
        PopupFile,

        /// <summary>
        /// Launches an <see cref="AppForms"/>.
        /// </summary>
        AppForms
    }

    /// <summary>
    /// Right-click options.
    /// </summary>
    public class YorotExtensionRCOption
    {
        /// <summary>
        /// Text to display in option
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Location of the script to run when user clicks on drive.
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// Icon to display near text.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Determines when to display this item.
        /// </summary>
        public RightClickOptionStyle Option { get; set; }

        /// <summary>
        /// Determines the launch mode of this option.
        /// </summary>
        public YorotExtensionLaunchMode LaunchMode { get; set; }
    }

    /// <summary>
    /// Right-Click options styles for extensions.
    /// </summary>
    public enum RightClickOptionStyle
    {
        /// <summary>
        /// Shows option when nothing is selected
        /// </summary>
        None,

        /// <summary>
        /// Shows option when user hovering or selected a link.
        /// </summary>
        Link,

        /// <summary>
        /// Shows option if user hovering or selected an image.
        /// </summary>
        Image,

        /// <summary>
        /// Shows option if user hovered or selected a text.
        /// </summary>
        Text,

        /// <summary>
        /// Shows option if user hovered or selected an edit text box.
        /// </summary>
        Edit,

        /// <summary>
        /// Always shows option on anything.
        /// </summary>
        Always
    }

    /// <summary>
    /// Settings for Yorot Extensions.
    /// </summary>
    public class YorotExtensionSettings
    {
        /// <summary>
        /// Load extension on Yorot start.
        /// </summary>
        public bool autoLoad { get; set; } = false;

        /// <summary>
        /// If set to <see cref="true"/>, shows pop-up menu when user clicks on extension icon.
        /// </summary>
        public bool showPopupMenu { get; set; } = false;

        /// <summary>
        /// <see cref="true"/> if Extension has Proxy manipulations.
        /// </summary>
        public bool hasProxy { get; set; } = false;
    }

    /// <summary>
    /// Permissions of a generic <see cref="YorotApp"/>.
    /// </summary>
    public class YorotExtPermissions
    {
        /// <summary>
        /// Creates a new <see cref="YorotExtPermissions"/> with default values.
        /// </summary>
        /// <param name="app"><see cref="YorotExtension"/></param>
        public YorotExtPermissions(YorotExtension ext) : this(ext, YorotPermissionMode.None, YorotPermissionMode.None, YorotPermissionMode.None, YorotPermissionMode.None) { }

        /// <summary>
        /// Creates a new <see cref="YorotExtPermissions"/> with custom values.
        /// </summary>
        /// <param name="ext"><see cref="YorotExtension"/></param>
        /// <param name="runInc">Determines if this app can be launched in Incognito mode.</param>
        /// <param name="runStart">Determines if this app will be launched on start.</param>
        /// <param name="allowNotif">Determines if this app can send notifications.</param>
        /// <param name="menuOptions">Determines if this extension can show menu options.</param>
        public YorotExtPermissions(YorotExtension ext, YorotPermissionMode runInc, YorotPermissionMode runStart, YorotPermissionMode allowNotif, YorotPermissionMode menuOptions)
        {
            Extension = ext;
            this.runInc = new YorotPermission("runInc", ext, ext.Manager.Main, runInc);
            this.runStart = new YorotPermission("runStart", ext, ext.Manager.Main, runStart);
            this.allowNotif = new YorotPermission("allowNotif", ext, ext.Manager.Main, allowNotif);
            this.menuOptions = new YorotPermission("menuOptions", ext, ext.Manager.Main, menuOptions);
        }

        /// <summary>
        /// The extension of these permissions.
        /// </summary>
        public YorotExtension Extension { get; set; }

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
        /// Determines if this extension can show menu options.
        /// </summary>
        public YorotPermission menuOptions { get; set; }
    }
}