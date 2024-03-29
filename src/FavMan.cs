﻿using HTAlt;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Yorot
{
    /// <summary>
    /// Yorot Favorites Manager.
    /// </summary>
    public class FavMan : YorotManager
    {
        private YorotFavFolder rootFolder;

        public FavMan(string configFile, YorotMain main) : base(configFile, main)
        {
        }

        /// <summary>
        /// <see cref="true"/> to show favorites bar, otherwise <seealso cref="false"/>.
        /// </summary>
        public bool ShowFavorites { get; set; } = true;

        /// <summary>
        /// A list contains loaded favorites.
        /// </summary>
        public YorotFavFolder RootFolder
        { get => rootFolder; set { Main.OnFavoriteChange(value); rootFolder = value; } }

        public List<YorotFavFolder> Favorites => RootFolder.Favorites;

        /// <summary>
        /// Recursively gets all URLs of every favorite of <paramref name="list"/>.
        /// </summary>
        /// <param name="list">List.</param>
        /// <returns>A <see cref="List{T}"/> containing all URLs.</returns>
        public List<string> GetAllURLs(List<YorotFavFolder> list)
        {
            List<string> urls = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                YorotFavFolder fav = list[i];
                if (fav is YorotFavorite)
                {
                    urls.Add((fav as YorotFavorite).Url);
                }
                else
                {
                    List<string> list1 = GetAllURLs(fav.Favorites);
                    for (int ı = 0; ı < list1.Count; ı++)
                    {
                        urls.Add(list1[ı]);
                    }
                }
            }
            return urls;
        }

        /// <summary>
        /// Gets all favorites with <paramref name="url"/>.
        /// </summary>
        /// <param name="url">URL to find.</param>
        /// <param name="folder">Used for recursive search.</param>
        /// <returns>A <see cref="List{T}"/> of <seealso cref="YorotFavorite"/>.</returns>
        public List<YorotFavorite> GetFavorite(string url, YorotFavFolder folder = null)
        {
            var list = new List<YorotFavorite>();
            for (int i = 0; i < (folder != null ? folder.Favorites.Count : Favorites.Count); i++)
            {
                var fav = folder != null ? folder.Favorites[i] : Favorites[i];
                if (fav is YorotFavorite _fav)
                {
                    if (_fav.Url == url)
                    {
                        list.Add(_fav);
                    }
                }
                else
                {
                    list.AddRange(GetFavorite(url, fav).ToArray());
                }
            }
            return list;
        }

        /// <summary>
        /// Gets if an URL is favorited by user.
        /// </summary>
        /// <param name="url">String</param>
        /// <returns><see cref="bool"/></returns>
        public bool isFavorited(string url)
        {
            return GetAllURLs(Favorites).FindAll(i => i == url).Count > 0;
        }

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
          "<root>" + Environment.NewLine +
          "<!-- Yorot Favorites Config File" + Environment.NewLine + Environment.NewLine +
           "This file is used to save browser favorites." + Environment.NewLine +
          "Editing this file might cause problems with Yorot." + Environment.NewLine +
          "-->" + Environment.NewLine +
          "<Favorites>" + Environment.NewLine;
            for (int i = 0; i < Favorites.Count; i++)
            {
                YorotFavFolder site = Favorites[i];
                x += site.ToXml() + Environment.NewLine;
            }
            return (x + "</Favorites>" + Environment.NewLine + "</root>").BeautifyXML();
        }

        public override void ExtractXml(XmlNode rootNode)
        {
            RootFolder = new YorotFavFolder(this, null);
            List<string> appliedSettings = new List<string>();
            for (int ı = 0; ı < rootNode.ChildNodes.Count; ı++)
            {
                XmlNode node = rootNode.ChildNodes[ı];
                if (appliedSettings.Contains(node.Name.ToLowerEnglish()))
                {
                    Output.WriteLine("[FavMan] Threw away \"" + node.OuterXml + "\", configuration already applied.", LogLevel.Warning);
                    break;
                }
                appliedSettings.Add(node.Name);
                switch (node.Name.ToLowerEnglish())
                {
                    case "favorites":
                        for (int i = 0; i < node.ChildNodes.Count; i++)
                        {
                            XmlNode subnode = node.ChildNodes[i];
                            switch (subnode.Name.ToLowerEnglish())
                            {
                                case "favorite":
                                    Favorites.Add(new YorotFavorite(this, subnode) { Manager = this });
                                    break;

                                case "folder":
                                    Favorites.Add(new YorotFavFolder(this, subnode) { Manager = this });
                                    break;

                                default:
                                    if (!subnode.NodeIsComment()) { Output.WriteLine("[FavMan] Threw away \"" + subnode.OuterXml + "\", unsupported."); }
                                    break;
                            }
                        }
                        break;

                    default:
                        if (!node.NodeIsComment())
                        {
                            Output.WriteLine("[FavMan] Threw away \"" + node.OuterXml + "\", unsupported.", LogLevel.Warning);
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Favorites folder (in Favorites). Also works as skeleton class for Yorot Favorites.
    /// </summary>
    public class YorotFavFolder
    {
        /// <summary>
        /// Creates a new Yorot Favorite Folder.
        /// </summary>
        /// <param name="node">XML node associated with this folder.</param>
        public YorotFavFolder(FavMan favman, XmlNode node)
        {
            Manager = favman ?? throw new ArgumentNullException(nameof(favman));
            if (node != null)
            {
                // NAME
                if (node.Attributes["Name"] != null)
                {
                    Name = node.Attributes["Name"].Value.XmlToString();
                }
                else
                {
                    Name = HTAlt.Tools.GenerateRandomText(17);
                }
                // TEXT
                if (node.Attributes["Text"] != null)
                {
                    Text = node.Attributes["Text"].Value.XmlToString();
                }
                else
                {
                    Text = "";
                }
                // ICON
                if (node.Attributes["Icon"] != null)
                {
                    IconLoc = node.Attributes["Icon"].Value.XmlToString();
                }
                else
                {
                    IconLoc = "";
                }
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    XmlNode subnode = node.ChildNodes[i];
                    switch (subnode.Name)
                    {
                        case "Favorite":
                            Favorites.Add(new YorotFavorite(Manager, subnode) { Manager = Manager });
                            break;

                        case "Folder":
                            Favorites.Add(new YorotFavFolder(Manager, subnode) { Manager = Manager });
                            break;

                        default:
                            if (!subnode.NodeIsComment()) { Output.WriteLine("[FavMan] Threw away \"" + subnode.OuterXml + "\", unsupported.", LogLevel.Warning); }
                            break;
                    }
                }
            }
        }

        public YorotFavFolder(FavMan manager, string name = "", string text = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = HTAlt.Tools.GenerateRandomText(17);
            }
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            Name = name;
            Text = text;
        }

        /// <summary>
        /// Retrieves configuration as XML format.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public virtual string ToXml()
        {
            string x = "<Folder Name=\"" + Name.ToXML() + "\" Text=\"" + Text.ToXML() + "\" " + (IconLoc != null ? "Icon=\"" + IconLoc.ToXML() + "\"" : "") + " >" + Environment.NewLine;
            for (int i = 0; i < Favorites.Count; i++)
            {
                x += Favorites[i].ToXml() + Environment.NewLine;
            }
            return x + "</Folder>";
        }

        /// <summary>
        /// Actual location of icon.
        /// </summary>
        private string iconLoc;

        private string text;
        private string name;
        private List<YorotFavFolder> favorites = new List<YorotFavFolder>();

        /// <summary>
        /// Favorites manager associated with this folder/favorite.
        /// </summary>
        public FavMan Manager { get; set; }

        /// <summary>
        /// Subfolders and favorites inside this folder.
        /// </summary>
        public List<YorotFavFolder> Favorites
        { get => favorites; set { Manager.Main.OnFavoriteChange(this); favorites = value; } }

        /// <summary>
        /// Name, or kinda like ID of the folder/favorite.
        /// </summary>
        public string Name
        { get => name; set { Manager.Main.OnFavoriteChange(this); name = value; } }

        /// <summary>
        /// Display text of this folder/favorite.
        /// </summary>
        public string Text
        { get => text; set { Manager.Main.OnFavoriteChange(this); text = value; } }

        /// <summary>
        /// Easy-to-read version of icon.
        /// </summary>
        public string IconLoc
        { get => iconLoc.ShortenPath(Manager.Main); set { Manager.Main.OnFavoriteChange(this); iconLoc = value.GetPath(Manager.Main); } }

        /// <summary>
        /// Moves folder/favorite in <paramref name="oi"/> to <paramref name="ni"/>.
        /// </summary>
        /// <param name="oi">Old index.</param>
        /// <param name="ni">New index.</param>
        public virtual void Move(int oi, int ni)
        {
            if (oi < 0 || oi > Favorites.Count - 1)
            {
                throw new ArgumentOutOfRangeException("\"oi\" was out of the bounds.");
            }
            if (ni < 0 || ni > Favorites.Count - 1)
            {
                throw new ArgumentOutOfRangeException("\"ni\" was out of the bounds.");
            }
            Move(Favorites[oi], ni);
        }

        /// <summary>
        /// Moves <paramref name="f"/> to <paramref name="i"/>.
        /// </summary>
        /// <param name="f">Folder/favorite</param>
        /// <param name="i">New index.</param>
        public virtual void Move(YorotFavFolder f, int i)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }
            if (!Favorites.Contains(f))
            {
                throw new ArgumentOutOfRangeException("f");
            }
            if (i < 0 || i > Favorites.Count)
            {
                throw new ArgumentOutOfRangeException("i");
            }
            Favorites.Remove(f);
            Favorites.Insert(i, f);
        }

        /// <summary>
        /// Moves folder/favorite in <paramref name="i"/> to 1 up.
        /// </summary>
        /// <param name="i">Index of folder/favorite.</param>
        public virtual void MoveUp(int i)
        {
            if (i < 1)
            {
                throw new ArgumentOutOfRangeException("i");
            }
            Move(Favorites[i], i - 1);
        }

        /// <summary>
        /// Moves <paramref name="f"/> to 1 up.
        /// </summary>
        /// <param name="f">Folder/Favorite</param>
        public virtual void MoveUp(YorotFavFolder f)
        {
            int i = Favorites.IndexOf(f);
            if (i < 1)
            {
                throw new ArgumentOutOfRangeException("f");
            }
            Move(f, i - 1);
        }

        /// <summary>
        /// Moves the folder/favorite in <paramref name="i"/> to 1 down.
        /// </summary>
        /// <param name="i">Index of folder/favorite.</param>
        public virtual void MoveDown(int i)
        {
            if (i > Favorites.Count)
            {
                throw new ArgumentOutOfRangeException("i");
            }
            Move(Favorites[i], i + 1);
        }

        /// <summary>
        /// Moves <paramref name="f"/> to 1 down.
        /// </summary>
        /// <param name="f">Folder/Favorite.</param>
        public virtual void MoveDown(YorotFavFolder f)
        {
            int i = Favorites.IndexOf(f);
            if (i > Favorites.Count)
            {
                throw new ArgumentOutOfRangeException("f");
            }
            Move(f, i + 1);
        }
    }

    /// <summary>
    /// A Yorot Favorite.
    /// </summary>
    public class YorotFavorite : YorotFavFolder
    {
        private string url;

        /// <summary>
        /// Creates a new Yorot Favorite.
        /// </summary>
        /// <param name="node">XML node associated with this favorite.</param>
        public YorotFavorite(FavMan favman, XmlNode node) : base(favman, node)
        {
            // NAME
            if (node.Attributes["Name"] != null)
            {
                Name = node.Attributes["Name"].Value.XmlToString();
            }
            else
            {
                Name = HTAlt.Tools.GenerateRandomText(17);
            }
            // TEXT
            if (node.Attributes["Text"] != null)
            {
                Text = node.Attributes["Text"].Value.XmlToString();
            }
            else
            {
                Text = "";
            }
            // ICON
            if (node.Attributes["Icon"] != null)
            {
                Name = node.Attributes["Icon"].Value.XmlToString();
            }
            else
            {
                IconLoc = "";
            }
            // URL
            if (node.Attributes["Url"] != null)
            {
                Url = node.Attributes["Url"].Value.XmlToString();
            }
            else
            {
                Url = "yorot://error/?e=FAVORITE_MISSING_URL";
            }
        }

        public YorotFavorite(YorotFavFolder parentFolder, string url, string title = "") : base(parentFolder.Manager, "", title)
        {
            ParentFolder = parentFolder;
            Url = url;
        }

        /// <summary>
        /// Retrieves configuration as XML format.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public override string ToXml()
        {
            return "<Favorite Name=\"" + Name.ToXML() + "\" Text=\"" + Text.ToXML() + "\" " + (!string.IsNullOrWhiteSpace(IconLoc) ? "Icon=\"" + IconLoc.ToXML() + "\" " : "") + "Url=\"" + Url.ToXML() + "\" />";
        }

        /// <summary>
        /// Parent folder of this favorite. <see cref="null"/> if it's on top.
        /// </summary>
        public YorotFavFolder ParentFolder { get; set; }

        /// <summary>
        /// You cannot use this void. Favorites are not containers! It will throw an exception!
        /// </summary>
        /// <param name="i">DO NOT USE</param>
        /// <param name="di">DO NOT USE</param>
        public override void Move(int i, int di)
        {
            throw new Exception("Favorites are not containers, thus cannot move anything.");
        }

        /// <summary>
        /// You cannot use this void. Favorites are not containers! It will throw an exception!
        /// </summary>
        /// <param name="f">DO NOT USE</param>
        /// <param name="i">DO NOT USE</param>
        public override void Move(YorotFavFolder f, int i)
        {
            throw new Exception("Favorites are not containers, thus cannot move anything.");
        }

        /// <summary>
        /// Moves this favorite to <paramref name="i"/>.
        /// </summary>
        /// <param name="i">New index of this favorite.</param>
        public void MoveTo(int i)
        {
            ParentFolder.Move(this, i);
        }

        /// <summary>
        /// Moves this favorite 1 up.
        /// </summary>
        public void MoveUp()
        {
            ParentFolder.MoveUp(this);
        }

        /// <summary>
        /// You cannot use this void. Favorites are not containers! It will throw an exception!
        /// </summary>
        /// <param name="f">DO NOT USE</param>
        public override void MoveUp(YorotFavFolder f)
        {
            throw new Exception("Favorites are not containers, thus cannot move anything.");
        }

        /// <summary>
        /// Moves this favorite 1 down.
        /// </summary>
        public void MoveDown()
        {
            ParentFolder.MoveDown(this);
        }

        /// <summary>
        /// You cannot use this void. Favorites are not containers! It will throw an exception!
        /// </summary>
        /// <param name="f">DO NOT USE</param>
        public override void MoveDown(YorotFavFolder f)
        {
            throw new Exception("Favorites are not containers, thus cannot move anything.");
        }

        /// <summary>
        /// You cannot use this void. Favorites are not containers! It will throw an exception!
        /// </summary>
        /// <param name="i">DO NOT USE</param>
        public override void MoveUp(int i)
        {
            throw new Exception("Favorites are not containers, thus cannot move anything.");
        }

        /// <summary>
        /// You cannot use this void. Favorites are not containers! It will throw an exception!
        /// </summary>
        /// <param name="i">DO NOT USE</param>
        public override void MoveDown(int i)
        {
            throw new Exception("Favorites are not containers, thus cannot move anything.");
        }

        /// <summary>
        /// It returns null, so don't use!
        /// </summary>
        public new List<YorotFavFolder> Favorites => null;

        /// <summary>
        /// Website of the favorite.
        /// </summary>
        public string Url
        { get => url; set { Manager.Main.OnFavoriteChange(this); url = value; } }
    }
}