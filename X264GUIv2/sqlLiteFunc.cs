using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Reflection;
using X264GUIv2.Models;

namespace X264GUIv2
{
    internal class sqlLiteFunc : IDisposable
    {
        public SqliteConnection connection;

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
            command.CommandText = @$"CREATE TABLE IF NOT EXISTS Detail 
(
    GUID TEXT,
    bitrate INTEGER,
    frameMode INTEGER,
    frameStr TEXT,
    resolutionW INTEGER,
    resolutionH INTEGER,
    isNew NUMERIC
)";
            command.ExecuteNonQuery();

            command.CommandText = @$"CREATE TABLE IF NOT EXISTS Main 
(
    GUID TEXT,
    isAcc NUMERIC,
    duration REAL,
    size INTEGER,
    InFile TEXT,
    idx INTEGER,
    run INTEGER
)";
            command.ExecuteNonQuery();
        }

        public void Insert(IList<FfprobeOutput> ffprobeOutputs)
        {
            connection.Execute("DELETE FROM Detail");
            connection.Execute("DELETE FROM Main");

            var detail = ffprobeOutputs.Select(x => new
            {
                GUID = x.Guid.ToString(),
                x.OriDetail.bitrate,
                frameMode = (int)x.OriDetail.frameMode,
                x.OriDetail.frameStr,
                x.OriDetail.resolutionW,
                x.OriDetail.resolutionH,
                isNew = 0,
            }).ToList();

            detail.AddRange([.. ffprobeOutputs.Select(x => new
            {
                GUID = x.Guid.ToString(),
                x.NewDetail.bitrate,
                frameMode = (int)x.NewDetail.frameMode,
                x.NewDetail.frameStr,
                x.NewDetail.resolutionW,
                x.NewDetail.resolutionH,
                isNew = 1,
            })]);

            connection.Execute(@"
                INSERT INTO Detail (
                    GUID,
                    bitrate,
                    frameMode,
                    frameStr,
                    resolutionW,
                    resolutionH,
                    isNew
                ) VALUES (
                    @GUID,
                    @bitrate,
                    @frameMode,
                    @frameStr,
                    @resolutionW,
                    @resolutionH,
                    @isNew
                )",
            detail);

            connection.Execute(@"
                INSERT INTO Main (
                    GUID,
                    isAcc,
                    duration,
                    size,
                    InFile,
                    idx,
                    run
                ) VALUES (
                    @GUID,
                    @isAcc,
                    @duration,
                    @size,
                    @InFile,
                    @idx,
                    @run
                )",
                ffprobeOutputs.Select(x => new
                {
                    GUID = x.Guid.ToString(),
                    isAcc = x.isAcc ? 1 : 0,
                    x.duration,
                    x.size,
                    x.InFile,
                    idx = x.index,
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

        public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
        {
            public override void SetValue(IDbDataParameter parameter, Guid value)
            {
                parameter.Value = value.ToString();
            }

            public override Guid Parse(object value)
            {
                return Guid.Parse(value.ToString() ?? new Guid().ToString());
            }
        }
    }
}
