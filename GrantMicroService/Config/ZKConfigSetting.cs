/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.Config
 文件名：  ZookeeperConfigSetting
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/11/13 18:10:27

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GrantMicroService.Config
{
    /// <summary>
    /// ZKConfigSetting
    /// </summary>
    public class ZKConfigSetting
    {
        /// <summary>
        /// LoadConfig
        /// </summary>
        /// <param name="appName">appName</param>
        /// <param name="ip">ip</param>
        /// <param name="port">port</param>
        /// <returns>xml</returns>
        public XElement LoadConfig(string appName, string ip, int port)
        {
            throw new NotImplementedException();
        }
    }
}
