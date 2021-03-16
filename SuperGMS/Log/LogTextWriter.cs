/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Log
 文件名：LogTextWriter
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/21 11:31:08

 功能描述：

----------------------------------------------------------------*/
using SuperGMS.Tools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SuperGMS.Log
{
    /// <summary>
    /// 底层框架使用的日志本地保存为文件（日志组件未加载之前用）
    /// </summary>
    internal class LogTextWriter
    {
        private static Queue<string> tracingQueue = new Queue<string>();

        //当前文件名，文件名计数，写入数量
        private static ComboxClass<string, int, int> last = new ComboxClass<string, int, int>() { V1 = "first", V2 = 0 };

        private static object rootLock = new object();
        private static bool running = false;

        /// <summary>
        /// 记录文本日志
        /// </summary>
        /// <param name="ex"></param>
        public static void Write(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendFormat(
                    "[{0}]\r\n{1}\r\n[ExceptionMessage]:{2}\r\n[ExceptionStackTrace]:{3}\r\n-------------------------------------------------------------------------------\r\n",
                    "Error", ServiceEnvironment.EnvironmentInfo, ex.Message, ex.StackTrace);
                ex = ex.InnerException;
            }

            Write(sb.ToString());
        }

        /// <summary>
        /// 记录文本日志
        /// </summary>
        /// <param name="message">message</param>
        public static void Write(string message)
        {
            lock (rootLock)
            {
                tracingQueue.Enqueue(message);
                if (!running)
                {
                    start();  // 如果添加就看看当前队列是否处于工作状态，如果没有，就启动工作
                }
            }
        }

        private static void start()
        {
            try
            {
                Thread th = new Thread(new ThreadStart(get));
                th.Name = "LogTextThread";
                th.IsBackground = true;
                running = true;
                th.Start();
            }
            catch (Exception ex)
            {
            }
        }

        private static void get()
        {
            List<string> traceArry = new List<string>();
            while (true)
            {
                try
                {
                    lock (rootLock)
                    {
                        while (tracingQueue.Count > 0)
                        {
                            // 一次最多从队列里取出20条
                            traceArry.Add(tracingQueue.Dequeue());
                            if (traceArry.Count >= 20)
                            {
                                break;
                            }
                        }

                        if (traceArry.Count <= 0)
                        {
                            // 队列没有数据，可以退出了
                            running = false;
                            return;
                        }
                    }

                    WriteTextLog(string.Join("\r\n", traceArry.ToArray()));
                    traceArry.Clear();

                    // 给入队lock留点空隙，要不影响入队，
                    // 另外如果等待时间太短，会导致IO太频繁，
                    // 如果等待时间太长，会导致内存过高
                    Thread.Sleep(50);
                }
                catch
                {
                }
            }
        }

        private static void WriteTextLog(string message)
        {
            // Console.WriteLine(message);
            int i = 0;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

        goHere:
            try
            {
                string temp = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "txtLog" + Path.DirectorySeparatorChar + ServiceEnvironment.ComputerAddress.Replace(".", "_").Replace(":", "_") + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(temp))
                {
                    Directory.CreateDirectory(temp);
                }

                string fileName = "log" + DateTime.Now.ToString("yyyyMMddHH"); // 一个小时一个文件

                // 第一次肯定不同,说明一个小时内没有切换文件，是同一个文件，就需要判断写入量
                if (last.V1.ToLower() == fileName ||
                    last.V1.ToLower() == fileName + string.Format("({0})", last.V2)) // 当前小时没切换过
                {
                    // 30条*5000次=1.5W
                    if (last.V3 > 5000) // 同一个文件写入量不能太大，要不文件打不开了
                    {
                        last.V2 += 1; // 文件名计数+1
                        last.V1 = fileName = fileName + string.Format("({0})", last.V2);
                        last.V3 = 0;
                    }
                }
                else // 跟默认不同，跟扩展也不同，有两种情况：

                // 1、一个小时只有一个文件，到了下一个小时；2、上一个小时有多个文件，到了当前小时第一个文件
                {
                    if (last.V1 == "first") // 第一次也在这个判断
                    {
                        last.V1 = fileName;
                    }
                    else
                    {
                        last.V3 = 0;
                        last.V2 = 0;
                        last.V1 = fileName;
                    }
                }
                last.V3 += 1;
                String logFilePath = System.IO.Path.Combine(temp, last.V1 + ".txt");
                using (StreamWriter writer = new StreamWriter(logFilePath, true, Encoding.Default))
                {
                    writer.Write(message);
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(800);//失败往往是文件占用，可以停800毫秒
                if (i < 3)//做3次尝试，如果失败将会扔掉
                {
                    i += 1;
                    goto goHere;
                }
            }
        }
    }
}
