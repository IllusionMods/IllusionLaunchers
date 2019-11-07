using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace InitDialog
{
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            this.InitializeComponent();
        }

        public void SetupWindow(string title, string format, params object[] args)
        {
            base.Title = title;
            string text = format;
            for (int i = 0; i < args.Length; i++)
            {
                text = text.Replace("{" + i + "}", args[i].ToString());
            }
            this.textMsg.Text = text;
            base.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            base.Close();
        }
    }
}