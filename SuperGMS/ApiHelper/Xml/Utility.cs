using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperGMS.ApiHelper.Xml
{
    /// <summary>
    /// This class contains utility and extension methods
    /// </summary>
    public static class Utility
    {
        // Regular expressions used for encoding detection and parsing
        private static Regex reXmlEncoding = new Regex("^<\\?xml.*?encoding\\s*=\\s*\"(?<Encoding>.*?)\".*?\\?>");

        /// <summary>
        /// This is used to read in a file using an appropriate encoding method
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <param name="encoding">Pass the default encoding to use.  On return, it contains the actual encoding
        /// for the file.</param>
        /// <returns>The contents of the file</returns>
        /// <remarks>When reading the file, it uses the default encoding specified but detects the encoding if
        /// byte order marks are present.  In addition, if the template is an XML file and it contains an
        /// encoding identifier in the XML tag, the file is read using that encoding.</remarks>
        public static string ReadWithEncoding(string filename, ref Encoding encoding)
        {
            Encoding fileEnc;
            string content;

            using (StreamReader sr = new StreamReader(filename, encoding, true))
            {
                content = sr.ReadToEnd();
                encoding = sr.CurrentEncoding;
            }

            Match m = reXmlEncoding.Match(content);

            // Re-read an XML file using the correct encoding?
            if (m.Success)
            {
                fileEnc = Encoding.GetEncoding(m.Groups["Encoding"].Value);

                if (fileEnc != encoding)
                {
                    encoding = fileEnc;

                    using (StreamReader sr = new StreamReader(filename, encoding, true))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }

            return content;
        }
    }
}