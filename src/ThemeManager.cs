using HTAlt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Yorot
{
    /// <summary>
    /// Yorot Theme Manager
    /// </summary>
    public class ThemeManager : YorotManager
    {
        private List<YorotTheme> themes = new List<YorotTheme>();

        public ThemeManager(YorotMain main) : base(main.ThemeConfig, main)
        {
            Themes.Add(DefaultThemes.YorotLight(this).CarbonCopy());
            Themes.Add(DefaultThemes.YorotDark(this).CarbonCopy());
            Themes.Add(DefaultThemes.YorotDeepBlue(this).CarbonCopy());
            Themes.Add(DefaultThemes.YorotRazor(this).CarbonCopy());
            Themes.Add(DefaultThemes.YorotShadow(this).CarbonCopy());
            Themes.Add(DefaultThemes.YorotStone(this).CarbonCopy());
            ClaimMan();
        }

        /// <summary>
        /// Claims management for every theme.
        /// </summary>
        private void ClaimMan()
        {
            for (int i = 0; i < Themes.Count; i++)
            {
                Themes[i].Manager = this;
            }
        }

        /// <summary>
        /// A list of loaded themes.
        /// </summary>
        public List<YorotTheme> Themes
        {
            get => themes;
            set
            {
                Main.OnThemeListChanged();
                themes = value;
            }
        }

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
                "<root>" + Environment.NewLine +
                "<!-- Yorot Theme Config File" + Environment.NewLine + Environment.NewLine +
                "This file is used to configure themes." + Environment.NewLine +
                "Editing this file might cause problems with themes." + Environment.NewLine +
                "-->" + Environment.NewLine +
                "<Themes>" + Environment.NewLine;
            for (int i = 0; i < Themes.Count; i++)
            {
                YorotTheme theme = Themes[i];
                if (!theme.isDefaultTheme)
                {
                    x += "<Theme>" + theme.Config.ShortenPath(Main) + "</Theme>" + Environment.NewLine;
                }
            }
            return (x + "</Themes>" + Environment.NewLine + "</root>").BeautifyXML();
        }

        public override void ExtractXml(XmlNode rootNode)
        {
            List<string> acceptedSetting = new List<string>();
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                switch (node.Name)
                {
                    case "Themes":
                        if (acceptedSetting.Contains(node.Name))
                        {
                            Output.WriteLine("[ThemeMan] Threw away \"" + node.OuterXml + "\". Setting already applied.", LogLevel.Warning);
                            break;
                        }
                        acceptedSetting.Add(node.Name);
                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                        {
                            XmlNode subnode = node.ChildNodes[ı];
                            switch (subnode.Name)
                            {
                                case "Theme":
                                    Themes.Add(new YorotTheme(this, subnode.InnerXml.XmlToString().ShortenPath(Main)));
                                    break;

                                default:
                                    if (!subnode.NodeIsComment())
                                    {
                                        Output.WriteLine("[ThemeMan] Threw away \"" + subnode.OuterXml + "\". unsupported.", LogLevel.Warning);
                                    }
                                    break;
                            }
                        }
                        break;

                    default:
                        if (!node.NodeIsComment())
                        {
                            Output.WriteLine("[ThemeMan] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Finds and returns a <see cref="bool"/> if a theme exists in the drive.
        /// </summary>
        /// <param name="value"><see cref="YorotTheme.CodeName"/> of theme.</param>
        /// <returns><see cref="bool"/></returns>
        public bool ThemeExists(string value)
        {
            return Themes.FindAll(i => i.CodeName == value).Count > 0;
        }

        /// <summary>
        /// Enables a theme.
        /// </summary>
        /// <param name="value"><see cref="YorotTheme.CodeName"/> of theme.</param>
        public void Enable(string value)
        {
            List<YorotTheme> l = Themes.FindAll(i => i.CodeName == value);
            if (l.Count > 0)
            {
                l[0].Enabled = true;
            }
            else
            {
                throw new ArgumentException("Cannot find theme with codename \"" + value + "\".");
            }
        }

        /// <summary>
        /// Gets theme from code name.
        /// </summary>
        /// <param name="codeName">Code name of the theme.</param>
        /// <returns><see cref="YorotTheme"/></returns>
        public YorotTheme GetThemeByCN(string codeName)
        {
            List<YorotTheme> l = Themes.FindAll(i => i.CodeName == codeName);
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
    /// Default themes that comes with Yorot and all other flavors.
    /// </summary>
    public static class DefaultThemes
    {
        /// <summary>
        /// Light theme
        /// </summary>
        public static YorotTheme YorotLight(ThemeManager man) => new YorotTheme(man)
        {
            Name = "Yorot Light",
            Author = "Haltroy",
            CodeName = "com.haltroy.yorotlight",
            isDefaultTheme = true,
            Version = 1,
            ThumbLoc = @"YorotLight.png",
            BackColor = new YorotColor(255, 255, 255, 255),
            ForeColor = new YorotColor(255, 0, 0, 0),
            OverlayColor = new YorotColor(255, 64, 128, 255),
            ArtColor = new YorotColor(255, 235, 235, 235),
            ArtForeColor = new YorotColor(255, 16, 16, 16),
            OverlayForeColor = new YorotColor(255, 16, 16, 16),
            Enabled = true,
        };

        /// <summary>
        /// Light gray theme, looks like it's made in early 90's
        /// </summary>
        public static YorotTheme YorotStone(ThemeManager man) => new YorotTheme(man)
        {
            Name = "Yorot Stone",
            Author = "Haltroy",
            CodeName = "com.haltroy.yorotstone",
            isDefaultTheme = true,
            Version = 1,
            ThumbLoc = @"YorotStone.png",
            BackColor = new YorotColor(255, 155, 155, 155),
            ForeColor = new YorotColor(255, 0, 0, 0),
            OverlayColor = new YorotColor(255, 64, 128, 255),
            ArtColor = new YorotColor(255, 0, 0, 255),
            ArtForeColor = new YorotColor(255, 255, 255, 255),
            OverlayForeColor = new YorotColor(255, 205, 205, 205),
            Enabled = true,
        };

        /// <summary>
        /// Razor theme
        /// </summary>
        public static YorotTheme YorotRazor(ThemeManager man) => new YorotTheme(man)
        {
            Name = "Yorot Razor",
            Author = "Haltroy",
            CodeName = "com.haltroy.yorotrazor",
            isDefaultTheme = true,
            Version = 1,
            ThumbLoc = @"YorotRazor.png",
            BackColor = new YorotColor(255, 255, 255, 255),
            ForeColor = new YorotColor(255, 0, 0, 0),
            OverlayColor = new YorotColor(255, 102, 84, 160),
            ArtColor = new YorotColor(255, 64, 32, 16),
            ArtForeColor = new YorotColor(255, 205, 205, 205),
            OverlayForeColor = new YorotColor(255, 205, 205, 205),
            Enabled = true,
        };

        /// <summary>
        /// The "Dark mode"
        /// </summary>
        public static YorotTheme YorotDark(ThemeManager man) => new YorotTheme(man)
        {
            Name = "Yorot Dark",
            Author = "Haltroy",
            CodeName = "com.haltroy.yorotdark",
            isDefaultTheme = true,
            Version = 1,
            ThumbLoc = @"YorotDark.png",
            BackColor = new YorotColor(255, 0, 0, 0),
            ForeColor = new YorotColor(255, 195, 195, 195),
            OverlayColor = new YorotColor(255, 64, 128, 255),
            ArtColor = new YorotColor(255, 64, 64, 64),
            ArtForeColor = new YorotColor(255, 205, 205, 205),
            OverlayForeColor = new YorotColor(255, 16, 16, 16),
            Enabled = true,
        };

        /// <summary>
        /// A little brighter than <see cref="YorotDark"/>
        /// </summary>
        public static YorotTheme YorotShadow(ThemeManager man) => new YorotTheme(man)
        {
            Name = "Yorot Shadow",
            Author = "Haltroy",
            CodeName = "com.haltroy.yorotshadow",
            isDefaultTheme = true,
            Version = 1,
            ThumbLoc = @"YorotShadow.png",
            BackColor = new YorotColor(255, 32, 32, 32),
            ForeColor = new YorotColor(255, 195, 195, 195),
            OverlayColor = new YorotColor(255, 64, 128, 255),
            ArtColor = new YorotColor(255, 32, 32, 32),
            ArtForeColor = new YorotColor(255, 205, 205, 205),
            OverlayForeColor = new YorotColor(255, 16, 16, 16),
            Enabled = true,
        };

        /// <summary>
        /// Theme used in Haltroy's website
        /// </summary>
        public static YorotTheme YorotDeepBlue(ThemeManager man) => new YorotTheme(man)
        {
            Name = "Yorot Deep Blue",
            Author = "Haltroy",
            CodeName = "com.haltroy.yorotdeepblue",
            isDefaultTheme = true,
            Version = 1,
            ThumbLoc = @"YorotDeepBlue.png",
            BackColor = new YorotColor("#080020"),
            ForeColor = new YorotColor(255, 64, 128, 255),
            OverlayColor = new YorotColor(255, 0, 255, 196),
            ArtColor = new YorotColor(255, 16, 8, 82),
            ArtForeColor = new YorotColor(255, 0, 255, 196),
            OverlayForeColor = new YorotColor(255, 64, 128, 255),
            Enabled = true,
        };
    }

    /// <summary>
    /// Yorot Theme
    /// </summary>
    public partial class YorotTheme : IEquatable<YorotTheme>
    {
        #region Stuff

        private string name;
        private string author;
        private string codeName;
        private string hTUPDATE;
        private int version;
        private string thumbLoc;
        private string config;
        private bool isDefaultTheme1 = false;
        private YorotColor backColor;
        private YorotColor foreColor;
        private YorotColor overlayColor;
        private YorotColor overlayForeColor;
        private YorotColor artColor;
        private YorotColor artForeColor;
        private bool enabled = false;

        #endregion Stuff

        /// <summary>
        /// Creates a new Yorot theme. This constructor does not initializes the theme.
        /// </summary>
        public YorotTheme(ThemeManager man)
        {
            Manager = man;
        }

        /// <summary>
        /// Creates a new Yorot theme and initializes it.
        /// </summary>
        /// <param name="fileLoc">Location of the theme file on disk.</param>
        public YorotTheme(ThemeManager man, string fileLoc) : this(man)
        {
            if (!string.IsNullOrWhiteSpace(fileLoc))
            {
                if (File.Exists(fileLoc))
                {
                    Config = fileLoc;
                    string xml = HTAlt.Tools.ReadFile(fileLoc, Encoding.Unicode);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlNode rootNode = HTAlt.Tools.FindRoot(doc);
                    List<string> applied = new List<string>();
                    for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                    {
                        XmlNode node = rootNode.ChildNodes[i];
                        string nodeName = node.Name.ToLowerEnglish();
                        if (applied.Contains(nodeName))
                        {
                            Output.WriteLine("[Theme:\"" + Config + "\"] Threw away \"" + node.OuterXml + "\", configuration already applied.", LogLevel.Warning);
                            break;
                        }
                        applied.Add(nodeName);
                        switch (nodeName)
                        {
                            case "name":
                                Name = node.InnerXml.XmlToString();
                                break;

                            case "author":
                                Author = node.InnerXml.XmlToString();
                                break;

                            case "codename":
                                CodeName = node.InnerXml.XmlToString();
                                break;

                            case "version":
                                Version = int.Parse(node.InnerXml.XmlToString());
                                break;

                            case "thumbnail":
                                ThumbLoc = node.InnerXml.XmlToString();
                                break;

                            case "backcolor":
                                BackColor = new YorotColor(node.InnerXml.XmlToString());
                                break;

                            case "forecolor":
                                ForeColor = node.InnerXml.XmlToString().ToLowerEnglish() == "auto" ? BackColor.AutoWhiteBlack : new YorotColor(node.InnerXml.XmlToString());
                                break;

                            case "overlaycolor":
                                OverlayColor = new YorotColor(node.InnerXml.XmlToString());
                                break;

                            case "overlayforecolor":
                                OverlayForeColor = node.InnerXml.XmlToString().ToLowerEnglish() == "auto" ? OverlayColor.AutoWhiteBlack : new YorotColor(node.InnerXml.XmlToString());
                                break;

                            case "artcolor":
                                ArtColor = new YorotColor(node.InnerXml.XmlToString());
                                break;

                            case "artforecolor":
                                ArtColor = node.InnerXml.XmlToString().ToLowerEnglish() == "auto" ? ArtColor.AutoWhiteBlack : new YorotColor(node.InnerXml.XmlToString());
                                break;

                            default:
                                if (!node.NodeIsComment())
                                {
                                    Output.WriteLine("[Theme:\"" + Config + "\"] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning);
                                }
                                break;
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("File \"" + fileLoc + "\" on argument \"fileLoc\" does not exists.");
                }
            }
            else
            {
                throw new ArgumentNullException("fileLoc");
            }
        }

        /// <summary>
        /// Creates a carbon-copy of this theme.
        /// </summary>
        /// <returns><see cref="YorotTheme"/></returns>
        public YorotTheme CarbonCopy()
        {
            return new YorotTheme(Manager)
            {
                Name = Name,
                Author = Author,
                CodeName = CodeName,
                HTUPDATE = HTUPDATE,
                Version = Version,
                ThumbLoc = ThumbLoc,
                isDefaultTheme = isDefaultTheme,
                BackColor = BackColor,
                ForeColor = ForeColor,
                OverlayColor = OverlayColor,
                ArtColor = ArtColor,
                ArtForeColor = ArtForeColor,
                OverlayForeColor = OverlayForeColor,
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as YorotTheme);
        }

        public bool Equals(YorotTheme other)
        {
            return other != null &&
                   Name == other.Name &&
                   Author == other.Author &&
                   CodeName == other.CodeName &&
                   HTUPDATE == other.HTUPDATE &&
                   Version == other.Version &&
                   ThumbLoc == other.ThumbLoc &&
                   Config == other.Config &&
                   isDefaultTheme == other.isDefaultTheme &&
                   EqualityComparer<YorotColor>.Default.Equals(BackColor, other.BackColor) &&
                   EqualityComparer<YorotColor>.Default.Equals(ForeColor, other.ForeColor) &&
                   EqualityComparer<YorotColor>.Default.Equals(OverlayColor, other.OverlayColor) &&
                   EqualityComparer<YorotColor>.Default.Equals(OverlayForeColor, other.OverlayForeColor) &&
                   EqualityComparer<YorotColor>.Default.Equals(ArtColor, other.ArtColor) &&
                   EqualityComparer<YorotColor>.Default.Equals(ArtForeColor, other.ArtForeColor) &&
                   Enabled == other.Enabled;
        }

        public override int GetHashCode()
        {
            int hashCode = 530125858;
            hashCode = hashCode * -1521134295 + EqualityComparer<ThemeManager>.Default.GetHashCode(Manager);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Author);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CodeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HTUPDATE);
            hashCode = hashCode * -1521134295 + Version.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ThumbLoc);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Config);
            hashCode = hashCode * -1521134295 + isDefaultTheme.GetHashCode();
            hashCode = hashCode * -1521134295 + BackColor.GetHashCode();
            hashCode = hashCode * -1521134295 + ForeColor.GetHashCode();
            hashCode = hashCode * -1521134295 + OverlayColor.GetHashCode();
            hashCode = hashCode * -1521134295 + OverlayForeColor.GetHashCode();
            hashCode = hashCode * -1521134295 + ArtColor.GetHashCode();
            hashCode = hashCode * -1521134295 + ArtForeColor.GetHashCode();
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Manager associated with this theme
        /// </summary>
        public ThemeManager Manager { get; set; }

        /// <summary>
        /// Name of the theme.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                name = value;
            }
        }

        /// <summary>
        /// Author of the theme.
        /// </summary>
        public string Author
        {
            get => author;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                author = value;
            }
        }

        /// <summary>
        /// CodeName of the theme.
        /// </summary>
        public string CodeName
        {
            get => codeName;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                codeName = value;
            }
        }

        /// <summary>
        /// HTUPDATE of the theme.
        /// </summary>
        public string HTUPDATE
        {
            get => hTUPDATE;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                hTUPDATE = value;
            }
        }

        /// <summary>
        /// Version of the theme.
        /// </summary>
        public int Version
        {
            get => version;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                version = value;
            }
        }

        /// <summary>
        /// Thumbnail location of the theme.
        /// </summary>
        public string ThumbLoc
        {
            get => thumbLoc;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                thumbLoc = value;
            }
        }

        /// <summary>
        /// Location of the theme file on disk.
        /// </summary>
        public string Config
        {
            get => config;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                config = value;
            }
        }

        /// <summary>
        /// Determines if this theme comes pre-installed with Yorot.
        /// </summary>
        public bool isDefaultTheme
        {
            get => isDefaultTheme1;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                isDefaultTheme1 = value;
            }
        }

        /// <summary>
        /// Background Color
        /// </summary>
        public YorotColor BackColor
        {
            get => backColor;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                backColor = value;
            }
        }

        /// <summary>
        /// A little brighter/darker <see cref="BackColor"/>.
        /// </summary>
        public YorotColor BackColor2 => BackColor.ShiftBrightness(20, false);

        /// <summary>
        /// Brighter/darker <see cref="BackColor"/>.
        /// </summary>
        public YorotColor BackColor3 => BackColor.ShiftBrightness(40, false);

        /// <summary>
        /// More brighter/darker <see cref="BackColor"/>.
        /// </summary>
        public YorotColor BackColor4 => BackColor.ShiftBrightness(60, false);

        /// <summary>
        /// The foreground color, determines the text and button images colors.
        /// </summary>
        public YorotColor ForeColor
        {
            get => foreColor;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                foreColor = value;
            }
        }

        /// <summary>
        /// The overlay color, determines the edges etc.
        /// </summary>
        public YorotColor OverlayColor
        {
            get => overlayColor;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                overlayColor = value;
            }
        }

        /// <summary>
        /// A little brighter/darker <see cref="OverlayColor"/>.
        /// </summary>
        public YorotColor OverlayColor2 => OverlayColor.ShiftBrightness(20, false);

        /// <summary>
        /// Brighter/darker <see cref="OverlayColor"/>.
        /// </summary>
        public YorotColor OverlayColor3 => OverlayColor.ShiftBrightness(40, false);

        /// <summary>
        /// More brighter/darker <see cref="OverlayColor"/>.
        /// </summary>
        public YorotColor OverlayColor4 => OverlayColor.ShiftBrightness(60, false);

        /// <summary>
        /// Fore color for <see cref="OverlayColor"/>.
        /// </summary>
        public YorotColor OverlayForeColor
        {
            get => overlayForeColor;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                overlayForeColor = value;
            }
        }

        /// <summary>
        /// A little brighter/darker <see cref="OverlayForeColor"/>.
        /// </summary>

        public YorotColor OverlayForeColor2 => OverlayForeColor.ShiftBrightness(20, false);

        /// <summary>
        /// Brighter/darker <see cref="OverlayForeColor"/>.
        /// </summary>
        public YorotColor OverlayForeColor3 => OverlayForeColor.ShiftBrightness(40, false);

        /// <summary>
        /// More brighter/darker <see cref="OverlayForeColor"/>.
        /// </summary>
        public YorotColor OverlayForeColor4 => OverlayForeColor.ShiftBrightness(60, false);

        /// <summary>
        /// Artiliary color, similar to <see cref="OverlayColor"/>
        /// </summary>
        public YorotColor ArtColor
        {
            get => artColor;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                artColor = value;
            }
        }

        /// <summary>
        /// A little brighter/darker <see cref="ArtColor"/>.
        /// </summary>
        public YorotColor ArtColor2 => ArtColor.ShiftBrightness(20, false);

        /// <summary>
        /// Brighter/darker <see cref="ArtColor"/>.
        /// </summary>
        public YorotColor ArtColor3 => ArtColor.ShiftBrightness(40, false);

        /// <summary>
        /// More brighter/darker <see cref="ArtColor"/>.
        /// </summary>
        public YorotColor ArtColor4 => ArtColor.ShiftBrightness(60, false);

        /// <summary>
        /// Fore Color for <see cref="ArtColor"/>
        /// </summary>
        public YorotColor ArtForeColor
        {
            get => artForeColor;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                artForeColor = value;
            }
        }

        /// <summary>
        /// A little brighter/darker <see cref="ArtForeColor"/>.
        /// </summary>
        public YorotColor ArtForeColor2 => ArtForeColor.ShiftBrightness(20, false);

        /// <summary>
        /// Brighter/darker <see cref="ArtForeColor"/>.
        /// </summary>
        public YorotColor ArtForeColor3 => ArtForeColor.ShiftBrightness(40, false);

        /// <summary>
        /// More brighter/darker <see cref="ArtForeColor"/>.
        /// </summary>
        public YorotColor ArtForeColor4 => ArtForeColor.ShiftBrightness(60, false);

        /// <summary>
        /// Determines if this theme is enabled or disabled by user or the system.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (Manager != null)
                {
                    Manager.Main.OnThemeChange(this);
                }
                enabled = value;
            }
        }

        public static bool operator ==(YorotTheme left, YorotTheme right)
        {
            return EqualityComparer<YorotTheme>.Default.Equals(left, right);
        }

        public static bool operator !=(YorotTheme left, YorotTheme right)
        {
            return !(left == right);
        }
    }
}