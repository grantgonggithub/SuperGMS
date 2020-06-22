using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperGMS.DB.MapperEx
{
    using AutoMapper;

    /// <summary>
    /// 通用映射配置
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TEditDTO"></typeparam>
    /// <typeparam name="TQueryDTO"></typeparam>
    public class AutoMapperProfile<TSource, TEditDTO, TQueryDTO> : Profile
    {
        public AutoMapperProfile()
        {
            //Source->Destination
            this.DestinationMemberNamingConvention = new AutoMapperTool.MyConvention();
            this.CreateMap<TSource, TEditDTO>();
            this.CreateMap<TSource, TQueryDTO>();
            this.CreateMap<TEditDTO, TSource>();
            this.CreateMap<TQueryDTO, TSource>();
        }
    }
}