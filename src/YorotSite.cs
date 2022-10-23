using System;

namespace Yorot
{
    /// <summary>
    /// Site class used by History and Download managers.
    /// </summary>
    public class YorotSite
    {
        /// <summary>
        /// Manager of this site.
        /// </summary>
        public YorotManager Manager { get; set; }

        /// <summary>
        /// Name of the site.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URI of the site.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The Percentage of download.
        /// </summary>
        public float DownloadPercentage { get; set; }

        /// <summary>
        /// Location on drive of site (probably Download manager site).
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Status of the site (probably Download Manager site).
        /// </summary>
        public YorotSiteStatus Status { get; set; }

        /// <summary>
        /// Actual download object provided by the web engine.
        /// </summary>
        public object EngineDownloadObject { get; set; }

        /// <summary>
        /// Error code of site (probably Download manager site).
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Date of the site (probably History site).
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Determines if this site can show JS message boxes or not.
        /// </summary>
        public bool ShowMessageBoxes { get; set; } = true;

        /// <summary>
        /// Permissions of this site.
        /// </summary>
        public YorotSitePermissions Permissions { get; set; }
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
        public YorotSitePermissions(YorotSite site) : this(site, YorotPermissionMode.None, YorotPermissionMode.None, YorotPermissionMode.None, 0, false, YorotPermissionMode.None, YorotPermissionMode.None) { }

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
        public YorotSitePermissions(YorotSite site, YorotPermissionMode allowMic, YorotPermissionMode allowCam, YorotPermissionMode allowNotif, int notifPriority, bool startNotifOnBoot, YorotPermissionMode allowPopup, YorotPermissionMode allowYS)
        {
            Site = site;
            this.allowMic = new YorotPermission("allowMic", site, site.Manager.Main, allowMic);
            this.allowCam = new YorotPermission("allowCam", site, site.Manager.Main, allowCam);
            this.allowNotif = new YorotPermission("allowNotif", site, site.Manager.Main, allowNotif);
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