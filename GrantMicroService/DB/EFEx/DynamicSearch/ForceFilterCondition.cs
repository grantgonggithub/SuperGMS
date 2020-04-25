using System;
using GrantMicroService.DB.EFEx.DynamicSearch.Model;
using System.Linq;

namespace GrantMicroService.DB.EFEx.DynamicSearch
{
    using GrantMicroService.ExceptionEx;
    using GrantMicroService.Tools;
    using System.Data;

    /// <summary>
    /// 强制过滤条件
    /// </summary>
    public class ForceFilterCondition
    {
        /// <summary>
        /// 如果数据源有TTID,TT_ID,DomainName,DOMAIN_NAME,WHID, 强制过滤
        /// 客户身份还过滤 `CustomerBUGID``CustomerGID`,CUSTOMER_GID,BU_GID,
        /// 承运商身份还过滤 `ServiceProviderGID`,SERVICE_PROVIDER_GID,PLAN_SP_GID,ACTUAL_SP_GID,TO_SP_GID,SP_GID 
        /// 如果数据源有
        /// </summary>
        /// <param name="searchParameters"></param>
        /// <param name="dt">带有列名的一个空DataTable</param>
        public static void FilterByTenantDomainWearHouse(SearchParameters searchParameters, DataTable dt)
        {
            FilterByTenantDomainWearHouse(searchParameters, dt, null);
        }
        /// <summary>
        /// 如果数据源有TTID,TT_ID,DomainName,DOMAIN_NAME,WHID, 强制过滤
        /// 客户身份还过滤 `CustomerBUGID``CustomerGID`,CUSTOMER_GID,BU_GID,
        /// 承运商身份还过滤 `ServiceProviderGID`,SERVICE_PROVIDER_GID,PLAN_SP_GID,ACTUAL_SP_GID,TO_SP_GID,SP_GID 
        /// 如果数据源有
        /// </summary>
        public static void FilterByTenantDomainWearHouse(SearchParameters searchParameters, DataTable dt, Type t)
        {
            throw new NotImplementedException();
            //if (UserContext.IsLogin())
            //{
            //    var user = UserContext.GetLoginUser();
            //    FilterByTenant(searchParameters, dt, t);

            //    CheckIsExist(searchParameters, dt, t, "DomainName", QueryMethod.StdIn, user.AllDomain.ToArray());
            //    CheckIsExist(searchParameters, dt, t, "DOMAIN_NAME", QueryMethod.StdIn, user.AllDomain.ToArray());

            //    if (user.UserIdentity == "10")
            //    {
            //        //客户身份
            //        CheckIsExist(searchParameters, dt, t, "CustomerGID", QueryMethod.Equal, user.CustomerGID, "Customer");
            //        CheckIsExist(searchParameters, dt, t, "BU_GID", QueryMethod.Equal, user.CustomerGID, "Customer");
            //        CheckIsExist(searchParameters, dt, t, "CUSTOMER_GID", QueryMethod.Equal, user.CustomerGID, "Customer");

            //        CheckIsExist(searchParameters, dt, t, "CustomerID", QueryMethod.Equal, user.CustomerID, "Customer");
            //        CheckIsExist(searchParameters, dt, t, "BU_ID", QueryMethod.Equal, user.CustomerID, "Customer");
            //        CheckIsExist(searchParameters, dt, t, "CUSTOMER_ID", QueryMethod.Equal, user.CustomerID, "Customer");
            //    }
            //    if (user.UserIdentity == "20")
            //    {
            //        //承运商身份
            //        CheckIsExist(searchParameters, dt, t, "ServiceProviderGID", QueryMethod.Equal, user.ServiceProviderGID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "SERVICE_PROVIDER_GID", QueryMethod.Equal, user.ServiceProviderGID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "PLAN_SP_GID", QueryMethod.Equal, user.ServiceProviderGID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "ACTUAL_SP_GID", QueryMethod.Equal, user.ServiceProviderGID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "TO_SP_GID", QueryMethod.Equal, user.ServiceProviderGID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "SP_GID", QueryMethod.Equal, user.ServiceProviderGID, "ServiceProvider");

            //        CheckIsExist(searchParameters, dt, t, "ServiceProviderID", QueryMethod.Equal, user.ServiceProviderID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "SERVICE_PROVIDER_ID", QueryMethod.Equal, user.ServiceProviderID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "PLAN_SP_ID", QueryMethod.Equal, user.ServiceProviderID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "ACTUAL_SP_ID", QueryMethod.Equal, user.ServiceProviderID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "TO_SP_ID", QueryMethod.Equal, user.ServiceProviderID, "ServiceProvider");
            //        CheckIsExist(searchParameters, dt, t, "SP_ID", QueryMethod.Equal, user.ServiceProviderID, "ServiceProvider");
            //    }
            //}
        }

        private static void FilterByTenant(SearchParameters searchParameters, DataTable dt, Type t)
        {
            throw new NotImplementedException();
            //if (UserContext.IsLogin())
            //{
            //    var user = UserContext.GetLoginUser();
            //    CheckIsExist(searchParameters, dt, t, "TTID", QueryMethod.StdIn, new[] { user.TTID, "PUBLIC" });
            //    CheckIsExist(searchParameters, dt, t, "TT_ID", QueryMethod.StdIn, new[] { user.TTID, "PUBLIC" });
            //}
        }

        /// <summary>
        /// 只按租户隔离
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="searchParameters"></param>
        public static void FilterByTenant<TSource>(SearchParameters searchParameters)
        {
            FilterByTenant(searchParameters, null, typeof(TSource));
        }
        /// <summary>
        /// 如果数据源有TTID,TT_ID,DomainName,DOMAIN_NAME,WHID, 强制过滤
        /// 客户身份还过滤 `CustomerBUGID``CustomerGID`,CUSTOMER_GID,BU_GID,
        /// 承运商身份还过滤 `ServiceProviderGID`,SERVICE_PROVIDER_GID,PLAN_SP_GID,ACTUAL_SP_GID,TO_SP_GID,SP_GID 
        /// 如果数据源有
        /// </summary>
        /// <param name="searchParameters"></param>
        /// <typeparam name="TSource"></typeparam>
        public static void FilterByTenantDomainWearHouse<TSource>(SearchParameters searchParameters)
        {
            FilterByTenantDomainWearHouse(searchParameters, null, typeof(TSource));
        }

        private static void CheckIsExist(SearchParameters searchParameters, DataTable dt, Type type, string name, QueryMethod method, object value, string orGroup = "")
        {

            if (dt == null && type == null)
            {
                throw new BusinessException("dt和type参数不能同时为空");
            }
            var checkFlag = false;
            if (dt != null)
            {
                checkFlag = dt.Columns.Contains(name);
            }
            if (type != null)
            {
                checkFlag = ReflectionTool.GetPropertyInfoFromCache(type, name) != null;
            }
            if (checkFlag)
            {
                searchParameters.QueryModel.Items.Add(new ConditionItem() { Field = name, Method = method, Value = value, OrGroup = orGroup });
            }
        }

        /// <summary>
        /// 如果数据源有TTID,DomainName,WHID 的话,则自动根据用户进行过滤
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        public static void FilterByTenantDomainWearHouse<TSource>(ref string sqlwhere)
        {
            var sp = new SearchParameters();
            FilterByTenantDomainWearHouse(sp, null, typeof(TSource));
            sqlwhere = sp.GetSqlWhere();
        }
        /// <summary>
        /// 强制过滤用户
        /// </summary>
        /// <param name="sqlwhere"></param>
        /// <param name="dt"></param>
        public static void FilterByTenantDomainWearHouse(ref string sqlwhere, DataTable dt)
        {
            var sp = new SearchParameters();
            FilterByTenantDomainWearHouse(sp, dt, null);
            sqlwhere = sp.GetSqlWhere();
        }

    }
}
