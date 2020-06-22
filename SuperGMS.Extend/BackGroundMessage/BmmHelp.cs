using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SuperGMS.ExceptionEx;
using SuperGMS.Log;
using SuperGMS.Protocol.RpcProtocol;

namespace SuperGMS.Extend.BackGroundMessage
{
    /// <summary>
    /// BackGroundMessageMgr 发送进度的一个简短封装
    /// </summary>
    public class BmmHelp
    {
        private string _rid;
        private readonly static ILogger logger = LogFactory.CreateLogger<BmmHelp>();
        /// <summary>
        /// Initializes a new instance of the <see cref="BmmHelp"/> class.
        /// 构造
        /// </summary>
        /// <param name="rid">rid</param>
        public BmmHelp(string rid)
        {
            _rid = rid;
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="rid">请求</param>
        /// <param name="code">状态</param>
        /// <param name="totalNum">总数</param>
        /// <param name="processNum">处理数</param>
        /// <param name="successNum">成功数</param>
        /// <param name="data">数据</param>
        public static void SetProgress(string rid, StatusCode code, int totalNum, int processNum, int successNum,  string data = "")
        {
            var rtn = BackGroundMessageMgr.SetProcessStatus(new BackGroundMessageProcessResult()
            {
                Rid = rid,
                Code = code,
                Data = data,
                ProcessNum = processNum,
                SuccessNum = successNum,
                TotalNum = totalNum,
            });

            if (!rtn)
            {
                logger.LogWarning($"rid[{rid}]: redis 不能写入数据。");
            }
        }

        /// <summary>
        /// 建立开始消息回告
        /// </summary>
        /// <param name="data">数据</param>
        public void SetStartProgress(string data = "")
        {
            SetProgress(
                this._rid,
                new StatusCode(200, "开始解析文件"),
                200,
                1,
                1,
                data);
        }

        /// <summary>
        /// 建立成功结束消息回告
        /// </summary>
        /// <param name="totalNum">总数据</param>
        /// <param name="sucessNum">成功数据</param>
        /// <param name="data">数据</param>
        public void SetSuccessProgress(int totalNum, int sucessNum, string data = "")
        {
            SetProgress(
                this._rid,
                new StatusCode(200, "开始解析文件"),
                totalNum,
                totalNum,
                sucessNum,
                data);
        }

        /// <summary>
        /// 建立失败结束消息回告
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="totalNum">总数据</param>
        /// <param name="data">数据</param>
        public void SetFailProgress(StatusCode code, int totalNum,  string data = "")
        {
            SetProgress(
                this._rid,
                code,
                totalNum,
                totalNum,
                -1,
                data);
        }

        /// <summary>
        /// 建立进度消息回告
        /// </summary>
        /// <param name="totalNum">总数据</param>
        /// <param name="progress">进度，0~1范围</param>
        /// <param name="data">数据</param>
        public void SetPerProgress(int totalNum, double progress, string data = null)
        {
            var success = (int)Math.Min(totalNum * progress, totalNum);
            SetProgress(
                this._rid,
                StatusCode.OK,
                totalNum,
                success,
                success,
                data);
        }
    }
}
