/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Utility
 文件名：ComboxClass
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:59:07

 功能描述：一个可用复用的实体类

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.Tools
{
    /// <summary>
    /// 1个泛型的可用复用的实体类
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    [Serializable]
    public class ComboxClass<T>
    {
        private T v;

        /// <summary>
        /// Gets or sets 泛型类
        /// </summary>
        public T V1
        {
            get { return v; }
            set { v = value; }
        }
    }

    /// <summary>
    ///  2个泛型的可用复用的实体类
    /// </summary>
    /// <typeparam name="T1">类型1</typeparam>
    /// <typeparam name="T2">类型2</typeparam>
    [Serializable]
    public class ComboxClass<T1, T2> : ComboxClass<T1>
    {
        private T2 v2;

        /// <summary>
        /// Gets or sets v2 泛型
        /// </summary>
        public T2 V2
        {
            get { return v2; }
            set { v2 = value; }
        }
    }

    /// <summary>
    ///  3个泛型的可用复用的实体类
    /// </summary>
    /// <typeparam name="T1">类型1</typeparam>
    /// <typeparam name="T2">类型2</typeparam>
    /// <typeparam name="T3">类型3</typeparam>
    [Serializable]
    public class ComboxClass<T1, T2, T3> : ComboxClass<T1, T2>
    {
        private T3 v3;

        /// <summary>
        /// Gets or sets V3
        /// </summary>
        public T3 V3
        {
            get { return v3; }
            set { v3 = value; }
        }
    }

    /// <summary>
    /// ComboxClass<T1, T2, T3, T4>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    [Serializable]
    public class ComboxClass<T1, T2, T3, T4>: ComboxClass<T1, T2, T3>
    {
        private T4 v4;

        public T4 V4
        {
            get { return v4; }
            set { v4 = value; }
        }
    }

    /// <summary>
    /// ComboxClass<T1, T2, T3, T4, T5>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    [Serializable]
    public class ComboxClass<T1, T2, T3, T4, T5>: ComboxClass<T1, T2, T3, T4>
    {
        private T5 v5;

        public T5 V5
        {
            get { return v5; }
            set { v5 = value; }
        }
    }

    /// <summary>
    /// ComboxClass<T1, T2, T3, T4, T5,T6>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    [Serializable]
    public class ComboxClass<T1, T2, T3, T4, T5, T6> : ComboxClass<T1, T2, T3, T4, T5>
    {
        private T6 v6;

        public T6 V6
        {
            get { return v6; }
            set { v6 = value; }
        }
    }

    /// <summary>
    /// ComboxClass<T1, T2, T3, T4, T5>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    [Serializable]
    public class ComboxClass<T1, T2, T3, T4, T5, T6,T7> : ComboxClass<T1, T2, T3, T4, T5,T6>
    {
        private T7 v7;

        public T7 V7
        {
            get { return v7; }
            set { v7 = value; }
        }
    }
}