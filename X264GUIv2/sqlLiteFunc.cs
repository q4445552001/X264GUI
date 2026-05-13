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

        private readonly SqlLiteTableCreate mainSql = GetTableCreateSql<FfprobeOutputMain>();
        private readonly SqlLiteTableCreate detailSql = GetTableCreateSql<FfprobeOutputDetail>();

        public sqlLiteFunc()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            string dbName = $"{Assembly.GetExecutingAssembly().EntryPoint?.DeclaringType?.Namespace}.db";
            connection = new SqliteConnection($@"Data Source={AppDomain.CurrentDomain.BaseDirectory}{dbName};Pooling=False;");
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

            #region Detail

            //來源
            var detail = ffprobeOutputs.Where(x => x.MergeData is null).Select(x => new
            {
                x.MainData.OriDetail.bitrate,
                frameMode = (int)x.MainData.OriDetail.frameMode,
                x.MainData.OriDetail.frameStr,
                x.MainData.OriDetail.resolutionW,
                x.MainData.OriDetail.resolutionH,
                Guid = x.MainData.Guid.ToString(),
                isNew = 0,
            }).ToList();

            //改後
            detail.AddRange([.. ffprobeOutputs.Where(x => x.MergeData is null).Select(x => new
            {
                x.MainData.NewDetail.bitrate,
                frameMode = (int)x.MainData.NewDetail.frameMode,
                x.MainData.NewDetail.frameStr,
                x.MainData.NewDetail.resolutionW,
                x.MainData.NewDetail.resolutionH,
                Guid = x.MainData.Guid.ToString(),
                isNew = 1,
            })]);

            //來源
            detail.AddRange([.. ffprobeOutputs.Where(x => x.MergeData is not null).SelectMany(x => x.MergeData!).Select(x => new
            {
                x.OriDetail.bitrate,
                frameMode = (int)x.OriDetail.frameMode,
                x.OriDetail.frameStr,
                x.OriDetail.resolutionW,
                x.OriDetail.resolutionH,
                Guid = x.Guid.ToString(),
                isNew = 0,
            })]);

            //改後
            detail.AddRange([.. ffprobeOutputs.Where(x => x.MergeData is not null).SelectMany(x => x.MergeData!).Select(x => new
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
            #endregion

            #region Main
            var main = ffprobeOutputs.Where(x => x.MergeData is null).Select(x => new
            {
                Guid = x.MainData.Guid.ToString(),
                isAac = x.MainData.isAac ? 1 : 0,
                x.MainData.duration,
                x.MainData.videoSize,
                x.MainData.audioSize,
                x.MainData.InFile,
                x.MainData.idx,
                run = (int)x.MainData.run,
                x.MainData.videoType,
                x.MainData.audioMap,
                x.MainData.MergeGuid,
                x.MainData.videoCodeName,
            }).ToList();

            main.AddRange([.. ffprobeOutputs.Where(x => x.MergeData is not null).SelectMany(x => x.MergeData!).Select(x => new
            {
                Guid = x.Guid.ToString(),
                isAac = x.isAac ? 1 : 0,
                x.duration,
                x.videoSize,
                x.audioSize,
                x.InFile,
                x.idx,
                run = (int)x.run,
                x.videoType,
                x.audioMap,
                x.MergeGuid,
                x.videoCodeName,
            })]);

            connection.Execute(@$"INSERT INTO Main ({mainSql.Str}) VALUES ({mainSql.InsStr})", main);
            #endregion
        }

        public void DropTable()
        {
            connection.Execute(@"DROP TABLE IF EXISTS Main");
            connection.Execute(@"DROP TABLE IF EXISTS Detail");
            CreateTable();
        }

        public List<FfprobeOutput> SelectTable()
        {
            IEnumerable<FfprobeOutputMain> ffprobeOutputs = connection.Query<FfprobeOutputMain>("select * from Main");
            IEnumerable<FfprobeOutputDetail> details = connection.Query<FfprobeOutputDetail>("select * from Detail");

            List<FfprobeOutput> ffprobes = [..
                from ffprobeOutput in ffprobeOutputs
                join detail1 in details.Where(x => x.isNew == 0) on ffprobeOutput.Guid equals detail1.Guid into detail1Empty
                from detail1 in detail1Empty.DefaultIfEmpty()
                join detail2 in details.Where(x => x.isNew == 1) on ffprobeOutput.Guid equals detail2.Guid into detail2Empty
                from detail2 in detail2Empty.DefaultIfEmpty()
                orderby ffprobeOutput.idx
                select new FfprobeOutput
                {
                    MainData = new FfprobeOutputMain
                    {
                        Guid = ffprobeOutput.Guid,
                        MergeGuid = ffprobeOutput.MergeGuid,
                        InFile = ffprobeOutput.InFile,
                        isAac = ffprobeOutput.isAac,
                        audioMap = ffprobeOutput.audioMap,
                        videoType = ffprobeOutput.videoType,
                        duration = ffprobeOutput.duration,
                        videoSize = ffprobeOutput.videoSize,
                        audioSize = ffprobeOutput.audioSize,
                        videoCodeName = ffprobeOutput.videoCodeName,
                        idx = ffprobeOutput.idx,
                        run = ffprobeOutput.run,
                        OriDetail = detail1,
                        NewDetail = detail2,
                    }
                }];

            List<FfprobeOutput> ffprobes1 = [.. ffprobes.Where(x => x.MainData.MergeGuid is null).OrderBy(x => x.MainData.idx)];
            List<FfprobeOutput> ffprobes2 = [.. ffprobes.Where(x => x.MainData.MergeGuid is not null).OrderBy(x => x.MainData.idx)];

            ffprobes1.AddRange([..ffprobes2.Where(x => x.MainData.Guid == x.MainData.MergeGuid).Select(x => new FfprobeOutput
            {
                MainData = x.MainData,
                MergeData = [..ffprobes2.Where(p => p.MainData.MergeGuid == x.MainData.Guid).OrderBy(p => p.MainData.idx).Select(x => x.MainData)],
            })]);

            return ffprobes1;
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
