using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Permadelete.Views
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : UserControl
    {
        public Notification()
        {
            InitializeComponent();
        }

        private void BeginHideStoryboard()
        {
            var sb = FindResource("hide") as Storyboard;
            sb.Begin();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var notification = (DataContext as IExpireable);
            if (notification != null)
                notification.Expired += (s, args) => BeginHideStoryboard();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            BeginHideStoryboard();
        }
    }
}
