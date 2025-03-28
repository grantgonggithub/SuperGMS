using System;
using System.Collections.Generic;

namespace SuperGMS.FileEx
{
    using System.Timers;

    /// <summary>
    /// 通用自定义按钮基类
    /// 继承此类后,需要重写Excute 即可实现自定义按钮功能,
    /// 执行到结束请返回提示信息, 如果需要提示错误信息, 可以直接抛出 BusinessException
    /// </summary>
    public class CommonUdfButtonJob : IUdfButtonJob
    {
        public virtual string Excute(List<string> listObject, string buttonId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步删除文件, 主要是为了给自定义打印实现一个辅助函数
        /// </summary>
        /// <param name="delaySencods">延迟秒数</param>
        /// <param name="fileFullPath">文件完成物理路径</param>
        public void DeleteFileAsyn(double delaySencods, string fileFullPath)
        {
            var deleteFileTimer = new MyTimer(delaySencods * 1000, this.DeleteFileDelegate) { filePath = fileFullPath };
            deleteFileTimer.Start();
        }

        /// <summary>
        /// 删除文件委托方法
        /// </summary>
        /// <param name="sender">myTimer</param>
        /// <param name="e">参数</param>
        private void DeleteFileDelegate(object sender, System.Timers.ElapsedEventArgs e)
        {
            //var myTimer = (MyTimer)sender;
            //if (File.Exists(myTimer.filePath))
            //{
            //    File.Delete(myTimer.filePath);
            //}
            //try
            //{
            //    int idx = myTimer.filePath.LastIndexOf("\\");
            //    string fileName = myTimer.filePath.Substring(idx + 1);
            //    OperFtp ftpClient = new OperFtp(FtpInfo.GetDefault());
            //    ftpClient.DeleteFile(fileName);
            //}
            //catch { }
        }

        /// <summary>
        /// 自定义Timer 用来删除文件
        /// </summary>
        public class MyTimer : System.Timers.Timer
        {
            public MyTimer(double interval, ElapsedEventHandler handler)
                : base(interval)
            {
                this.Elapsed += handler;
                this.AutoReset = false;
            }

            /// <summary>
            /// 要删除的文件路径
            /// </summary>
            public string filePath { get; set; }
        }
    }
}