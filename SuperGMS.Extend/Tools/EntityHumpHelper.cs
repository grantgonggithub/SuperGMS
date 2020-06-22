/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Extend.Tools
 文件名：  EntityHumpHelper
 创建者：  grant(巩建春)
 CLR版本： 4.0.30319.42000
 时间：    2018/1/29 13:37:10

 功能描述：收集nick的一个驼峰转换方法到框架

----------------------------------------------------------------*/

namespace SuperGMS.Extend.Tools
{
    /// <summary>
    /// EntityHumpHelper
    /// </summary>
    public class EntityHumpHelper
    {
        /// <summary>
        /// 转成驼峰命名
        /// </summary>
        /// <param name="fieldName">需要转换为驼峰的字段名称</param>
        /// <returns>string</returns>
        public static string GetNiceName(string fieldName)
        {
            // 段生成规则
            string result = string.Empty;

            string[] tempField = fieldName.Split('_');
            if (tempField.Length > 1)
            {
                result = tempField[0].Substring(0, 1).ToUpper() +
                         tempField[0].Substring(1, tempField[0].Length - 1).ToLower();
                for (int i = 1; i <= tempField.Length - 1; i++)
                {
                    if (!string.IsNullOrEmpty(tempField[i]))
                    {
                        result += tempField[i].Substring(0, 1).ToUpper() +
                                  tempField[i].Substring(1, tempField[i].Length - 1).ToLower();
                    }
                }
            }
            else
            {
                result = tempField[0].Substring(0, 1).ToUpper() +
                         tempField[0].Substring(1, tempField[0].Length - 1).ToLower();
            }

            return result;
        }
    }
}
