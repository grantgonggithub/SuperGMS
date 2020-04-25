/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：GrantMicroService.DB.EFEx
 文件名：  DBContextOptionsManager
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2017/12/2 19:18:27

 功能描述：

----------------------------------------------------------------*/

using Microsoft.EntityFrameworkCore;

namespace GrantMicroService.DB.EFEx
{
    /// <summary>
    /// DBContextOptionsManager
    /// </summary>
    public class DBContextOptionsLoader
    {
        private static IContextOptionBuilderFactory optionFactory;

        private static object lockRoot = new object();

        /// <summary>
        /// 判断是否内存数据库，如果是则认为是在测试
        /// </summary>
        public static bool IsMemoryDb => optionFactory is InMemoryDBContextOptionBuilder;

        /// <summary>
        /// 基于EF的单元测试可以通过初始化自己的OptionBuilder
        /// </summary>
        /// <param name="optionFactory"></param>
        public static void Initlize(IContextOptionBuilderFactory optionFactory)
        {
            if (DBContextOptionsLoader.optionFactory == null)
            {
                lock (lockRoot)
                {
                    if (DBContextOptionsLoader.optionFactory == null)
                    {
                        DBContextOptionsLoader.optionFactory = optionFactory;
                    }
                }
            }
        }

        public static DbContextOptionsBuilder<T> CreateDbOption<T>(DbInfo dbInfo)
            where T : DbContext
        {
            // 这里是public,没办法防止外面随意调用，所以为了防止初始化后被随意改变，这里搞成单例
            if (optionFactory == null)
            {
                lock (lockRoot)
                {
                    if (optionFactory == null)
                    {
                        switch (dbInfo.DbType)
                        {
                            default:
                            case DbType.MySql:
                                optionFactory = new MySqlDBContextOptionBuilder();
                                break;

                            case DbType.Oracle:
                                optionFactory = new OracleDBContextOptionBuilder();
                                break;

                            case DbType.SqlServer:
                                optionFactory = new SqlServerDBContextOptionBuilder();
                                break;

                            case DbType.InMemory:
                                optionFactory = new InMemoryDBContextOptionBuilder();
                                break;
                            case DbType.PostgreSQL:
                                optionFactory = new PostgresqlDBContextOptionBuilder();
                                break;
                        }
                    }
                }
            }
            DbContextOptionsBuilder<T> options = optionFactory.CreateOptionsBuilder<T>(optionFactory.GetConnectionString(dbInfo));

            return options;
        }
    }
}