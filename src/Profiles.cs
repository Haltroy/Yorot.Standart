using HTAlt;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Yorot
{
    /// <summary>
    /// Yorot Profile Manager
    /// </summary>
    public class ProfileManager : YorotManager
    {
        /// <summary>
        /// Creates a new Profile Manager.
        /// </summary>
        /// <param name="main"><see cref="YorotMain"/></param>
        public ProfileManager(YorotMain main) : base(main.ProfileConfig, main)
        {
            Profiles.Add(DefaultProfiles.Root(this).CreateCarbonCopy());
            if (Current is null)
            {
                Current = Profiles.FindAll(it => it.Name == "root")[0];
            }
        }

        /// <summary>
        /// A list of loaded profiles.
        /// </summary>
        public List<YorotProfile> Profiles { get; set; } = new List<YorotProfile>();

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
                "<root>" + Environment.NewLine +
                "<!-- Yorot Profiles Config File" + Environment.NewLine + Environment.NewLine +
                "This file is used to configure profiles." + Environment.NewLine +
                "Editing this file might cause problems with themes." + Environment.NewLine +
                "-->" + Environment.NewLine +
                "<Current Name=\"" + Current.Name.ToXML() + "\" Text=\"" + Current.Text.ToXML() + "\" />" + Environment.NewLine +
                "<Profiles>" + Environment.NewLine;
            for (int i = 0; i < Profiles.Count; i++)
            {
                YorotProfile user = Profiles[i];
                if (user.Name != "root")
                {
                    x += "<Profile Name=\"" + user.Name.ToXML() + "\" Text=\"" + user.Text + "\" />" + Environment.NewLine;
                }
            }
            return (x + "</Profiles>" + Environment.NewLine + "</root>").BeautifyXML();
        }

        public override void ExtractXml(XmlNode rootNode)
        {
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                bool loadedCurrent = false;
                bool loadedProf = false;
                switch (node.Name.ToLowerEnglish())
                {
                    case "current":
                        if (loadedCurrent)
                        {
                            Output.WriteLine("[Profiles] Threw away \"" + node.OuterXml + "\" because configuration is already loaded.", LogLevel.Warning);
                            break;
                        }
                        loadedCurrent = true;
                        if (node.Attributes["Name"] == null) { throw new XmlException("Current profile node does not have \"Name\" attribute."); }
                        string currentName = node.Attributes["Name"].Value.XmlToString();
                        if (Profiles.FindAll(it => it.Name == currentName).Count > 0)
                        {
                            Current = Profiles.FindAll(it => it.Name == currentName)[0];
                        }
                        else
                        {
                            if (node.Attributes["Text"] == null) { throw new XmlException("Current profile node does not have \"Text\" attribute."); }
                            string currentText = node.Attributes["Text"].Value.XmlToString();
                            Current = new YorotProfile(currentName, currentText, this, true);
                            Profiles.Add(Current);
                        }
                        break;

                    case "profiles":
                        if (loadedProf)
                        {
                            Output.WriteLine("[Profiles] Threw away \"" + node.OuterXml + "\" because configuration is already loaded.", LogLevel.Warning);
                            break;
                        }
                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                        {
                            XmlNode subnode = node.ChildNodes[ı];
                            switch (subnode.Name.ToLowerEnglish())
                            {
                                case "profile":
                                    if (subnode.Attributes["Name"] != null && subnode.Attributes["Text"] != null)
                                    {
                                        string name = subnode.Attributes["Name"].Value.XmlToString();
                                        string text = subnode.Attributes["Text"].Value.XmlToString();
                                        if (Profiles.FindAll(it => it.Name == name).Count > 0)
                                        {
                                            Output.WriteLine("[Profiles] Threw away \"" + subnode.OuterXml + "\", profile already loaded.", LogLevel.Warning);
                                        }
                                        else
                                        {
                                            Profiles.Add(new YorotProfile(name, text, this));
                                        }
                                    }
                                    else
                                    {
                                        Output.WriteLine("[Profiles] Threw away \"" + subnode.OuterXml + "\" because configuration is missing at least one attribute.", LogLevel.Warning);
                                    }
                                    break;

                                default:
                                    if (!subnode.NodeIsComment()) { Output.WriteLine("[Profiles] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning); }
                                    break;
                            }
                        }
                        loadedProf = true;
                        break;

                    default:
                        if (!node.NodeIsComment()) { Output.WriteLine("[Profiles] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning); }
                        break;
                }
            }
        }

        /// <summary>
        /// The current profile.
        /// </summary>
        public YorotProfile Current { get; set; }
    }

    /// <summary>
    /// A static class containing the default profiles.
    /// </summary>
    public static class DefaultProfiles
    {
        /// <summary>
        /// The root user
        /// </summary>
        /// <param name="man">Manager</param>
        /// <returns><see cref="YorotProfile"/></returns>
        public static YorotProfile Root(ProfileManager man)
        {
            return new YorotProfile("root", "Root", man)
            { };
        }
    }

    /// <summary>
    /// Class for handling Yorot profiles.
    /// </summary>
    public class YorotProfile
    {
        /// <summary>
        /// Creates a new profile. Does not inits the profile.
        /// </summary>
        /// <param name="manager">Manager of this profile.</param>
        public YorotProfile(ProfileManager manager)
        {
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>
        /// Creates and initializes a profile.
        /// </summary>
        /// <param name="name">Name of this profile, used for directory name.</param>
        /// <param name="text">Display text of this user.</param>
        /// <param name="manager">Manager of this profile.</param>
        /// <param name="isCurrent">Determines if this profile is the current profile.</param>
        public YorotProfile(string name, string text, ProfileManager manager, bool isCurrent = true) : this(manager)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentNullException(nameof(name)); }
            Name = name;
            if (string.IsNullOrWhiteSpace(text)) { throw new ArgumentNullException(nameof(text)); }
            Text = text;
            if (name != "root" && !System.IO.Directory.Exists(CacheLoc)) { System.IO.Directory.CreateDirectory(CacheLoc); }
            if (!System.IO.Directory.Exists(Path))
            {
                System.IO.Directory.CreateDirectory(Path);
                Output.WriteLine("[Profile:\"" + name + "\"] Profile directory does not exists. Created directory.", LogLevel.Info);
            }
            if (isCurrent)
            {
                Settings = new Settings(this);
            }
        }

        /// <summary>
        /// Creates a carbon copy of this profile.
        /// </summary>
        /// <returns><see cref="YorotProfile"/></returns>
        public YorotProfile CreateCarbonCopy()
        {
            return new YorotProfile(Manager)
            {
                Name = Name,
                Text = Text,
                Settings = Settings,
                Manager = Manager,
            };
        }

        /// <summary>
        /// Name of the profile, used as the folder name of the profile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name of the profile. This text will be displayed as the name instead.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Full path of the profile directory.
        /// </summary>
        public string Path => Manager.Main.ProfilesFolder + Name + System.IO.Path.DirectorySeparatorChar;

        /// <summary>
        /// Settings of this profile.
        /// </summary>
        public Settings Settings { get; set; }

        /// <summary>
        /// Path of profile picture of this profile.
        /// </summary>
        public string PicturePath => Path + "picture.png";

        /// <summary>
        /// Manager of this profile.
        /// </summary>
        public ProfileManager Manager { get; set; }

        /// <summary>
        /// User Cache location.
        /// </summary>
        public string CacheLoc => Name == "root" ? "" : Path + "cache" + System.IO.Path.DirectorySeparatorChar;

        /// <summary>
        /// Add-on main local file folder.
        /// </summary>
        public string LocalLoc => Path + "local" + System.IO.Path.DirectorySeparatorChar;

        /// <summary>
        /// User settings location.
        /// </summary>
        public string UserSettings => Path + "settings.ycf";

        /// <summary>
        /// User sites location.
        /// </summary>
        public string UserSites => Path + "sites.ycf";

        /// <summary>
        /// History Manager configuration file location.
        /// </summary>
        public string UserHistory => Path + "history.ycf";

        /// <summary>
        /// Favorites Manager configuration file location.
        /// </summary>
        public string UserFavorites => Path + "favorites.ycf";

        /// <summary>
        /// Downloads Manager configuration file location.
        /// </summary>
        public string UserDownloads => Path + "downloads.ycf";
    }
}