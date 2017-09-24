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

namespace HtmlRenderer.DomParseTester.DomComparing
{
    /// <summary>
    /// Interaction logic for DomComparer.xaml
    /// </summary>
    public partial class DomComparer : Window
    {
        public DomComparer()
        {
            this.DataContext = new MainViewModel();
            this.InitializeComponent();
        }

        private void OpenRecentLeft_Click(object sender, RoutedEventArgs e)
        {
            RecentItem item = (e.OriginalSource as MenuItem)?.DataContext as RecentItem;
            ((MainViewModel)this.DataContext)?.LeftOpenRecent(item);
        }

        private void OpenRecentRight_Click(object sender, RoutedEventArgs e)
        {
            RecentItem item = (e.OriginalSource as MenuItem)?.DataContext as RecentItem;
            ((MainViewModel)this.DataContext)?.RightOpenRecent(item);
        }
    }
}