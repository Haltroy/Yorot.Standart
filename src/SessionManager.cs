using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Yorot
{
    public class SessionSystem
    {
        public SessionSystem(YorotMain main, string XMLCode)
        {
            if (main == null) { throw new ArgumentNullException(nameof(main)); }
            Main = main;
            if (!string.IsNullOrWhiteSpace(XMLCode))
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(XMLCode);
                XmlNode workNode = document.FirstChild;
                if (document.FirstChild.Name.ToLower() == "xml") { workNode = document.FirstChild.NextSibling; }
                if (workNode.Attributes["Index"] != null)
                {
                    int si = Convert.ToInt32(workNode.Attributes["Index"].Value);
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

        public SessionSystem(YorotMain main) : this(main, "")
        {
        }

        public YorotMain Main { get; set; }

        private List<Session> _Sessions = new List<Session>();

        public string XmlOut()
        {
            string x = "<Session Index=\"" + SelectedIndex + "\" >" + Environment.NewLine;
            for (int i = 0; i < Sessions.Count; i++)
            {
                x += "<SessionSite Url=\"" + Sessions[i].Url + "\" Title=\"" + Sessions[i].Title + "\" >" + Environment.NewLine;
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
            if (Main.GetWebSource(Session.Url) is YorotBrowserWebSource websrc && websrc.IgnoreOnSessionList)
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