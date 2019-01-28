using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RikrerTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
        start:
            int i = 1;
            var nonActvate = new[] { "Main", "Sum", "ToString", "ReferenceEquals" };
            Dictionary<int, MethodInfo> dictMethod = new Dictionary<int, MethodInfo>();
            foreach (var method in typeof(Program).GetRuntimeMethods().Where(x => x.Name.StartsWith("Richter")))
            {
                dictMethod.Add(i, method);
                Console.WriteLine($"{i}-{method.Name.Replace("Richter", "")}");
                i++;
            }

            Console.WriteLine("Enter the number of method");

            Thread.Sleep(1000);

            int number = 0;

            int.TryParse(Console.ReadLine(), out number);

            if (number == 0)
            {
                goto start;
            }
            if (number > dictMethod.Count())
            {
                goto start;
            }


            if (number != -1)
            {
                dictMethod[number].Invoke(null, null);
                goto start;
            }



            //Task parent = new Task(() =>
            //{
            //    var cts = new CancellationTokenSource();
            //    var tf = new TaskFactory<Int32>(cts.Token, 
            //        TaskCreationOptions.AttachedToParent,
            //        TaskContinuationOptions.ExecuteSynchronously, 
            //        TaskScheduler.Default
            //        );
            //    var childTask = new[]
            //    {
            //        tf.StartNew(()=>Sum)
            //    };
            //});

        }



        //Автоматический запуск задания по завершению придыдущего
        private static void RichterAutomaticlyPlayTask()
        {
            Task<int> t = Task.Run(() => Sum(CancellationToken.None, 10000));
            Task cwr = t.ContinueWith(task => Console.WriteLine("The sum is:" + (task.IsFaulted?task.Exception.Message:task.Result.ToString()))/*,TaskContinuationOptions.OnlyOnFaulted*/);
        }

        //Отмена всех автоматически запускаемых задач
        private static void RichterCanceldAutomaticlyPlayTask()
        {
            Task<int> t = Task.Run(() => Sum(10000));
             t.ContinueWith(task => Console.WriteLine("The sum is:" + (task.IsFaulted ? task.Exception.Message : task.Result.ToString())),TaskContinuationOptions.OnlyOnRanToCompletion);

            t.ContinueWith(task => Console.WriteLine("The sum is:" + (task.IsFaulted ? task.Exception.Message : task.Result.ToString())), TaskContinuationOptions.OnlyOnFaulted);

            t.ContinueWith(task => Console.WriteLine("Sum was canceled"), TaskContinuationOptions.OnlyOnRanToCompletion);

        }

        static int Sum(int n)
        {
            int sum = 0;
            for (; n > 0; n--)
            {
                checked { sum += n; }
            }

            return sum;
        }

        static int Sum(CancellationToken ct, int n)
        {
            int sum = 0;
            for (; n > 0; n--)
            {
                ct.ThrowIfCancellationRequested();
                checked { sum += n; }
            }
            
            return sum;
        }
    }
}
