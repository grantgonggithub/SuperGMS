using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GrantMicroService.DB.EFEx.Udf
{
    /// <summary>
    /// 获取实体对象的主键值,使用了 延迟加载模式,并且使用了内存缓存提高性能
    /// 代码来源 http://michaelmairegger.wordpress.com/2013/03/30/find-primary-keys-from-entities-from-dbcontext/
    /// </summary>
    public sealed class EntityKeyHelper
    {
        private static readonly Lazy<EntityKeyHelper> LazyInstance = new Lazy<EntityKeyHelper>(() => new EntityKeyHelper());
        private readonly Dictionary<Type, string[]> _dict = new Dictionary<Type, string[]>();
        private EntityKeyHelper() { }

        public static EntityKeyHelper Instance
        {
            get { return LazyInstance.Value; }
        }
        /// <summary>
        /// 获取主键名,增加了Lock,支持并发操作
        /// Add By Grant
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns>主键数组</returns>
        public string[] GetKeyNames<T>(DbContext context) where T : class
        {
            throw new NotImplementedException();
            //Type t = typeof(T);

            ////retreive the base type
            //while (t.BaseType != typeof(object))
            //    t = t.BaseType;

            //string[] keys;
            //string[] keyNames;
            //lock (_dict)
            //{
            //    _dict.TryGetValue(t, out keys);
            //    if (keys != null)
            //        return keys;

            //    ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;

            //    //create method CreateObjectSet with the generic parameter of the base-type
            //    MethodInfo method = typeof(ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes)
            //                                             .MakeGenericMethod(t);
            //    dynamic objectSet = method.Invoke(objectContext, null);

            //    IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

            //    keyNames = keyMembers.Select(k => (string)k.Name).ToArray();

            //    _dict.Add(t, keyNames);
            //}

            //return keyNames;
        }

        /// <summary>
        /// 获取主键名
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string[] GetKeyNames(DbContext context, Type type)
        {
            throw new NotImplementedException();
            //Type t = type;

            ////retreive the base type
            //while (t.BaseType != typeof(object))
            //    t = t.BaseType;

            //string[] keys;

            //_dict.TryGetValue(t, out keys);
            //if (keys != null)
            //    return keys;

            //ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;

            ////create method CreateObjectSet with the generic parameter of the base-type
            //MethodInfo method = typeof(ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes)
            //                                         .MakeGenericMethod(t);
            //dynamic objectSet = method.Invoke(objectContext, null);

            //IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

            //string[] keyNames = keyMembers.Select(k => (string)k.Name).ToArray();

            //_dict.Add(t, keyNames);

            //return keyNames;
        }

        /// <summary>
        /// 获取主键内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object[] GetKeys<T>(T entity, DbContext context) where T : class
        {
            var keyNames = GetKeyNames<T>(context);
            Type type = typeof(T);

            object[] keys = new object[keyNames.Length];
            for (int i = 0; i < keyNames.Length; i++)
            {
                keys[i] = type.GetProperty(keyNames[i]).GetValue(entity, null);
            }
            return keys;
        }

        /// <summary>
        /// 获取主键内容
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object[] GetKeys(object entity, DbContext context, Type type)
        {
            var keyNames = GetKeyNames(context, type);
            object[] keys = new object[keyNames.Length];
            for (int i = 0; i < keyNames.Length; i++)
            {
                keys[i] = type.GetProperty(keyNames[i]).GetValue(entity, null);
            }
            return keys;
        }
    }

}
