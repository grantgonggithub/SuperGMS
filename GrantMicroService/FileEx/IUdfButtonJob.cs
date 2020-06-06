using System;
using System.Collections.Generic;
using System.Text;

namespace GrantMicroService.FileEx
{
    /// <summary>
    /// 自定义按钮接口
    /// 当自定义按钮是以Job形式运行,则会通过接口实例化一个Job对象并调用Excute方法
    /// 需要将主键数组传入进来
    /// </summary>
    public interface IUdfButtonJob
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="listObject">
        /// 关于对象的泛型主键集合
        /// </param>
        /// <param name="buttonId">
        /// 自定义按钮ID
        /// </param>
        /// <returns>
        /// </returns>
        string Excute(List<string> listObject, string buttonId);
    }
}
