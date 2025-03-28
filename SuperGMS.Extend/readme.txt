            Initlize
            // 点对点消息
            //{
            //    "ct": "youct",
            //    "tk": "ManagementSystems:fe5edd3ba7e0496e9cf8955029082aad",
            //    "cv": "1.0.0",
            //    "v": {
            //                    "Message": "我的测试消息-点对点消息",
            //        "IsDirectMessage": true,
            //        "ExChangeName": "mytest_direct_msg",
            //        "RouterName": "mytest_direct_msg"
            //    }
            //}
            // SuperDirectMessageHelper.Initlize(new MessageRouterMap { BussinessApiName=nameof(ReceiveDirectMessage), MQRouterName="mytest_direct_msg" });
            // 扇波消息 
            //{
            //   "ct": "youct",
            //   "tk": "ManagementSystems:a98164ffe0734ff28cb6e51b62987230",
            //   "cv": "1.0.0",
            //   "v": {
            //                   "Message": "我的测试消息-扇播消息",
            //       "IsDirectMessage": false,
            //       "ExChangeName": "mytest_fanout_msg",
            //       "RouterName": "mytest_fanout_msg"
            //   }
            // }
            // SuperFanoutMessageHelper.Initlize(new MessageRouterMap { BussinessApiName = nameof(ReceiveFanoutMessage), MQRouterName = "mytest_fanout_msg" });



    /// <summary>
    /// 点对点接收方消息测试例子
    /// </summary>
    //public class ReceiveDirectMessage : RpcBaseServer<ReceiveMessageArgs, Nullables>
    //{
    //    protected override Nullables Process(ReceiveMessageArgs valueArgs, out StatusCode code)
    //    {
    //        System.Console.WriteLine("收到消息:"+valueArgs.Msg);
    //        code = StatusCode.OK;
    //        return Nullables.NullValue;
    //    }

    //    protected override bool Check(ReceiveMessageArgs args, out StatusCode code)
    //    {
    //        return base.CheckLogin(args, out code);
    //    }
    //}

        /// <summary>
    /// 扇波接收方消息例子
    /// </summary>
    //public class ReceiveFanoutMessage : RpcBaseServer<ReceiveMessageArgs, Nullables>
    //{
    //    protected override Nullables Process(ReceiveMessageArgs valueArgs, out StatusCode code)
    //    {
    //        System.Console.WriteLine("收到消息:" + valueArgs.Msg);
    //        code = StatusCode.OK;
    //        return Nullables.NullValue;
    //    }

    //    protected override bool Check(ReceiveMessageArgs args, out StatusCode code)
    //    {
    //        return base.CheckLogin(args, out code);
    //    }
    //}


    /// <summary>
    /// 消息发送例子
    /// </summary>
    //public class SendMessage : RpcBaseServer<SendMessageArgs, Nullables>
    //{
    //    protected override Nullables Process(SendMessageArgs valueArgs, out StatusCode code)
    //    {
    //        var userInfo = Context.GetUserContext()?.UserInfo;
    //        var argsMs = new SuperGMS.Extend.BackGroundMessage.SetBackGroudMessageArgs
    //        {
    //            UserId = userInfo.UserId,
    //            TtId = userInfo.TenantInfo.TTID.ToString(),
    //            Args = args.Copy(),
    //            Data = JsonConvert.SerializeObject(new { Msg = valueArgs.Message }),
    //            CreateDateTime = DateTime.Now,
    //        };
    //        //argsMs.Args.v = new { Msg = valueArgs.Message };
    //        if (valueArgs.IsDirectMessage)
    //        {
    //            argsMs.MQRouterName = valueArgs.RouterName;
    //            SuperDirectMessageHelper.SetDirectMessage(argsMs) ;
    //        }
    //        else
    //        {
    //            argsMs.MQRouterName = valueArgs.ExChangeName;
    //            SuperFanoutMessageHelper.SetFanoutMessage(argsMs);
    //        }
    //        code = StatusCode.OK;
    //        return Nullables.NullValue;
    //    }

    //    protected override bool Check(SendMessageArgs args, out StatusCode code)
    //    {
    //        return base.CheckLogin(args, out code);
    //    }
    //}
