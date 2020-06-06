using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using GrantMicroService.Config;
using GrantMicroService.Protocol.RpcProtocol;
using GrantMicroService.Rpc.Server;
using GrantMicroService.Tools;

namespace GrantMicroService.Log
{
    /// <summary>
    /// 日志信息
    /// </summary>
    public class LogInfo : LogBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        public LogInfo()
        {
            this.CreatedDate = DateTime.Now;
            this.ComputerIp = ServiceEnvironment.ComputerAddress;
            this.ComputerName = ServiceEnvironment.ComputerName;
            this.AppNamespace = string.Empty;
            this.ServiceName = ServerSetting.AppName;
        }
        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string CodeMsg { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public string Parameters { get; set; }
        /// <summary>
        /// 调用链
        /// </summary>
        public string UseChain { get; set; }

        /// <summary>
        /// app命名空间
        /// </summary>
        public string AppNamespace { get; set; }

        /// <summary>
        /// 计算机名
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// 计算机ip
        /// </summary>
        public string ComputerIp { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string CreatedBy { get; set; }

        /// <inheritdoc cref="LogBase"/>
        public override void SetInfo<T>(Args<object> args, Result<T> result = null, Exception ex = null)
        {
            if (result == null && args == null)
            {
                Trace.WriteLine("SetInfo 调用时传入的参数和结果均为空，不记入统计日志");
                return;
            }

            this.TransactionId = result?.rid ?? args?.rid;
            this.Code = result?.c ?? 0;
            this.CodeMsg = result?.msg;            

            if (string.IsNullOrEmpty(this.ApiName))
            {
                this.ApiName = args?.m;
            }
        }

        /// <summary>
        /// 日志格式化字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            if (this.CreatedBy == "FrameWork")
            {
                return $"[{this.CreatedDate:yyyy-MM-dd HH:mm:ss:fffff}]\r\n\t{this.Desc}\r\n";
            }
            else
            {
                return $"[{this.CreatedDate:yyyy-MM-dd HH:mm:ss:fffff}]\r\n\trid:{this.TransactionId} , url:/{this.ServiceName}/{this.ApiName}\r\n\tcode:{this.Code},msg:{this.CodeMsg},serverIp:{this.ComputerIp},name:{this.ComputerName}\r\n\tParam:{this.Parameters}\r\n\tchain:{this.UseChain}\r\n\tdesc:{this.Desc}\r\n";
            }
        }
    }
}
