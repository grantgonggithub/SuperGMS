/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.RpcProxyTools
 文件名：DefaultRedis
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2020/6/28 13:30:03

 功能描述：提供一个生成远程微服务的一个本地代理类

----------------------------------------------------------------*/

using System.Threading;

namespace SuperGMS.RpcProxyTools
{
    using System;

    using SuperGMS.Rpc.AssemblyTools;

    internal class Program
    {
        /// <summary>
        /// 1
        /// </summary>
        private static void Main(string[] args)
        {
            gotoLable:
            try
            {
                string serviceDllPath = string.Empty;
                string outPutPath;

                gotoLable1:
                Console.WriteLine("请输入要生成代理类的主DLL完整路径");
                serviceDllPath = Console.ReadLine();
                if (string.IsNullOrEmpty(serviceDllPath))
                {
                    goto gotoLable1;
                }

                gotoLable2:
                Console.WriteLine("请输入生成文件保存的路径，如：D:\\");
                outPutPath = Console.ReadLine();
                if (string.IsNullOrEmpty(outPutPath))
                {
                    goto gotoLable2;
                }

                string template = getFileTxt("ClassTemplate.txt");
                string templateBody = getFileTxt("ClassBodyTemplate.txt");
                string interfaceBodyTemplate = getFileTxt("InterfaceBodyTemplate.txt");
                AssemblyToolProxy proxy = new AssemblyToolProxy();

                if (proxy.Create(serviceDllPath, outPutPath, template, templateBody, interfaceBodyTemplate))
                {
                    Console.WriteLine($"生成完毕,请在:{outPutPath}查看生成文件");
                }
                else
                {
                    Console.WriteLine("生成失败");
                }

                Thread.Sleep(int.MaxValue);

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("生成失败:" + ex.Message + "   |   " + ex.StackTrace);
                if (args?.Length >= 2)
                {
                    return;
                }

                goto gotoLable;
            }
        }

        private static string getFileTxt(string filePath)
        {
            filePath = AppContext.BaseDirectory + filePath;
            return SuperGMS.Tools.FileHelper.ReadFile(filePath);
        }
    }
}