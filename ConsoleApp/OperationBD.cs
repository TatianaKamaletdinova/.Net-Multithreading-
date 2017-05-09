using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class OperationBD
    {
        static object lockerBD = new object();
        static string nameSum;
        
        public void CheckingBD(string connBD, string checking)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connBD))
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(checking, connect))
                        {
                            connect.Open();
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); Console.ReadLine(); }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); Console.ReadLine(); }
        } 

        public static void InsertBD(object data)
        {
            try
            {
                Data dataResult = (Data)data;
                nameSum = dataResult.QueueHashSum.Dequeue();
  
                if (nameSum == null) { return; }
                
                String[] fileInfo = nameSum.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                string sql = string.Format("Insert Into HashSumFiles" + "(fileName, hashSum, createWrite) Values(@fileName, @hashSum, @createWrite)");
                
                try
                {
                    using (SqlConnection connect = new SqlConnection(dataResult.ConnBD))
                    {
                        using (SqlCommand command = new SqlCommand(sql, connect))
                        {
                            connect.Open();
                            command.Parameters.AddWithValue("@fileName", fileInfo[0]);
                            command.Parameters.AddWithValue("@hashSum", fileInfo[1]);
                            command.Parameters.AddWithValue("@createWrite", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Message); Console.ReadLine(); }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); Console.ReadLine(); }
        }

        public static void InsertErrorBD(object data)
        {
            Data errorMessage = (Data)data;

            string sql = string.Format("Insert Into ErrorFiles" + "(errorFiles, createWrite) Values(@errorFiles, @createWrite)");
            
            try
            {
                using (SqlConnection connect = new SqlConnection(errorMessage.ConnBD))
                {
                    using (SqlCommand command = new SqlCommand(sql, connect))
                    {
                        connect.Open();
                        command.Parameters.AddWithValue("@errorFiles", errorMessage.ErrorMessage);
                        command.Parameters.AddWithValue("@createWrite", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); Console.ReadLine(); }
        }      
    }
}
