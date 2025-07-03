using SqlSugar;
using SuperGMS.DB.EFEx.GrantDbFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.DB.EFEx.MyDbContext
{
    public class SqlSugarDBContext : ISqlSugarDbContext
    {
        private DbInfo dbInfo;
        public DbInfo DbInfo => dbInfo;
        private ISqlSugarClient sqlSugarClient;

        public SqlSugarDBContext(DbInfo info)
        {
            dbInfo = info;
        }

        public void Dispose()
        {
            sqlSugarClient?.Ado.Close();
            sqlSugarClient?.Ado.Dispose();
            sqlSugarClient?.Dispose();
            dbInfo=null;
        }

        public ISqlSugarClient GetRepository()
        {
            if(sqlSugarClient!=null) throw new Exception("SqlSugarDBContext.GetRepository()，在一个rpc上下文只能获取一次");
            return sqlSugarClient = SqlRepositoryManager.GetSqlSugarClient(dbInfo);
        }
    }
}
