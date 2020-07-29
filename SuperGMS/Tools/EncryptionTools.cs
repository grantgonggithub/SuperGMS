/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Utility
 文件名：EncryptionTools
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 15:59:07

 功能描述：加密工具集

----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SuperGMS.Tools
{
    /// <summary>
    /// 加密工具集
    /// </summary>
  public class EncryptionTools
    {
        /// <summary>
        /// 字符串转换为32位MD5大写或者小写
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <param name="toLower">转为小写，默认大写</param>
        /// <returns></returns>
        public static string HashMd5(string input, bool toLower=false)
        {
            byte[] result = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder ouput = new StringBuilder(32);
            string prix = toLower ? "x2" : "x";
            for (int i = 0; i < result.Length; i++)
            {
                ouput.Append((result[i]).ToString(prix, System.Globalization.CultureInfo.InvariantCulture));
            }
            return ouput.ToString();
        }

        /// <summary>
        /// SHA256
        /// </summary>
        /// /// <param name="str">原始字符串</param>
        /// <returns>结果</returns>
        public static string Sha256(string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] Result = Sha256.ComputeHash(SHA256Data);
            return Convert.ToBase64String(Result);
        }
    }
}
