using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace GrantMicroService.DB.MapperEx
{
    using System.Text.RegularExpressions;

    using AutoMapper.Configuration;

    /// <summary>
    /// Auto Mapper 注册工具类
    /// </summary>
    public abstract class AutoMapperTool
    {
        /// <summary>
        /// 获取映射关系
        /// </summary>
        public abstract List<Profile> GetProfiles();

        /// <summary>
        /// 将所有的profiles 注册到 AutoMapper中
        /// </summary>
        /// <param name="profiles"></param>
        public void RegAllProfiles(List<Profile> profiles)
        {
            Mapper.Initialize(
                a =>
                    {
                        foreach (var profile in profiles)
                        {
                            a.AddProfile(profile);
                        }
                    });
        }

        /// <summary>
        /// 注册Map
        /// </summary>
        public void RegisterMap()
        {
            this.RegAllProfiles(this.GetProfiles());
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return Mapper.Map<TSource, TDestination>(source);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Mapper.Map(source, destination);
        }

        /// <summary>
        /// 自定义Automaper 属性Name转换,全属性转换
        /// </summary>
        public class MyConvention : INamingConvention
        {
            /// <summary>
            /// Grant没意义, 因为写成空或者Null 会报错.
            /// </summary>
            public Regex SplittingExpression { get; } = new Regex(@"Grant");

            public string SeparatorCharacter => string.Empty;

            public string ReplaceValue(Match match)
            {
                return match.Value;
            }
        }
    }
}