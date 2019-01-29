using System;
using System.Collections.Generic;
using System.IO;
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
                var task = new Task(() => { dictMethod[number].Invoke(null, null); }, TaskCreationOptions.AttachedToParent);

                task.Start();
                task.Wait();
                goto start;
            }





        }



        //Автоматический запуск задания по завершению придыдущего
        private static void RichterAutomaticlyPlayTask()
        {
            Task<int> t = Task.Run(() => Sum(CancellationToken.None, 10000));
            Task cwr = t.ContinueWith(task => Console.WriteLine("The sum is:" + (task.IsFaulted ? task.Exception.Message : task.Result.ToString()))/*,TaskContinuationOptions.OnlyOnFaulted*/);
        }

        //Отмена всех автоматически запускаемых задач
        private static void RichterCanceldAutomaticlyPlayTask()
        {
            Task<int> t = Task.Run(() => Sum(10000));
            t.ContinueWith(task => Console.WriteLine("The sum is:" + (task.IsFaulted ? task.Exception.Message : task.Result.ToString())), TaskContinuationOptions.OnlyOnRanToCompletion);

            t.ContinueWith(task => Console.WriteLine("The sum is:" + (task.IsFaulted ? task.Exception.Message : task.Result.ToString())), TaskContinuationOptions.OnlyOnFaulted);

            t.ContinueWith(task => Console.WriteLine("Sum was canceled"), TaskContinuationOptions.OnlyOnRanToCompletion);

        }

        //Дочерние задания
        private static void RichterChikdernTasks()
        {
            Task<int[]> parent = new Task<int[]>(() =>
            {
                var result = new Int32[3];

                new Task(() => { result[0] = Sum(1000); }, TaskCreationOptions.AttachedToParent).Start();
                new Task(() => { result[1] = Sum(2000); }, TaskCreationOptions.AttachedToParent).Start();
                new Task(() => { result[2] = Sum(3000); }, TaskCreationOptions.AttachedToParent).Start();

                return result;
            });
            Console.WriteLine(parent.Status);
            var cwt = parent.ContinueWith(parentTask => Array.ForEach(parentTask.Result, Console.WriteLine), TaskContinuationOptions.AttachedToParent);

            parent.Start();

        }

        //Фабрика заданий
        private static void RichterTasksFactory()
        {
            Task parent = new Task(() =>
            {
                var cts = new CancellationTokenSource();
                var tf = new TaskFactory<Int32>(cts.Token,
                    TaskCreationOptions.AttachedToParent,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default
                    );

                var childTask = new[]
                {
                     tf.StartNew(()=>Sum(cts.Token,1000)),
                     tf.StartNew(()=>Sum(cts.Token,2000000)),
                     tf.StartNew(()=>Sum(cts.Token,int.MaxValue)),
                };

                for (int task = 0; task < childTask.Length; task++)
                {
                    childTask[task].ContinueWith(t => cts.Cancel(), TaskContinuationOptions.OnlyOnFaulted);
                }

                tf.ContinueWhenAll(childTask, compleedTask =>
                 compleedTask.Where(t => !t.IsFaulted && !t.IsCanceled)
                .Max(t => t.Result), CancellationToken.None)
                .ContinueWith(t => Console.WriteLine("The maximum is:" + t.Result),
                TaskContinuationOptions.ExecuteSynchronously);

            });

            parent.ContinueWith(p =>
            {
                StringBuilder sb = new StringBuilder("The follow exception occured" + Environment.NewLine);

                foreach (var e in p.Exception.Flatten().InnerExceptions)
                {
                    sb.AppendLine("" + e.GetType().ToString());
                    Console.WriteLine(sb.ToString());
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            parent.Start();
        }

        //Планировщик заданий
        private static void RichterTaskScheduler()
        {
            MyForm myForm = new MyForm();
            myForm.ShowDialog();
        }

        //ParalelForEach
        private static void RichterParalelForEach()
        {
           Console.WriteLine( DirectoryBytes(@"D:\Xamarin", "*", SearchOption.AllDirectories));
        }

            #region HelpMethod
            public static int Sum(int n)
        {
            int sum = 0;
            for (; n > 0; n--)
            {
                checked { sum += n; }
            }

            return sum;
        }

        public static int Sum(CancellationToken ct, int n)
        {
            int sum = 0;
            for (; n > 0; n--)
            {

                ct.ThrowIfCancellationRequested();
                checked { sum += n; }
            }
            Thread.Sleep(5000);
            return sum;
        }

        public static long DirectoryBytes(string path, string searchPattern, SearchOption searchOption)
        {
            var files = Directory.EnumerateFiles(path, searchPattern, searchOption);
            int masterTotal=0;
            ParallelLoopResult result = Parallel.ForEach<string, Int64>
                (files, () =>
                 {
                     //Инциализация задач
                     return 0;
                 }, (file, loopState, index, taskLocalTotal) =>
                 {
                     //body вызывается один раз для каждого элемента
                     long fileLenght = 0;
                     FileStream fs = null;
                     try
                     {
                         fs = File.OpenRead(file);
                         fileLenght = fs.Length;

                     }
                     catch (IOException)
                     {
                         //Игнорирование файлов к которым нет доступа
                     }
                     finally
                     {
                         if (fs != null) fs.Dispose();

                     }
                     return taskLocalTotal + fileLenght;
                 },
                 taskLocalTotal => 
                 {
                     //localFinaly вызывается один раз в конце задания
                     Interlocked.Add(ref masterTotal, (int)taskLocalTotal);
                 }
                 );
            return masterTotal;
        }
        #endregion

    }
}
