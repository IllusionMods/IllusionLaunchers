using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Input;

namespace InitSetting
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        public Update()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();

            base.ShowDialog();
        }

        public void SetupWindow(string title, string format, params object[] args)
        {
            base.Title = title;
            string text = format;
            base.ShowDialog();
        }
    }
}
