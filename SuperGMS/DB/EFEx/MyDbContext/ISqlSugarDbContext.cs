using SqlSugar;
using SuperGMS.DB.EFEx.CrudRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.DB.EFEx.MyDbContext
{
    public interface ISqlSugarDbContext : IDbContext
    {
        ISqlSugarClient GetRepository();
    }
}
