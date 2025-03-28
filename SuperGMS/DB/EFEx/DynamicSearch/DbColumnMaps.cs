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
    public class _DbColumnMaps
    {
        private Dictionary<string, string> _dbContextFiledMaps = null;
        private Dictionary<string, string> _dbKeyHumpMaps = null;
        private Dictionary<string, List<string>> _dbTableFileds = null;
        private Dictionary<string, string> _dbFiledComment = null;

        private Type _dbContextType;

        /// <summary>
        /// 根据DbContext 获取字段映射关系
        /// 注意：已知问题: 如果在.net core 2.0版本，视图未定义主键会引起异常。
        ///      .netcore 2.1 版本已经修改该bug.
        /// </summary>
        /// <returns>映射表</returns>
        public void InitDbContextFiledMaps<TContext>() where TContext : DbContext
        {
            if (_dbContextFiledMaps == null)
            {
                _dbContextFiledMaps = new Dictionary<string, string>();
                _dbKeyHumpMaps = new Dictionary<string, string>();
                _dbTableFileds = new Dictionary<string, List<string>>();
                _dbFiledComment = new Dictionary<string, string>();
                _dbContextType = typeof(TContext);
                var optionBuilder = new InMemoryDBContextOptionBuilder();
                var option = optionBuilder.CreateOptionsBuilder<TContext>(String.Empty);
                using (DbContext dbContext = (DbContext)Activator.CreateInstance(typeof(TContext), option.Options))
                {
                    var types = dbContext.Model.GetEntityTypes();
                    foreach (var entityType in types)
                    {
                        var props = entityType.GetProperties();
                        var displayName = entityType.DisplayName();
                        _dbTableFileds.Add(displayName, props.Select(x => x.Name).ToList());
                        // 可以从Annotations获取tableName
                        foreach (var property in props)
                        {
                            var anns = property?.GetAnnotations();
                            if (anns == null || !anns.Any())
                            {
                                continue;
                            }
                            var pName = property.Name;
                            var columnName = anns.Where(a => a.Name.Contains("ColumnName"))?.FirstOrDefault()?.Value?.ToString();
                            var columnComment = anns.Where(a => a.Name.Contains("Comment"))?.FirstOrDefault()?.Value?.ToString();
                            if (!string.IsNullOrEmpty(columnName))
                            {

                                if (!_dbContextFiledMaps.ContainsKey(pName))
                                {
                                    _dbContextFiledMaps.Add(pName, columnName);
                                }
                                if (!_dbKeyHumpMaps.ContainsKey(columnName))
                                {
                                    _dbKeyHumpMaps.Add(columnName, pName);
                                }
                            }
                            if (!string.IsNullOrEmpty(columnComment))
                            {
                                var key = $"{displayName}_{pName}"; // 表名_字段名
                                if (!_dbFiledComment.ContainsKey(key))
                                    _dbFiledComment.Add(key, columnComment);
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<string, string> GetDbContextFiledMaps()
        {
            if (_dbContextFiledMaps == null)
            {
                _dbContextFiledMaps = new Dictionary<string, string>();
            }

            return _dbContextFiledMaps;
        }

        /// <summary>
        /// 获取指定tableName或者viewName包含的字段列表
        /// </summary>
        /// <param name="tableName">tableName或者viewName</param>
        /// <returns></returns>
        public List<string> GetDbContextTableFileds(string tableName)
        {
            if (_dbTableFileds == null || !_dbTableFileds.ContainsKey(tableName))
                return null;
            return _dbTableFileds[tableName];
        }

        /// <summary>
        /// 获取指定字段的数据库备注
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="filedName"></param>
        /// <returns></returns>
        public string GetDbContextTableFiledComment(string tableName,string filedName)
        {
            var key = $"{tableName}_{filedName}";
            if (_dbFiledComment == null || !_dbFiledComment.ContainsKey(key))
                return null;
            return _dbFiledComment[key];
        }

        public Dictionary<string, string> GetKeyHumpMaps()
        {
            if (_dbKeyHumpMaps == null)
                _dbKeyHumpMaps = new Dictionary<string, string>();
            return _dbKeyHumpMaps;
        }

        public Type GetDbContextType()
        {
            return _dbContextType;
        }
    }

    public class DbColumnMaps
    {
        private static Dictionary<string, _DbColumnMaps> _contextMaps = new Dictionary<string, _DbColumnMaps>();

        /// <summary>
        /// 初始化DbContextFiled的对照表
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        public static void InitDbContextFiledMaps<TContext>() where TContext : DbContext
        {
            var dbContestName = typeof(TContext).Name.ToLower();
            if (!_contextMaps.ContainsKey(dbContestName))
            {
                var columnMaps = new _DbColumnMaps();
                _contextMaps.Add(dbContestName, columnMaps);
                columnMaps.InitDbContextFiledMaps<TContext>();
            }
        }

        private static _DbColumnMaps getContext(string contextName)
        {
            var ctxName = contextName.ToLower();
            if (_contextMaps.ContainsKey(ctxName))
                return _contextMaps[ctxName];
            return null;
        }

        /// <summary>
        /// 获取指定字段的数据库备注
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="filedName"></param>
        /// <returns></returns>
        public static string GetDbContextTableFiledComment<TContext>(string tableName,string filedName) where TContext : DbContext
        {
            var dbContestName = typeof(TContext).Name;
            return getContext(dbContestName)?.GetDbContextTableFiledComment(tableName,filedName);
        }


        /// <summary>
        /// 获取指定tableName或者viewName包含的字段列表
        /// </summary>
        /// <param name="tableName">tableName或者viewName</param>
        /// <returns></returns>
        public static List<string> GetDbContextTableFileds<TContext>(string tableName) where TContext : DbContext
        {
            var dbContestName = typeof(TContext).Name;
            return getContext(dbContestName)?.GetDbContextTableFileds(tableName);
        }

        public static Dictionary<string, string> GetDbContextFiledMaps<TContext>() where TContext : DbContext
        {
            var dbContestName = typeof(TContext).Name;
            return getContext(dbContestName)?.GetDbContextFiledMaps();
        }

        public static Dictionary<string, string> GetDbContextFiledMaps(string dbContextName)
        {
            return getContext(dbContextName)?.GetDbContextFiledMaps();
        }
    }
}