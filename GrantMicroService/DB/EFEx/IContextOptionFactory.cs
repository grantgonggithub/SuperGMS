/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.DB.EFEx
 文件名：  IContextOptionFactory
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 19:53:33

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GrantMicroService.DB.EFEx
{
    /// <summary>
    /// IContextOptionFactory
    /// </summary>
   public interface IContextOptionBuilderFactory
    {
        DbContextOptionsBuilder<T> CreateOptionsBuilder<T>(string connectionString)
            where T : DbContext;

        string GetConnectionString(DbInfo dbInfo);

    }
}
