/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.Utility
 文件名：  ReTryTools
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/9/29 13:19:30

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GrantMicroService.Audit.Model;

namespace GrantMicroService.Tools
{
    /// <summary>
    /// ReTryTools
    /// </summary>
    public class ReTryTools
    {
        private static void SleepWhile()
        {
            Thread.Sleep(800);
        }

        /// <summary>
        /// 一个参数的方法重试
        /// </summary>
        /// <typeparam name="A1">参数类型</typeparam>
        /// <typeparam name="R">返回值类型</typeparam>
        /// <param name="fn">方法名</param>
        /// <param name="args1">参数</param>
        /// <param name="tryNum">重试次数，默认3次</param>
        /// <returns></returns>
        public static R ReTry<A1, R>(Func<A1, R> fn, A1 args1, int tryNum = 3)
        {
            if (fn == null) return default(R);
            int num = 0;
            gotoLable:
            try
            {
                return fn(args1);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }

        public static R ReTry<A1, A2, R>(Func<A1, A2, R> fn, A1 args1, A2 args2, int tryNum = 3)
        {
            if (fn == null) return default(R);
            int num = 0;
            gotoLable:
            try
            {
                return fn(args1, args2);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }

        public static R ReTry<A1, A2, A3, R>(Func<A1, A2, A3, R> fn, A1 args1, A2 args2, A3 args3, int tryNum = 3)
        {
            if (fn == null) return default(R);
            int num = 0;
            gotoLable:
            try
            {
                return fn(args1, args2, args3);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }

        public static R ReTry<A1, A2, A3,A4, R>(Func<A1, A2, A3,A4, R> fn, A1 args1, A2 args2, A3 args3,A4 args4, int tryNum = 3)
        {
            if (fn == null) return default(R);
            int num = 0;
            gotoLable:
            try
            {
                return fn(args1, args2, args3,args4);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }

        /// <summary>
        /// 无返回值得方法重试
        /// </summary>
        /// <typeparam name="A1"></typeparam>
        /// <param name="fn"></param>
        /// <param name="args1"></param>
        /// <param name="tryNum"></param>
        public static void ReTry<A1>(Action<A1> fn, A1 args1, int tryNum = 3)
        {
            int num = 0;
            gotoLable:
            try
            {
                fn(args1);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }

        public static void ReTry<A1, A2>(Action<A1, A2> fn, A1 args1, A2 args2, int tryNum = 3)
        {
            int num = 0;
            gotoLable:
            try
            {
                fn(args1, args2);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }

        public static void ReTry<A1, A2, A3>(Action<A1, A2, A3> fn, A1 args1, A2 args2, A3 args3, int tryNum = 3)
        {
            int num = 0;
            gotoLable:
            try
            {
                fn(args1, args2, args3);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }

        public static void ReTry<A1, A2, A3, A4>(Action<A1, A2, A3, A4> fn, A1 args1, A2 args2, A3 args3, A4 args4, int tryNum = 3)
        {
            int num = 0;
            gotoLable:
            try
            {
                fn(args1, args2, args3, args4);
            }
            catch (Exception ex)
            {
                num += 1;
                if (num < tryNum)
                {
                    SleepWhile();
                    goto gotoLable;
                }
                throw ex;
            }
        }
    }
}
