using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using SuperGMS.Cache;
using SuperGMS.Extend.BackGroundMessage;
using SuperGMS.Log;
using SuperGMS.MQ;
using SuperGMS.Protocol.MQProtocol;
using SuperGMS.Protocol.RpcProtocol;

namespace SuperGMS.Extend.MQ
{
    /// <summary>
    /// 点对点消息处理类
    /// </summary>
    public class SuperDirectMessageHelper
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<SuperDirectMessageHelper>();

        /// <summary>
        /// 初始化消息处理类
        /// </summary>
        /// <param name="bussinessTypes"></param>
        public static void Initlize(params MessageRouterMap[] messageRouterMaps)
        {
            BackGroundDirectMessage messageMgr = new BackGroundDirectMessage(messageRouterMaps);
            messageMgr.OnBackGroundMessageReceive += MessageMgr_OnBackGroundMessageReceive;
        }


        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="m">消息</param>
        /// <param name="ex">异常</param>
        /// <returns>处理结果，决定是否删除消息</returns>
        private static bool MessageMgr_OnBackGroundMessageReceive(Protocol.MQProtocol.MQProtocol<SetBackGroudMessageArgs> m, System.Exception ex,object objCtx)
        {
            try
            {
                logger.LogDebug($"开始处理消息[{m?.Msg?.Args?.rid}]: {m?.ToString()}");
                var code = new StatusCode(805, "消息分发时发生异常");
                if (ex != null || m?.Msg?.Args == null)
                {
                    logger.LogError($"处理消息[{m?.Msg?.Args?.rid}] 异常 : 消息分发时发生异常");
                }
                if (objCtx == null)
                {
                    logger.LogError($"处理消息[{m?.Msg?.Args?.rid}] 异常 : objCtx消息处理接口未指定");
                    return true;
                }
                if (m?.Msg != null)
                {
                    SetBackGroudMessageArgs args = m.Msg;
                    try
                    {
                        var ctxArgs = args.Args.Copy();
                        ctxArgs.ct = "Import & Export function inner use.";
                        var data = JsonConvert.DeserializeObject<object>(args.Data, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate });
                        ctxArgs.v = data;
                        MicroServiceAssembly.Run(objCtx.ToString(), ctxArgs, out code);

                        if (code.code != StatusCode.OK.code)
                        {
                            logger.LogError($"消息[{m.Msg.Args?.rid}] SuperDirectMessageHelper.MessageMgr_OnBackGroundMessageReceive.Error.args={m.ToString()}，code:{code.code},msg:{code.msg}");
                            //return false;
                        }
                    }
                    catch (Exception e)
                    {
                        StatusCode c = new StatusCode(500, "SuperDirectMessageHelper.MessageMgr_OnBackGroundMessageReceive.Error=" + e.Message);
                        logger.LogError(e, $"消息[{m.Msg.Args?.rid}] SuperDirectMessageHelper.MessageMgr_OnBackGroundMessageReceive.Error.args={m.ToString()}");
                        //return false;
                    }

                    logger.LogDebug($"结束处理消息[{m.Msg.Args?.rid}] .");
                }
            }
            catch (Exception ex1)
            {
                logger.LogError(ex1, $"处理消息[{m?.Msg?.Args?.rid}] .异常...");
            }
            return true;
            //这里最大的问题是如果MQ在连续3个消息都没有收到ACK就不在给这个客户端发送消息，直到收到为止
            // 全部返回ACK  因为消息到这里来，就说明已经收到消息了，
        }


        /// <summary>
        /// 发现点对点消息
        /// </summary>
        /// <param name="valueArgs">参数</param>
        /// <returns>是否成功</returns>
        public static bool SetDirectMessage(SetBackGroudMessageArgs valueArgs)
        {
            var msg = new MQProtocol<SetBackGroudMessageArgs>("SetDirectMessage", valueArgs, valueArgs.Args.rid);
            var routeKey = BackGroundDirectMessage.GetRouter(valueArgs.MQRouterName);
            var mq = MQManager<SetBackGroudMessageArgs>.Publish(msg, routeKey);

            if (!mq)
            {
                logger.LogError($"SuperDirectMessageHelper.SetDirectMessage.MQ.Error.rid = {valueArgs.Args.rid}");
                return false;
            }

            logger.LogInformation($"SuperDirectMessageHelper.SetDirectMessage.MQ.Success.rid = {valueArgs.Args.rid}");
            return true;
        }

        /// <summary>
        /// 发送广播消息
        /// </summary>
        /// <param name="valueArgs">参数</param>
        /// <returns>是否成功</returns>
        public static bool SetFanoutMessage(SetBackGroudMessageArgs valueArgs)
        {
            var msg = new MQProtocol<SetBackGroudMessageArgs>("SetFanoutMessage", valueArgs, valueArgs.Args.rid);
            var routeKey = BackGroundDirectMessage.GetRouter(valueArgs.MQRouterName);
            var mq = MQManager<SetBackGroudMessageArgs>.PublishFanout(msg, routeKey);

            if (!mq)
            {
                logger.LogError($"SuperMessageHelper.SetFanoutMessage.MQ.Error.rid = {valueArgs.Args.rid}");
                return false;
            }

            logger.LogInformation($"SuperMessageHelper.SetFanoutMessage.MQ.Success.rid = {valueArgs.Args.rid}");
            return true;
        }
    }


    /// <summary>
    /// 扇波消息处理类
    /// </summary>
    public class SuperFanoutMessageHelper
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<SuperFanoutMessageHelper>();

        /// <summary>
        /// 初始化消息处理类
        /// </summary>
        /// <param name="bussinessTypes"></param>
        public static void Initlize(params MessageRouterMap[] messageRouterMaps)
        {
            BackGroundFanoutMessage messageMgr = new BackGroundFanoutMessage(messageRouterMaps);
            messageMgr.OnBackGroundMessageReceive += MessageMgr_OnBackGroundMessageReceive;
        }


        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="m">消息</param>
        /// <param name="ex">异常</param>
        /// <returns>处理结果，决定是否删除消息</returns>
        private static bool MessageMgr_OnBackGroundMessageReceive(Protocol.MQProtocol.MQProtocol<SetBackGroudMessageArgs> m, System.Exception ex, object objCtx)
        {
            try
            {
                logger.LogDebug($"开始处理消息[{m?.Msg?.Args?.rid}]: {m?.ToString()}");
                var code = new StatusCode(805, "消息分发时发生异常");
                if (ex != null || m?.Msg?.Args == null)
                {
                    logger.LogError($"处理消息[{m?.Msg?.Args?.rid}] 异常 : 消息分发时发生异常");
                }
                if (objCtx == null)
                {
                    logger.LogError($"处理消息[{m?.Msg?.Args?.rid}] 异常 : objCtx消息处理接口未指定");
                    return true;
                }
                if (m?.Msg != null)
                {
                    SetBackGroudMessageArgs args = m.Msg;
                    try
                    {
                        var ctxArgs = args.Args.Copy();
                        ctxArgs.ct = "Import & Export function inner use.";
                        var data = JsonConvert.DeserializeObject<object>(args.Data, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate });
                        ctxArgs.v = data;
                        MicroServiceAssembly.Run(objCtx.ToString(), ctxArgs, out code);

                        if (code.code != StatusCode.OK.code)
                        {
                            logger.LogError($"消息[{m.Msg.Args?.rid}] SuperDirectMessageHelper.MessageMgr_OnBackGroundMessageReceive.Error.args={m.ToString()}，code:{code.code},msg:{code.msg}");
                            //return false;
                        }
                    }
                    catch (Exception e)
                    {
                        StatusCode c = new StatusCode(500, "SuperDirectMessageHelper.MessageMgr_OnBackGroundMessageReceive.Error=" + e.Message);
                        logger.LogError(e, $"消息[{m.Msg.Args?.rid}] SuperDirectMessageHelper.MessageMgr_OnBackGroundMessageReceive.Error.args={m.ToString()}");
                        //return false;
                    }

                    logger.LogDebug($"结束处理消息[{m.Msg.Args?.rid}] .");
                }
            }
            catch (Exception ex1)
            {
                logger.LogError(ex1, $"处理消息[{m?.Msg?.Args?.rid}] .异常...");
            }
            return true;
            //这里最大的问题是如果MQ在连续3个消息都没有收到ACK就不在给这个客户端发送消息，直到收到为止
            // 全部返回ACK  因为消息到这里来，就说明已经收到消息了，
        }

        /// <summary>
        /// 发送广播消息
        /// </summary>
        /// <param name="valueArgs">参数</param>
        /// <returns>是否成功</returns>
        public static bool SetFanoutMessage(SetBackGroudMessageArgs valueArgs)
        {
            var msg = new MQProtocol<SetBackGroudMessageArgs>("SetFanoutMessage", valueArgs, valueArgs.Args.rid);
            var exChange = BackGroundFanoutMessage.GetExchange(valueArgs.MQRouterName);
            var mq = MQManager<SetBackGroudMessageArgs>.PublishFanout(msg, exChange);

            if (!mq)
            {
                logger.LogError($"SuperMessageHelper.SetFanoutMessage.MQ.Error.rid = {valueArgs.Args.rid}");
                return false;
            }

            logger.LogInformation($"SuperMessageHelper.SetFanoutMessage.MQ.Success.rid = {valueArgs.Args.rid}");
            return true;
        }
    }

    /// <summary>
    /// 导入导出异步消息
    /// </summary>
    public class SuperImportExprotMessageHelper
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<SuperImportExprotMessageHelper>();

        public static void Initlize(params MessageRouterMap[] messageRouterMaps)
        {
            BackGroundDirectMessage messageMgr = new BackGroundDirectMessage(messageRouterMaps);
            messageMgr.OnBackGroundMessageReceive += MessageMgr_OnBackGroundMessageReceive;
        }

        /// <summary>
        /// 设置导入消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendImportMessage(SetBackGroudMessageArgs msg)
        {
            if (SuperDirectMessageHelper.SetDirectMessage(msg))
            {
                    var processMsg = new BackGroundMessageProcessResult() // 初始化一个未开始状态，所有值都为-1，表示还在队列中，未开始
                    {
                        ProcessNum = -1,
                        SuccessNum = -1,
                        TotalNum = -1,
                        Data = string.Empty,
                        Rid = msg.Args.rid,
                    };

               return SetProcessStatus(processMsg);
            }
            return false;
        }

        /// <summary>
        /// 处理导入导出消息
        /// </summary>
        /// <param name="m">消息</param>
        /// <param name="ex">异常</param>
        /// <returns>处理结果，决定是否删除消息</returns>
        private static bool MessageMgr_OnBackGroundMessageReceive(Protocol.MQProtocol.MQProtocol<SetBackGroudMessageArgs> m, System.Exception ex,object objCtx)
        {
            if (objCtx == null)
            {
                logger.LogError($"处理消息[{m?.Msg?.Args?.rid}] 异常 : objCtx消息处理接口未指定");
                return true;
            }

            try
            {
                BmmHelp bmm = new BmmHelp(m?.Msg?.Args?.rid);
                bmm.SetStartProgress();
                logger.LogDebug($"开始处理消息[{m?.Msg?.Args?.rid}]: {m?.ToString()}");
                var code = new StatusCode(805, "导入导出消息分发时发生异常");
                if (ex != null || m?.Msg?.Args == null)
                {
                    bmm.SetFailProgress(code, int.MaxValue);
                    logger.LogError($"处理消息[{m?.Msg?.Args?.rid}] 异常 : 导入导出消息分发时发生异常");
                    //return true;
                }

                if (m?.Msg != null)
                {
                    SetBackGroudMessageArgs args = m.Msg;
                    try
                    {
                        var ctxArgs = args.Args.Copy();
                        ctxArgs.ct = "Import & Export function inner use.";
                        var data = JsonConvert.DeserializeObject<object>(args.Data, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate });
                        ctxArgs.v = data;
                        MicroServiceAssembly.Run(objCtx.ToString(), ctxArgs, out code);

                        if (code.code != StatusCode.OK.code)
                        {
                            bmm.SetFailProgress(code, int.MaxValue);
                            logger.LogError($"消息[{m.Msg.Args?.rid}] Initialization.MessageMgr_OnBackGroundMessageReceive.Error.args={m.ToString()}，code:{code.code},msg:{code.msg}");
                            //return false;
                        }

                        // 消息兜底异步处理
                        var rst = GetProcessStatus(m.Msg.Args?.rid);
                        if (rst == null || (rst.Code.IsSuccess && (rst.ProcessNum != rst.TotalNum || rst.ProcessNum == -1)))
                        {
                            bmm.SetSuccessProgress(
                                (rst?.TotalNum ?? -1) == -1 ? int.MaxValue : rst?.TotalNum ?? int.MaxValue,
                                rst?.SuccessNum ?? 0,
                                "框架兜底完成异步任务");
                            logger.LogWarning($"消息[{m.Msg.Args?.rid}] 处理返回成功，但是没有设置进度100%,{m.ToString()}，code:{code.code},msg:{code.msg}");
                            //return true;
                        }

                    }
                    catch (Exception e)
                    {
                        StatusCode c = new StatusCode(500, "ImportExportMsgDeal.MessageMgr_OnBackGroundMessageReceive.Error=" + e.Message);
                        bmm.SetFailProgress(c, int.MaxValue);
                        logger.LogError(e, $"消息[{m.Msg.Args?.rid}] Initialization.MessageMgr_OnBackGroundMessageReceive.Error.args={m.ToString()}");
                        //return false;
                    }

                    logger.LogDebug($"结束处理消息[{m.Msg.Args?.rid}] .");
                }
            }
            catch (Exception ex1)
            {
                logger.LogError(ex1, $"处理消息[{m?.Msg?.Args?.rid}] .异常...");
            }
            return true;
            //这里最大的问题是如果MQ在连续3个消息都没有收到ACK就不在给这个客户端发送消息，直到收到为止
            // 全部返回ACK  因为消息到这里来，就说明已经收到消息了，最终错误会写到Redis里面，前端还是接到了错误响应，本条消息处理完毕，而不是把完毕的消息继续留在队列中  add by grant 2019-2-26
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="processMsg">信息</param>
        /// <returns>是否成功</returns>
        public static bool SetProcessStatus(BackGroundMessageProcessResult processMsg)
        {
            return ResourceCache.Instance.Set(processMsg.Rid, processMsg);
        }

        /// <summary>
        /// 获取处理进度信息
        /// </summary>
        /// <param name="taskGuid">taskGuid</param>
        /// <returns>BackGroundMessageProcessResult</returns>
        public static BackGroundMessageProcessResult GetProcessStatus(string taskGuid)
        {
            return ResourceCache.Instance.Get<BackGroundMessageProcessResult>(taskGuid);
        }
    }
}
