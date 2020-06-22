using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperGMS.ExceptionEx
{
    public class ExceptionTool
    {
        /// <summary>
        /// 获取异常信息
        /// 如果异常时 EF 异常,则获取详情内容
        /// 如果异常是普通异常,则递归获取innerException的  msg 和 stackTrace
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetSimpleErrorMsgByException(Exception ex)
        {
            var sbLog = new StringBuilder();
            sbLog.AppendLine(ex.Message + ex.StackTrace);
            var inner = ex.InnerException;
            while (inner != null)
            {
                sbLog.AppendLine(inner.Message + inner.StackTrace);
                inner = inner.InnerException;
            }
            return sbLog.ToString();
        }

        /// <summary>
        /// 获取异常信息
        /// 如果异常时 EF 异常,则获取详情内容
        /// 如果异常是普通异常,则递归获取innerException的  msg 和 stackTrace
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetSimpleErrorMsgByValidationException(ValidationException e)
        {
            var sbLog = new StringBuilder();
            foreach (var eve in e.Data)
            {
                //sbLog.AppendFormat(
                //    "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //    eve.Entry.Entity.GetType().Name,
                //    eve.Entry.State);
                //foreach (DbValidationError ve in eve.ValidationErrors)
                //{
                //    sbLog.AppendFormat("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                //}
            }


            return sbLog.ToString();
        }

        /// <summary>
        /// EF抛出的常见异常有长度,主键,外键异常,可以简化翻译给用户直接查看
        /// 例：Data too long for column 'REMARK' at row 1
        /// </summary>
        /// <param name = "e" ></param>
        /// <returns></returns>
        public static string GetEasyErrorMessage(string e)
        {
            if (e.Contains("Data too long"))
            {
                return Regex.Match(e, "'.*?'") + "长度过长";
            }
            return e;
        }


    }
}
