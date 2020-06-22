/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Utility
 文件名：ServiceEnvironment
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/16 11:53:25

 功能描述：一个获取当前程序运行环境的辅助类

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using SuperGMS.Config;
using SuperGMS.Log;

namespace SuperGMS.Tools
{
    /// <summary>
    /// 一个获取当前程序运行环境的辅助类
    /// </summary>
    public static class ServiceEnvironment
    {
        #region Private Members
        private static string _serviceName;
        private static string _computerName;
        private static string _computerAddress;
        private static string _processInfo;
        private static string _workPath;
        private static int _pid;
        private static List<string> _ipList=new List<string>();

        private static string _environmentInfo;
        #endregion

        #region Static Constructor
        static ServiceEnvironment()
        {
            Process process = Process.GetCurrentProcess();

            _serviceName = GetAppNamespace(process);
            _computerName = Environment.MachineName;
            _workPath =AppContext.BaseDirectory;

            _pid = process.Id;
            _processInfo = string.Format("{0}-{1}", _pid, process.ProcessName);



            try
            {
                // 这种方式在linux上面只能获取到一个127.0.0.1,其他什么都获取不到，只能用下面的方式，DNS方式只适合winx  遍历网卡方式都适用
                //string host = Dns.GetHostName();
                //var addrs = Dns.GetHostAddresses(host);
                //foreach (var a in addrs)
                //{
                //    if (a.AddressFamily == AddressFamily.InterNetwork)
                //    {
                //        if (a.ToString() == "127.0.0.1" ||string.IsNullOrEmpty(a.ToString()))
                //        {
                //           continue;
                //        }
                //        //_computerAddress = a.ToString();
                //        //break;
                //        if (_ipList.Count==0) // 第一个
                //        {
                //            _computerAddress = a.ToString();
                //        }
                //        _ipList.Add(a.ToString());
                //    }
                //}

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var item in interfaces)
                {
                    if (item.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        IPInterfaceProperties ipInterfaceProperties = item.GetIPProperties();
                        UnicastIPAddressInformationCollection ipCollection = ipInterfaceProperties.UnicastAddresses;
                        foreach (UnicastIPAddressInformation ipadd in ipCollection)
                        {
                            var ip = ipadd.Address;
                            if (ip.AddressFamily == AddressFamily.InterNetwork)
                            {
                                if (ip.ToString() == "127.0.0.1" || string.IsNullOrEmpty(ip.ToString()))
                                {
                                    continue;
                                }

                                if (_ipList.Count == 0) // 第一个
                                {
                                    _computerAddress = ip.ToString();
                                }
                                _ipList.Add(ip.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                _computerAddress = "0.0.0.0";
            }

            _environmentInfo = string.Format("[serviceName]:{0} [computerName]:{1} [workPath]:{2} [processInfo]:{3} [computerAddress]:{4}",
                ServerSetting.AppName, _computerName, _workPath, _processInfo, _computerAddress);
        }
        #endregion

        #region Public Properties
        /// <summary>进程运行路径</summary>
        public static string WorkPath
        {
            get { return _workPath; }
        }

        /// <summary>进程Id</summary>
        public static int ProcessId
        {
            get { return _pid; }
        }

        /// <summary>进程消息</summary>
        public static string ProcessInfo
        {
            get { return _processInfo; }
        }

        /// <summary>服务名称</summary>
        public static string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        /// <summary>计算机名称</summary>
        public static string ComputerName
        {
            get { return _computerName; }
        }

        /// <summary>本机服务器地址, 自动获取</summary>
        public static string ComputerAddress
        {
            get { return _computerAddress; }
            internal set { _computerAddress = value; }
        }

        public static List<string> IpList
        {
            get { return _ipList; }
        }

        /// <summary>
        /// 完整的运行环境信息
        /// </summary>
        public static string EnvironmentInfo
        {
            get { return _environmentInfo; }
            set { _environmentInfo = value; }
        }
        #endregion


        /// <summary>
        /// 获取当前进程名称
        /// </summary>
        /// <returns></returns>
        public static string GetAppNamespace(Process process)
        {
            return AppContext.BaseDirectory;

               // return HttpContext.Current != null ? GetProcessUserName(process.Id) : process.ProcessName;
        }

        /// <summary>
        /// 获取进程用户名, 因为可以通过应用程序池来区分
        /// </summary>
        /// <param name="pID">pId</param>
        /// <returns></returns>
        public static string GetProcessUserName(int pID)
        {
            //string text1 = null;
            //try
            //{
            //    SelectQuery query1 = new SelectQuery("Select * from Win32_Process WHERE processID=" + pID);
            //    ManagementObjectSearcher searcher1 = new ManagementObjectSearcher(query1);
            //    foreach (ManagementObject disk in searcher1.Get())
            //    {
            //        ManagementBaseObject inPar = null;
            //        ManagementBaseObject outPar = null;

            //        inPar = disk.GetMethodParameters("GetOwner");

            //        outPar = disk.InvokeMethod("GetOwner", inPar, null);

            //        text1 = outPar["User"].ToString();
            //        break;
            //    }
            //}
            //catch
            //{
            //    text1 = "SYSTEM";
            //}

            //return text1;
            return pID.ToString();
        }
    }
}
