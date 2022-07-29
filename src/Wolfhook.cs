using HTAlt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yorot
{
    /// <summary>
    /// Hooks other instances to itself.
    /// </summary>
    public class Wolfhook
    {
        /// <summary>
        /// Location of Wolves.
        /// </summary>
        public string WhFolder = "";

        /// <summary>
        /// Creates a new Wolfhook.
        /// </summary>
        /// <param name="whFolder">Location of the wolves.</param>
        /// <param name="defaultEncoding"><see cref="System.Text.Encoding"/></param>
        /// <param name="timeout">The timeout between each search.</param>
        /// <param name="logWork">Logs messages when working.</param>
        public Wolfhook(string whFolder, Encoding defaultEncoding, int timeout, bool logWork)
        {
            WhFolder = whFolder ?? throw new ArgumentNullException(nameof(whFolder));
            DefaultEncoding = defaultEncoding ?? throw new ArgumentNullException(nameof(defaultEncoding));
            Timeout = timeout;
            LogWork = logWork;
        }

        /// <summary>
        /// Creates a new Wolfhook.
        /// </summary>
        /// <param name="whFolder">Location of the wolves.</param>
        public Wolfhook(string whFolder) : this(whFolder, System.Text.Encoding.Unicode, 5000, false)
        {
        }

        /// <summary>
        /// Determines the <see cref="Encoding"/> of Wolves.
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.Unicode;

        /// <summary>
        /// Determines the timeout between each search.
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// Logs messages when working.
        /// </summary>
        public bool LogWork { get; set; } = false;

        /// <summary>
        /// List of fetched wolves.
        /// </summary>
        public List<string> Wolves { get; set; } = new List<string>();

        public void SendWolf(string message, string id = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = HTAlt.Tools.GenerateRandomText(17);
            }
            if (LogWork) { Output.WriteLine("<WOLFHOOK> Created message=\"" + message + "\" from ID=\"" + id + "\" without error(s).", LogLevel.Info); }
            message.WriteToFile(WhFolder + id + ".wh", DefaultEncoding);
        }

        private bool StopTask { get; set; } = false;

        /// <summary>
        /// Starts searching.
        /// </summary>
        public void StartSearch()
        {
            StopTask = false;
            Task.Run(() => SearchForWolves());
        }

        /// <summary>
        /// Stops before starting a new search in next search.
        /// </summary>
        public void StopSearch()
        {
            StopTask = true;
        }

        private async void SearchForWolves()
        {
            if (StopTask)
            {
                return;
            }
            await Task.Run(() =>
            {
                string[] whFiles = Directory.GetFiles(WhFolder, "*.wh", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < whFiles.Length; i++)
                {
                    string message = HTAlt.Tools.ReadFile(whFiles[i], DefaultEncoding);
                    string id = Path.GetFileNameWithoutExtension(whFiles[i]);
                    Wolves.Add(message);
                    if (LogWork)
                    {
                        Output.WriteLine("<WOLFHOOK> Received message=\"" + message + "\" from ID=\"" + id + "\".", LogLevel.Info);
                    }
                    File.Delete(whFiles[i]);
                }
                Thread.Sleep(Timeout);
                Task.Run(() => SearchForWolves());
            });
        }
    }
}