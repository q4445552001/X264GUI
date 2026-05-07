namespace X264GUIv2.Models
{
    public class SqlLiteTableCreate
    {
        public List<string> CreateArr { get; set; } = [];
        public string CreateStr => string.Join(",", CreateArr);

        public List<string> Arr { get; set; } = [];
        public string Str => string.Join(",", Arr);

        public string InsStr => $"@{string.Join(",@", Arr)}";
    }
}
