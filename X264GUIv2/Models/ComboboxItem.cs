namespace X264GUIv2.Models
{
    internal class ComboboxItem(string text, string value)
    {
        public string Value { get; set; } = value;
        public string Text { get; set; } = text;
        public override string ToString() { return Text; }
    }
}
