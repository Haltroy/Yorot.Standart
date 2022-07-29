﻿using HTAlt;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Yorot
{
    public class YorotWEManager : YorotManager
    {
        /// <summary>
        /// Creates a new Web Engine Manager.
        /// </summary>
        /// <param name="main"><see cref="YorotMain"/></param>
        public YorotWEManager(YorotMain main) : base(main.WEConfig, main) { }

        /// <summary>
        /// A list of loaded web engines.
        /// </summary>
        public List<YorotWebEngine> Engines { get; set; } = new List<YorotWebEngine>();

        public override void ExtractXml(XmlNode rootNode)
        {
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                if (node.Name.ToLowerEnglish() == "we")
                {
                    if (node.Attributes["CodeName"] != null)
                    {
                        try
                        {
                            YorotWebEngine engine = new YorotWebEngine(Main.WEFolder + node.Attributes["CodeName"].Value + "\\engine.ycf".XmlToString(), this);
                            if (node.Attributes["AllowInc"] != null)
                            {
                                engine.AllowInIncognito = node.Attributes["AllowInc"].Value.XmlToString().ToLowerEnglish() == "true";
                            }
                            Engines.Add(engine);
                        }
                        catch (Exception e)
                        {
                            Output.WriteLine("[WEMan] Threw away \"" + node.OuterXml + "\", exception caught: " + e.ToString(), LogLevel.Warning);
                        }
                    }
                    else
                    {
                        Output.WriteLine("[WEMan] Threw away \"" + node.OuterXml + "\", configuration does not includes \"CodeName\" attribute.", LogLevel.Warning);
                    }
                }
                else
                {
                    if (!node.IsComment())
                    {
                        Output.WriteLine("[WEMan] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning);
                    }
                }
            }
        }

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
                       "<root>" + Environment.NewLine +
                       "<!-- Yorot Web Engines Config File" + Environment.NewLine + Environment.NewLine +
                       "This file is used to save web engines." + Environment.NewLine +
                       "Editing this file might cause problems with webpages and web engines." + Environment.NewLine +
                       "-->" + Environment.NewLine;
            for (int i = 0; i < Engines.Count; i++)
            {
                YorotWebEngine we = Engines[i];
                x += "<WE CodeName=\"" + we.CodeName.ToXML() + "\" " + (we.AllowInIncognito ? "AllowInc=\"true\" " : "") + "/>" + Environment.NewLine;
            }
            return (x + "</root>").BeautifyXML();
        }

        /// <summary>
        /// Enables a web engine.
        /// </summary>
        /// <param name="value">CodeName of the engine.</param>
        public void Enable(string value)
        {
            List<YorotWebEngine> l = Engines.FindAll(i => i.CodeName == value);
            if (l.Count > 0)
            {
                l[0].isEnabled = true;
            }
            else
            {
                throw new ArgumentException("Cannot find a web engine with code name \"" + value + "\".");
            }
        }

        /// <summary>
        /// Checks if an engine exists or not.
        /// </summary>
        /// <param name="value">CodeName of the engine.</param>
        public bool WEExists(string value)
        {
            return Engines.FindAll(i => i.CodeName == value).Count > 0;
        }

        /// <summary>
        /// Gets web engine by code name.
        /// </summary>
        /// <param name="codeName">Code name of the engine.</param>
        /// <returns><see cref="YorotWebEngine"/></returns>
        public YorotWebEngine GetWEByCN(string codeName)
        {
            List<YorotWebEngine> l = Engines.FindAll(i => i.CodeName == codeName);
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
    /// Yorot Web Engine
    /// </summary>
    public class YorotWebEngine
    {
        /// <summary>
        /// Creates a new <see cref="YorotWebEngine"/>.
        /// </summary>
        /// <param name="configFile">Location of configuration file for this engine.</param>
        public YorotWebEngine(string configFile, YorotWEManager manager)
        {
            if (manager is null) { throw new ArgumentNullException("\"manager\" cannot be null."); }
            Manager = manager;
            if (!string.IsNullOrWhiteSpace(configFile))
            {
                if (System.IO.File.Exists(configFile))
                {
                    ConfigFile = configFile;
                    EngineLoc = System.IO.Path.Combine(new System.IO.FileInfo(configFile).Directory.FullName, System.IO.Path.GetFileNameWithoutExtension(configFile) + "\\");
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(HTAlt.Tools.ReadFile(configFile, System.Text.Encoding.Unicode));
                    XmlNode rootNode = HTAlt.Tools.FindRoot(doc);
                    List<string> appliedC = new List<string>();
                    for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                    {
                        XmlNode node = rootNode.ChildNodes[i];
                        string name = node.Name.ToLowerEnglish();
                        if (appliedC.Contains(name))
                        {
                            HTAlt.Output.WriteLine("[YorotWebEngine] Threw away \"" + node.OuterXml + "\", configuration already applied.");
                            break;
                        }
                        appliedC.Add(name);
                        switch (name)
                        {
                            case "name":
                                Name = node.InnerXml.XmlToString();
                                break;

                            case "version":
                                Version = int.Parse(node.InnerXml.XmlToString());
                                break;

                            case "author":
                                Author = node.InnerXml.XmlToString();
                                break;

                            case "codename":
                                CodeName = node.InnerXml.XmlToString();
                                break;

                            case "desc":
                                Desc = node.InnerXml.XmlToString();
                                break;

                            case "iconloc":
                                IconLoc = node.InnerXml.XmlToString().GetPath(Manager.Main);
                                break;

                            default:
                                if (!node.IsComment())
                                {
                                    HTAlt.Output.WriteLine("[YorotWebEngine] Threw away \"" + node.OuterXml + "\", unsupported.");
                                }
                                break;
                        }
                    }
                }
                else
                {
                    throw new System.IO.FileNotFoundException("The config file \"" + configFile + "\" does not exists.");
                }
            }
            else
            {
                throw new ArgumentException("\"configFile\" cannot be empty.");
            }
        }

        /// <summary>
        /// Location of the engine folder in drive.
        /// </summary>
        public string EngineLoc { get; set; }

        /// <summary>
        /// Manager of this web engine.
        /// </summary>
        public YorotWEManager Manager { get; set; }

        /// <summary>
        /// HTUPDATE address of this engine.
        /// </summary>
        public string HTU_Url { get; set; }

        /// <summary>
        /// Determines if this engine is enabled or not.
        /// </summary>
        public bool isEnabled { get; set; }

        /// <summary>
        /// Current version of this engine.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Creator of this web engine.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Name of this engine.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Codename of the engine.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Description of this engine.
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Location of this engine's logo on local drive.
        /// </summary>
        public string IconLoc { get; set; }

        /// <summary>
        /// Gets the engine's logo.
        /// </summary>
        public System.Drawing.Image Icon => HTAlt.Tools.ReadFile(IconLoc, System.Drawing.Imaging.ImageFormat.Png);

        /// <summary>
        /// Configuration file of this engine.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Returns web engine size in bytes.
        /// </summary>+
        public long WESize => (Manager.Main.WEFolder + CodeName).GetDirectorySize();

        /// <summary>
        /// Determines if this web engine can be initialized in Incognito mode.
        /// </summary>
        public bool AllowInIncognito { get; set; }

        /// <summary>
        /// Gets size of this web engine.
        /// </summary>
        /// <param name="bytes">Translation of word "bytes".</param>
        /// <returns><see cref="string"/></returns>
        public string GetWESizeInfo(string bytes)
        {
            long size = WESize;
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
    }
}