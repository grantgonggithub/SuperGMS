using SuperGMS.ExceptionEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperGMS.Tools
{
    /// <summary>
    /// 字符串辅助类
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// 根据字符类型返回表达式
        /// </summary>
        /// <param name="cType">字符类型</param>
        /// <param name="text">字符</param>
        /// <returns></returns>
        public static bool Validate(CharacterType cType, string text)
        {
            string pattern = string.Empty;
            string validationMessage = string.Empty;
            switch (cType)
            {
                case CharacterType.Simple:
                    pattern = @"[a-zA-Z0-9]*";
                    break;

                case CharacterType.Code128:
                    pattern = @"[a-zA-Z0-9\u4e00-\u9fa5\-\.\s\_\/\(\)\（\）\[\]\【\】]*";
                    break;

                case CharacterType.Code39:
                    pattern = @"[a-zA-Z0-9\-\.\s\_\+\%]*";
                    break;

                case CharacterType.Complex:
                    pattern = @"[^,]*";
                    break;

                case CharacterType.Email:
                    pattern = @"([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)";
                    break;

                case CharacterType.Mobile:
                    pattern = @"13\d{9}|15[0-35-9]\d{8}|18[0-9]\d{8}|17[0-9]\d{8}|147\d{8}|166\d{8}|198\d{8}|199\d{8}|00852\d{8}|\+852\d{8}|\+84\d{9}|\+84\d{10}|0084\d{9}|0084\d{10}";

                    break;

                case CharacterType.Period:
                    pattern = @"\d{1,2}:\d{2}-\d{1,2}:\d{2}|\d{1,2}:\d{2}-\d{1,2}:\d{2},\d{1,2}:\d{2}-\d{1,2}:\d{2}|\d{1,2}:\d{2}-\d{1,2}:\d{2},\d{1,2}:\d{2}-\d{1,2}:\d{2},\d{1,2}:\d{2}-\d{1,2}:\d{2}";

                    break;

                case CharacterType.Number:
                    pattern = @"[0-9]*";
                    break;

                case CharacterType.PostCode:
                    pattern = @"\d{6}";
                    break;

                case CharacterType.PrimaryKey:
                    pattern = @"[a-zA-Z0-9\-_]*";
                    break;
                default:
                    throw new BusinessException("CharacterType 枚举类型错误");
            }
            // 增加^ 和 $ 表示已正则内容开始并结束,
            return Regex.IsMatch(text, $"^{pattern}$");
        }
    }

    /// <summary>
    /// 字符类型
    /// </summary>
    public enum CharacterType
    {
        /// <summary>
        /// 请输入字母和数字
        /// </summary>
        Simple = 10,

        /// <summary>
        /// 数字
        /// </summary>
        Number = 12,

        /// <summary>
        /// 请输入字母数字短横线小数点
        /// </summary>
        Code39 = 39,

        /// <summary>
        /// 请输入字母数字短横线中文小数点括号
        /// </summary>
        Code128 = 128,

        /// <summary>
        /// 不能输入逗号
        /// </summary>
        Complex = 20,

        /// <summary>
        /// 邮箱
        /// </summary>
        Email = 30,

        /// <summary>
        /// 手机号码
        /// </summary>
        Mobile = 40,

        /// <summary>
        /// 时间段
        /// </summary>
        Period = 50,

        /// <summary>
        /// 邮编
        /// </summary>
        PostCode = 85,

        /// <summary>
        /// 主键限制：只能输入数字、字母、下划线、中横杠
        /// </summary>
        PrimaryKey = 200,
    }
}