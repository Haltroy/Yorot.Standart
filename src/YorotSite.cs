using System;

namespace Yorot
{
    /// <summary>
    /// Site class used by History and Download managers.
    /// </summary>
    public class YorotSite
    {
        #region Ugly Stuff

        private string name;
        private string url;
        private float downloadPercentage;
        private string filePath;
        private YorotSiteStatus status;
        private object engineDownloadObject;
        private string errorCode;
        private DateTime date;
        private bool showMessageBoxes = true;
        private YorotSitePermissions permissions;
        private YorotSiteType type;

        #endregion Ugly Stuff

        /// <summary>
        /// Manager of this site.
        /// </summary>
        public YorotManager Manager { get; set; }

        /// <summary>
        /// Name of the site.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                name = value;
            }
        }

        /// <summary>
        /// URI of the site.
        /// </summary>
        public string Url
        {
            get => url;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                url = value;
            }
        }

        /// <summary>
        /// The Percentage of download.
        /// </summary>
        public float DownloadPercentage
        {
            get => downloadPercentage;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                downloadPercentage = value;
            }
        }

        /// <summary>
        /// Location on drive of site (probably Download manager site).
        /// </summary>
        public string FilePath
        {
            get => filePath;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                filePath = value;
            }
        }

        /// <summary>
        /// Status of the site (probably Download Manager site).
        /// </summary>
        public YorotSiteStatus Status
        {
            get => status;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                status = value;
            }
        }

        /// <summary>
        /// Actual download object provided by the web engine.
        /// </summary>
        public object EngineDownloadObject
        {
            get => engineDownloadObject;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                engineDownloadObject = value;
            }
        }

        /// <summary>
        /// Error code of site (probably Download manager site).
        /// </summary>
        public string ErrorCode
        {
            get => errorCode;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                errorCode = value;
            }
        }

        /// <summary>
        /// Date of the site (probably History site).
        /// </summary>
        public DateTime Date
        {
            get => date;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                date = value;
            }
        }

        /// <summary>
        /// Determines if this site can show JS message boxes or not.
        /// </summary>
        public bool ShowMessageBoxes
        {
            get => showMessageBoxes;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                showMessageBoxes = value;
            }
        }

        /// <summary>
        /// Permissions of this site.
        /// </summary>
        public YorotSitePermissions Permissions
        {
            get => permissions;
            set
            {
                switch (Type)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                permissions = value;
            }
        }

        /// <summary>
        /// Determines if this site is used a site or a download.
        /// </summary>
        public YorotSiteType Type
        {
            get => type;
            set
            {
                switch (value)
                {
                    case YorotSiteType.Site:
                        Manager.Main.OnSiteChange(this);
                        break;

                    case YorotSiteType.Download:
                        Manager.Main.OnDownloadChange(this);
                        break;
                }
                type = value;
            }
        }

        public enum YorotSiteType
        {
            /// <summary>
            /// This YorotSite is used as a site.
            /// </summary>
            Site,

            /// <summary>
            /// This YorotSite is used as a download.
            /// </summary>
            Download
        }

        /// <summary>
        /// Permissions of a generic <see cref="YorotSite"/>.
        /// </summary>
        public class YorotSitePermissions
        {
            /// <summary>
            /// Creates a new <see cref="YorotSitePermissions"/> with default values.
            /// </summary>
            /// <param name="site"><see cref="YorotSite"/></param>
            public YorotSitePermissions(YorotSite site) : this(site, YorotPermissionMode.None, YorotPermissionMode.None, YorotPermissionMode.None, YorotPermissionMode.Allow, 0, false, YorotPermissionMode.None, YorotPermissionMode.None) { }

            /// <summary>
            /// Creates a new <see cref="YorotSitePermissions"/> with custom values.
            /// </summary>
            /// <param name="site"><see cref="YorotSite"/></param>
            /// <param name="allowMic">Determines if the microphone can be used by this site.</param>
            /// <param name="allowCam">Determines if the camera can be used by this site.</param>
            /// <param name="allowNotif">Determines if this site can send notifications.</param>
            /// <param name="notifPriority">Determines the priority of the notifications coming from this website.
            /// <para></para>
            /// -1 = Prioritize others
            /// <para></para>
            /// 0 = Normal
            /// <para></para>
            /// 1 = Prioritize this</param>
            /// <param name="startNotifOnBoot">Determines if Yorot should start notification listener on start for this site.</param>
            /// <param name="allowWE">Determines if this website can use Web Engines.</param>
            /// <param name="allowYS">Determines if this site can access Yorot Special.</param>
            public YorotSitePermissions(YorotSite site, YorotPermissionMode allowMic = YorotPermissionMode.Deny, YorotPermissionMode allowCam = YorotPermissionMode.Deny, YorotPermissionMode allowNotif = YorotPermissionMode.Deny, YorotPermissionMode allowCookies = YorotPermissionMode.Allow, int notifPriority = 0, bool startNotifOnBoot = false, YorotPermissionMode allowPopup = YorotPermissionMode.Deny, YorotPermissionMode allowYS = YorotPermissionMode.Deny)
            {
                Site = site;
                this.allowMic = new YorotPermission("allowMic", site, site.Manager.Main, allowMic);
                this.allowCam = new YorotPermission("allowCam", site, site.Manager.Main, allowCam);
                this.allowNotif = new YorotPermission("allowNotif", site, site.Manager.Main, allowNotif);
                this.allowCookies = new YorotPermission("allowCookies", site, site.Manager.Main, allowCookies);
                this.notifPriority = notifPriority;
                this.startNotifOnBoot = startNotifOnBoot;
                this.allowPopup = new YorotPermission("allowPopup", site, site.Manager.Main, allowPopup);
                this.allowYS = new YorotPermission("allowYS", site, site.Manager.Main, allowYS);
            }

            /// <summary>
            /// The site of these permissions.
            /// </summary>
            public YorotSite Site { get; set; }

            /// <summary>
            /// Determines if the microphone can be used by this site.
            /// </summary>
            public YorotPermission allowMic { get; set; }

            /// <summary>
            /// Determines if the camera can be used by this site.
            /// </summary>
            public YorotPermission allowCam { get; set; }

            /// <summary>
            /// Determines if this site can use cookies.
            /// </summary>
            public YorotPermission allowCookies { get; set; }

            /// <summary>
            /// Determines if this site can send notifications.
            /// </summary>
            public YorotPermission allowNotif { get; set; }

            /// <summary>
            /// Determines the priority of the notifications coming from this website.
            /// <para></para>
            /// -1 = Prioritize others
            /// <para></para>
            /// 0 = Normal
            /// <para></para>
            /// 1 = Prioritize this
            /// </summary>
            public int notifPriority { get; set; } = 0;

            /// <summary>
            /// Determines if Yorot should start notification listener on start for this site.
            /// </summary>
            public bool startNotifOnBoot { get; set; } = false;

            /// <summary>
            /// Determines if this website can use Pop-ups.
            /// </summary>
            public YorotPermission allowPopup { get; set; }

            /// <summary>
            /// Determines if this site can access Yorot Special.
            /// </summary>
            public YorotPermission allowYS { get; set; }
        }

        /// <summary>
        /// <see cref="Enum"/> used by YorotSites in Download manager.
        /// </summary>
        public enum YorotSiteStatus
        {
            Finished,
            Error,
            Cancelled,
        }
    }
}