/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Config
 文件名：  ConfigManager
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/11/20 13:02:06

 功能描述：

----------------------------------------------------------------*/

using Microsoft.Extensions.Logging;
using GrantMicroService.Log;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

namespace GrantMicroService.Config
{
    /// <summary>
    /// ConfigManager
    /// </summary>
    public class ConfigManager
    {
        /// <summary>
        /// rpc服务端的配置
        /// </summary>
        private const string RPCSERVICE = "RpcService";

        /// <summary>
        /// rpc客戶端在配文件中的名字
        /// </summary>
        private const string RPCCLIENT = "RpcClients";

        /// <summary>
        /// 常量在配置文件中的名字
        /// </summary>
        public const string CONSTKEYVALUE = "ConstKeyValue";

        public static Dictionary<string, ConstItem> constDic = new Dictionary<string, ConstItem>();

        private static ReaderWriterLock readerWriterLock = new ReaderWriterLock();
        private readonly static ILogger logger = LogFactory.CreateLogger<ConfigManager>();
        /// <summary>
        /// Initlize
        /// </summary>
        /// <param name="xml">xml</param>
        public static void Initlize(ConstKeyValue constKeyValue)
        {
            // keyvalue解析
            if (constKeyValue != null)
            {
                updateConstKeyValue(constKeyValue);
            }
        }

        /// <summary>
        /// 获取grant.config中ConstKeyValue 的某一个item值
        /// </summary>
        /// <param name="key">key的值</param>
        /// <returns>KeyValue对象</returns>
        public static ConstItem GetConstKeyValue(string key)
        {
            try
            {
                readerWriterLock.AcquireReaderLock(100);
                if (constDic.ContainsKey(key.ToLower()))
                {
                    return constDic[key.ToLower()];
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ConfigManager.GetConstKeyValue.Error");
            }
            finally
            {
                if (readerWriterLock.IsReaderLockHeld)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }

            return null;
        }

        private static void updateConstKeyValue(ConstKeyValue constKeyValue)
        {
            try
            {
                readerWriterLock.AcquireWriterLock(100);
                for(int i = 0;i<constKeyValue.Items.Count;i++)
                {
                    if (!string.IsNullOrEmpty(constKeyValue.Items[i].Key))
                    {
                        ConstItem kv = new ConstItem() { Key = constKeyValue.Items[i].Key.ToLower(), Value = constKeyValue.Items[i].Value };
                        if (!constDic.ContainsKey(kv.Key))
                        {
                            constDic.Add(kv.Key, kv);
                        }
                        else
                        {
                            constDic[kv.Key] = kv;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ConfigManager.updateConstKeyValue.Error");
            }
            finally
            {
                if (readerWriterLock.IsWriterLockHeld)
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
        }
    }
}