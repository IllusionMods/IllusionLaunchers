using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InitSetting
{
    /// <summary>
    /// Interaction logic for advMode.xaml
    /// </summary>
    public partial class advMode : Window
    {
        public advMode()
        {
            InitializeComponent();
        }

        public void Start(params object[] args)
        {
            ShowDialog();
        }
    }
}
