/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途

 项目名称：SuperGMS.Protocol
 文件名：StatusCode
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 16:07:15

 功能描述：

----------------------------------------------------------------*/
using System;

namespace SuperGMS.Protocol.RpcProtocol
{
    [Serializable]
    public class StatusCode
    {
        private int _code;

        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get { return _code; } set { _code = value; } }

        private string _msg;

        /// <summary>
        /// 状态码描述
        /// </summary>
        public string msg { get { return _msg; } set { _msg = value; } }

        private string[] _msgParam;

        public string[] MsgParam
        {
            get { return _msgParam; }
            set { _msgParam = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCode"/> class.
        /// OK=200,//成功
        ///   MethodNotExist=404,//方法不存在
        ///   ServerError=500,//服务器内部错误
        ///   BadRequest=400,//非法请求
        ///   Unauthorized=401,//访问需要授权的资源，如密码登陆等...
        ///  ArgesError=403,//参数错误，服务器无法解析
        ///  如果你所定义的状态码在以上列表中存在，并且表达的意思一样，请直接StatusCode.OK这样使用，如果没有
        ///  可以自定义600以上
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="msg">状态码描述</param>
        public StatusCode(int code, string msg)
        {
            // Code cd;
            // if (CodeParser.isCode(code, out cd))
            //    throw new Exception(string.Format("状态码'{0}'是系统预留状态码，它的含义是'{1}',如果你的状态码跟此含义一致，请直接使用StatusCode.{2},如果不一致，请改用其他状态码", (int)cd, cd.ToString(), cd.ToString()));
            // if (code < 600)
            //    throw new Exception("用户自定义状态码，必须大于等于600");
            this._msg = msg;
            this._code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCode"/> class.
        /// OK=200,//成功
        ///   MethodNotExist=404,//方法不存在
        ///   ServerError=500,//服务器内部错误
        ///   BadRequest=400,//非法请求
        ///   Unauthorized=401,//访问需要授权的资源，如密码登陆等...
        ///  ArgesError=403,//参数错误，服务器无法解析
        ///  如果你所定义的状态码在以上列表中存在，并且表达的意思一样，请直接StatusCode.OK这样使用，如果没有
        ///  可以自定义600以上
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="msg">状态码描述</param>
        /// <param name="msgParam">msg的多语言部分参数</param>
        public StatusCode(int code, string msg, string[] msgParam)
        {
            this._msg = msg;
            this._code = code;
            this._msgParam = msgParam;
        }

        /// <summary>
        /// 框架自己用，外部业务不能用，请用正常的构造，
        /// 外面不知道当前的状态码是否为系统预留，只知道状态码的值和msg，这个不能用作业务状态码的生成
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="msg">msg</param>
        /// <returns>StatusCode</returns>
        public static StatusCode GetCode(int code, string msg)
        {
            Code cd;
            if (CodeParser.isCode(code, out cd))
            {
                StatusCode c = new StatusCode();
                c.code = code;
                c.msg = msg;
                return c;
            }
            else
            {
                return new StatusCode(code, msg);
            }
        }

        /// <summary>
        /// 判断code是否成功，
        /// code == 200 
        /// </summary>
        /// <returns></returns>
        public bool IsSuccess
        {
            get { return this.code == StatusCode.OK.code; }
        }

        #region 内部预留状态码

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCode"/> class.
        /// 内部用，不让外部用，外部用必须走有参构造，便于验证是否占用了预留状态码却表达了不同的含义
        /// </summary>
        private StatusCode()
        {
        }

        enum Code : int
        {
            OK = 200, // 成功
            MethodNotExist = 404, // 方法不存在
            ServerError = 500, // 服务器内部错误
            BadRequest = 400, // 非法请求
            Unauthorized = 401, // 访问需要授权的资源，如密码登陆等...
            ArgesError = 403, // 参数错误，服务器无法解析
            ErrorCodeUndefined = 405, // 错误码未定义
            LoginFailed = 505, // 登陆失效
            Unknown = -1,
            RouterNotFound = 503, // httpProxy 服务获取后端服务的路由
            RpcServerNotAlive = 502, // 无法请求远端服务器
            ResultError = 501, // 远端服务器返回的结果无法解析
        }

        class CodeParser
        {
            public static Code Parser(string s)
            {
                Code c;
                if (!Enum.TryParse<Code>(s, out c))
                {
                    return Code.Unknown;
                }

                return c;
            }

            /// <summary>
            /// 检查是否是已经存在的值
            /// </summary>
            /// <param name="s">s</param>
            /// <returns>bool</returns>
            public static bool isCode(int s, out Code code)
            {
                bool isDef = Enum.IsDefined(typeof(Code), s);
                if (isDef)
                {
                    code = (Code)s;
                }
                else
                {
                    code = Code.Unknown;
                }

                return isDef;
            }
        }

        /// <summary>
        /// 成功
        /// </summary>
        public static StatusCode OK
        {
            get { return new StatusCode() { code = (int)Code.OK, msg = Code.OK.ToString() }; }
        }
        /// <summary>
        /// 登陆失效
        /// </summary>
        public static StatusCode LoginFailed
        {
            get { return new StatusCode() { code = (int)Code.LoginFailed, msg = Code.LoginFailed.ToString() }; }
        }

        /// <summary>
        /// 请求方法不存在
        /// </summary>
        public static StatusCode MethodNotExist
        {
            get { return new StatusCode() { code = (int)Code.MethodNotExist, msg = Code.MethodNotExist.ToString() }; }
        }

        /// <summary>
        /// 服务器内部错误
        /// </summary>
        public static StatusCode ServerError
        {
            get { return new StatusCode() { code = (int)Code.ServerError, msg = Code.ServerError.ToString() }; }
        }

        /// <summary>
        /// 非法请求
        /// </summary>
        public static StatusCode BadRequest
        {
            get { return new StatusCode() { code = (int)Code.BadRequest, msg = Code.BadRequest.ToString() }; }
        }

        /// <summary>
        /// 访问未授权的资源
        /// </summary>
        public static StatusCode Unauthorized
        {
            get { return new StatusCode() { code = (int)Code.Unauthorized, msg = Code.Unauthorized.ToString() }; }
        }

        /// <summary>
        /// 参数错误
        /// </summary>
        public static StatusCode ArgesError
        {
            get { return new StatusCode() { code = (int)Code.ArgesError, msg = Code.ArgesError.ToString() }; }
        }

        /// <summary>
        /// 错误码未定义错误
        /// </summary>
        public static StatusCode ErrorCodeUndefined
        {
            get { return new StatusCode() { code = (int)Code.ErrorCodeUndefined, msg = Code.ErrorCodeUndefined.ToString() }; }
        }
        #endregion

    }

}
