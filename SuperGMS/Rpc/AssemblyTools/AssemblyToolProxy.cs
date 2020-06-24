using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.Logging;
using SuperGMS.Log;
using SuperGMS.Rpc.Server;

namespace SuperGMS.Rpc.AssemblyTools
{
    /// <summary>
    /// 根据程序集自动生成Proxy代理类
    /// </summary>
    public class AssemblyToolProxy
    {
        private readonly static ILogger logger = LogFactory.CreateLogger<AssemblyToolProxy>();
        /// <summary>
        /// 创建主方法
        /// </summary>
        /// <param name="path">dll 路径</param>
        /// <param name="outPutDir">输出proxy.cs 文件路径</param>
        /// <param name="template">模板</param>
        /// <param name="templateBody">模板主体</param>
        /// <param name="interfaceBody">接口主体</param>
        /// <returns>返回是否创建成功</returns>
        public bool Create(string path, string outPutDir, string template, string templateBody, string interfaceBody)
        {
            try
            {
                var asLoad = LoadAssembly(path);

                Type[] types = asLoad.GetTypes();
                string assShortName = AssemblyToolProxy.GetCurrentAppName(asLoad);
                StringBuilder body = new StringBuilder();
                StringBuilder interfaceBodySb = new StringBuilder();
                body.AppendLine(string.Format("private const string serviceName=\"{0}\";", assShortName));
                body.AppendLine();

                // 加载所有继承了AppBase的类
                var useTypes = types.Where(t => RpcDistributer.isChildAppBase(t) && !t.IsAbstract).ToList();
                return WriteFile(outPutDir, template, templateBody, interfaceBody, useTypes, body, interfaceBodySb, assShortName);
            }
            catch (ReflectionTypeLoadException rtlEx)
            {
                logger.LogError(rtlEx, $"无法加载程序集:{rtlEx.Message}");
                if (rtlEx.LoaderExceptions != null && rtlEx.LoaderExceptions.Length > 0)
                {
                    foreach (var lex in rtlEx.LoaderExceptions)
                    {
                        logger.LogError(lex, $"无法加载程序集：{lex.Message}");
                    }
                }
            }
            catch (TypeLoadException tlEx)
            {
                logger.LogError(tlEx, $"无法加载程序集: {tlEx.TypeName}");
            }
            catch (FileNotFoundException fNtEx)
            {
                logger.LogError(fNtEx, $"无法加载程序集: {fNtEx.FileName}");
            }

            return false;
        }

        /// <summary>
        /// 根据服务命名空间获取名称, 这个需要和框架保持一致.
        /// </summary>
        /// <param name="asLoad">程序集</param>
        /// <returns>短名称</returns>
        public static string GetCurrentAppName(Assembly asLoad)
        {
            return asLoad.FullName.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Replace(".", string.Empty);
        }

        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>程序集</returns>
        private static Assembly LoadAssembly(string path)
        {
            Assembly asLoad = null;
            try
            {
                AssemblyLoadContext.Default.Resolving += (arg1, args2) =>
                {
                    try
                    {
                        // fix .resource.dll load error
                        // to see https://github.com/dotnet/coreclr/issues/8416
                        if (args2.Name.EndsWith(".resources"))
                        {
                            return null;
                        }

                        logger.LogInformation($"尝试加载：{args2.FullName}");
                        string p = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
                        return arg1.LoadFromAssemblyPath(string.Format("{0}{1}{2}.dll", p, Path.DirectorySeparatorChar, args2.Name));
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        logger.LogError(ex, "无法加载程序集");
                        if (ex.LoaderExceptions != null && ex.LoaderExceptions.Length > 0)
                        {
                            foreach (var lex in ex.LoaderExceptions)
                            {
                                logger.LogError(lex, "无法加载程序集");
                            }
                        }

                        throw;
                    }
                    catch (TypeLoadException tlEx)
                    {
                        logger.LogError(tlEx, $"无法加载程序集:{tlEx.TypeName}");
                        throw;
                    }
                    catch (FileNotFoundException fNtEx)
                    {
                        logger.LogError(fNtEx, $"无法加载程序集:{fNtEx.FileName},----->>>>>>>>{fNtEx.Message}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"{args2.FullName}，加载失败");
                        throw;
                    }
                };
                asLoad = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            }
            catch (ReflectionTypeLoadException rtlEx)
            {
                logger.LogError(rtlEx, "无法加载程序集");
                if (rtlEx.LoaderExceptions != null && rtlEx.LoaderExceptions.Length > 0)
                {
                    foreach (var lex in rtlEx.LoaderExceptions)
                    {
                        logger.LogError(lex, "无法加载程序集");
                    }
                }
            }
            catch (TypeLoadException tlEx)
            {
                logger.LogError(tlEx, $"无法加载程序集:{tlEx.TypeName}");
            }
            catch (FileNotFoundException fNtEx)
            {
                logger.LogError(fNtEx, $"无法加载程序集:{fNtEx.FileName},----->>>>>>>>{fNtEx.Message}");
            }

            return asLoad;
        }

        /// <summary>
        /// 将生成好的代理类保存到指定路径
        /// </summary>
        /// <param name="content">代理类内容</param>
        /// <param name="path">路径</param>
        /// <returns>保存是否成功</returns>
        private static bool SaveFile(string content, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(content);
                        writer.Flush();
                        writer.Close();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool WriteFile(string outPutDir, string template, string templateBody, string interfaceBody, List<Type> useTypes, StringBuilder body, StringBuilder interfaceBodySb, string assShortName)
        {
            foreach (Type t in useTypes)
            {
                Type[] ts = t.BaseType.GenericTypeArguments;
                if (ts.Length < 2)
                {
                    continue;
                }

                string returnType = GetTypeName(ts[1]);
                string pram = GetTypeName(ts[0]);
                string bodyLine = templateBody.Replace("#return_type#", returnType).Replace("#clazz_name#", t.Name)
                    .Replace("#args_type#", pram);
                body.AppendLine(bodyLine + "\r\n");
                string interfaceLine = interfaceBody.Replace("#return_type#", returnType).Replace("#clazz_name#", t.Name)
                    .Replace("#args_type#", pram);
                interfaceBodySb.AppendLine(interfaceLine + "\r\n");
            }

            string outPath = outPutDir.TrimEnd("\\".ToCharArray()) + "\\" + assShortName + "RpcProxy.cs";
            string content = template.Replace("#assembly_name#", assShortName + "RpcProxy")
                .Replace("#class_part#", body.ToString()).Replace("#interface_part#", interfaceBodySb.ToString());
            return SaveFile(content, outPath);
        }

        /// <summary>
        /// 获取Type类型
        /// </summary>
        private string GetTypeName(Type type)
        {
            if (!type.IsGenericType && !type.FullName.Contains("Generic.List"))
            {
                if (type == typeof(void))
                {
                    return "void";
                }
                else
                {
                    return type.Name;
                }
            }

            if (type.Name.IndexOf("Dictionary") > -1)
            {
                var pTypes = type.Name.Split('`');
                string returnTypeGenericType = pTypes[0];
                int genericTypeCount = int.Parse(pTypes[1]);
                string returnTypeGenericTypeArgumentName = string.Empty;
                for (int i = 0; i < genericTypeCount; i++)
                {
                    returnTypeGenericTypeArgumentName += type.GetGenericArguments()[i].Name + ",";
                }

                returnTypeGenericTypeArgumentName = returnTypeGenericTypeArgumentName.Remove(returnTypeGenericTypeArgumentName.Length - 1);
                return string.Format("{0}<{1}>", returnTypeGenericType, returnTypeGenericTypeArgumentName);
            }
            else
            {
                string returnTypeGenericTypeName = type.Name.Replace("`1", string.Empty);
                string returnTypeGenericTypeArgumentName = type.GetGenericArguments()[0].Name;
                return string.Format("{0}<{1}>", returnTypeGenericTypeName, returnTypeGenericTypeArgumentName);
            }
        }
    }
}