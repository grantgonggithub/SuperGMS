/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Extend.Extensions
 文件名：   TaskEx
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/4/4 14:24:37

 功能描述：

----------------------------------------------------------------*/

using System.Linq;
using SuperGMS.Log;

namespace SuperGMS.Extend.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SuperGMS.Extend.BackGroundMessage;
    using SuperGMS.Extend.ImportExport;
    using SuperGMS.Protocol.RpcProtocol;
    using SuperGMS.Rpc.Server;

    /// <summary>
    /// TaskEx  T
    /// </summary>
    public class TaskEx<T>
    {
        private readonly static ILogger logger = LogFactory.CreateLogger("TaskEx<T>");
        /// <summary>
        /// 并行执行操作
        /// </summary>
        /// <param name="tables">要多线程批量执行的集合</param>
        /// <param name="runOne">集合中每一项要执行的操作</param>
        /// <param name="callBack">最后集合执行完的回调</param>
        /// <param name="ctx">RpcContext</param>
        /// <param name="userCtx">userCtx</param>
        /// <param name="taskNum">同时并发的线程数 默认为10，用户可以自己指定这个值</param>
        /// <param name="userTotNum">默认为tables.length来算，用户可以自己指定这个值</param>
        /// <returns>返回执行结果</returns>
        public static List<TaskExResult<T>> DoRun(T[] tables, Func<TaskExArgs<T>, TaskExResult<T>> runOne, Func<List<TaskExResult<T>>, TaskExCallBackResult> callBack, RpcContext ctx, object userCtx, int taskNum = 0, int userTotNum = 0)
        {
            if (tables == null || runOne == null)
            {
                return null;
            }
            if (taskNum > 20)
            {
                taskNum = 20;
            }

            List<Task> li = new List<Task>();
            List<TaskExResult<T>> rst = new List<TaskExResult<T>>();
            object lockObj = new object();
            int current = 0;
            int success = 0;
            for (int i = 0; i < tables.Length; i++)
            {
                var item = new TaskExArgs<T>() { Row = tables[i], UserCtx = userCtx, RowIndx = i, Tables = tables,TaskResult = rst};
                Task tsk = new Task(() =>
                {
                    bool isOk = false;
                    TaskExResult<T> r;
                    try
                    {
                        // 这里try一下，防止外面传进来的方法没有处理异常，
                        // 但是强烈建议，外部自己处理异常，这样可以根据业务决定返回true,false,外部处理了异常，这里就不会捕获到
                        r = runOne(item);
                        isOk = r.IsSuccess;
                    }
                    catch (Exception ex)
                    {
                        r = new TaskExResult<T>
                        {
                            IsSuccess = false,
                            Ex = ex,
                        };
                        isOk = false;
                    }
                    r.Row = item.Row;
                    r.Tables = tables;
                    r.RowIndx = item.RowIndx;
                    r.Row = item.Row;
                    lock (lockObj)
                    {
                        rst.Add(r); // 把每行的执行情况保存下来，在最后执行完毕后返回给调用方

                        int cur_tot = r.TotNum > 0 ? r.TotNum : 1;
                        int tot_org = userTotNum > 0 ? userTotNum : tables.Length;

                        if (isOk)
                        {
                            int succ = r.SuccessNum > 0 ? r.SuccessNum : 1;
                            success += succ;
                        }
                        else
                        {
                            int succ = r.SuccessNum > 0 ? r.SuccessNum : 0;
                            success += succ;
                        }

                        current += cur_tot;
                        TaskExCallBackResult callRst = null;
                        if (current >= tot_org)
                        {
                            try
                            {
                               callRst = callBack(rst); // 完成之后执行回调
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, $"TaskEx.DoRun.callBack.Error.rid={ctx.Args.rid}");

                                // 把错误返回给客户端
                                callRst = new TaskExCallBackResult()
                                {
                                    SetCallBackData =
                                        $"TaskEx.DoRun.callBack.Error.rid={ctx.Args.rid},ex={e.Message},exState={e.StackTrace}",
                                };
                            }
                        }
                        BackGroundMessageProcessResult backGround = new BackGroundMessageProcessResult()
                        {
                            Code = StatusCode.OK,
                            Rid = ctx.Args.rid,
                            TotalNum = tot_org,
                            Data = callRst == null ? null : Newtonsoft.Json.JsonConvert.SerializeObject(callRst.SetCallBackData),
                        };

                        if (!string.IsNullOrEmpty(r.Data))
                        {
                            if (callRst == null) // 最后的内容以以回调为准
                            {
                                backGround.Data = r.Data;
                                r.Data = String.Empty;
                            }
                        }

                        backGround.SuccessNum = success;
                        backGround.ProcessNum = current;

                        MQ.SuperImportExprotMessageHelper.SetProcessStatus(backGround);
                    }
                });
                tsk.Start();
                li.Add(tsk);
                if (li.Count >= (taskNum > 0 ? taskNum : 10) || (i == (tables.Length - 1)))
                {
                    Task.WaitAll(li.ToArray());
                    li.Clear();
                }

                 if(taskNum > 1) Thread.Sleep(1);
            }

            return rst;
        }
    }
}