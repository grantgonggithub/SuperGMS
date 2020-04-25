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
        /// 当返回 xxx订单执行成功 ,页面将自动弹出右下角消息"xxx订单执行成功"给用户
        /// 当返回http://xxxx/xxxx时,页面将自动跳转到此页面
        /// </returns>
        string Excute(List<string> listObject, string buttonId);
    }
}
