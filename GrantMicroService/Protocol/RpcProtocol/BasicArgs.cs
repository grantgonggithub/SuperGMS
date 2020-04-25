/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.Protocol
 文件名：BasicArgs
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 11:49:24

 功能描述：Args和Result的基类

----------------------------------------------------------------*/
using System;

namespace GrantMicroService.Protocol.RpcProtocol
{
    /// <summary>
    /// 统一参数的基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
   public class BasicArgs<T>
    {
        private string checkSum;
        /// <summary>
        /// 校验值
        /// </summary>
        public string cs
        {
            get { return checkSum; }
            set { checkSum = value; }
        }

        private T _value;

        /// <summary>
        /// 业务数据
        /// </summary>
        public T v
        {
            get { return _value; }
            set { _value = value; }
        }

        private bool isCiphertext;
        /// <summary>
        /// 当前传输的是否密文,这里说的加密指的是对有效业务数据的加密指的是_Value;
        /// </summary>
        public bool icp
        {
            get { return isCiphertext; }
            set { isCiphertext = value; }
        }

        private string routerUri;
        /// <summary>
        /// 可以路由的Uri
        /// </summary>
        public string uri
        {
            get { return routerUri; }
            set { routerUri = value; }
        }

        private string language;

        /// <summary>
        /// 语言
        /// </summary>
        public string lg
        {
            get { return language; }
            set { language = value; }
        }

    }
}
