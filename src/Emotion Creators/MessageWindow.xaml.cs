using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace InitDialog
{
	// Token: 0x0200000B RID: 11
	public partial class MessageWindow : Window
	{
		// Token: 0x06000040 RID: 64 RVA: 0x00003A00 File Offset: 0x00001C00
		public MessageWindow()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00003A10 File Offset: 0x00001C10
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

		// Token: 0x06000042 RID: 66 RVA: 0x00003A6B File Offset: 0x00001C6B
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			base.Close();
		}
	}
}
