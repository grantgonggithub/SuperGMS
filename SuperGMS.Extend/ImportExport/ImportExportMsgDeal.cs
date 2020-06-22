using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SuperGMS.Extend.BackGroundMessage;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;

namespace SuperGMS.Extend.ImportExport
{
    /// <summary>
    /// 导入导出消息处理类
    /// </summary>
    public class ImportExportMsgDeal
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<ImportExportMsgDeal>();

        public static void Initlize(params string[] bussinessTypes)
        {
            BackGroundMessageMgr messageMgr = new BackGroundMessageMgr(bussinessTypes);
            messageMgr.OnBackGroundMessageReceive += MessageMgr_OnBackGroundMessageReceive;
        }

        /// <summary>
        /// 处理导入导出消息
        /// </summary>
        /// <param name="m">消息</param>
        /// <param name="ex">异常</param>
        /// <returns>处理结果，决定是否删除消息</returns>
        private static bool MessageMgr_OnBackGroundMessageReceive(Protocol.MQProtocol.MQProtocol<SetBackGroudMessageArgs> m, System.Exception ex)
        {
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
                        MicroServiceAssembly.Run(args.BussinessType, ctxArgs, out code);

                        if (code.code != StatusCode.OK.code)
                        {
                            bmm.SetFailProgress(code, int.MaxValue);
                            logger.LogError($"消息[{m.Msg.Args?.rid}] Initialization.MessageMgr_OnBackGroundMessageReceive.Error.args={m.ToString()}，code:{code.code},msg:{code.msg}");
                            //return false;
                        }

                        // 消息兜底异步处理
                        var rst = BackGroundMessageMgr.GetProcessStatus(m.Msg.Args?.rid);
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
    }
}
