using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using GrantMicroService.UserSession;

namespace GrantMicroService.DB.EFEx
{
    /// <summary>
    /// 自己管理释放内存
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DbContextFactory<TContext> : IDisposable where TContext : DbContext
    {
        private Dictionary<string, UnitOfWork<TContext>> _dicUnitOfWorks = new Dictionary<string, UnitOfWork<TContext>>();

        public void Dispose()
        {
            foreach (var dicUnitOfWork in _dicUnitOfWorks)
            {
                dicUnitOfWork.Value.Dispose();
            }
            _dicUnitOfWorks.Clear();
        }

        /// <summary>
        /// 根据系统ID, 匹配用户已授权的系统, 获取数据库信息
        /// </summary>
        /// <param name="sysId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual UnitOfWork<TContext> GetUnitOfWork(string sysId, UserContext user)
        {
            return GetUnitOfWork("192.168.100.201", "Grant", "admin", "grant");
        }

        /// <summary>
        /// 直接根据数据库信息获取
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="uName"></param>
        /// <param name="uPass"></param>
        /// <param name="dbName"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public virtual UnitOfWork<TContext> GetUnitOfWork(string ip, string uName, string uPass, string dbName, string port = "3306")
        {
            string uKey = ip + uName + uPass + dbName + port;
            if (_dicUnitOfWorks.ContainsKey(uKey))
            {
                return _dicUnitOfWorks[uKey];
            }
            var options = ConnectionManager.CreateMySqlDbOption<TContext>(
                ip, uName, uPass, dbName, port);

            var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options);
            var unitOfWork = new UnitOfWork<TContext>(dbContext);
            _dicUnitOfWorks.Add(uKey, unitOfWork);
            return unitOfWork;
        }
    }
}