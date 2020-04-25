/*----------------------------------------------------------------
 Copyright (C) 2018 GrantMicroService (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：GrantMicroService.Utility
 文件名：Class1
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 16:33:11

 功能描述：

----------------------------------------------------------------*/
using System;

namespace GrantMicroService.Rpc
{
    /// <summary>
    /// 系统可空类型
    /// </summary>
    [Serializable]
    public class Nullables
    {
        private static Nullables nullValue;

        public static Nullables NullValue
        {
            get
            {
                nullValue = new Nullables();
                return nullValue;
            }
        }
    }
}
