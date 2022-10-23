using HTAlt;
using System;
using System.Collections.Generic;
using System.Xml;
using LibFoster;

namespace Yorot
{
    /// <summary>
    /// <c>Yo</c>rot <c>Pa</c>ckage <c>D</c>istribution service.
    /// </summary>
    public class Yopad : YorotManager
    {
        /// <summary>
        /// Creates a new Yopad Service.
        /// </summary>
        /// <param name="main"><see cref="YorotMain"/></param>
        public Yopad(YorotMain main) : base(main.YopadConfig, main)
        {
            Repositories.Add(new YopadRepository()
            {
                Name = "Yopad Official Repository",
                ExpireTime = 300,
                CodeName = "com.haltroy.yopad",
                isEnabled = true,
                Description = "The official Yopad repository.",
                Yopad = this,
                Url = "https://raw.githubusercontent.com/Haltroy/Yopad/main/index.xml"
            });
        }

        /// <summary>
        /// A list of HTUPDATEs of installed add-ons.
        /// </summary>
        public List<Foster> Fosters { get; set; } = new List<Foster>();

        /// <summary>
        /// A list of installed add-ons.
        /// </summary>
        public List<YopadAddon> InstalledAddons { get; set; } = new List<YopadAddon>();

        /// <summary>
        /// A list of Yopad repositories.
        /// </summary>
        public List<YopadRepository> Repositories { get; set; } = new List<YopadRepository>();

        /// <summary>
        /// Registers Foster.
        /// </summary>
        /// <param name="url">URL of Foster.</param>
        public void RegisterFoster(string url)
        {
            var htu = new Foster() { URL = url };
            htu.OnLogEntry += YopadLog;
            Fosters.Add(htu);
        }

        /// <summary>
        /// Refreshes repositories without freezing app.
        /// </summary>
        /// <param name="force"><c>true</c> to refresh even not-expired repositories, otherwise <c>false</c>.</param>
        public async void RefreshReposAsync(bool force = false)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                RefreshRepos(force);
            });
        }

        /// <summary>
        /// Refreshes repositories (freezes app).
        /// </summary>
        /// <param name="force"><c>true</c> to refresh even not-expired repositories, otherwise <c>false</c>.</param>
        public void RefreshRepos(bool force = false)
        {
            for (int i = 0; i < Repositories.Count; i++)
            {
                var repo = Repositories[i];
                try
                {
                    if ((force || repo.ExpireDate.AddSeconds(repo.ExpireTime).HasExpired()) && repo.isEnabled)
                    {
                        YopadLog(this, new OnLogEntryEventArgs((force ? "Force check on repository: " : "Check on repository: ") + repo.CodeName, LibFoster.LogLevel.Info));
                        var e = new YopadProgressEventArgs() { Total = 100, Received = 0 };
                        YopadProgress(repo, e);
                        System.Net.WebClient wc = new System.Net.WebClient();
                        string repoXml = wc.DownloadString(repo.Url);
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(repoXml);
                        XmlNode rootNode = doc.FindRoot();
                        List<string> applied = new List<string>();
                        for (int _i = 0; _i < rootNode.ChildNodes.Count; _i++)
                        {
                            var node = rootNode.ChildNodes[_i];
                            switch (node.Name.ToLowerEnglish())
                            {
                                case "name":
                                    if (applied.Contains(node.Name.ToLowerEnglish()))
                                    {
                                        YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + "] Threw out \"" + node.OuterXml + "\", configuration already applied.", LibFoster.LogLevel.Warning));
                                        break;
                                    }
                                    applied.Add(node.Name.ToLowerEnglish());
                                    repo.Name = node.InnerXml.XmlToString();
                                    break;

                                case "codename":
                                    if (applied.Contains(node.Name.ToLowerEnglish()))
                                    {
                                        YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + "] Threw out \"" + node.OuterXml + "\", configuration already applied.", LibFoster.LogLevel.Warning));
                                        break;
                                    }
                                    applied.Add(node.Name.ToLowerEnglish());
                                    repo.CodeName = node.InnerXml.XmlToString();
                                    break;

                                case "description":
                                    if (applied.Contains(node.Name.ToLowerEnglish()))
                                    {
                                        YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + "] Threw out \"" + node.OuterXml + "\", configuration already applied.", LibFoster.LogLevel.Warning));
                                        break;
                                    }
                                    applied.Add(node.Name.ToLowerEnglish());
                                    repo.Description = node.InnerXml.XmlToString();
                                    break;

                                case "expire":
                                    if (applied.Contains(node.Name.ToLowerEnglish()))
                                    {
                                        YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + "] Threw out \"" + node.OuterXml + "\", configuration already applied.", LibFoster.LogLevel.Warning));
                                        break;
                                    }
                                    applied.Add(node.Name.ToLowerEnglish());
                                    repo.ExpireTime = int.Parse(node.InnerXml.XmlToString());
                                    break;

                                case "ref":
                                    if (node.Attributes["Name"] != null && node.Attributes["Url"] != null)
                                    {
                                        string refname = node.Attributes["Name"].Value.XmlToString();
                                        string refurl = node.Attributes["Url"].Value.XmlToString();
                                        if (!string.IsNullOrWhiteSpace(refname) && !string.IsNullOrWhiteSpace(refurl))
                                        {
                                            YopadAddonList _ref;
                                            if (repo.Addons.FindAll(it => it.Name == refname).Count > 0)
                                            {
                                                _ref = repo.Addons.FindAll(it => it.Name == refname)[0];
                                                string refdesc = string.Empty;
                                                if (node.Attributes["Description"] != null) { refdesc = node.Attributes["Description"].Value.XmlToString(); }
                                                _ref.Name = refname;
                                                _ref.Url = refurl;
                                                _ref.Description = refdesc;
                                            }
                                            else
                                            {
                                                string refdesc = string.Empty;
                                                if (node.Attributes["Description"] != null) { refdesc = node.Attributes["Description"].Value.XmlToString(); }
                                                _ref = new YopadAddonList(repo, refname, refdesc, refurl);
                                                repo.Addons.Add(_ref);
                                            }
                                            for (int j = 0; j < node.ChildNodes.Count; j++)
                                            {
                                                var subnode = node.ChildNodes[j];
                                                if (subnode.Attributes["Name"] != null && subnode.Attributes["CodeName"] != null)
                                                {
                                                    var catName = subnode.Attributes["Name"].Value.XmlToString();
                                                    var catCodeName = subnode.Attributes["CodeName"].Value.XmlToString();
                                                    var catDesc = string.Empty;
                                                    if (subnode.Attributes["Desc"] != null)
                                                    {
                                                        catDesc = subnode.Attributes["Desc"].Value.XmlToString();
                                                    }
                                                    var _list = _ref.Categories.FindAll(it => it.CodeName == catCodeName);
                                                    if (_list.Count > 0)
                                                    {
                                                        _list[0].Name = catName;
                                                        _list[0].Description = catDesc;
                                                    }
                                                    else
                                                    {
                                                        _ref.Categories.Add(new YopadCategory(_ref, catName, catCodeName, catDesc));
                                                    }
                                                }
                                                else
                                                {
                                                    YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + " : " + _ref.Name + "] Threw out \"" + subnode.OuterXml + "\", important XML node attribute(s) missing values.", LibFoster.LogLevel.Warning));
                                                }
                                            }
                                            try
                                            {
                                                _ref.Refresh();
                                            }
                                            catch (Exception lex)
                                            {
                                                YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + " : " + _ref.Name + "] Error on refresh, exception caught: " + lex.ToString(), LibFoster.LogLevel.Error));
                                            }
                                        }
                                        else
                                        {
                                            YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + "] Threw out \"" + node.OuterXml + "\", important XML node attribute(s) missing values.", LibFoster.LogLevel.Warning));
                                        }
                                    }
                                    else
                                    {
                                        YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + "] Threw out \"" + node.OuterXml + "\", missing XMl node attribute(s).", LibFoster.LogLevel.Warning));
                                    }
                                    break;

                                default:
                                    if (!node.NodeIsComment())
                                    {
                                        YopadLog(this, new OnLogEntryEventArgs("[" + repo.CodeName + "] Threw out \"" + node.OuterXml + "\", unsupported.", LibFoster.LogLevel.Warning));
                                    }
                                    break;
                            }
                        }
                        e.Received = 100;
                        YopadProgress(repo, e);
                    }
                    repo.ExpireDate = DateTime.Now;
                }
                catch (Exception ex)
                {
                    YopadLog(this, new OnLogEntryEventArgs("Error while refreshing repository \"" + repo.CodeName + "\". Exception caught: " + ex.ToString(), LibFoster.LogLevel.Error));
                }
            }
        }

        /// <summary>
        /// Finds <see cref="YopadAddon"/> by its <paramref name="codename"/> and <paramref name="listname"/>.
        /// </summary>
        /// <param name="codename"><see cref="YopadAddon.CodeName"/></param>
        /// <param name="listname"><see cref="YopadAddonList.Name"/></param>
        /// <returns><see cref="YopadAddon"/></returns>
        public YopadAddon FindAddonByCN(string codename, string listname)
        {
            YopadAddon addon = null;
            for (int i = 0; i < Repositories.Count; i++)
            {
                var repo = Repositories[i];
                var lists = repo.Addons.FindAll(it => it.Name.ToLowerEnglish() == listname);
                if (lists != null)
                {
                    if (lists.Count > 0)
                    {
                        var list = lists[0];
                        var _a = list.FindAddonByCN(codename);
                        if (_a != null)
                        {
                            addon = _a;
                            break;
                        }
                    }
                }
            }
            return addon;
        }

        /// <summary>
        /// Updates current add-ons (without freezing).
        /// </summary>
        /// <param name="force"><c>true</c> to refresh even not-expired repositories, otherwise <c>false</c>.</param>
        public async void UpdateAsync(bool force = false)
        {
            await System.Threading.Tasks.Task.Run(() => { Update(force); });
        }

        /// <summary>
        /// Updates current add-ons (might freeze).
        /// </summary>
        /// <param name="force"><c>true</c> to refresh even not-expired repositories, otherwise <c>false</c>.</param>
        public void Update(bool force = false)
        {
            RefreshRepos(force);
            var list = GetUpdateList(force);
            for (int i = 0; i < list.Length; i++)
            {
                Install(list[i].Addon, true);
            }
        }

        /// <summary>
        /// Gets codenames of the
        /// </summary>
        /// <param name="force"><c>true</c> to refresh even not-expired repositories, otherwise <c>false</c>.</param>
        /// <returns><see cref="YopadUpgradeListItem"/> <seealso cref="Array"/>.</returns>
        public YopadUpgradeListItem[] GetUpdateList(bool force = false)
        {
            RefreshRepos(force);
            List<YopadUpgradeListItem> list = new List<YopadUpgradeListItem>();
            for (int i = 0; i < Fosters.Count; i++)
            {
                var htu = Fosters[i];
                htu.LoadUrlSync();
                if (!htu.IsUpToDate)
                {
                    list.Add(new YopadUpgradeListItem() { CodeName = htu.Name });
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Installs an add-on (might freeze).
        /// </summary>
        /// <param name="addon"><see cref="YopadAddon"/></param>
        /// <param name="reinstall"><c>true</c> to reinstal lthe add-on, otherwise <c>false</c>.</param>
        public void Install(YopadAddon addon, bool reinstall = false)
        {
            if (addon.AssocAddon != null && !reinstall)
            {
                YopadLog(this, new OnLogEntryEventArgs("Cannot install \"" + addon.CodeName + "\", already installed.", LibFoster.LogLevel.Error));
            }
            else
            {
                // Find work folder
                string addonWF = string.Empty; // workfolder
                // Find type
                switch (addon.AddonList.Name.ToLowerEnglish())
                {
                    case "themes":
                        addonWF = Main.ThemesFolder + addon.CodeName + System.IO.Path.DirectorySeparatorChar;
                        break;

                    case "apps":
                        addonWF = Main.AppsFolder + addon.CodeName + System.IO.Path.DirectorySeparatorChar;
                        break;

                    case "extensions":
                        addonWF = Main.ExtFolder + addon.CodeName + System.IO.Path.DirectorySeparatorChar;
                        break;

                    case "exppacks":
                        addonWF = Main.EPFolder + addon.CodeName + System.IO.Path.DirectorySeparatorChar;
                        break;

                    case "languages":
                        addonWF = Main.LangFolder + addon.CodeName + System.IO.Path.DirectorySeparatorChar;
                        break;
                }
                // Create HTU if not exist
                if (addon.AssocFoster == null)
                {
                    addon.AssocFoster = new Foster(addon.CodeName, addon.Foster_Url, addonWF, 0, "noarch");
                    addon.AssocFoster.OnLogEntry += YopadLog;
                }
                // HTU ready, install it.
                addon.AssocFoster.DoSyncUpdate(true);

                if (!InstalledAddons.Contains(addon))
                {
                    InstalledAddons.Add(addon);
                }
            }
        }

        /// <summary>
        /// Installs an add-on (without freezing).
        /// </summary>
        /// <param name="addon"><see cref="YopadAddon"/></param>
        public async void InstallAsync(YopadAddon addon)
        {
            await System.Threading.Tasks.Task.Run(() => { Install(addon); });
        }

        /// <summary>
        /// Event handler for Yopad progress changes.
        /// </summary>
        /// <param name="sender"><see cref="object"/></param>
        /// <param name="e"><see cref="YopadProgressEventArgs"/></param>
        public delegate void YopadProgressEventHandler(object sender, YopadProgressEventArgs e);

        /// <summary>
        /// Event raised on Yopad progress change.
        /// </summary>
        public event YopadProgressEventHandler YopadProgress;

        /// <summary>
        /// Event raised when Yopad log.
        /// </summary>
        public event Foster.OnLogEntryDelegate YopadLog;

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
                "<root>" + Environment.NewLine +
                "<!-- Yopad Config File" + Environment.NewLine + Environment.NewLine +
                "This file is used to configure Yorot Package Distribution Service." + Environment.NewLine +
                "Editing this file might cause problems with add-on installation." + Environment.NewLine +
                "-->" + Environment.NewLine +
                "<Repos>" + Environment.NewLine;
            for (int i = 0; i < Repositories.Count; i++)
            {
                x += "<Repo CodeName=\"" + Repositories[i].CodeName.ToXML() + "\" Url=\"" + Repositories[i].Url.ToXML() + "\" Name=\"" + Repositories[i].Name.ToXML() + "\" Expire=\"" + Repositories[i].ExpireTime + "\" />" + Environment.NewLine;
            }
            x += "</Repos>" + Environment.NewLine;
            if (InstalledAddons != null && InstalledAddons.Count > 0)
            {
                x += "<Addons>" + Environment.NewLine;
                for (int i = 0; i < InstalledAddons.Count; i++)
                {
                    var addon = InstalledAddons[i];
                    x += "<Addon Name=\"" + addon.CodeName + "\" Repo=\"" + addon.Repository.CodeName + "\" Version=\"" + addon.InstalledVersion + "\" />" + Environment.NewLine;
                }
                x += "</Addons>" + Environment.NewLine;
            }
            return (x + "</root>").BeautifyXML();
        }

        public override void ExtractXml(XmlNode rootNode)
        {
            List<string> appliedSettings = new List<string>();
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                switch (node.Name.ToLowerEnglish())
                {
                    case "repos":
                        if (appliedSettings.FindAll(it => it == node.Name).Count > 0)
                        {
                            Output.WriteLine("[Yopad] Threw away \"" + node.OuterXml + "\". Configurtion already applied.", HTAlt.LogLevel.Warning);
                            break;
                        }
                        appliedSettings.Add(node.Name);
                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                        {
                            XmlNode subnode = node.ChildNodes[ı];
                            if (subnode.Name.ToLowerEnglish() == "repo")
                            {
                                if (subnode.Attributes["CodeName"] != null && subnode.Attributes["Expire"] != null)
                                {
                                    string cn = subnode.Attributes["CodeName"].Value.XmlToString();
                                    int exp = int.Parse(subnode.Attributes["Expire"].Value.XmlToString());
                                    if (Repositories.FindAll(it => it.CodeName == cn).Count > 0)
                                    {
                                        Repositories.FindAll(it => it.CodeName == cn)[0].ExpireTime = exp;
                                    }
                                    else
                                    {
                                        if (subnode.Attributes["Name"] != null && subnode.Attributes["URL"] != null)
                                        {
                                            string n = subnode.Attributes["Name"].Value.XmlToString();
                                            string url = subnode.Attributes["URL"].Value.XmlToString();
                                            Repositories.Add(new YopadRepository() { CodeName = cn, Name = n, Url = url, ExpireTime = exp });
                                        }
                                        else
                                        {
                                            Output.WriteLine("[Yopad] Threw away \"" + subnode.OuterXml + "\". missing required atrributes.", HTAlt.LogLevel.Warning);
                                        }
                                    }
                                }
                                else
                                {
                                    Output.WriteLine("[Yopad] Threw away \"" + subnode.OuterXml + "\". missing required atrributes.", HTAlt.LogLevel.Warning);
                                }
                            }
                            else
                            {
                                if (!subnode.NodeIsComment())
                                {
                                    Output.WriteLine("[Yopad] Threw away \"" + subnode.OuterXml + "\". unsupported.", HTAlt.LogLevel.Warning);
                                }
                            }
                        }
                        break;

                    case "addons":
                        if (appliedSettings.FindAll(it => it == node.Name).Count > 0)
                        {
                            Output.WriteLine("[Yopad] Threw away \"" + node.OuterXml + "\". Configurtion already applied.", HTAlt.LogLevel.Warning);
                            break;
                        }
                        appliedSettings.Add(node.Name);
                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                        {
                            XmlNode subnode = node.ChildNodes[ı];
                            if (subnode.Name.ToLowerEnglish() == "addon")
                            {
                                if (subnode.Attributes["Name"] != null && subnode.Attributes["Repo"] != null && subnode.Attributes["Version"] != null)
                                {
                                    string cn = subnode.Attributes["Name"].Value.XmlToString();
                                    string repo = subnode.Attributes["Repo"].Value.XmlToString();
                                    int ver = int.Parse(subnode.Attributes["Version"].Value.XmlToString());
                                    if (Repositories.FindAll(it => it.CodeName == cn).Count > 0)
                                    {
                                        // TODO: Find Add-on or create a new one and hook it.
                                        //Repositories.FindAll(it => it.CodeName == cn)[0].FindAddon;
                                    }
                                    else
                                    {
                                        Output.WriteLine("[Yopad] Threw away \"" + subnode.OuterXml + "\". cannot find repository with code name \"" + repo + "\" for add-onwith code name \"" + cn + "\".", HTAlt.LogLevel.Warning);
                                    }
                                }
                                else
                                {
                                    Output.WriteLine("[Yopad] Threw away \"" + subnode.OuterXml + "\". missing required atrributes.", HTAlt.LogLevel.Warning);
                                }
                            }
                            else
                            {
                                if (!subnode.NodeIsComment())
                                {
                                    Output.WriteLine("[Yopad] Threw away \"" + subnode.OuterXml + "\". unsupported.", HTAlt.LogLevel.Warning);
                                }
                            }
                        }
                        break;

                    default:
                        if (!node.NodeIsComment())
                        {
                            Output.WriteLine("[Yopad] Threw away \"" + node.OuterXml + "\". Invalid configurtion.", HTAlt.LogLevel.Warning);
                        }
                        break;
                }
            }
        }

        internal void Throw(object sender, string v, LibFoster.LogLevel warning)
        {
            YopadLog(sender, new OnLogEntryEventArgs(v, warning));
        }
    }

    /// <summary>
    /// Yopad Update List Item
    /// </summary>
    public class YopadUpgradeListItem
    {
        /// <summary>
        /// Code name of the add-on.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Total of bytes required to download update, might require twice amount of it for everything.
        /// </summary>
        public long DownloadBytes { get; set; }

        /// <summary>
        /// <see cref="YopadAddon"/>.
        /// </summary>
        public YopadAddon Addon { get; set; }
    }

    /// <summary>
    /// Yopad Progress Change Event arguments.
    /// </summary>
    public class YopadProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Total bytes.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// Received bytes.
        /// </summary>
        public long Received { get; set; }

        /// <summary>
        /// Bytes to receive.
        /// </summary>
        public long Left => Total - Received;

        /// <summary>
        /// Percentage in 0.x form.
        /// </summary>
        public double Percentage => Received / Total;

        /// <summary>
        /// Percentage in % form.
        /// </summary>
        public long Percentage100 => (long)(Percentage * 100);
    }

    /// <summary>
    /// Yopad repository.
    /// </summary>
    public class YopadRepository
    {
        /// <summary>
        /// Determine if this repository is enabled ro not.
        /// </summary>
        public bool isEnabled { get; set; }

        /// <summary>
        /// URL of the repository.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Service attached to this repository.
        /// </summary>
        public Yopad Yopad { get; set; }

        /// <summary>
        /// Determines which date Yopad should refresh it.
        /// </summary>
        public System.DateTime ExpireDate { get; set; }

        /// <summary>
        /// Name of the repository.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Codename of the repository.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Description of the repository.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Times between refreshes in second.
        /// </summary>
        public int ExpireTime { get; set; }

        /// <summary>
        /// A list of Add-ons in this repository.
        /// </summary>
        public List<YopadAddonList> Addons { get; set; } = new List<YopadAddonList>();
    }

    /// <summary>
    /// A list of Yopad Add-ons.
    /// </summary>
    public class YopadAddonList
    {
        public YopadAddonList(YopadRepository repository, string name, string description, string url)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Categories.Add(new YopadCategory(this, "Uncategorised", "uncategorised", "Add-ons with no categories."));
        }

        /// <summary>
        /// Repository of this list.
        /// </summary>
        public YopadRepository Repository { get; set; }

        /// <summary>
        /// Attached service of this list.
        /// </summary>
        public Yopad Yopad => Repository.Yopad;

        /// <summary>
        /// Name of the Add-on list.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the Add-on list.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL of the Add-on list.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Categories of the Add-on list.
        /// </summary>
        public List<YopadCategory> Categories { get; set; } = new List<YopadCategory>();

        /// <summary>
        /// Gets <see cref="YopadCategory"/> by <seealso cref="YopadCategory.CodeName"/>.
        /// </summary>
        /// <param name="cn"><seealso cref="YopadCategory.CodeName"/></param>
        /// <returns><see cref="YopadCategory"/></returns>
        public YopadCategory GetCatByCN(string cn) => Categories.FindAll(it => it.CodeName.ToLowerEnglish() == cn.ToLowerEnglish()).Count > 0 ? Categories.FindAll(it => it.CodeName.ToLowerEnglish() == cn.ToLowerEnglish())[0] : null;

        /// <summary>
        /// Finds <see cref="YopadAddon"/> by its <seealso cref="YopadAddon.CodeName"/>.
        /// </summary>
        /// <param name="codename"><see cref="YopadAddon.CodeName"/></param>
        /// <returns><see cref="YopadAddon"/></returns>
        public YopadAddon FindAddonByCN(string codename)
        {
            YopadAddon addon = null;
            for (int i = 9; i < Categories.Count; i++)
            {
                var cat = Categories[i];
                for (int _i = 0; _i < cat.Addons.Count; _i++)
                {
                    var _a = cat.Addons[_i];
                    if (_a.CodeName.ToLowerEnglish() == codename.ToLowerEnglish())
                    {
                        addon = _a;
                        break;
                    }
                }
                if (addon != null) { break; }
            }
            return addon;
        }

        /// <summary>
        /// Refreshes the current Add-on list.
        /// </summary>
        public void Refresh()
        {
            var webC = new System.Net.WebClient();
            var xml = webC.DownloadString(Url.Replace("[0]", "index.xml"));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var rootNode = HTAlt.Tools.FindRoot(doc);
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                var node = rootNode.ChildNodes[i];
                if (node.Name.ToLowerEnglish() == "packageref")
                {
                    if (node.Attributes["Url"] != null)
                    {
                        var url = node.Attributes["Url"].Value.XmlToString();
                        YopadCategory cat = Categories.FindAll(it => it.CodeName == "uncategorised")[0];
                        var nsfw = false;
                        if (node.Attributes["NSFW"] != null)
                        {
                            nsfw = node.Attributes["NSFW"].Value.XmlToString() == "true";
                        }
                        if (node.Attributes["Category"] != null)
                        {
                            var _cat = Categories.FindAll(it => it.CodeName == node.Attributes["Category"].Value.XmlToString());
                            if (_cat.Count > 0)
                            {
                                cat = _cat[0];
                            }
                        }
                        var addonXml = webC.DownloadString(Url.Replace("[0]", url + (!url.EndsWith("\\") || !url.EndsWith("/") ? "\\" : "") + "info.xml"));
                        XmlDocument infoDoc = new XmlDocument();
                        infoDoc.LoadXml(addonXml);
                        var infoRoot = HTAlt.Tools.FindRoot(infoDoc);
                        string name = string.Empty; string codename = string.Empty; string desc = string.Empty;
                        List<string> applied = new List<string>();
                        for (int j = 0; j < infoRoot.ChildNodes.Count; j++)
                        {
                            var subnode = infoRoot.ChildNodes[j];
                            var _name = subnode.Name.ToLowerEnglish();
                            if (applied.Contains(_name))
                            {
                                if (!node.NodeIsComment())
                                {
                                    Yopad.Throw(this, "[" + Repository.CodeName + ":" + Name + "] Threw \"" + subnode.OuterXml + "\", duplicate property.", LibFoster.LogLevel.Warning);
                                }
                                break;
                            }
                            switch (_name)
                            {
                                case "name":
                                    name = subnode.InnerXml.XmlToString();
                                    break;

                                case "codename":
                                    codename = subnode.InnerXml.XmlToString();
                                    break;

                                case "desc":
                                    desc = subnode.InnerXml.XmlToString();
                                    break;

                                default:
                                    if (!node.NodeIsComment())
                                    {
                                        Yopad.Throw(this, "[" + Repository.CodeName + ":" + Name + "] Threw \"" + subnode.OuterXml + "\", invalid property.", LibFoster.LogLevel.Warning);
                                    }
                                    break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(codename))
                        {
                            cat.Addons.Add(new YopadAddon(cat, name, codename, desc, url, nsfw));
                        }
                        else
                        {
                            if (!node.NodeIsComment())
                            {
                                Yopad.Throw(this, "[" + Repository.CodeName + ":" + Name + "] Threw \"" + node.OuterXml + "\", missing information(s).", LibFoster.LogLevel.Warning);
                            }
                        }
                    }
                    else
                    {
                        if (!node.NodeIsComment())
                        {
                            Yopad.Throw(this, "[" + Repository.CodeName + ":" + Name + "] Threw \"" + node.OuterXml + "\", missing URL.", LibFoster.LogLevel.Warning);
                        }
                    }
                }
                else
                {
                    if (!node.NodeIsComment())
                    {
                        Yopad.Throw(this, "[" + Repository.CodeName + ":" + Name + "] Threw \"" + node.OuterXml + "\", invalid entry.", LibFoster.LogLevel.Warning);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Yopad Add-on Category.
    /// </summary>
    public class YopadCategory
    {
        public YopadCategory(YopadAddonList addonList, string name, string codeName, string description)
        {
            AddonList = addonList;
            Name = name;
            CodeName = codeName;
            Description = description;
        }

        /// <summary>
        /// The list of this category.
        /// </summary>
        public YopadAddonList AddonList { get; set; }

        /// <summary>
        /// The repository of this category.
        /// </summary>
        public YopadRepository Repository => AddonList.Repository;

        /// <summary>
        /// Service of this category.
        /// </summary>
        public Yopad Yopad => Repository.Yopad;

        /// <summary>
        /// Name of this category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Code name of the category.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Description of the category.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Add-ons inside this category.
        /// </summary>
        public List<YopadAddon> Addons { get; set; } = new List<YopadAddon>();
    }

    /// <summary>
    /// Yopad Add-on.
    /// </summary>
    public class YopadAddon
    {
        public YopadAddon(YopadCategory category, string name, string codeName, string description, string url, bool isNSFW)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CodeName = codeName ?? throw new ArgumentNullException(nameof(codeName));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            IsNSFW = isNSFW;
        }

        /// <summary>
        /// Category of this add-on.
        /// </summary>
        public YopadCategory Category { get; set; }

        /// <summary>
        /// Type of this add-on.
        /// </summary>
        public YopadAddonList AddonList => Category.AddonList;

        /// <summary>
        /// Repository of this add-on.
        /// </summary>
        public YopadRepository Repository => AddonList.Repository;

        /// <summary>
        /// Service of this add-on.
        /// </summary>
        public Yopad Yopad => Repository.Yopad;

        /// <summary>
        /// Name of the add-on.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Determines if the add-on is Not Suitable For Work.
        /// </summary>
        public bool IsNSFW { get; set; }

        /// <summary>
        /// Gets the installed version of this add-on (if it's installed). Returns -1 if not installed.
        /// </summary>
        public int InstalledVersion { get; set; } = -1;

        /// <summary>
        /// Code name of the add-on.
        /// </summary>
        public string CodeName { get; set; }

        /// <summary>
        /// Description of the add-on.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the base URL of the add-on.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Foster URL of the add-on.
        /// </summary>
        public string Foster_Url => AddonList.Url + (!AddonList.Url.EndsWith("\\") || !AddonList.Url.EndsWith("/") ? "\\" : "") + Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + ".foster";

        /// <summary>
        /// A string array containing all screenshots' URLs.
        /// </summary>
        public string[] ScreenshotUrl => new string[]
        {
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "0.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "1.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "2.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "3.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "4.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "5.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "6.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "7.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "8.png"),
            AddonList.Url.Replace("[0]", Url + (!Url.EndsWith("\\") || !Url.EndsWith("/") ? "\\" : "") + "9.png"),
        };

        /// <summary>
        /// The Add-on that associated with this Yopad add-on.
        /// </summary>
        public object AssocAddon { get; set; }

        /// <summary>
        /// The <see cref="HTUPDATE"/> of this add-on.
        /// </summary>
        public Foster AssocFoster { get; set; }

        /// <summary>
        /// Determines if this Add-on is already installer or not.
        /// </summary>
        public bool isInstalled { get; set; }
    }
}