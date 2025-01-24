/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Extend.Extensions
 文件名：   TaskExArgs
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/4 15:32:34

 功能描述：

----------------------------------------------------------------*/
using System.Collections.Generic;

namespace SuperGMS.Extend.Extensions
{
    /// <summary>
    /// TaskExArgs
    /// </summary>
    public class TaskExArgs<T>
    {
        /// <summary>
        /// 每一行记录
        /// </summary>
        public T Row { get; set; }

        /// <summary>
        /// 用户需要传递给执行方法的上下文
        /// </summary>
        public object UserCtx { get; set; }

        /// <summary>
        /// Gets or sets 原始记录
        /// </summary>
        public T[] Tables { get; set; }

        /// <summary>
        /// Gets or sets 当前行在Tables 中的索引
        /// </summary>
        public int RowIndx { get; set; }

        /// <summary>
        ///  前面执行数据的结果集
        /// </summary>
        public List<TaskExResult<T>> TaskResult { get; set; }

    }
}