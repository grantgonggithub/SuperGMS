using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace SuperGMS.ApiHelper.Xml
{
    /// <summary>
    /// 注释文档集合
    /// </summary>
    public class XmlCommentsFileCollection
    {
        private List<XmlCommentsFile> files;
        private List<XmlNode> nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlCommentsFileCollection"/> class.
        /// </summary>
        public XmlCommentsFileCollection()
        {
            files = new List<XmlCommentsFile>();
            nodes = new List<XmlNode>();
        }

        /// <summary>
        /// 添加注释文档文件
        /// </summary>
        /// <param name="file">文档</param>
        public void Add(XmlCommentsFile file)
        {
            files.Add(file);
            nodes.Add(file.Members);
        }

        /// <summary>
        /// Search all comments files for the specified member.  If not found, add the blank member to the first
        /// file.
        /// </summary>
        /// <param name="memberName">The member name for which to search.</param>
        /// <returns>The XML node of the found or added member</returns>
        public XmlNode FindMember(string memberName)
        {
            XmlNode member = null;
            string xPathQuery = string.Format(CultureInfo.InvariantCulture, "member[@name='{0}']", memberName);
            foreach (var node in nodes)
            {
                member = node.SelectSingleNode(xPathQuery);

                if (member != null)
                {
                    break;
                }
            }

            return member;
        }
    }
}