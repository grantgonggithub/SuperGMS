using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SuperGMS.ApiHelper.Xml
{
    /// <summary>
    /// Xml 解析辅助类
    /// </summary>
    public class XmlCommentsFile
    {
        private string sourcePath;
        private Encoding enc;
        private XmlDocument comments;
        private XmlNode members;
        private bool wasModified;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlCommentsFile"/> class.
        /// Constructor
        /// </summary>
        /// <param name="filename">The XML comments filename</param>
        /// <exception cref="ArgumentNullException">This is thrown if the filename is null or an empty string</exception>
        public XmlCommentsFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename cannot be null", "filename");
            }

            sourcePath = filename;
        }

        /// <summary>
        /// Gets This read-only property is used to get the source path of the file
        /// </summary>
        public string SourcePath
        {
            get { return sourcePath; }
        }

        /// <summary>
        /// Gets This read-only property is used to get the encoding, typically UTF-8
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                if (comments == null)
                {
                    enc = Encoding.UTF8;
                    Utility.ReadWithEncoding(sourcePath, ref enc);
                }

                return enc;
            }
        }

        /// <summary>
        /// Gets This is used to load the comments file on first use
        /// </summary>
        public XmlDocument Comments
        {
            get
            {
                if (comments == null)
                {
                    // Although Visual Studio doesn't add an encoding, the files are UTF-8 encoded
                    enc = Encoding.UTF8;

                    comments = new XmlDocument();

                    // Read it with the appropriate encoding
                    comments.LoadXml(Utility.ReadWithEncoding(sourcePath, ref enc));

                    comments.NodeChanged += Comments_NodeChanged;
                    comments.NodeInserted += Comments_NodeChanged;
                    comments.NodeRemoved += Comments_NodeChanged;
                }

                return comments;
            }
        }

        /// <summary>
        /// Gets This read-only property is used to get the root members node
        /// </summary>
        public XmlNode Members
        {
            get
            {
                if (members == null)
                {
                    members = this.Comments.SelectSingleNode("doc/members");

                    if (members == null)
                    {
                        throw new InvalidOperationException(sourcePath + " does not contain a 'doc/members' node");
                    }
                }

                return members;
            }
        }

        /// <summary>
        /// Save the comments file if it was modified
        /// </summary>
        public void Save()
        {
            // Write the file back out with the appropriate encoding if it was modified
            if (wasModified)
            {
                using (StreamWriter sw = new StreamWriter(sourcePath, false, enc))
                {
                    comments.Save(sw);
                }

                wasModified = false;
            }
        }

        /// <summary>
        /// This can be used to force a reload of the comments file if changes were made to it outside of this
        /// instance.
        /// </summary>
        public void ForceReload()
        {
            comments = null;
            members = null;
            wasModified = false;
        }

        /// <summary>
        /// Mark the file as modified if a node is changed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Comments_NodeChanged(object sender, XmlNodeChangedEventArgs e)
        {
            wasModified = true;
        }
    }
}