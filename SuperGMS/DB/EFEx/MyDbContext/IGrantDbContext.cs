/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.DB.EFEx.GrantDbContext
 文件名：  IGrantDbContext
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/3 0:38:27

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperGMS.DB.EFEx
{
    /// <summary>
    /// IGrantDbContext
    /// modified by mark add Idisposable interface
    /// </summary>
   public interface IGrantDbContext : IDisposable
    {
        // void Commit();
    }
}
