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
        public int AutosaveTimer { get; set; } = 5000;

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
            return (x + "</Sessions>" + Environment.NewLine + "</root>").BeautifyXML();
        }

        public SessionSystem GenerateNew(string xml = null)
        {
            SessionSystem system = new SessionSystem(this, xml);
            Systems.Add(system);
            return system;
        }

        public List<SessionSystem> Systems { get; set; } = new List<SessionSystem>();

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
                                Sessions.Add(new Session(node.Attributes["Url"].Value, node.Attributes["Tİtle"].Value));
                            }
                        }
                    }
                    SelectedIndex = si;
                    SelectedSession = Sessions[si];
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
                            Sessions.Add(new Session(node.Attributes["Url"].Value, node.Attributes["Tİtle"].Value));
                        }
                    }
                }
                SelectedIndex = si;
                SelectedSession = Sessions[si];
            }
        }

        public SessionSystem(SessionManager man) : this(man, "")
        {
        }

        public DateTime? Date { get; set; }
        public SessionManager Manager { get; set; }
        public bool IsDead { get; set; } = false;

        private List<Session> _Sessions = new List<Session>();

        public string XmlOut()
        {
            string x = $"<Session Index=\"{SelectedIndex}\" IsDead=\"{(IsDead ? "true" : "false")}\" Date=\"{YorotDateAndTime.DMY.GetShortName(Date ?? DateTime.Today)}\" >{Environment.NewLine}";
            for (int i = 0; i < Sessions.Count; i++)
            {
                x += $"<SessionSite Url=\"{Sessions[i].Url}\" Title=\"{Sessions[i].Title}\" />{Environment.NewLine}";
            }
            return x + "</Session>";
        }

        public List<Session> Sessions
        {
            get => _Sessions;
            set => _Sessions = value;
        }

        public bool SkipAdd = false;

        public void GoBack()
        {
            if (CanGoBack)
            {
                SkipAdd = true;
                SelectedIndex -= 1;
                SelectedSession = Sessions[SelectedIndex];
                LoadPage(SelectedSession.Url);
            }
        }

        public void GoForward()
        {
            if (CanGoForward)
            {
                SkipAdd = true;
                SelectedIndex += 1;
                SelectedSession = Sessions[SelectedIndex];
                LoadPage(SelectedSession.Url);
            }
        }

        public Session SessionInIndex(int Index)
        {
            return Sessions[Index];
        }

        public Session SelectedSession { get; set; }
        public int SelectedIndex { get; set; }

        public void MoveTo(int i)
        {
            if (i >= 0 && i < Sessions.Count)
            {
                SkipAdd = true;
                SelectedIndex = i;
                SelectedSession = Sessions[i];
                LoadPage(SelectedSession.Url);
            }
            else
            {
                throw new ArgumentOutOfRangeException("\"i\" was bigger than Sessions.Count or smaller than 0. [i=\"" + i + "\" Count=\"" + Sessions.Count + "\"]");
            }
        }

        public delegate void LoadPageDelegate(string url);

        public event LoadPageDelegate LoadPage;

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
            if (CanGoForward && SelectedIndex + 1 < Sessions.Count)
            {
                if (!Session.Equals(Sessions[SelectedIndex]))
                {
                    Console.WriteLine("Session Not Equal: 1:" + Session.Url + " 2:" + Sessions[SelectedIndex].Url);
                    Session[] RemoveThese = After();
                    for (int i = 0; i < RemoveThese.Length; i++)
                    {
                        Sessions.Remove(RemoveThese[i]);
                    }
                    if (Sessions.Count > 0)
                    {
                        if (Sessions[Sessions.Count - 1].Url != Session.Url)
                        {
                            Sessions.Add(Session);
                        }
                    }
                    else
                    {
                        Sessions.Add(Session);
                    }
                }
            }
            else
            {
                if (Sessions.Count > 0)
                {
                    if (Sessions[Sessions.Count - 1].Url != Session.Url)
                    {
                        Sessions.Add(Session);
                    }
                }
                else
                {
                    Sessions.Add(Session);
                }
            }
            if (Sessions.Count > 0)
            {
                if (Sessions[Sessions.Count - 1].Url != Session.Url)
                {
                    SelectedSession = Session;
                    SelectedIndex = Sessions.IndexOf(Session);
                }
                else
                {
                    SelectedSession = Sessions[Sessions.Count - 1];
                    SelectedIndex = Sessions.Count - 1;
                }
            }
            else
            {
                Sessions.Add(Session);
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
            if (!Sessions.Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }
            int current = Sessions.IndexOf(Session);
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
            if (!Sessions.Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }

            int current = Sessions.IndexOf(Session) + 1;
            return current < Sessions.Count;
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
            if (!Sessions.Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }
            int current = Sessions.IndexOf(Session);
            List<Session> fs = new List<Session>();
            for (int i = 0; i < current; i++)
            {
                fs.Add(Sessions[i]);
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
            if (!Sessions.Contains(Session))
            {
                throw new ArgumentOutOfRangeException("Cannot find Session[Url=\"" + (Session.Url == null ? "null" : Session.Url) + "\" Title=\"" + (Session.Title == null ? "null" : Session.Title) + "\"].");
            }
            int current = Sessions.IndexOf(Session) + 1;
            List<Session> fs = new List<Session>();
            for (int i = current; i < Sessions.Count; i++)
            {
                fs.Add(Sessions[i]);
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