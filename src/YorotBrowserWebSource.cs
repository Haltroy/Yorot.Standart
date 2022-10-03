using System;
using System.Collections.Generic;
using System.Text;

namespace Yorot
{
    /// <summary>
    /// A Web Source used by the web browser.
    /// </summary>
    public class YorotBrowserWebSource
    {
        /// <summary>
        /// URL of the web source.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Determines if the web source takes arguments.
        /// </summary>
        public bool TakesArguments { get; set; }

        /// <summary>
        /// Class for arguments.
        /// </summary>
        public class Argument
        {
            public Argument(string name, string value)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Value = value ?? throw new ArgumentNullException(nameof(value));
            }

            /// <summary>
            /// Name of the argument. Used for replaceing [Name] in WebSource file.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Value of the Argument.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// The Data itself.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The type of data.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Determines if this Web Source should be ignored by <see cref="SessionSystem"/> or not.
        /// </summary>
        public bool IgnoreOnSessionList { get; set; }

        /// <summary>
        /// Returns the data as <see cref="string"/> if it is a <see cref="string"/> otherwise returns <seealso cref="string.Empty"/>.
        /// </summary>
        public string DataToString
        {
            get
            {
                if (Data != null && Data is string data)
                {
                    return data;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Return the data as <see cref="System.IO.Stream"/> if it is a <see cref="System.IO.Stream"/> otherwise returns <seealso cref="null"/>.
        /// </summary>
        public System.IO.Stream DataToStream
        {
            get
            {
                if (Data != null && Data is System.IO.Stream data)
                {
                    return data;
                }
                return null;
            }
        }
    }
}