using SuperGMS.Rpc;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SuperGMS.ApiHelper
{
    /// <summary>
    /// 解析类，
    /// </summary>
    internal class ParseTypeInfo
    {
        private static readonly Dictionary<Type, string> DefaultDicts = InitializeDicts();

        private readonly Type type;
        private readonly Assembly assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTypeInfo"/> class.
        /// 构造函数
        /// </summary>
        /// <param name="assembly">术语集合</param>
        /// <param name="type">类型参数</param>
        public ParseTypeInfo(Assembly assembly, Type type)
        {
            this.assembly = assembly;
            this.type = type;
        }

        /// <summary>
        /// Gets 程序集名称
        /// </summary>
        public string AssemblyName => assembly?.FullName;

        /// <summary>
        /// 解析给定的类
        /// </summary>
        /// <returns>类信息</returns>
        public ClassInfo Parse()
        {
            return ParseType(type, new Dictionary<Type, ClassInfo>());
        }

        private static Dictionary<Type, string> InitializeDicts()
        {
            return new Dictionary<Type, string>
            {
                { typeof(bool), "布尔值" },
                { typeof(byte), "8位整型值" },
                { typeof(char), "单个字符" },
                { typeof(DateTime), "日期" },
                { typeof(DateTimeOffset), string.Empty },
                { typeof(DBNull), "DBNull" },
                { typeof(decimal), "高精度数字" },
                { typeof(double), "双精度数字" },
                { typeof(Guid), "guid字符串" },
                { typeof(short), "16位整型" },
                { typeof(int), "32位整型" },
                { typeof(long), "64位整型" },
                { typeof(sbyte), "8位有符号整型值" },
                { typeof(float), "单精度数字" },
                { typeof(string), "字符串" },
                { typeof(TimeSpan), "时间段" },
                { typeof(ushort), "16位无符号整型" },
                { typeof(uint), "32位无符号整型" },
                { typeof(ulong), "64位无符号整型" },
                { typeof(Uri), "uri地址字符串" },
            };
        }

        /// <summary>
        /// 解析类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="createdObjectReferences">避免无限循环</param>
        /// <returns>类信息</returns>
        private ClassInfo ParseType(Type type, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            ClassInfo info = new ClassInfo(AssemblyName);
            if (type == null)
            {
                return info;
            }

            if (typeof(Nullables) == type)
            {
                return ClassInfo.Nullables;
            }

            // 简单类
            if (ParseTypeInfo.DefaultDicts.ContainsKey(type))
            {
                info.SetSimpleType(type, ParseTypeInfo.DefaultDicts[type]);
                return info;
            }

            if (type.IsArray)
            {
                info = ParseArray(type, createdObjectReferences);
                return info;
            }

            if (type.IsGenericType)
            {
                info = ParseGenericType(type, createdObjectReferences);
                return info;
            }

            if (type == typeof(IDictionary))
            {
                info = ParseDictionary(typeof(Hashtable), createdObjectReferences);
                return info;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                info = ParseDictionary(type, createdObjectReferences);
                return info;
            }

            if (type == typeof(IList) ||
                type == typeof(IEnumerable) ||
                type == typeof(ICollection))
            {
                info = ParseCollection(typeof(ArrayList), createdObjectReferences);
                return info;
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                info = ParseCollection(type, createdObjectReferences);
                return info;
            }

            if (type.IsPublic || type.IsNestedPublic)
            {
                info = ParseComplexObject(type, createdObjectReferences);
            }

            return info;
        }

        /// <summary>
        /// 解析数组
        /// </summary>
        /// <param name="arrayType">数组类型</param>
        /// <param name="createdObjectReferences">已有类型</param>
        /// <returns>类型西</returns>
        private ClassInfo ParseArray(Type arrayType, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            var info = new ClassInfo(AssemblyName)
            {
                Type = "数组集合",
                Name = "Array",
                FullName = arrayType.FullName,
            };
            var type = arrayType.GetElementType();
            info.PropertyInfo.Add(ParseType(type, createdObjectReferences));
            return info;
        }

        private ClassInfo ParseDictionary(Type dictionaryType, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            Type typeK = typeof(object);
            Type typeV = typeof(object);
            if (dictionaryType.IsGenericType)
            {
                Type[] genericArgs = dictionaryType.GetGenericArguments();
                typeK = genericArgs[0];
                typeV = genericArgs[1];
            }

            ClassInfo info = new ClassInfo(AssemblyName)
            {
                Type = "词典集合",
                Name = "Dict",
                FullName = dictionaryType.FullName,
            };
            info.PropertyInfo.Add(ParseType(typeK, createdObjectReferences));
            info.PropertyInfo.Add(ParseType(typeV, createdObjectReferences));
            return info;
        }

        private ClassInfo ParseCollection(Type collectionType, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            Type type = collectionType.IsGenericType ?
                collectionType.GetGenericArguments()[0] :
                typeof(object);

            ClassInfo info = new ClassInfo(AssemblyName)
            {
                Type = "集合",
                Name = "Collection",
                FullName = collectionType.FullName,
            };
            info.PropertyInfo.Add(ParseType(type, createdObjectReferences));
            return info;
        }

        private ClassInfo ParseGenericType(Type type, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Nullable<>))
            {
                var typeNull = type.GetGenericArguments()[0];
                return ParseType(typeNull, createdObjectReferences);
            }

            if (genericTypeDefinition == typeof(KeyValuePair<,>))
            {
                return ParseKeyValuePair(type, createdObjectReferences);
            }

            if (IsTuple(genericTypeDefinition))
            {
                return ParseTuple(type, createdObjectReferences);
            }

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 1)
            {
                if (genericTypeDefinition == typeof(IList<>) ||
                    genericTypeDefinition == typeof(IEnumerable<>) ||
                    genericTypeDefinition == typeof(ICollection<>))
                {
                    Type collectionType = typeof(List<>).MakeGenericType(genericArguments);
                    return ParseCollection(collectionType, createdObjectReferences);
                }

                Type closedCollectionType = typeof(ICollection<>).MakeGenericType(genericArguments[0]);
                if (closedCollectionType.IsAssignableFrom(type))
                {
                    return ParseCollection(type, createdObjectReferences);
                }
            }

            if (genericArguments.Length == 2)
            {
                if (genericTypeDefinition == typeof(IDictionary<,>))
                {
                    Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericArguments);
                    return ParseDictionary(dictionaryType, createdObjectReferences);
                }

                Type closedDictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]);
                if (closedDictionaryType.IsAssignableFrom(type))
                {
                    return ParseDictionary(type, createdObjectReferences);
                }
            }

            if (type.IsPublic || type.IsNestedPublic)
            {
                return ParseComplexObject(type, createdObjectReferences);
            }

            return null;
        }

        private bool IsTuple(Type genericTypeDefinition)
        {
            return genericTypeDefinition == typeof(Tuple<>) ||
                   genericTypeDefinition == typeof(Tuple<,>) ||
                   genericTypeDefinition == typeof(Tuple<,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,,,>) ||
                   genericTypeDefinition == typeof(Tuple<,,,,,,,>);
        }

        private ClassInfo ParseTuple(Type type, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            Type[] genericArgs = type.GetGenericArguments();
            object[] parameterValues = new object[genericArgs.Length];
            ClassInfo info = new ClassInfo(AssemblyName)
            {
                Type = "Tuple",
                Name = "Tuple",
                FullName = type.FullName,
            };

            for (int i = 0; i < genericArgs.Length; i++)
            {
                info.PropertyInfo.Add(ParseType(genericArgs[i], createdObjectReferences));
            }

            return info;
        }

        private ClassInfo ParseKeyValuePair(Type keyValuePairType, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            Type[] genericArgs = keyValuePairType.GetGenericArguments();
            Type typeK = genericArgs[0];
            Type typeV = genericArgs[1];

            ClassInfo info = ParseType(typeK, createdObjectReferences);
            info?.PropertyInfo.Add(ParseType(typeV, createdObjectReferences));

            return info;
        }

        private ClassInfo ParseComplexObject(Type complexType, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            ClassInfo cInfo = null;
            if (createdObjectReferences.TryGetValue(complexType, out cInfo))
            {
                // The object has been created already, just return it. This will handle the circular reference case.
                return cInfo;
            }

            object result = null;
            if (complexType.IsValueType)
            {
                result = Activator.CreateInstance(complexType);
            }
            else
            {
                ConstructorInfo defaultCtor = complexType.GetConstructor(Type.EmptyTypes);
                if (defaultCtor == null)
                {
                    return new ClassInfo(AssemblyName);
                }

                result = defaultCtor.Invoke(new object[0]);
            }

            ClassInfo info = new ClassInfo(AssemblyName)
            {
                Type = complexType.FullName,
                Name = complexType.Name,
                FullName = complexType.FullName,
            };
            info.PropertyInfo.AddRange(GetPublicProperties(complexType, result, createdObjectReferences));
            info.PropertyInfo.AddRange(GetPublicFields(complexType, result, createdObjectReferences));
            createdObjectReferences.Add(complexType, info);
            return info;
        }

        private List<ClassInfo> GetPublicProperties(Type type, object obj, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            List<ClassInfo> infos = new List<ClassInfo>();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                var info = ParseType(property.PropertyType, createdObjectReferences);
                info.Name = property.Name;
                infos.Add(info);
            }

            return infos;
        }

        private List<ClassInfo> GetPublicFields(Type type, object obj, Dictionary<Type, ClassInfo> createdObjectReferences)
        {
            List<ClassInfo> infos = new List<ClassInfo>();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                var info = ParseType(field.FieldType, createdObjectReferences);
                info.Name = field.Name;
                infos.Add(info);
            }

            return infos;
        }
    }
}