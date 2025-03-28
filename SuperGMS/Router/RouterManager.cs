/*----------------------------------------------------------------
 Copyright (C) 2018 SuperGMS (Grant 巩建春)  本软件的所有源码都可以免费的进行学习交流,切勿用于商业用途 

 项目名称：SuperGMS.Router
 文件名：RouterManager
 创建者：grant (巩建春 e-mail : nnn987@126.com ;  QQ:406333743;   Tel:+86  15619212255)
 CLR版本：4.0.30319.42000
 时间：2017/5/8 18:18:09

 功能描述：

----------------------------------------------------------------*/
using System;
using System.Threading;

namespace SuperGMS.Router
{
   public class RouterManager
    {
        public static int GetPool(string uri)
        {
            return 0;
        }

        private static int next = -1;

        /// <summary>
        /// 顺序轮询
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetPolling(int min, int max)
        {
            int newValue = -1;
            do
            {
                newValue = next + 1;
                if (newValue >= max) newValue = min;
            }
            while ((Interlocked.CompareExchange(ref next, newValue, next) == next));
            return next;
        }


        private static Random random = new Random(DateTime.Now.Millisecond);
        /// <summary>
        /// 随机获取服务器
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandom(int min,int max)
        {
            ////构造一个100的样本空间
            //int[] idxs = new int[100];
            //for (int i = 0; i < idxs.Length;)
            //{
            //    int n = 0;
            //    for (int j = min; j < max; j++)
            //    {
            //        idxs[i + n] = j;
            //        n++;
            //    }
            //    i += n;
            //}
            //Random r = new Random(DateTime.Now.Millisecond);
            //return idxs[r.Next(0, idxs.Length)];

            return random.Next(min, max);
        }
    }
}
