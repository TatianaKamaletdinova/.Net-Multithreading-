using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static object lockerSum = new object();
        static object lockersWrite = new object();
        static Queue<string> queueFiles = new Queue<string>(); //создание очереди файлов
        static Queue<string> queueHashSum = new Queue<string>();
        static void Main(string[] args)
        {
            Console.WriteLine("Введите каталог");
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Console.ReadLine());//указывем каталог
                foreach (var item in dir.GetFiles())
                {
                    queueFiles.Enqueue(item.FullName); //добавляем в очередь найденные файлы               
                    Thread threadCash = new Thread(new ThreadStart(SumHash));
                    threadCash.Start();//запускаем поток для расчёта хэш-суммы файла   
                }              
                Console.ReadLine();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                Main(args);
            }
        }
        public static void SumHash()
        {
            int y=0;
            lock (lockerSum)
                { 
                string firstItem = queueFiles.Dequeue();
                using (var md5 = MD5.Create())
                {
                    try
                    {
                        using (var stream = File.OpenRead(firstItem))
                        {
                            string hashSum = firstItem + " = " + BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty); //расчёт хеш-суммы файла      
                            y++;
                            queueHashSum.Enqueue(hashSum); //добавляем в очередь хеш-сумму файла       
                            Thread writeBD = new Thread(new ThreadStart(WriteBD)); 
                            writeBD.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }            
                }
            }
            Console.WriteLine("Кол-во обработанных файлов: " + y + " / " + "Кол-во файлов для обработки: " + queueFiles.Count);                          
        }
        public static void WriteBD()
        {
            //подключение к БД и запись в таблицу строки hashSum
            lock (lockersWrite)
            {             
                string stringSum = queueHashSum.Dequeue();
                Console.WriteLine(stringSum);                  
            }
        }
    }
}