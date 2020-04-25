using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using GrantMicroService.ApiHelper.Xml;
using GrantMicroService.AttributeEx;
using GrantMicroService.Tools;

namespace GrantMicroService.ApiHelper
{
    /// <summary>
    /// 解析类信息
    /// </summary>
    public class ObjectGeneratorEx
    {
        /// <summary>
        /// 默认集合深度
        /// </summary>
        internal const int DefaultCollectionSize = 1;
        public ApiClassInfo ClassInfo { get; set; }
        private readonly SimpleTypeObjectGenerator simpleObjectGenerator = new SimpleTypeObjectGenerator();

        public ObjectGeneratorEx()
        {
        }
       
        public ApiClassInfo GenerateObject(Type type)
        {
            return GenerateObject(type, new Dictionary<Type, object>());
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Here we just want to return null if anything goes wrong.")]
        private ApiClassInfo GenerateObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            try
            {
                if (SimpleTypeObjectGenerator.CanGenerateObject(type))
                {
                    return new ApiClassInfo(type, ApiPropertyType.SimpleObject);
                }

                if (type.Name.StartsWith("<>f__AnonymousType"))
                {
                    return GenerateComplexObject(type, createdObjectReferences);
                }

                if (type.IsArray)
                {
                    return GenerateArray(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type.IsGenericType)
                {
                    return GenerateGenericType(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IDictionary))
                {
                    return GenerateDictionary(typeof(Hashtable), DefaultCollectionSize, createdObjectReferences);
                }

                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    return GenerateDictionary(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IList) ||
                    type == typeof(IEnumerable) ||
                    type == typeof(ICollection))
                {
                    return GenerateCollection(typeof(ArrayList), DefaultCollectionSize, createdObjectReferences);
                }

                if (typeof(IList).IsAssignableFrom(type))
                {
                    return GenerateCollection(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IQueryable))
                {
                    return GenerateQueryable(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type.IsEnum)
                {
                    return GenerateEnum(type);
                }

                if (type.IsPublic || type.IsNestedPublic)
                {
                    return GenerateComplexObject(type, createdObjectReferences);
                }
            }
            catch (Exception ex)
            {
                // Returns null if anything fails
                Trace.WriteLine(ex.Message);
                return null;
            }

            return null;
        }

        private ApiClassInfo GenerateGenericType(Type type, int collectionSize, Dictionary<Type, object> createdObjectReferences)
        {
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Nullable<>))
            {
                return GenerateNullable(type, createdObjectReferences);
            }

            if (genericTypeDefinition == typeof(KeyValuePair<,>))
            {
                return GenerateKeyValuePair(type, createdObjectReferences);
            }

            if (IsTuple(genericTypeDefinition))
            {
                return GenerateTuple(type, createdObjectReferences);
            }

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 1)
            {
                if (genericTypeDefinition == typeof(IList<>) ||
                    genericTypeDefinition == typeof(IEnumerable<>) ||
                    genericTypeDefinition == typeof(ICollection<>))
                {
                    Type collectionType = typeof(List<>).MakeGenericType(genericArguments);
                    return GenerateCollection(collectionType, collectionSize, createdObjectReferences);
                }

                if (genericTypeDefinition == typeof(IQueryable<>))
                {
                    return GenerateQueryable(type, collectionSize, createdObjectReferences);
                }

                Type closedCollectionType = typeof(ICollection<>).MakeGenericType(genericArguments[0]);
                if (closedCollectionType.IsAssignableFrom(type))
                {
                    return GenerateCollection(type, collectionSize, createdObjectReferences);
                }
            }

            if (genericArguments.Length == 2)
            {
                if (genericTypeDefinition == typeof(IDictionary<,>))
                {
                    Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericArguments);
                    return GenerateDictionary(dictionaryType, collectionSize, createdObjectReferences);
                }

                Type closedDictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]);
                if (closedDictionaryType.IsAssignableFrom(type))
                {
                    return GenerateDictionary(type, collectionSize, createdObjectReferences);
                }
            }

            if (type.IsPublic || type.IsNestedPublic)
            {
                return GenerateComplexObject(type, createdObjectReferences);
            }

            return null;
        }

        private ApiClassInfo GenerateTuple(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            var classInfo = new ApiClassInfo(type, ApiPropertyType.ClassObject);
            Type[] genericArgs = type.GetGenericArguments();
            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                var ci = objectGenerator.GenerateObject(genericArgs[i], createdObjectReferences);
                ci.Name = $"Item{i + 1}";
                classInfo.Properties.Add(ci);
            }
            
            return classInfo;
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

        private ApiClassInfo GenerateKeyValuePair(Type keyValuePairType, Dictionary<Type, object> createdObjectReferences)
        {
            var classInfo = new ApiClassInfo(keyValuePairType, ApiPropertyType.ClassObject);
            Type[] genericArgs = keyValuePairType.GetGenericArguments();
            Type typeK = genericArgs[0];
            Type typeV = genericArgs[1];
            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            var keyObject = objectGenerator.GenerateObject(typeK, createdObjectReferences);
            var valueObject = objectGenerator.GenerateObject(typeV, createdObjectReferences);
            keyObject.Name = "Key";
            valueObject.Name = "Value";
            classInfo.Properties.Add(keyObject);
            classInfo.Properties.Add(valueObject);
           
            return classInfo;
        }

        private ApiClassInfo GenerateArray(Type arrayType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            var classInfo = new ApiClassInfo(arrayType, ApiPropertyType.ArrayObject);

            Type type = arrayType.GetElementType();
            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            for (int i = 0; i < size; i++)
            {
                var ci = objectGenerator.GenerateObject(type, createdObjectReferences);
                classInfo.Properties.Add(ci);
            }
           
            return classInfo;
        }

        private ApiClassInfo GenerateDictionary(Type dictionaryType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            var classInfo = new ApiClassInfo(dictionaryType, ApiPropertyType.ArrayObject);
            Type typeK = typeof(object);
            Type typeV = typeof(object);
            if (dictionaryType.IsGenericType)
            {
                Type[] genericArgs = dictionaryType.GetGenericArguments();
                typeK = genericArgs[0];
                typeV = genericArgs[1];
            }

            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            for (int i = 0; i < size; i++)
            {
                var ci = new ApiClassInfo(dictionaryType, ApiPropertyType.ClassObject);
                var newKey = objectGenerator.GenerateObject(typeK, createdObjectReferences);
                var newValue = objectGenerator.GenerateObject(typeV, createdObjectReferences);
                newKey.Name = "Key";
                newValue.Name = "Value";
                ci.Properties.Add(newKey);
                ci.Properties.Add(newValue);
                classInfo.Properties.Add(ci);
            }
            
            return classInfo;
        }

        private ApiClassInfo GenerateEnum(Type enumType)
        {
            //var possibleNames = Enum.GetNames(enumType);
            var possibleValues = Enum.GetValues(enumType);
            var classInfo = new ApiClassInfo(enumType, ApiPropertyType.SimpleObject);
            for (int i =0;i< possibleValues.Length;i++)
            {
                classInfo.Remark +=  ((int)possibleValues.GetValue(i)).ToString();
                if (i < (possibleValues.Length -1)) classInfo.Remark += ",";
            }
            return classInfo;
        }

        private ApiClassInfo GenerateQueryable(Type queryableType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            bool isGeneric = queryableType.IsGenericType;
            ApiClassInfo list;
            if (isGeneric)
            {
                Type listType = typeof(List<>).MakeGenericType(queryableType.GetGenericArguments());
                list = GenerateCollection(listType, size, createdObjectReferences);
            }
            else
            {
                list = GenerateArray(typeof(object[]), size, createdObjectReferences);
            }

            return list;
        }

        private ApiClassInfo GenerateCollection(Type collectionType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            var classInfo = new ApiClassInfo(collectionType, ApiPropertyType.ArrayObject);
            Type type = collectionType.IsGenericType ?
                collectionType.GetGenericArguments()[0] :
                typeof(object);
            bool areAllElementsNull = true;
            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            for (int i = 0; i < size; i++)
            {
                var ci = objectGenerator.GenerateObject(type, createdObjectReferences);
                classInfo.Properties.Add(ci);
            }

            return classInfo;
        }

        private ApiClassInfo GenerateNullable(Type nullableType, Dictionary<Type, object> createdObjectReferences)
        {
            Type type = nullableType.GetGenericArguments()[0];
            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            var ci= objectGenerator.GenerateObject(type, createdObjectReferences);
            ci.CanNull = true;
            return ci;
        }

        private ApiClassInfo GenerateComplexObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            var classInfo = new ApiClassInfo(type, ApiPropertyType.ClassObject);
            var at = type.GetCustomAttributes(typeof(UdfModelAttribute), true);
            if (at.Length > 0)
            {
                classInfo.UdfModel = at[0] as UdfModelAttribute;
            }

            if (createdObjectReferences.TryGetValue(type, out var result))
            {
                // The object has been created already, just return it. This will handle the circular reference case.
                return result as ApiClassInfo;
            }

            createdObjectReferences.Add(type, classInfo);

            var list = SetPublicProperties(type, createdObjectReferences);
            if (list.Count > 0)
            {
                classInfo.Properties.AddRange(list);
            }

            list = SetPublicFields(type, createdObjectReferences);
            if (list.Count > 0)
            {
                classInfo.Properties.AddRange(list);
            }

            if (classInfo.Properties.Count <= 0)
            {
                classInfo.JsonType = ApiPropertyType.SimpleObject;
            }

           
            return classInfo;
        }

        
        private List<ApiClassInfo> SetPublicProperties(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            List<ApiClassInfo> infos = new List<ApiClassInfo>();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite || property.CanRead)
                {
                    var ci = objectGenerator.GenerateObject(property.PropertyType, createdObjectReferences);
                    ci.Name = property.Name;
                    infos.Add(ci);
                }
            }
            return infos;
        }

        private List<ApiClassInfo> SetPublicFields(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            List<ApiClassInfo> infos = new List<ApiClassInfo>();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            ObjectGeneratorEx objectGenerator = new ObjectGeneratorEx();
            foreach (FieldInfo field in fields)
            {
                var ci = objectGenerator.GenerateObject(field.FieldType, createdObjectReferences);
                ci.Name = field.Name;
                infos.Add(ci);
            }

            return infos;
        }

        public object GenerateSimpleObject(Type type)
        {
            if (SimpleTypeObjectGenerator.CanGenerateObject(type))
            {
                return this.simpleObjectGenerator.GenerateObject(type);
            }
            else
            {
                if (type.IsValueType)
                {
                    return 0;
                }
                else
                {
                    return new object();
                }
            }
        }

        private class SimpleTypeObjectGenerator
        {
            private static readonly Dictionary<Type, Func<long, object>> DefaultGenerators = InitializeGenerators();

            private long index = 0;

            /// <summary>
            /// 是否可创建为一个对象
            /// </summary>
            /// <param name="type">类型</param>
            /// <returns>判断标记</returns>
            public static bool CanGenerateObject(Type type)
            {
                return DefaultGenerators.ContainsKey(type);
            }

            public object GenerateObject(Type type)
            {
                return DefaultGenerators[type](++index);
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple type factories and cannot be split up.")]
            private static Dictionary<Type, Func<long, object>> InitializeGenerators()
            {
                return new Dictionary<Type, Func<long, object>>
                {
                    { typeof(Boolean), index => true },
                    { typeof(Byte), index => (Byte)64 },
                    { typeof(Char), index => (Char)65 },
                    { typeof(DateTime), index => DateTime.Now },
                    { typeof(DateTimeOffset), index => new DateTimeOffset(DateTime.Now) },
                    { typeof(DBNull), index => DBNull.Value },
                    { typeof(Decimal), index => (Decimal)index },
                    { typeof(Double), index => (Double)(index + 0.1) },
                    { typeof(Guid), index => Guid.NewGuid() },
                    { typeof(Int16), index => (Int16)(index % Int16.MaxValue) },
                    { typeof(Int32), index => (Int32)(index % Int32.MaxValue) },
                    { typeof(Int64), index => (Int64)index },
                    { typeof(Object), index => new object() },
                    { typeof(SByte), index => (SByte)64 },
                    { typeof(Single), index => (Single)(index + 0.1) },
                    { typeof(String), index => String.Format(CultureInfo.CurrentCulture, "sample string {0}", index) },
                    { typeof(TimeSpan), index =>TimeSpan.FromTicks(1234567) },
                    { typeof(UInt16), index => (UInt16)(index % UInt16.MaxValue) },
                    { typeof(UInt32), index => (UInt32)(index % UInt32.MaxValue) },
                    { typeof(UInt64), index => (UInt64)index },
                    { typeof(Uri), index => new Uri(String.Format(CultureInfo.CurrentCulture, "http://webapihelppage{0}.com", index)) },
                };
            }
        }

       
    }

   
}
