using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperGMS.AttributeEx;

namespace SuperGMS.ApiHelper
{
    /// <summary>
    /// api类信息
    /// </summary>
    public class ApiClassInfo
    {
        public ApiClassInfo()
        {
            this.JsonType = ApiPropertyType.SimpleObject;
        }
        public ApiClassInfo(Type type, ApiPropertyType pType)
        {
            this.Name = "";
            this.TypeName = type.FullName;
            this.JsonType = pType;
            if (this.JsonType != ApiPropertyType.SimpleObject)
            {
                this.Properties = new List<ApiClassInfo>();
            }
        }

        public List<ApiClassInfo> Properties { get; set; }
        public string Name { get; set; }

        public string TypeName { get; set; }

        public bool CanNull { get; set; }
        public string Remark { get; set; }
        public ApiPropertyType JsonType { get; set; }

        /// <summary>
        /// 是否定义了属性 UdfModel
        /// </summary>
        public UdfModelAttribute UdfModel { get; set; }

        public string ToJson(Func<ApiClassInfo, Task<List<FieldDescInfo>>> func, Dictionary<string, string> dict)
        {
            var o = this.CreateObject(func, dict);

            return Newtonsoft.Json.JsonConvert.SerializeObject(
                o,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });
        }

        public object CreateObject(Func<ApiClassInfo, Task<List<FieldDescInfo>>> func, Dictionary<string, string> dict, ApiClassInfo info = null)
        {
            if (info == null)
            {
                info = this;
            }

            var o = new ObjectGeneratorEx();
            if (info.JsonType == ApiPropertyType.SimpleObject)
            {
                var t = Type.GetType(info.TypeName);
                //如果没有类型，则应该是无属性的类，则序列化为对象
                if (t == null)
                {
                    var obj = o.GenerateSimpleObject(typeof(object));
                    return obj;
                }
                else
                {
                    var obj = o.GenerateSimpleObject(t);
                    return obj;
                }
            }
            else if (info.JsonType == ApiPropertyType.ArrayObject)
            {
                var len = info.Properties.Count;
                var objs = new object[len];
                for (int i = 0; i < len; i++)
                {
                    if (info.Properties[i] != null)
                    {
                        objs[i] = info.Properties[i].CreateObject(func, dict);
                    }
                }
                return objs;
            }
            else if (info.JsonType == ApiPropertyType.ClassObject)
            {
                if (info.Properties.Count <= 0)
                {
                    return new object();
                }

                dynamic dobj = new System.Dynamic.ExpandoObject();
                var d = (IDictionary<string, object>)dobj;
                var f = func(info).Result;

                foreach (var p in info.Properties)
                {
                    if (p != null)
                    {
                        //对属性就行整理
                        if (p.JsonType == ApiPropertyType.SimpleObject)
                        {
                            var s = GetFieldDesc(p,f,dict);
                            if (!string.IsNullOrEmpty(s))
                            {
                                d[p.Name] = s;
                            }
                            else
                            {
                                d[p.Name] = p.CreateObject(func, dict);
                            }
                        }
                        else
                        {
                            d[p.Name] = p.CreateObject(func, dict);
                        }
                    }
                }

                return d;
            }

            return null;
        }

        /// <summary>
        /// eg:*必输字段,【shippingorderid】,字符串类型,出库单ID
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cInfo"></param>
        /// <param name="fInfo"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private string GetFieldDesc(ApiClassInfo cInfo, List<FieldDescInfo> fInfo, Dictionary<string, string> dict)
        {
            if (cInfo == null)
            {
                return null;
            }

            var name = cInfo.Name;
            StringBuilder sb = new StringBuilder();
            if (fInfo != null)
            {
                if (fInfo.FirstOrDefault(x=>x.FieldName==name)?.IsRequired == 1)
                {
                    sb.AppendFormat("*{0},",R("必输字段", dict));
                }
            }
            sb.AppendFormat("【{0}】,", R(name, dict));
            sb.AppendFormat("{0},",GetTypeDesc(cInfo.TypeName, dict));
            if (fInfo != null)
            {
                if (cInfo.TypeName == typeof(String).FullName)
                {
                    if (fInfo.FirstOrDefault(x => x.FieldName == name)?.MaxLength != null)
                    {
                        sb.AppendFormat("{0}{1},", R("最大长度", dict), fInfo.FirstOrDefault(x => x.FieldName == name)?.MaxLength);
                    }
                }

                
            }

            return sb.ToString();
        }

        private string R(string key, Dictionary<string, string> dict)
        {
            if (dict == null || !dict.ContainsKey(key))
            {
                return key;
            }

            return dict[key];
        }

        private string GetTypeDesc(string typeName, Dictionary<string, string> dict)
        {
            if (typeDict.ContainsKey(typeName))
            {
                return R(typeDict[typeName], dict);
            }

            return string.Empty;
        }

        private static Dictionary<string, string> typeDict= new Dictionary<string,string>
            {
                { typeof(Boolean).FullName, "布尔类型" },
                { typeof(Byte).FullName, "数字类型" },
                { typeof(Char).FullName, "字符类型" },
                { typeof(DateTime).FullName, "日期类型"},
                { typeof(DateTimeOffset).FullName, "日期类型" },
                { typeof(DBNull).FullName, "DBNull" },
                { typeof(Decimal).FullName, "浮点类型"},
                { typeof(Double).FullName, "浮点类型" },
                { typeof(Guid).FullName, "字符串类型"},
                { typeof(Int16).FullName, "数字类型" },
                { typeof(Int32).FullName, "数字类型" },
                { typeof(Int64).FullName, "数字类型" },
                { typeof(Object).FullName, "字符串类型或其他类型" },
                { typeof(SByte).FullName, "数字类型" },
                { typeof(Single).FullName, "浮点类型" },
                { typeof(String).FullName, "字符串类型" },
                { typeof(TimeSpan).FullName, "日期类型" },
                { typeof(UInt16).FullName, "数字类型" },
                { typeof(UInt32).FullName, "数字类型" },
                { typeof(UInt64).FullName, "数字类型" },
                { typeof(Uri).FullName, "字符串类型" },
            };
    }

    

    
    /// <summary>
    /// 属性json类型
    /// </summary>
    public enum ApiPropertyType
    {
        /// <summary>
        /// 简单类型
        /// </summary>
        SimpleObject = 0,
        /// <summary>
        /// 类
        /// </summary>
        ClassObject = 1,
        /// <summary>
        /// 数组
        /// </summary>
        ArrayObject = 2,
    }
}
