using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.Tools
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class ExtentionTool
    {
        /// <summary>
        /// Copy 方法, 解决引用类型无法复制问题
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>新对象</returns>
        public static T GrantCopy<T>(this T obj)
        {
            return JsonEx.JsonConvert.DeserializeObject<T>(JsonEx.JsonConvert.JsonSerializer(obj));
        }
    }
}