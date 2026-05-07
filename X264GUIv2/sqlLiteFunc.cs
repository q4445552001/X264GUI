using Dapper;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using X264GUIv2.Models;

namespace X264GUIv2
{
    internal class sqlLiteFunc : IDisposable
    {
        public SqliteConnection connection;

        private readonly SqlLiteTableCreate mainSql = GetTableCreateSql<FfprobeOutput>();
        private readonly SqlLiteTableCreate detailSql = GetTableCreateSql<FfprobeOutput.Detail>();

        public sqlLiteFunc()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            string dbName = $"{Assembly.GetExecutingAssembly().EntryPoint?.DeclaringType?.Namespace}.db";
            connection = new SqliteConnection($"Data Source={dbName}");
            connection.Open();

            CreateTable();
        }

        public void CreateTable()
        {
            var command = connection.CreateCommand();
            command.CommandText = @$"CREATE TABLE IF NOT EXISTS Detail ({detailSql.CreateStr})";
            command.ExecuteNonQuery();

            command.CommandText = @$"CREATE TABLE IF NOT EXISTS Main ({mainSql.CreateStr})";
            command.ExecuteNonQuery();
        }

        public void Insert(IList<FfprobeOutput> ffprobeOutputs)
        {
            connection.Execute("DELETE FROM Detail");
            connection.Execute("DELETE FROM Main");

            var detail = ffprobeOutputs.Select(x => new
            {
                x.OriDetail.bitrate,
                frameMode = (int)x.OriDetail.frameMode,
                x.OriDetail.frameStr,
                x.OriDetail.resolutionW,
                x.OriDetail.resolutionH,
                Guid = x.Guid.ToString(),
                isNew = 0,
            }).ToList();

            detail.AddRange([.. ffprobeOutputs.Select(x => new
            {
                x.NewDetail.bitrate,
                frameMode = (int)x.NewDetail.frameMode,
                x.NewDetail.frameStr,
                x.NewDetail.resolutionW,
                x.NewDetail.resolutionH,
                Guid = x.Guid.ToString(),
                isNew = 1,
            })]);

            connection.Execute(@$"INSERT INTO Detail ({detailSql.Str}) VALUES ({detailSql.InsStr})", detail);

            connection.Execute(@$"INSERT INTO Main ({mainSql.Str}) VALUES ({mainSql.InsStr})",
                ffprobeOutputs.Select(x => new
                {
                    Guid = x.Guid.ToString(),
                    isAac = x.isAac ? 1 : 0,
                    x.duration,
                    x.size,
                    x.InFile,
                    x.idx,
                    run = (int)x.run
                }));
        }

        public void DropTable()
        {
            connection.Execute(@"DROP TABLE IF EXISTS Main");
            connection.Execute(@"DROP TABLE IF EXISTS Detail");
            CreateTable();
        }

        public List<FfprobeOutput> SelectTable()
        {
            IList<FfprobeOutput> ffprobeOutputs = [.. connection.Query<FfprobeOutput>("select * from Main")];
            IList<FfprobeOutput.Detail> details = [.. connection.Query<FfprobeOutput.Detail>("select * from Detail")];

            for (int i = 0; i < ffprobeOutputs.Count; i++)
            {
                IList<FfprobeOutput.Detail> _details = [.. details.Where(x => x.Guid == ffprobeOutputs[i].Guid)];
                ffprobeOutputs[i].OriDetail = _details.FirstOrDefault(x => x.isNew == 0) ?? new();
                ffprobeOutputs[i].NewDetail = _details.FirstOrDefault(x => x.isNew == 1) ?? new();
            }

            return [.. ffprobeOutputs];
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
            GC.SuppressFinalize(this);
        }

        public static SqlLiteTableCreate GetTableCreateSql<T>()
        {
            SqlLiteTableCreate sqlLiteTableCreate = new();

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var getMethod = property.GetGetMethod();
                if (getMethod == null || !getMethod.IsPublic || property.IsDefined(typeof(NotMappedAttribute)))
                    continue;

                var t = property.PropertyType switch
                {
                    var a when typeof(string) == a => "TEXT",
                    var a when typeof(char) == a => "TEXT",
                    var a when typeof(int) == a || typeof(int?) == a => "INTEGER",
                    var a when typeof(long) == a || typeof(long?) == a => "INTEGER",
                    var a when typeof(short) == a || typeof(short?) == a => "INTEGER",
                    var a when typeof(byte) == a || typeof(byte?) == a => "BLOB",
                    var a when typeof(bool) == a || typeof(bool?) == a => "INTEGER",
                    var a when typeof(DateTime) == a || typeof(DateTime?) == a => "TEXT",
                    var a when typeof(decimal) == a || typeof(decimal?) == a => "REAL",
                    var a when typeof(double) == a || typeof(double?) == a => "REAL",
                    var a when typeof(float) == a || typeof(float?) == a => "REAL",
                    var a when typeof(Guid) == a || typeof(Guid?) == a => "TEXT",
                    _ => "TEXT",
                };

                sqlLiteTableCreate.Arr.Add(property.Name);
                sqlLiteTableCreate.CreateArr.Add($"[{property.Name}] {t}");
            }

            return sqlLiteTableCreate;
        }

        public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
        {
            public override void SetValue(IDbDataParameter parameter, Guid value) =>
                parameter.Value = value.ToString();

            public override Guid Parse(object value) =>
                Guid.Parse(value.ToString() ?? new Guid().ToString());
        }
    }
}
