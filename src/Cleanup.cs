using System;
using System.Collections.Generic;
using System.Text;

namespace Yorot
{
    /// <summary>
    /// Yorot Cleanup Service.
    /// </summary>
    public class Cleanup
    {
        /// <summary>
        /// The <see cref="YorotMain"/>.
        /// </summary>
        public YorotMain Main { get; set; }

        /// <summary>
        /// Creates a new Cleanup Service.
        /// </summary>
        /// <param name="main"><see cref="YorotMain"/></param>
        public Cleanup(YorotMain main)
        {
            Main = main;
        }

        /// <summary>
        /// Period of each history, log and download cleanup. <c>-1</c> for no cleanup.
        /// </summary>
        public int CleanupHistory { get; set; } = -1;

        /// <summary>
        /// period of each add-on cleanup. <c>-1</c> for no cleanup.
        /// </summary>
        public int CleanupAddon { get; set; } = -1;

        /// <summary>
        /// Date of the last history, log and download cleanup.
        /// </summary>
        public System.DateTime HistoryLastClear { get; set; } = DateTime.Now;

        /// <summary>
        /// Date of the last add-on cleanup.
        /// </summary>
        public System.DateTime AddonLastClear { get; set; } = DateTime.Now;

        /// <summary>
        /// Checks if a history, log and download cleanup should be done or not.
        /// </summary>
        public bool ClearHistory => CleanupHistory != -1 ? DateTime.Compare(HistoryLastClear.AddDays(CleanupHistory), DateTime.Now) >= 0 : false;

        /// <summary>
        /// Checks if an add-on cleanup should be done or not.
        /// </summary>
        public bool ClearAddon => CleanupAddon != -1 ? DateTime.Compare(AddonLastClear.AddDays(CleanupAddon), DateTime.Now) >= 0 : false;

        /// <summary>
        /// Performs cleanup.
        /// </summary>
        /// <param name="force">Force to cleanup regardless of the next date.</param>
        public void PerformCleanup(bool force = false)
        {
            if (force || ClearHistory)
            {
                Main.CurrentSettings.SessionManager.Systems.Clear();
                Main.CurrentSettings.DownloadManager.Downloads.Clear();
                var files = System.IO.Directory.GetFiles(Main.LogFolder);
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        System.IO.File.Delete(files[i]);
                    }
                    catch (Exception) { } // ignored
                }
                HistoryLastClear = DateTime.Now;
            }
            if (force || ClearAddon)
            {
                // TODO: Remove addons that are not touched by either user, system or another addon
                AddonLastClear = DateTime.Now;
            }
        }
    }
}