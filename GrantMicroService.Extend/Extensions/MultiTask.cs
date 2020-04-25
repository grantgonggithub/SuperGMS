using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrantMicroService.ExceptionEx;

namespace GrantMicroService.Extend.Extensions
{
    /// <summary>
    /// 多任务并发处理类
    /// 如果你有一个任务需要循环执行并返回结果,那么骚年就用这个东西吧, 让程序瞬间提速100倍
    /// 会同时将任务拆分成N个相同的任务只是参数不同, 同时运行,通过.net Task 类库自动控制同时并发线程数,不会因为线程数过多而阻塞线程, 并把执行结果合并返回.
    /// 执行单个任务的时候如果发生异常,则会将异常保留同其他返回结果一起返回.
    /// 
    /// 注意: 
    /// 1.因为是并发多线程处理, 所以保证线程安全, 也就是方法内不要用静态变量, 全局变量
    /// 2.DBContext需要独立, 不能让多个任务里面使用相同的DBContext.
    /// 3.每个方法是独立的事务,单独其中一个任务失败不影响其他任务成功
    /// </summary>
    public class MultiTask
    {
        /// <summary>
        /// 自动拆分参数
        /// 比如 线程最小任务为10个.
        /// 如果你有11个参数,会拆分为 1个线程10个参数,1个线程1个参数
        /// 如果你有110个参数,会拆分为10个线程,每个线程11个
        /// </summary>
        /// <param name="listObject">参数数组</param>
        /// <param name="minTaskInput">最小线程参数个数,最大线程数,同学不是说线程数越多越快, 需要考虑你的任务里面是否复杂,这个需要和下面那个参数进行配合调优</param>
        /// <param name="maxTaskInput">最大线程参数个数,目前还没有想好这个怎么用.所以暂时没用,写了也没用</param>
        /// <returns></returns>
        public static List<object> SplitInputObjects(List<object[]> listObject, int minTaskInput = 10, int maxTaskInput = 1000)
        {
            var inputArray = new List<object>();
            //取到每个线程的数量
            // var taskInputCount = listObject.Count / maxTaskInput;
            //设置最大线程参数数量
            var taskInputCount = minTaskInput;
            for (int i = 0; i < listObject.Count; i++)
            {
                var list = new List<object[]>();
                for (int j = 0; j < taskInputCount && i < listObject.Count; j++, i++)
                {
                    list.Add(listObject[i]);
                }
                inputArray.Add(list);
                //避免因taskInputCount跳出循环后多一次i++
                i--;
            }
            return inputArray;
        }
        /// <summary>
        /// 参数分组，通过分组个数，对参数进行平均分组
        /// </summary>
        /// <param name="listObject">参数</param>
        /// <param name="groupCount">分组个数</param>
        /// <returns></returns>
        public static List<object> InputObjectsGroups(List<object> listObject, int groupCount = 10)
        {
            var inputArray = new List<object>();
            var count = listObject.Count / groupCount;
            if (listObject.Count % groupCount != 0)
                count++;
            for (var i = 0; i < groupCount; i++)
            {
                var data = listObject.Skip(i * count).Take(count).ToList();
                if (data.Any())
                {
                    inputArray.Add(data);
                }
            }
            return inputArray;
        }
        /// <summary>
        /// 自动拆分参数重构
        /// </summary>
        /// <param name="listObject"><see cref="listObject"/></param>
        /// <param name="maxTaskCount"><see cref="maxTaskCount"/></param>
        /// <param name="minTaskInput"><see cref="minTaskInput"/></param>
        /// <returns></returns>
        public static List<object> SplitInputObjects(List<object> listObject, int minTaskInput = 10, int maxTaskInput = 1000)
        {
            List<object[]> inputs = listObject.Select(a => new object[] { a }).ToList();
            return SplitInputObjects(inputs, minTaskInput, maxTaskInput);
        }
        /// <summary>
        /// 并行处理方法 , 输入参数必须是object 类型
        /// </summary>
        /// <param name="function">此方法的输入参数必须是一个object参数</param>
        /// <param name="inputObjects">输入参数</param>
        /// <returns>返回值</returns>
        public MultiTaskResult[] Go(Action<object> function, List<object> inputObjects, int taskNum = 10)
        {
            object lockobj = new object();
            taskNum = 2;
            List<MultiTaskResult> listReturn = new List<MultiTaskResult>();
            List<Task> temp = new List<Task>();
            for (int i = 0; i < inputObjects.Count; i++)
            {
                if (function != null)
                {
                    var input = inputObjects[i];
                    Task t = new Task(() =>
                    {
                        var rst = new MultiTaskResult { inputObject = input };
                        try
                        {
                            function(input);
                        }
                        catch (Exception ex)
                        {
                            rst.exp = ex;
                        }
                        lock (lockobj)
                        {
                            listReturn.Add(rst);
                        }
                    });
                    t.Start();
                    temp.Add(t);
                }
                if (temp.Count >= taskNum || i + 1 >= inputObjects.Count)
                {
                    Task.WaitAll(temp.ToArray());
                    temp.Clear();
                }
            }
            return listReturn.ToArray();
            
        }

        /// <summary>
        /// 并行处理方法 , 输入参数必须是object 类型
        /// </summary>
        /// <param name="function">此方法的输入参数必须是一个object参数</param>
        /// <param name="inputObjects">输入参数</param>
        /// <returns>返回值</returns>
        public MultiTaskResult<TResult>[] Go<TResult>(Func<object, TResult> function, List<object> inputObjects, int taskNum = 10)
        {
            object lockobj = new object();
            taskNum = 2;
            List<MultiTaskResult<TResult>> listReturn = new List<MultiTaskResult<TResult>>();
            List<Task> temp = new List<Task>();
            for (int i = 0; i < inputObjects.Count; i++)
            {
                if (function != null)
                {
                    var input = inputObjects[i];
                    Task t = new Task(() =>
                    {
                        MultiTaskResult<TResult> rst = new MultiTaskResult<TResult> { inputObject = input };
                        try
                        {
                            rst.Results = function(input);
                        }
                        catch (Exception ex)
                        {
                            rst.exp = ex;
                        }
                        lock (lockobj)
                        {
                            listReturn.Add(rst);
                        }

                    });
                    t.Start();
                    temp.Add(t);
                }
                if (temp.Count > taskNum || i + 1 >= inputObjects.Count)
                {
                    Task.WaitAll(temp.ToArray());
                    temp.Clear();
                }
            }
            return listReturn.ToArray();
        }


        /// <summary>
        /// 将异步返回的异常构建一个大异常抛出去,当然你也可以自己擦屁股
        /// </summary>
        /// <param name="results"></param>
        public void BuildBigException<TResult>(List<MultiTaskResult<TResult>> results)
        {
            var sb = new StringBuilder();
            var errors = results.Where(a => a.exp != null).ToList();
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    if (error.exp is BusinessException)
                    {
                        sb.Append(error.exp.Message + ",");
                    }
                    else
                    {
                        sb.Append(ExceptionTool.GetSimpleErrorMsgByException(error.exp) + ",");
                    }
                }
                string sbString = sb.ToString();
                throw new Exception(sbString.Substring(0, sbString.Length - 1));
            }
        }

        /// <summary>
        /// 将异步返回的异常构建一个大异常抛出去,当然你也可以自己擦屁股
        /// </summary>
        /// <param name="results"></param>
        public void BuildBigException(MultiTaskResult[] results)
        {
            var sb = new StringBuilder();
            var errors = results.Where(a => a.exp != null).ToList();
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    if (error.exp is BusinessException)
                    {
                        sb.Append(error.exp.Message + ",");
                    }
                    else
                    {
                        sb.Append(ExceptionTool.GetSimpleErrorMsgByException(error.exp) + ",");
                    }
                }
                string sbString = sb.ToString();
                throw new Exception(sbString.Substring(0, sbString.Length - 1));
            }
        }
    }

    /// <summary>
    /// 多任务返回结果类
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class MultiTaskResult<TResult> : MultiTaskResult
    {
        /// <summary>
        /// 任务返回结果
        /// </summary>
        public TResult Results { set; get; }
    }

    /// <summary>
    /// 多任务返回结果类
    /// </summary>
    public class MultiTaskResult
    {
        /// <summary>
        /// 任务输入参数
        /// </summary>
        public object inputObject { set; get; }

        /// <summary>
        /// 任务异常内容
        /// </summary>
        public Exception exp { set; get; }
    }
}
