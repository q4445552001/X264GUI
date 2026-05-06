using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace X264GUIv2
{
    internal class sqlLiteFunc : IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            throw new NotImplementedException();
        }

        public sqlLiteFunc()
        {
            //            await ApplicationData.Current.LocalFolder
            //            .CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            //            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
            //                                         "sqliteSample.db");

            //            using var connection = new SqliteConnection("Data Source=hello.db");
            //            connection.Open();

            //            using var command = connection.CreateCommand();
            //            command.CommandText = """
            //    SELECT name
            //    FROM user
            //    WHERE id = $id
            //""";
            //            command.Parameters.AddWithValue("$id", id);

            //            using var reader = command.ExecuteReader();

            //            while (reader.Read())
            //            {
            //                var name = reader.GetString(0);

            //                Console.WriteLine($"Hello, {name}!");
            //            }
        }
    }
}
