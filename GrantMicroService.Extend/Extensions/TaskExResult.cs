/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： GrantMicroService.Extend.Extensions
 文件名：   TaskExResult
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/4 14:32:29

 功能描述：

----------------------------------------------------------------*/
namespace GrantMicroService.Extend.Extensions
{
    using System;

    /// <summary>
    /// TaskExResult
    /// </summary>
    public class TaskExResult<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets 错误内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets 如果失败，失败原因
        /// </summary>
        public Exception Ex { get; set; }

        /// <summary>
        /// Gets or sets 调用方想传递的上下文
        /// </summary>
        public object ObjCxt { get; set; }

        /// <summary>
        /// Gets or sets 本次成功数量，如果是单条这个值可以省略，如果是自己改变成功数量则需要赋值>1
        /// </summary>
        public int SuccessNum { get; set; }

        /// <summary>
        /// Gets or sets 本次执行的总数，注意区别：Tot，如果单条可以省略，如果自己想改变成功数量则需要赋值>1
        /// </summary>
        public int TotNum { get; set; }

        /// <summary>
        /// Gets or sets 当前处理的记录
        /// </summary>
        public T Row { get; set; }

        /// <summary>
        /// Gets or sets 原始记录
        /// </summary>
        public T[] Tables { get; set; }

        /// <summary>
        /// Gets or sets 当前行在Tables 中的索引
        /// </summary>
        public int RowIndx { get; set; }

        /// <summary>
        /// 想传给客户端的数据
        /// </summary>
        public string Data { get; set; }
    }
}