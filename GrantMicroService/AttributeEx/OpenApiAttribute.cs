using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.AttributeEx
{
    /// <summary>
    /// 指定api为开放的供第三方调用Api
    /// </summary>
    public class OpenApiAttribute : Attribute
    {
        /// <summary>
        /// 初始化api为开放api
        /// </summary>
        /// <param name="ttid"></param>
        /// <param name="keyDesc">api描述的资源Key</param>
        public OpenApiAttribute(string keyDesc = "", string[] ttid = null)
        {
            this.Ttids = new List<string>();
            if (ttid != null)
            {
                this.Ttids.AddRange(ttid);
            }

            this.ApiDesc = keyDesc;
        }

        /// <summary>
        /// 允许查看的ttid
        /// </summary>
        public List<string> Ttids { get; set; }

        /// <summary>
        /// api描述资源Key
        /// </summary>
        public string ApiDesc { get; set; }
    }
}
