/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称： SuperGMS.Tools
 文件名：   WebClientEx
 创建者：   grant(巩建春) nnn987@126.com
 CLR版本：  4.0.30319.42000
 时间：     2018/8/29 10:12:55

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SuperGMS.Tools
{
    /// <summary>
    /// WebClientEx
    /// </summary>
    public class WebClientEx:WebClient
    {
        private int _timeOut;

        public int TimeOut {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        /// <summary>
        /// 无参构造，默认超时6s
        /// </summary>
        public WebClientEx()
        {
            this._timeOut = 3 * 1000; // 默认2s
        }

        /// <summary>
        /// 需要传入超时时间
        /// </summary>
        /// <param name="timeOut"></param>
        public WebClientEx(int timeOut)
        {
            this._timeOut = timeOut > 3000 ? timeOut : 3000;
        }

        /// <summary>
        /// 重载
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var req=base.GetWebRequest(address);
            req.Timeout = this._timeOut;
            return req;
        }
    }
}