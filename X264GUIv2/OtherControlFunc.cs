namespace X264GUIv2
{
    internal class OtherControlFunc(Form1 form)
    {
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        /// <param name="MessageText"></param>
        public static void ShowError(string MessageText) => MessageBox.Show(MessageText, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
