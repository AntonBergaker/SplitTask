﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SplitTask
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        private void buttonCustomServer_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ServersWindow window = new ServersWindow();
            window.Show();
        }

        private void buttonLocal_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
