using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SuperGMS.ExceptionEx;

namespace SuperGMS.DB.EFEx.DynamicSearch
{
    /// <summary>
    /// DbContext 中的属性 与 数据库列的对应关系 , 用作SearchParameter 转换 Sql 使用.
    /// </summary>
    public class DbColumnMaps
    {
        private static Dictionary<string, string> _dbContextFiledMaps = null;
        private static Dictionary<string, string> _dbKeyHumpMaps = null;

        private static Type _dbContextType;

        /// <summary>
        /// 根据DbContext 获取字段映射关系
        /// 注意：已知问题: 如果在.net core 2.0版本，视图未定义主键会引起异常。
        ///      .netcore 2.1 版本已经修改该bug.
        /// </summary>
        /// <returns>映射表</returns>
        public static Dictionary<string, string> InitDbContextFiledMaps<TContext>() where TContext : DbContext
        {
            if (_dbContextFiledMaps == null)
            {
                _dbContextFiledMaps = new Dictionary<string, string>();
                _dbKeyHumpMaps = new Dictionary<string, string>();
                _dbContextType = typeof(TContext);
                var optionBuilder = new InMemoryDBContextOptionBuilder();
                var option = optionBuilder.CreateOptionsBuilder<TContext>(String.Empty);
                using (DbContext dbContext = (DbContext)Activator.CreateInstance(typeof(TContext), option.Options))
                {
                    var types = dbContext.Model.GetEntityTypes();
                    foreach (var entityType in types)
                    {
                        var props = entityType.GetProperties();

                        // 可以从Annotations获取tableName
                        foreach (var property in props)
                        {
                            var anns = property?.GetAnnotations();
                            if (anns == null || !anns.Any())
                            {
                                continue;
                            }
                            var columnName = anns.Where(a => a.Name.Contains("ColumnName"))?.FirstOrDefault()?.Value?.ToString();
                            if (string.IsNullOrEmpty(columnName))
                            {
                                continue;
                            }
                            var pName = property.Name;
                            if (!_dbContextFiledMaps.ContainsKey(pName))
                            {
                                _dbContextFiledMaps.Add(pName, columnName);
                            }
                            if (!_dbKeyHumpMaps.ContainsKey(columnName))
                            {
                                _dbKeyHumpMaps.Add(columnName, pName);
                            }
                        }
                    }
                }
            }

            return _dbContextFiledMaps;
        }

        public static Dictionary<string, string> GetDbContextFiledMaps()
        {
            if (_dbContextFiledMaps == null)
            {
                _dbContextFiledMaps = new Dictionary<string, string>();
            }

            return _dbContextFiledMaps;
        }

        public static Dictionary<string, string> GetKeyHumpMaps()
        {
            if (_dbKeyHumpMaps == null)
                _dbKeyHumpMaps = new Dictionary<string, string>();
            return _dbKeyHumpMaps;
        }

        public static Type GetDbContextType()
        {
            return _dbContextType;
        }
    }
}