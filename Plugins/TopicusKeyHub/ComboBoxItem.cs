namespace pGina.Plugin.TopicusKeyHub
{
    public class ComboBoxItem
    {
        public ComboBoxItem(string text, string value)
        {
            this.Text = text;
            this.Value = value;
        }

        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
