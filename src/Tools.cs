using System;
using System.Drawing;
using System.IO;

namespace Yorot
{
    /// <summary>
    /// Public tools that are used by Yorot.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Determines if we passed a date.
        /// </summary>
        /// <param name="date"><see cref="DateTime"/></param>
        /// <returns><see cref="bool"/></returns>
        public static bool HasExpired(this DateTime date) => DateTime.Now.CompareTo(date.Add(new TimeSpan(2, 0, 0))) > 0;

        /// <summary>
        /// Uploads a file to server
        /// </summary>
        /// <param name="url">Address of the server.</param>
        /// <param name="filePath">Path of the file that is going to be sent.</param>
        /// <param name="username">FTP Username</param>
        /// <param name="password">FTP Password</param>
        public static void UploadFileToFtp(string url, string filePath, string username, string password)
        {
            string fileName = System.IO.Path.GetFileName(filePath);
            System.Net.FtpWebRequest request = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(url + fileName);

            request.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new System.Net.NetworkCredential(username, password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            using (System.IO.FileStream fileStream = System.IO.File.OpenRead(filePath))
            {
                using (System.IO.Stream requestStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(requestStream);
                    requestStream.Close();
                }
            }

            System.Net.FtpWebResponse response = (System.Net.FtpWebResponse)request.GetResponse();
            Console.WriteLine("Upload done: {0}", response.StatusDescription);
            response.Close();
        }

        /// <summary>
        /// Shortens the path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="main"><see cref="YorotMain"/></param>
        /// <returns><see cref="string"/></returns>
        public static string ShortenPath(this string path, YorotMain main)
        {
            return string.IsNullOrWhiteSpace(path)
                ? path
                : path.Replace(main.ProfilesFolder, "[PROFILES]")
                .Replace(main.ExtFolder, "[USEREXT]")
                .Replace(main.LangFolder, "[USERLANG]")
                .Replace(main.AppsFolder, "[USERAPPS]")
                .Replace(main.ThemesFolder, "[USERTHEME]")
                .Replace(main.Profiles.Current.CacheLoc, "[USERCACHE]")
                .Replace(main.LogFolder, "[LOGS]")
                .Replace(main.Profiles.Current.Path, "[USER]")
                .Replace(main.AppPath, "[APPPATH]");
        }

        /// <summary>
        /// Unshortens the path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="main"><see cref="YorotMain"/></param>
        /// <returns><see cref="string"/></returns>
        public static string GetPath(this string path, YorotMain main)
        {
            return path.Replace("[PROFILES]", main.ProfilesFolder)
                .Replace("[USEREXT]", main.ExtFolder)
                .Replace("[USERLANG]", main.LangFolder)
                .Replace("[USERAPPS]", main.AppsFolder)
                .Replace("[USERTHEME]", main.ThemesFolder)
                //.Replace("[USERCACHE]", main.Profiles.Current.CacheLoc) // Disabled for now.
                .Replace("[LOGS]", main.LogFolder)
                //.Replace("[USER]", main.Profiles.Current.Path) // Disabled for now.
                .Replace("[APPPATH]", main.AppPath);
        }

        /// <summary>
        /// Returns either <paramref name="black"/> or <paramref name="white"/> by determining with the brightess of <paramref name="color"/>.
        /// </summary>
        /// <param name="color">Color for determining.</param>
        /// <param name="white">White/Bright object to return.</param>
        /// <param name="black">Black/Dark object  to return</param>
        /// <param name="reverse"><sse cref=true"/> to return <paramref name="black"/> on black/dark object and <paramref name="white"/> for white/bright object, otherwise <sse cref=false"/>.</param>
        /// <returns><paramref name="black"/> or <paramref name="white"/>.</returns>
        public static object SelectObjectFromColor(this YorotColor color, ref object white, ref object black, bool reverse = false)
        {
            return color.IsBright ? (reverse ? white : black) : (reverse ? black : white);
        }
    }
}