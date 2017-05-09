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
        static void Main(string[] args)
        {
            string сheckTableHashSumFiles = "IF OBJECT_ID (N'dbo.HashSumFiles', N'U') IS NULL CREATE TABLE dbo.HashSumFiles (id int IDENTITY(1,1), fileName varchar(250), hashSum Nvarchar(250), createWrite datetime2(7))";
            string сheckTableErrorFiles = "IF OBJECT_ID (N'dbo.ErrorFiles', N'U') IS NULL CREATE TABLE dbo.ErrorFiles (id int IDENTITY(1,1), errorFiles Nvarchar(250), createWrite datetime2(7))";         
            Console.WriteLine("Введите строку подключения к базе данных MSSQL в формате: Data Source=..; Initial Catalog=..; Integrated Security=True \n");
            OperationBD bdo = new OperationBD();
            string connectBD = Console.ReadLine();
            bdo.CheckingBD(connectBD, сheckTableHashSumFiles); // проверка наличия таблицы в БД  / создание таблицы HashSumFiles
            bdo.CheckingBD(connectBD, сheckTableErrorFiles);  //  проверка наличия таблицы в БД  / создание таблицы ErrorFiles
            Console.WriteLine("Введите каталог");
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Console.ReadLine());
                Console.WriteLine("Подождите, идёт подготовка");
                foreach (string file in OperationFile.DirSearch(dir))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state) { OperationFile.SumHash(connectBD); }), null); // обработка рабочими потоками файлов из очереди
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); Console.ReadLine();
            }
        }
    }
}