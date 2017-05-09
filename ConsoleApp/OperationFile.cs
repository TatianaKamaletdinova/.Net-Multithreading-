using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{        
    class OperationFile
    {
        
        static object lockerSum = new object();
        static public Queue<string> queueFiles = new Queue<string>();
        static Queue<string> queueHashSum = new Queue<string>();
        static public Queue<string> DirSearch(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;
            try
            {
                files = root.GetFiles("*.*");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    queueFiles.Enqueue(fi.FullName);
                }
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    DirSearch(dirInfo);
                }
            }
            return queueFiles;
        }

        public static void SumHash(string connBD)
        {
            string firstItem;
            int y = 0;
            int coutnFiles;
           
            lock (lockerSum)
            {
                if (queueFiles.Count == 0) { return; }
                firstItem = queueFiles.Dequeue(); 
            }

            coutnFiles = queueFiles.Count;          
            using (var md5 = MD5.Create())
            {
                try
                {
                    using (var stream = File.OpenRead(firstItem))
                    {
                        string hashSum = firstItem + " = " + BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty); //расчёт хеш-суммы файла      
                        y++;
                        queueHashSum.Enqueue(hashSum); //добавляем в очередь хеш-сумму файла   
                        Data dataResult = new Data();
                        dataResult.QueueHashSum = queueHashSum;
                        dataResult.ConnBD = connBD;
                        Thread myThread = new Thread(new ParameterizedThreadStart(OperationBD.InsertBD)); // запись результатов хеш-сумм в БД
                        myThread.Start(dataResult); 
                        Console.WriteLine("Кол-во обработанных файлов: " + y + " / " + "Кол-во файлов для обработки: " + coutnFiles); Console.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    Data errorMes = new Data();
                    errorMes.ErrorMessage = ex.Message;
                    errorMes.ConnBD = connBD;
                    Thread myThread = new Thread(new ParameterizedThreadStart(OperationBD.InsertErrorBD)); // запись ошибок при обработке файлов в БД
                    myThread.Start(errorMes); 
               }
           }
        }
    }
}
