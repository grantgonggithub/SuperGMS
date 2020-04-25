using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrantMicroService.Audit.Model
{
    public class AuditConfig
    {
        public string TTID { set; get; }
        public string SYS_ID { set; get; }
        public string WH_ID { set; get; }
        public string DB_NAME { set; get; }
        public string TABLE_NAME { set; get; }
        public string FILED_NAME { set; get; }
        public int IS_PRIMARY_KEY { set; get; }
        public int IS_AUDIT { set; get; }

        /// <summary>
        /// 缓存Key前缀
        /// </summary>
        /// <returns></returns>
        public static string GetCachePrefix()
        {
            return "QTFramework.AuditConfig";
        }
        /// <summary>
        /// 根据租户ID，DbName获取审计配置信息。审计配置信息缓存10分钟。
        /// </summary>
        /// <param name="ttid"></param>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static List<AuditConfig> GetAuditConfig(string ttid, string dbName, string tableName)
        {
            var temp = new List<AuditConfig>()
            {
                new AuditConfig
                {
                    DB_NAME="demo", FILED_NAME = "ItemGID", IS_AUDIT = 0, IS_PRIMARY_KEY = 1, TABLE_NAME="item", TTID="fuji"
                },
                new AuditConfig
                {
                    DB_NAME="demo", FILED_NAME = "ItemID", IS_AUDIT = 0, IS_PRIMARY_KEY = 1, TABLE_NAME="item", TTID="fuji"
                },
                new AuditConfig
                {
                    DB_NAME="demo", FILED_NAME = "ItemDesc", IS_AUDIT = 1, IS_PRIMARY_KEY = 0, TABLE_NAME="item", TTID="fuji"
                }
            };

            return temp;
        }
    }
}
