using System.ComponentModel;

namespace X264GUIv2
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string appendText
        {
            set
            {
                if (Visible)
                    LogBox.AppendText(value + "\r\n");
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public void clear()
        {
            LogBox.Clear();
        }
    }
}
