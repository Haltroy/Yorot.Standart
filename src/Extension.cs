﻿using HTAlt;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Yorot
{
    /// <summary>
    /// Yorot Extension Manager
    /// </summary>
    public class ExtensionManager : YorotManager
    {
        public ExtensionManager(YorotMain main) : base(main.ExtConfig, main)
        { }

        /// <summary>
        /// A list of loaded Yorot extensions.
        /// </summary>
        public List<YorotExtension> Extensions { get; set; } = new List<YorotExtension>();

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
                    if (!node.IsComment())
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
                ManifestFile = extman.Main.ExtFolder + codeName + "\\ext.yem";
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
                                Size = new System.Drawing.Size(int.Parse(w), int.Parse(h));
                                break;

                            case "popup":
                                Popup = node.InnerXml.XmlToString();
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
                                            if (!subnode.IsComment())
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
                                if (!node.IsComment())
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
        public bool isPinned { get; set; }

        /// <summary>
        /// Location fo the manifest file for this extension on drive.
        /// </summary>
        public string ManifestFile { get; set; }

        /// <summary>
        /// Code Name of the extension.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Display name of the extension.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// HTUPDATE URL of this extension.
        /// </summary>
        public string HTU_Url { get; set; }

        /// <summary>
        /// Author of the extension.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Version of the extension.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Display Icon of the extension.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Sİze of the Pop-up menu used in this extension.
        /// </summary>
        public System.Drawing.Size Size { get; set; }

        /// <summary>
        /// Document to load as pop-up menu.
        /// </summary>
        public string Popup { get; set; }

        /// <summary>
        /// Script to run on Yorot startup.
        /// </summary>
        public string Startup { get; set; }

        /// <summary>
        /// Script to run on background for allowed pages.
        /// </summary>
        public string Background { get; set; }

        /// <summary>
        /// List of locations to files that used in this extension.
        /// </summary>
        public List<string> Files { get; set; } = new List<string>();

        /// <summary>
        /// Settings for this extension.
        /// </summary>
        public YorotExtensionSettings Settings { get; set; } = new YorotExtensionSettings();

        /// <summary>
        /// List of pages that user allowed this extension to run.
        /// </summary>
        public List<string> PageList { get; set; } = new List<string>();

        /// <summary>
        /// Right-click options for this extension.
        /// </summary>
        public List<YorotExtensionRCOption> RCOptions { get; set; } = new List<YorotExtensionRCOption>();

        /// <summary>
        /// Determines if this extension is enabled or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Returns extension size in bytes.
        /// </summary>+
        public long ExtSize => (Manager.Main.AppsFolder + CodeName).GetDirectorySize();

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