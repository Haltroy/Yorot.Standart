﻿using HTAlt;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Yorot
{
    public static class YorotDefaultLanguages
    {
        public static string[] DefaultLangList => new string[]
        {
            "com.haltroy.english" ,
            "com.haltroy.english-us",
            "com.haltroy.english-gb",
            "com.haltroy.turkish",
            "com.haltroy.japanese",
            "com.haltroy.chinese-s",
            "com.haltroy.chinese-t",
            "com.haltroy.french",
            "com.haltroy.german",
            "com.haltroy.itallian",
            "com.haltroy.russian",
            "com.haltroy.ukranian",
            "com.haltroy.arabic",
            "com.haltroy.persian",
            "com.haltroy.spanish",
            "com.haltroy.portuguese",
            "com.haltroy.greek",
            "com.haltroy.latin",
            "com.haltroy.swedish",
            "com.haltroy.norwegian",
            "com.haltroy.danish",
            "com.haltroy.punjabi",
            "com.haltroy.romanian",
            "com.haltroy.serbian",
            "com.haltroy.hungarian",
            "com.haltroy.dutch",
            "com.haltroy.georgian",
            "com.haltroy.hebrew"
        };
    }

    /// <summary>
    /// Yorot Language manager.
    /// </summary>
    public class YorotLangManager : YorotManager
    {
        /// <summary>
        /// Creates a new Language manager.
        /// </summary>
        public YorotLangManager(YorotMain main) : base(main.LangConfig, main) { string[] d = YorotDefaultLanguages.DefaultLangList; for (int i = 0; i < d.Length; i++) { Languages.Add(new YorotLanguage(Main.LangFolder + d[i] + System.IO.Path.DirectorySeparatorChar + d[i] + ".ylf", this) { isDefaultLang = true, Enabled = true }); } }

        /// <summary>
        /// A list of loaded languages.
        /// </summary>
        public List<YorotLanguage> Languages { get; set; } = new List<YorotLanguage>();

        /// <summary>
        /// Gets language from code name.
        /// </summary>
        /// <param name="codeName">Code name of the language.</param>
        /// <returns><see cref="YorotLanguage"/></returns>
        public YorotLanguage GetLangByCN(string codeName)
        {
            List<YorotLanguage> l = Languages.FindAll(i => i.CodeName.ToLowerEnglish() == codeName.ToLowerEnglish());
            if (l.Count > 0)
            {
                return l[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a language is loaded.
        /// </summary>
        /// <param name="value">CodeName of the language.</param>
        /// <returns><see cref="bool"/></returns>
        public bool LangExists(string value)
        {
            return Languages.FindAll(i => i.CodeName == value).Count > 0;
        }

        /// <summary>
        /// Enables a language.
        /// </summary>
        /// <param name="value">CodeName of the language.</param>
        public void Enable(string value)
        {
            List<YorotLanguage> l = Languages.FindAll(i => i.CodeName == value);
            if (l.Count > 0)
            {
                l[0].Enabled = true;
            }
            else
            {
                throw new ArgumentException("Cannot find specific language.");
            }
        }

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
                "<root>" + Environment.NewLine +
                "<!-- Yorot Languages Config File" + Environment.NewLine + Environment.NewLine +
                "This file is used to configure languages." + Environment.NewLine +
                "Editing this file might cause problems with languagess." + Environment.NewLine +
                "-->" + Environment.NewLine +
                "<Langs>" + Environment.NewLine;
            for (int i = 0; i < Languages.Count; i++)
            {
                if (!Languages[i].isDefaultLang)
                {
                    x += "<Lang CodeName=\"" + Languages[i].CodeName + "\" />" + Environment.NewLine;
                }
            }
            return (x + "</Langs>" + Environment.NewLine + "</root>").BeautifyXML();
        }

        public override void ExtractXml(XmlNode rootNode)
        {
            List<string> appliedSettings = new List<string>();
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                switch (node.Name.ToLowerEnglish())
                {
                    case "langs":
                        if (appliedSettings.FindAll(it => it == node.Name).Count > 0)
                        {
                            Output.WriteLine("[LangMan] Threw away \"" + node.OuterXml + "\". Configurtion already applied.", LogLevel.Warning);
                            break;
                        }
                        appliedSettings.Add(node.Name);
                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                        {
                            XmlNode subnode = node.ChildNodes[ı];
                            if (subnode.Name.ToLowerEnglish() == "lang")
                            {
                                if (subnode.Attributes["CodeName"] != null)
                                {
                                    string s = subnode.Attributes["CodeName"].Value.XmlToString();
                                    YorotLanguage lang = new YorotLanguage(s.StartsWith("com.haltroy") ? s : Main.LangFolder + s + ".ylf", this);
                                    Languages.Add(lang);
                                }
                                else
                                {
                                    Output.WriteLine("[LangMan] Threw away \"" + subnode.OuterXml + "\". missing required atrributes.", LogLevel.Warning);
                                }
                            }
                            else
                            {
                                if (!subnode.IsComment())
                                {
                                    Output.WriteLine("[LangMan] Threw away \"" + subnode.OuterXml + "\". unsupported.", LogLevel.Warning);
                                }
                            }
                        }
                        break;

                    default:
                        if (!node.IsComment())
                        {
                            Output.WriteLine("[LangMan] Threw away \"" + node.OuterXml + "\". Invalid configurtion.", LogLevel.Warning);
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Yorot Language Class, used for translation user interface.
    /// </summary>
    public class YorotLanguage
    {
        /// <summary>
        /// Creates a new language.
        /// </summary>
        /// <param name="configFile">Location of the language file on drive.</param>
        /// <param name="manager">Manager</param>
        public YorotLanguage(string configFile, YorotLangManager manager)
        {
            if (manager is null) { throw new ArgumentNullException("manager"); }
            Manager = manager;
            if (!string.IsNullOrWhiteSpace(configFile))
            {
                if (System.IO.File.Exists(configFile))
                {
                    AddDefaultVars();
                    LangFile = configFile;
                    LoadedRoot = false;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(HTAlt.Tools.ReadFile(configFile, Encoding.Unicode));
                    XmlNode rootNode = HTAlt.Tools.FindRoot(doc);
                    RecursiveAdd(rootNode, "");
                }
                else
                {
                    throw new ArgumentException("File \"" + configFile + "\" does not exists.");
                }
            }
            else
            {
                throw new ArgumentNullException("configFile");
            }
        }

        /// <summary>
        /// Location of this language file in drive.
        /// </summary>
        public string LangFile { get; set; }

        /// <summary>
        /// Adds default variables to language.
        /// </summary>
        private void AddDefaultVars()
        {
            LangVars.Add(new YorotLangVar("NEWLINE", Environment.NewLine));
            LangVars.Add(new YorotLangVar("APPNAME", Manager.Main.Name));
            LangVars.Add(new YorotLangVar("APPCODENAME", Manager.Main.CodeName));
            LangVars.Add(new YorotLangVar("APPVER", Manager.Main.VersionText));
            LangVars.Add(new YorotLangVar("APPVERNO", "" + Manager.Main.Version));
            // TODO: (LONG TERM) Add more
        }

        /// <summary>
        /// Manager associated with this language.
        /// </summary>
        public YorotLangManager Manager { get; set; }

        /// <summary>
        /// Used for dynamically inputting <paramref name="langVar"/> to <seealso cref="YorotLangItem"/>.
        /// </summary>
        /// <param name="main"><see cref="YorotLangItem"/></param>
        /// <param name="langVar"><see cref="YorotLangVar"/></param>
        /// <returns><see cref="string"/></returns>
        private static string RuleifyString(string main, YorotLangVar langVar)
        {
            string ignored = "§IGNORED_" + HTAlt.Tools.GenerateRandomText(17) + "§";
            return main
                .Replace("![" + langVar.Name.ToUpper() + "]", ignored)
                .Replace("[" + langVar.Name.ToUpper() + "]", string.IsNullOrEmpty(langVar.Text) ? "" : langVar.Text)
                .Replace(ignored, "[" + langVar.Name.ToUpper() + "]");
        }

        /// <summary>
        /// Determines if this language is bundled with Yorot.
        /// </summary>
        public bool isDefaultLang { get; set; }

        /// <summary>
        /// HTUPDATE of this language.
        /// </summary>
        public string HTUPDATE { get; set; }

        /// <summary>
        /// Loaded Language Variables.
        /// </summary>
        public List<YorotLangVar> LangVars { get; set; } = new List<YorotLangVar>();

        /// <summary>
        /// Loaded Language Items.
        /// </summary>
        public List<YorotLangItem> LangItems { get; set; } = new List<YorotLangItem>();

        /// <summary>
        /// Name of this language.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Author information about this langauge.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Yorot version that this language is made for.
        /// </summary>
        public int CompatibleVer { get; set; }

        /// <summary>
        /// Version of this language file.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Finds Language Item from ID.
        /// </summary>
        /// <param name="ID">ID of translation.</param>
        /// <returns><see cref="string"/></returns>
        public string GetItemText(string ID)
        {
            YorotLangItem item = LangItems.Find(i => i.ID.Trim() == ID.Trim());
            if (item == null)
            {
                Output.WriteLine("[Language] Missing Item [ID=\"" + ID + "\" LangFile=\"" + LangFile + "\"]", LogLevel.Warning);
                return "[MI] " + ID;
            }
            else
            {
                string itemText = item.Text;
                for (int i = 0; i < LangVars.Count; i++)
                {
                    YorotLangVar globalVar = LangVars[i];
                    itemText = RuleifyString(itemText, globalVar);
                }
                return itemText;
            }
        }

        /// <summary>
        /// Determines if group #YOROT-ROOT is loaded.
        /// </summary>
        public bool LoadedRoot { get; set; }

        /// <summary>
        /// The code name of this language file.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Determines if this language is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Recursively adds items to system.
        /// </summary>
        /// <param name="rootNode">Root Node</param>
        private void RecursiveAdd(XmlNode rootNode, string groupID)
        {
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                switch (node.Name.ToLowerEnglish())
                {
                    case "var":
                        {
                            if (node.Attributes["ID"] != null && node.Attributes["Text"] != null)
                            {
                                if (LangVars.FindAll(it => it.Name == node.Attributes["ID"].Value && it.Text == node.Attributes["Text"].Value).Count > 0)
                                {
                                    Output.WriteLine("[LangManager] Threw away \"" + node.OuterXml + "\", Language Variable already exists.", LogLevel.Warning);
                                }
                                else
                                {
                                    LangVars.Add(new YorotLangVar((string.IsNullOrWhiteSpace(groupID) ? "" : groupID + ".") + node.Attributes["ID"].Value.XmlToString(), node.Attributes["Text"].Value.XmlToString()));
                                }
                            }
                            else
                            {
                                Output.WriteLine("[LangManager] Threw away \"" + node.OuterXml + "\", unsupported for Yorot Language Variable.", LogLevel.Warning);
                            }
                            break;
                        }
                    case "translation":
                        {
                            string id = node.Attributes["ID"] != null ? node.Attributes["ID"].Value.XmlToString() : HTAlt.Tools.GenerateRandomText(12);
                            string text = node.Attributes["Text"] != null ? node.Attributes["Text"].Value.XmlToString() : id;
                            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(text))
                            {
                                if (LangItems.FindAll(it => it.ID == id && it.Text == text).Count > 0)
                                {
                                    Output.WriteLine("[LangManager] Threw away \"" + node.OuterXml + "\", Language Item already exists.", LogLevel.Warning);
                                }
                                else
                                {
                                    LangItems.Add(new YorotLangItem() { ID = (string.IsNullOrWhiteSpace(groupID) ? "" : groupID + ".") + id, Text = text });
                                }
                            }
                            else
                            {
                                Output.WriteLine("[LangManager] Threw away \"" + node.OuterXml + "\", unsupported for Yorot Language Item.", LogLevel.Warning);
                            }
                            break;
                        }
                    case "group":
                        {
                            if (node.Attributes["Name"] != null)
                            {
                                if (node.Attributes["Name"].Value == "#YOROT-ROOT")
                                {
                                    if (!LoadedRoot)
                                    {
                                        LoadedRoot = true;
                                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                                        {
                                            XmlNode subnode = node.ChildNodes[ı];
                                            List<string> appliedSettings = new List<string>();
                                            if (appliedSettings.Contains(subnode.Name.ToLowerEnglish()))
                                            {
                                                Output.WriteLine("[LangManager] Threw away \"" + node.OuterXml + "\", #YOROT-ROOT configuration already loaded.", LogLevel.Warning);
                                                break;
                                            }
                                            appliedSettings.Add(subnode.Name);
                                            switch (subnode.Name.ToLowerEnglish())
                                            {
                                                case "name":
                                                    Name = subnode.InnerXml.XmlToString();
                                                    break;

                                                case "author":
                                                    Author = subnode.InnerXml.XmlToString();
                                                    break;

                                                case "codename":
                                                    CodeName = subnode.InnerXml.XmlToString();
                                                    break;

                                                case "compatibleversion":
                                                    CompatibleVer = int.Parse(subnode.InnerXml.XmlToString());
                                                    break;

                                                case "version":

                                                    Version = int.Parse(subnode.InnerXml.XmlToString());
                                                    break;

                                                default:
                                                    if (!subnode.IsComment())
                                                    {
                                                        Output.WriteLine("[LangManager] Threw away \"" + subnode.OuterXml + "\", unsupported format for #YOROT-ROOT.", LogLevel.Warning);
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Output.WriteLine("[LangManager] Threw away \"" + node.OuterXml + "\", #YOROT-ROOT already loaded.", LogLevel.Warning);
                                    }
                                }
                                else
                                {
                                    RecursiveAdd(node, (string.IsNullOrWhiteSpace(groupID) ? "" : groupID + ".") + node.Attributes["Name"].Value.XmlToString());
                                }
                            }
                            else
                            {
                                RecursiveAdd(node, string.IsNullOrWhiteSpace(groupID) ? "" : groupID);
                            }
                            break;
                        }
                    default:
                        if (!node.IsComment()) Output.WriteLine("[LangManager] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning);
                        break;
                }
            }
        }

        /// <summary>
        /// Prepares a string.
        /// </summary>
        /// <param name="newstr">String to prepare.</param>
        /// <returns><see cref="string"/></returns>
        public string Prepare(string newstr)
        {
            for (int i = 0; i < LangItems.Count; i++)
            {
                newstr = newstr.Replace("[" + LangItems[i].ID + "]", GetItemText(LangItems[i].ID));
            }
            return newstr;
        }
    }

    /// <summary>
    /// Item used in translation of Yorot.
    /// </summary>
    public class YorotLangItem
    {
        /// <summary>
        /// Name/CodeName of item.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Text to display.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Variable used in <see cref="YorotLangItem"/>.
    /// </summary>
    public class YorotLangVar
    {
        /// <summary>
        /// Creates a new <see cref="YorotLangVar"/>.
        /// </summary>
        /// <param name="name">Name of <see cref="YorotLangVar"/>.</param>
        /// <param name="defaultVal">Default value of <see cref="YorotLangVar"/>.</param>
        public YorotLangVar(string name, string defaultVal = "")
        {
            Text = defaultVal;
            Name = name;
        }

        /// <summary>
        /// nbame of <see cref="YorotLangVar"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Text or value of <see cref="YorotLangVar"/>.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Locales that are supported by Yorot.
    /// </summary>
    public enum YorotLocale
    {
        ///
        /// Amharic
        ///
        am,

        ///
        /// Arabic
        ///
        ar,

        ///
        /// Bengali
        ///
        bn,

        ///
        /// Bulgarian
        ///
        bg,

        ///
        /// Catalan
        ///
        ca,

        ///
        /// Chinese (China)
        ///
        zh_CN,

        ///
        /// Chinese (Taiwan)
        ///
        zh_TW,

        ///
        /// Croatian
        ///
        hr,

        ///
        /// Czech
        ///
        cs,

        ///
        /// Danish
        ///
        da,

        ///
        /// Dutch
        ///
        nl,

        ///
        /// English (Great Britain)
        ///
        en_GB,

        ///
        /// English (USA)
        ///
        en_US,

        ///
        /// English
        ///
        en,

        ///
        /// Estonian
        ///
        et,

        ///
        /// Filipino
        ///
        fil,

        ///
        /// Finnish
        ///
        fi,

        ///
        /// French
        ///
        fr,

        ///
        /// German
        ///
        de,

        ///
        /// Greek
        ///
        el,

        ///
        /// Gujarati
        ///
        gu,

        ///
        /// Hebrew
        ///
        he,

        ///
        /// Hindi
        ///
        hi,

        ///
        /// Hungarian
        ///
        hu,

        ///
        /// Indonesian
        ///
        id,

        ///
        /// Italian
        ///
        it,

        ///
        /// Japanese
        ///
        ja,

        ///
        /// Kannada
        ///
        kn,

        ///
        /// Korean
        ///
        ko,

        ///
        /// Latvian
        ///
        lv,

        ///
        /// Lithuanian
        ///
        lt,

        ///
        /// Malay
        ///
        ms,

        ///
        /// Malayalam
        ///
        ml,

        ///
        /// Marathi
        ///
        mr,

        ///
        /// Norwegian
        ///
        no,

        ///
        /// Persian
        ///
        fa,

        ///
        /// Polish
        ///
        pl,

        ///
        /// Portuguese (Brazil)
        ///
        pt_BR,

        ///
        /// Portuguese (Portugal)
        ///
        pt_PT,

        ///
        /// Romanian
        ///
        ro,

        ///
        /// Russian
        ///
        ru,

        ///
        /// Serbian
        ///
        sr,

        ///
        /// Slovak
        ///
        sk,

        ///
        /// Slovenian
        ///
        sl,

        ///
        /// Spanish (Latin America and Caribbean)
        ///
        es_419,

        ///
        /// Spanish
        ///
        es,

        ///
        /// Swahili
        ///
        sw,

        ///
        /// Swedish
        ///
        sv,

        ///
        /// Tamil
        ///
        ta,

        ///
        /// Telugu
        ///
        te,

        ///
        /// Thai
        ///
        th,

        ///
        /// Turkish
        ///
        tr,

        ///
        /// Ukrainian
        ///
        uk,

        ///
        /// Vietnamese
        ///
        vi
    }
}