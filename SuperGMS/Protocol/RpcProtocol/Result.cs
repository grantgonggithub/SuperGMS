/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Protocol
 文件名：Result
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/4/19 11:40:44

 功能描述：定义一个统一的返回结果外壳
 
{
    "rid": "0ad0ffcd5e6546eab1612885709adc49",
    "c": 200,
    "msg": "ok",
    "v": {
        "v1": true
    },
    "icp": false,
    "cs": "xx234rwerwerwerwer"
}
----------------------------------------------------------------*/


namespace SuperGMS.Protocol.RpcProtocol
{
    /// <summary>
    /// 请求返回结果
    /// </summary>
    /// <typeparam name="R"></typeparam>
    public class Result<R>:BasicArgs<R>
    {
        private string responseId;
        /// <summary>
        /// 响应的Id，跟RequestId对应
        /// </summary>
        public string rid
        {
            get { return responseId; }
            set { responseId = value; }
        }

        private int code;
        /// <summary>
        /// 业务处理状态
        /// </summary>
        public int c
        {
            get { return code; }
            set { code = value; }
        }

        private string codeMsg;
        /// <summary>
        /// 状态描述
        /// </summary>
        public string msg
        {
            get { return codeMsg; }
            set { codeMsg = value; }
        }

        private string _error;
        /// <summary>
        /// 错误描述
        /// </summary>
        public string error
        {
            get { return _error; }
            set { _error = value; }
        }

        private string[] _msgParam;

        public string[] MsgParam
        {
            get { return _msgParam; }
            set { _msgParam = value; }
        }
    }
}
