using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HTAlt;

namespace Yorot
{
    public class SessionManager : YorotManager
    {
        public SessionManager(string configFile, YorotMain main) : base(configFile, main)
        {
            Autosave();
        }

        /// <summary>
        /// Determines the autosave interval in milliseconds.
        /// </summary>
        public int AutosaveTimer { get; set; } = 300000;

        public override void ExtractXml(XmlNode rootNode)
        {
            List<string> appliedSettings = new List<string>();
            for (int i = 0; i < rootNode.ChildNodes.Count; i++)
            {
                XmlNode node = rootNode.ChildNodes[i];
                if (appliedSettings.Contains(node.Name.ToLowerEnglish()))
                {
                    Output.WriteLine("[SessionMan] Threw away \"" + node.OuterXml + "\", configuration already applied.", LogLevel.Warning);
                    break;
                }
                appliedSettings.Add(node.Name.ToLowerEnglish());
                switch (node.Name.ToLowerEnglish())
                {
                    case "sessions":
                        for (int ı = 0; ı < node.ChildNodes.Count; ı++)
                        {
                            XmlNode subnode = node.ChildNodes[ı];
                            if (subnode.Name.ToLowerEnglish() == "session")
                            {
                                var site = new YorotSite() { Manager = this };
                                for (int _i = 0; _i < subnode.ChildNodes.Count; _i++)
                                {
                                    var siteNode = subnode.ChildNodes[_i];
                                    Systems.Add(new SessionSystem(this, siteNode));
                                }
                            }
                            else
                            {
                                Output.WriteLine("[SessionMan] Threw away \"" + subnode.OuterXml + "\", unsupported.", LogLevel.Warning);
                            }
                        }
                        break;

                    case "autosave":
                        if (node.InnerXml.XmlToString() == "true")
                        {
                            PreviousShutdownWasSafe = false;
                        }
                        break;

                    case "interval":
                        AutosaveTimer = int.Parse(node.InnerXml.XmlToString());
                        break;

                    default:
                        if (!node.NodeIsComment())
                        {
                            Output.WriteLine("[SessionMan] Threw away \"" + node.OuterXml + "\", invalid configuration.", LogLevel.Warning);
                        }
                        break;
                }
            }
        }

        public override string ToXml()
        {
            string x = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
    "<root>" + Environment.NewLine +
    "<!-- Yorot Session Config File" + Environment.NewLine + Environment.NewLine +
    "This file is used to save site settings." + Environment.NewLine +
    "Editing this file might cause problems with Yorot sessions." + Environment.NewLine +
    "-->" + Environment.NewLine +
    "<AutoSave>" + (_autosave ? "true" : "false") + "</AutoSave>" + Environment.NewLine +
    "<Interval>" + AutosaveTimer + "</Interval>" + Environment.NewLine +
    "<Sessions>" + Environment.NewLine;
            for (int i = 0; i < Systems.Count; i++)
            {
                SessionSystem site = Systems[i];
                x += site.XmlOut();
            }
            x += "</Sessions>" + Environment.NewLine + "</root>";

            return x.BeautifyXML();
        }

        public SessionSystem GenerateNew(string xml = null)
        {
            SessionSystem system = new SessionSystem(this, xml);
            Systems.Add(system);
            return system;
        }

        public void Close(SessionSystem session)
        {
            if (Systems.Contains(session))
            {
                Systems.Remove(session);
            }
            if (!ClosedSystems.Contains(session))
            {
                ClosedSystems.Add(session);
            }
        }

        public void Open(SessionSystem session)
        {
            if (ClosedSystems.Contains(session))
            {
                ClosedSystems.Remove(session);
            }
            if (!Systems.Contains(session))
            {
                Systems.Add(session);
            }
        }

        public List<SessionSystem> Systems { get; set; } = new List<SessionSystem>();
        public List<SessionSystem> ClosedSystems { get; set; } = new List<SessionSystem>();

        /// <summary>
        /// Determines if the previous shutdow was safely done. If not, then you can ask user if they want to restore the last session.
        /// </summary>
        public bool PreviousShutdownWasSafe { get; set; } = true;

        private bool _autosave = true;
        private bool _stop = false;

        public void StopAutoSave()
        { _stop = true; }

        public async void Autosave()
        {
            await Task.Run(async () =>
            {
                if (!_stop)
                {
                    System.Threading.Thread.Sleep(AutosaveTimer);
                    Save();
                    await Task.Run(() => { Autosave(); });
                }
            });
        }

        public void Shutdown()
        {
            _autosave = false;
            Save();
        }
    }

    public class SessionSystem
    {
        public SessionSystem(SessionManager man, string XMLCode)
        {
            if (man == null) { throw new ArgumentNullException(nameof(man)); }
            Manager = man;
            if (!string.IsNullOrWhiteSpace(XMLCode))
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(XMLCode);
                XmlNode workNode = document.DocumentElement;
                if (workNode.Attributes["Index"] != null && workNode.Attributes["IsDead"] != null && workNode.Attributes["Date"] != null)
                {
                    int si = Convert.ToInt32(workNode.Attributes["Index"].Value);
                    IsDead = workNode.Attributes["IsDead"].Value == "true";
                    Date = YorotDateAndTime.DMY.ShortToDateTime(workNode.Attributes["Date"].Value);
                    foreach (XmlNode node in workNode.ChildNodes)
                    {
                        if (node.Name.ToLower() == "sessionsite")
                        {
                            if (node.Attributes["Url"] != null && node.Attributes["Title"] != null)
                            {
                                Add(new Session(node.Attributes["Url"].Value, node.Attributes["Title"].Value));
                            }
                        }
                    }
                    SelectedIndex = si;
                    SelectedSession = this[si];
                }
            }
        }

        public SessionSystem(SessionManager man, XmlNode workNode)
        {
            if (man == null) { throw new ArgumentNullException(nameof(man)); }
            if (workNode == null) { throw new ArgumentNullException(nameof(workNode)); }
            if (workNode.Attributes["Index"] != null && workNode.Attributes["IsDead"] != null && workNode.Attributes["Date"] != null)
            {
                int si = Convert.ToInt32(workNode.Attributes["Index"].Value);
                IsDead = workNode.Attributes["IsDead"].Value == "true";
                Date = Manager.Main.CurrentSettings.DateFormat.ShortToDateTime(workNode.Attributes["Date"].Value);
                foreach (XmlNode node in workNode.ChildNodes)
                {
                    if (node.Name.ToLower() == "sessionsite")
                    {
                        if (node.Attributes["Url"] != null && node.Attributes["Title"] != null)
                        {
                            Add(new Session(node.Attributes["Url"].Value, node.Attributes["Tİtle"].Value));
                        }
                    }
                }
                SelectedIndex = si;
                SelectedSession = this[si];
            }
        }

        public SessionSystem(SessionManager man) : this(man, "")
        {
        }

        public DateTime? Date { get; set; }
        public SessionManager Manager { get; set; }
        public bool IsDead { get; set; } = false;
        public int Count => _Sessions.Count;

        public Session this[int i]
        {
            get
            {
                return _Sessions[i];
            }
        }

        private List<Session> _Sessions = new List<Session>();

        public string XmlOut()
        {
            string x = $"<Session Index=\"{SelectedIndex}\" IsDead=\"{(IsDead ? "true" : "false")}\" Date=\"{YorotDateAndTime.DMY.GetShortName(Date ?? DateTime.Today)}\" >{Environment.NewLine}";
            for (int i = 0; i < Count; i++)
            {
                x += $"<SessionSite Url=\"{this[i].Url}\" Title=\"{this[i].Title}\" />{Environment.NewLine}";
            }
            return x + "</Session>";
        }

        public bool SkipAdd = false;

        public void GoBack()
        {
            if (CanGoBack)
            {
                SkipAdd = true;
                SelectedIndex -= 1;
                SelectedSession = this[SelectedIndex];
                LoadPageReal(SelectedSession.Url);
            }
        }

        public void GoForward()
        {
            if (CanGoForward)
            {
                SkipAdd = true;
                SelectedIndex += 1;
                SelectedSession = this[SelectedIndex];
                LoadPageReal(SelectedSession.Url);
            }
        }

        public Session SelectedSession { get; set; }
        public int SelectedIndex { get; set; }

        public void MoveTo(int i)
        {
            if (i >= 0 && i < this.Count)
            {
                SkipAdd = true;
                SelectedIndex = i;
                SelectedSession = this[i];
                LoadPageReal(SelectedSession.Url);
            }
            else
            {
                throw new ArgumentOutOfRangeException("\"i\" was bigger than Sessions.Count or smaller than 0. [i=\"" + i + "\" Count=\"" + this.Count + "\"]");
            }
        }

        private List<LoadPageDelegate> delegates = new List<LoadPageDelegate>();

        private event LoadPageDelegate LoadPageReal;

        public delegate void LoadPageDelegate(string url);

        public event LoadPageDelegate LoadPage
        {
            add
            {
                LoadPageReal += value;
                delegates.Add(value);
            }
            remove
            {
                LoadPageReal -= value;
                delegates.Remove(value);
            }
        }

        public void RemoveAllEvents()
        {
            foreach (LoadPageDelegate eh in delegates)
            {
                LoadPageReal -= eh;
            }
            delegates.Clear();
        }

        public void Remove(Session session) => _Sessions.Remove(session);

        public void RemoveAt(int index) => _Sessions.RemoveAt(index);

        public int IndexOf(Session session) => _Sessions.IndexOf(session);

        public bool Contains(Session session) => _Sessions.Contains(session);

        public void Add(string url, string title)
        {
            Add(new Session(url, title));
        }

        public void Add(Session Session)
        {
            if (Session is null)
            {
                throw new ArgumentNullException("\"Session\" was null.");
            }
            if (Manager.Main.GetWebSource(Session.Url) is YorotBrowserWebSource websrc && websrc.IgnoreOnSessionList)
            {
                return;
            }
            if (SkipAdd) { SkipAdd = false; return; }
            if (CanGoForward && SelectedIndex + 1 < this.Count)
            {
                if (!Session.Equals(this[SelectedIndex]))
                {
                    Console.WriteLine("Session Not Equal: 1:" + Session.Url + " 2:" + this[SelectedIndex].Url);
                    Session[] RemoveThese = After();
                    for (int i = 0; i < RemoveThese.Length; i++)
                    {
                        this.Remove(RemoveThese[i]);
                    }
                    if (this.Count > 0)
                    {
                        if (this[this.Count - 1].Url != Session.Url)
                        {
                            _Sessions.Add(Session);
                        }
                    }
                    else
                    {
                        _Sessions.Add(Session);
                    }
                }
            }
            else
            {
                if (this.Count > 0)
                {
                    if (this[this.Count - 1].Url != Session.Url)
                    {
                        _Sessions.Add(Session);
                    }
                }
                else
                {
                    _Sessions.Add(Session);
                }
            }
            if (this.Count > 0)
            {
                if (this[this.Count - 1].Url != Session.Url)
                {
                    SelectedSession = Session;
                    SelectedIndex = this.IndexOf(Session);
                }
                else
                {
                    SelectedSession = this[this.Count - 1];
                    SelectedIndex = this.Count - 1;
                }
            }
            else
            {
                _Sessions.Add(Session);
            }
        }

        public bool CanGoBack
        {
            get
            {
                return SessionCanGoBack(SelectedSession);
            }
        }

        public bool SessionCanGoBack(Session Session)
        {
            if (Session is null)
            {
                return false;
            }
            if (!Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }
            int current = this.IndexOf(Session);
            return current > 0;
        }

        public bool CanGoForward
        {
            get
            {
                return SessionCanGoForward(SelectedSession);
            }
        }

        public bool SessionCanGoForward(Session Session)
        {
            if (Session is null)
            {
                return false;
            }
            if (!this.Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }

            int current = this.IndexOf(Session) + 1;
            return current < this.Count;
        }

        public Session[] Before()
        {
            return Before(SelectedSession);
        }

        public Session[] Before(Session Session)
        {
            if (Session is null)
            {
                return new Session[] { };
            }
            if (!this.Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }
            int current = this.IndexOf(Session);
            List<Session> fs = new List<Session>();
            for (int i = 0; i < current; i++)
            {
                fs.Add(this[i]);
            }
            return fs.ToArray();
        }

        public Session[] After()
        {
            return After(SelectedSession);
        }

        public Session[] After(Session Session)
        {
            if (Session is null)
            {
                return new Session[] { };
            }
            if (!this.Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }
            int current = this.IndexOf(Session) + 1;
            List<Session> fs = new List<Session>();
            for (int i = current; i < this.Count; i++)
            {
                fs.Add(this[i]);
            }
            return fs.ToArray();
        }
    }

    public class Session
    {
        public override bool Equals(object obj)
        {
            return obj is Session session && Url == session.Url;
        }

        public override int GetHashCode()
        {
            return -1915121810 + EqualityComparer<string>.Default.GetHashCode(Url);
        }

        public Session(string _Url, string _Title)
        {
            Url = _Url;
            Title = _Title;
        }

        public Session() : this("", "")
        {
        }

        public Session(string _Url) : this(_Url, _Url)
        {
        }

        public string Url { get; set; }
        public string Title { get; set; }
    }
}