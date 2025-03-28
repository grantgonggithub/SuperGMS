using Newtonsoft.Json;

using SuperGMS.ApiHelper.Xml;
using SuperGMS.AttributeEx;
using SuperGMS.Log;
using SuperGMS.Rpc.Server;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.ApiHelper
{
    /// <summary>
    /// api帮助类
    /// </summary>
    public class ApiHelper
    {
        /// <summary>
        /// RPC 基类
        /// </summary>
        private static readonly string RpcBaseName = typeof(RpcBaseServer<object, object>).GetGenericTypeDefinition().FullName;

        private readonly Assembly assembly;
        private XmlCommentsFileCollection xmls=new XmlCommentsFileCollection();
        //private static Dictionary<string, List<ClassInfo>> sAllApiInfo = new Dictionary<string, List<ClassInfo>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHelper"/> class.
        /// 根据程序集搜集 所有相关的接口帮助
        /// </summary>
        /// <param name="assembly">程序集</param>
        public ApiHelper(Assembly assembly)
        {
            this.assembly = assembly;
            this.xmls = this.GetParseAllXml();
        }

        /// <summary>
        /// 查找所有的App
        /// </summary>
        /// <returns>所有app</returns>
        public List<Type> GetAllInterface(bool bOpenApi = false)
        {
            var ts = this.assembly.GetTypes();
            //if (bOpenApi)
            //{
            //    return ts.Where(this.IsGrantRpcBaseServerChildClass).Where(x=>x.GetCustomAttributes(typeof(OpenApiAttribute), true).Length > 0).ToList();
            //}
            //else
            //{
                return ts.Where(this.IsGrantRpcBaseServerChildClass).ToList();
            //}
        }

        /// <summary>
        /// 查找所有的 GrantRpcBaseServer 继承类
        /// </summary>
        /// <returns>所有的 GrantRpcBaseServer 继承类</returns>
        public List<ClassInfo> GetAllInterfaceClass()
        {
            //string lang = string.Empty;
            //if (string.IsNullOrEmpty(lang))
            //{
            //    lang = "zh_cn";
            //}

            //var key = lang + "_f";
            //if (sAllApiInfo.ContainsKey(key))
            //{
            //    return sAllApiInfo[key];
            //}

            //if (this.assembly == null)
            //{
            //    return new List<ClassInfo>();
            //}


            // var ts = this.assembly.GetTypes();
            var list = GetAllInterface(); // ts.Where(this.IsGrantRpcBaseServerChildClass).ToList();
            var sum = list.Count;
            if (sum <= 0)
            {
                return new List<ClassInfo>();
            }

            var infos = new ClassInfo[sum];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            

            var rangesize = (int)(sum / Environment.ProcessorCount) + 1;
            Parallel.ForEach(Partitioner.Create(1, sum + 1, rangesize), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var x = list[i - 1];
                    try
                    {
                        var t = this.GetInterfaceParam(x);
                        var info = this.GetInterfaceInfo(x, t.args,"");

                        // var arg = new ParseTypeInfo(this.assembly, t.args);
                        // var infoArgs = arg.Parse();
                        var infoArgs = new ClassInfo(this.assembly.FullName);
                        var json = this.ToJson(t.args, false);
                        var apiInfo = GetApiInfoByType(t.args);
                        infoArgs.JsonDesc = json.json;
                        infoArgs.LimitDesc = json.replaceJson;
                        infoArgs.ApiClassInfo = apiInfo;

                        // var rtn = new ParseTypeInfo(this.assembly, t.result);
                        // var infoResult = rtn.Parse();
                        var infoResult = new ClassInfo(this.assembly.FullName);
                        apiInfo = GetApiInfoByType(t.result);
                        json = this.ToJson(t.result, false);
                        infoResult.JsonDesc = json.json;
                        infoResult.LimitDesc = json.replaceJson;
                        infoResult.ApiClassInfo = apiInfo;

                        info.PropertyInfo.Add(infoArgs);
                        info.PropertyInfo.Add(infoResult);
                        infos[i - 1] = info;
                    }
                    catch (Exception ex)
                    {
                        LogTextWriter.Write($"{x.FullName}:{ex.Message}");
                        var infoArgs = new ClassInfo(this.assembly.FullName);
                        infoArgs.Name = x.Name;
                        infoArgs.FullName = x.FullName;
                        infoArgs.Desc = $"Parse Error:{ex.Message}";
                        infos[i - 1] = infoArgs;
                    }
                }
            });

            sw.Stop();
            Trace.WriteLine($"Parallel.foreach {sw.ElapsedMilliseconds} ms");

            return infos.ToList();
        }
        /// <summary>
        /// 接口信息
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="bOpenApi"></param>
        /// <returns></returns>
        public List<ClassInfo> GetAllInterfaceClass(string lang, bool bOpenApi = false)
        {
            //if (string.IsNullOrEmpty(lang))
            //{
            //    lang = "zh_cn";
            //}

            //var key = lang + (bOpenApi ? "_t" : "_f");

            //if (sAllApiInfo.ContainsKey(key))
            //{
            //    return sAllApiInfo[key];
            //}

            if (this.assembly == null)
            {
                return new List<ClassInfo>();
            }


            // var ts = this.assembly.GetTypes();
            var list = GetAllInterface(bOpenApi); // ts.Where(this.IsGrantRpcBaseServerChildClass).ToList();
            var sum = list.Count;
            if (sum <= 0)
            {
                return new List<ClassInfo>();
            }

            var infos = new ClassInfo[sum];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            

            var rangesize = (int)(sum / Environment.ProcessorCount) + 1;
            Parallel.ForEach(Partitioner.Create(1, sum + 1, rangesize), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var x = list[i - 1];
                    try
                    {
                        var t = this.GetInterfaceParam(x);
                        var info = this.GetInterfaceInfo(x, t.args, lang);

                        // var arg = new ParseTypeInfo(this.assembly, t.args);
                        // var infoArgs = arg.Parse();
                        var infoArgs = new ClassInfo(this.assembly.FullName);
                        var json = this.ToJson(t.args, false);
                        var apiInfo = GetApiInfoByType(t.args);
                        infoArgs.JsonDesc = json.json;
                        infoArgs.LimitDesc = json.replaceJson;
                        infoArgs.ApiClassInfo = apiInfo;

                        // var rtn = new ParseTypeInfo(this.assembly, t.result);
                        // var infoResult = rtn.Parse();
                        var infoResult = new ClassInfo(this.assembly.FullName);
                        apiInfo = GetApiInfoByType(t.result);
                        json = this.ToJson(t.result, false);
                        infoResult.JsonDesc = json.json;
                        infoResult.LimitDesc = json.replaceJson;
                        infoResult.ApiClassInfo = apiInfo;


                        info.PropertyInfo.Add(infoArgs);
                        info.PropertyInfo.Add(infoResult);
                        infos[i - 1] = info;
                    }
                    catch (Exception ex)
                    {
                        LogTextWriter.Write($"{x.FullName}:{ex.Message}");
                        var infoArgs = new ClassInfo(this.assembly.FullName);
                        infoArgs.Name = x.Name;
                        infoArgs.FullName = x.FullName;
                        infoArgs.Desc = $"Parse Error:{ex.Message}";
                        infos[i - 1] = infoArgs;
                    }
                }
            });

            sw.Stop();
            Trace.WriteLine($"Parallel.foreach {sw.ElapsedMilliseconds} ms");

            return infos.ToList();
        }

        /// <summary>
        /// 获取类信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ApiClassInfo GetApiInfoByType(Type type)
        {
            ObjectGeneratorEx ex = new ObjectGeneratorEx();
            return ex.GenerateObject(type);
        }

        /// <summary>
        /// 获取json描述 ,有嵌套引用的忽略第二个及以后
        /// </summary>
        /// <param name="type">给定类型</param>
        /// <param name="original">是否已经替换非字符串类型，默认不替换</param>
        /// <returns>json实例</returns>
        public (string json, string replaceJson) ToJson(Type type, bool original = true)
        {
            ObjectGenerator generater = new ObjectGenerator(this.xmls);

            var obj = generater.GenerateObject(type);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(
                obj,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });
            if (original)
            {
                return (json, string.Empty);
            }
            else
            {
                JsonSampleReplace jsonReplace = new JsonSampleReplace(generater.NoStringProperties, json);
                return (json, jsonReplace.TryFormatJson());
            }
        }

        /// <summary>
        /// 得到接口信息 文档
        /// </summary>
        /// <param name="type">接口类</param>
        /// <param name="argType">接口参数类</param>
        /// <returns>文档信息</returns>
        private ClassInfo GetInterfaceInfo(Type type, Type argType, string lang = "zh_cn")
        {
            var info = new ClassInfo(this.assembly.FullName);
            info.Name = type.Name;
            info.FullName = type.FullName;
            info.XmlNode = string.Format("T:{0}", type.FullName);
            var c = type.GetCustomAttributes(typeof(OpenApiAttribute), true);
            if (c.Length > 0)
            {
                var oa = c[0] as OpenApiAttribute;
                info.Desc = oa?.ApiDesc;//C.R(oa?.ApiDesc, lang);
                info.IsPublic = true;
            }

            if (string.IsNullOrEmpty(info.Desc))
            {
                info.Desc = this.xmls?.FindMember(info.XmlNode)?.InnerXml;
            }

            var processXmlNode = $"M:{type.FullName}.Process({argType.FullName},SuperGMS.Protocol.RpcProtocol.StatusCode@)";
            info.LimitDesc = this.xmls?.FindMember(processXmlNode)?.InnerXml;

            var dict = this.GetAllErrorCodeClass();
            var codeDesc = dict.ContainsKey(type.Name) ? dict[type.Name] : new List<(string code, string desc)>();
            StringBuilder sb = new StringBuilder();
            codeDesc.ForEach(x => sb.Append($"{x.code}:{x.desc}<br />"));
            info.CodeDesc = sb.ToString();
            return info;
        }

        /// <summary>
        /// 解析xml文档
        /// </summary>
        /// <returns>xml注释集合</returns>
        private XmlCommentsFileCollection GetParseAllXml()
        {

            var xs = new XmlCommentsFileCollection();

            if (File.Exists(this.GetXmlDllPath()))
            {
                this.xmls.Add(new XmlCommentsFile(this.GetXmlDllPath()));
            }
            this.GetAllXml().ForEach(x =>
            {
                xs.Add(new XmlCommentsFile(x));
            });

            return xs;
        }

        private object _lockObject = new object();
        private Dictionary<string, List<(string code, string desc)>> _dictErrorCode = new Dictionary<string, List<(string code, string desc)>>();
        /// <summary>
        /// 收集错误代码 枚举类定义
        /// </summary>
        /// <returns>错误代码定义类</returns>
        private Dictionary<string, List<(string code, string desc)>> GetAllErrorCodeClass()
        {
            lock (_lockObject)
            {
                if (_dictErrorCode.Count <= 0)
                {
                    var ts = this.assembly.GetTypes();
                    var list = ts.Where(x => x.GetCustomAttribute<ErrorCodeAttribute>() != null).ToList();

                    list.ForEach(x =>
                    {
                        var dictTmp = this.GetEnumCodeDescription(x);
                        foreach(var dict in dictTmp)
                        {
                            if (_dictErrorCode.ContainsKey(dict.Key))
                            {
                                _dictErrorCode[dict.Key].AddRange(dict.Value);
                            }
                            else
                            {
                                _dictErrorCode.Add(dict.Key, dict.Value);
                            }
                        }
                    });
                }
            }

            return _dictErrorCode;
        }

        /// <summary>
        /// 得到枚举类型的CodeDesc属性标签
        /// </summary>
        /// <param name="enumType">枚举值类型</param>
        /// <returns>按接口分组的属性标签</returns>
        private Dictionary<string, List<(string code, string desc)>> GetEnumCodeDescription(Type enumType)
        {
            var fields = enumType.GetFields();
            var obj = Activator.CreateInstance(enumType);
            Dictionary<string, List<(string code, string desc)>> dict = new Dictionary<string, List<(string code, string desc)>>();
            foreach (var field in fields)
            {
                var objs = field.GetCustomAttributes(typeof(CodeDescAttribute), false);
                if (objs == null || objs.Length <= 0)
                {
                    continue;
                }

                var attr = (CodeDescAttribute)objs[0];
                if (!(attr.InterfaceName?.Length > 0))
                {
                    continue;
                }

                foreach (var s in attr.InterfaceName)
                {
                    if (dict.ContainsKey(s))
                    {
                        dict[s].Add((((int)field.GetValue(obj)).ToString(), attr.Description));
                    }
                    else
                    {
                        dict.Add(s, new List<(string, string)> { (((int)field.GetValue(obj)).ToString(), attr.Description) });
                    }
                }
            } // end foreach

            return dict;
        }

        /// <summary>
        /// 查找 GrantRpcBaseServer 继承类的输入和输出参数类型
        /// </summary>
        /// <param name="type">GrantRpcBaseServer 继承类</param>
        /// <returns>得到接口的参数和接口类型</returns>
        public (Type args, Type result) GetInterfaceParam(Type type)
        {
            if (type.IsAbstract)
            {
                return (null, null);
            }

            // 不支持泛型接口类
            if (type.IsGenericType)
            {
                return (null, null);
            }

            var baseType = GetGrantRpcBaseServerClassType(type);
            if (baseType == null)
            {
                return (null, null);
            }

            var ps = baseType.GenericTypeArguments;
            if (ps.Length >= 2)
            {
                return (ps[0], ps[1]);
            }

            return (null, null);
        }

        /// <summary>
        /// 判断是否Rpc接口类
        /// </summary>
        /// <param name="type">本微服务中的类型</param>
        /// <returns>是否Rpc接口类</returns>
        private Type GetGrantRpcBaseServerClassType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == RpcBaseName)
            {
                return type;
            }

            if (type.BaseType == null)
            {
                return null;
            }

            return GetGrantRpcBaseServerClassType(type.BaseType);
        }

        /// <summary>
        /// 判断是否Rpc接口类
        /// </summary>
        /// <param name="type">本微服务中的类型</param>
        /// <returns>是否Rpc接口类</returns>
        private bool IsGrantRpcBaseServerChildClass(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == RpcBaseName)
            {
                return true;
            }

            return (type.BaseType != null) && IsGrantRpcBaseServerChildClass(type.BaseType);
        }

        /// <summary>
        /// 微服务帮助的全路径
        /// </summary>
        private string GetXmlDllPath()
        {
            return $"{this.assembly.Location.Substring(0, this.assembly.Location.Length - ".dll".Length)}.xml";
        }

        /// <summary>
        /// 微服务帮助的全路径
        /// </summary>
        /// <returns>xml集合</returns>
        private List<string> GetAllXml()
        {
            FileInfo fi = new FileInfo(this.assembly.Location);
            var di = fi.Directory;
            var xmlAlls = di.GetFiles("*.xml");
            if (xmlAlls?.Count() > 0)
            {
                return xmlAlls.Select(x => x.FullName).ToList();
            }

            return new List<string>();
        }
    }
}