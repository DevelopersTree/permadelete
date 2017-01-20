using DragDropLib;
using RudeFox.Controls;
using RudeFox.ViewModels;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace RudeFox.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FlatWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (lstOperations.SelectedItem == null)
                (DataContext as MainWindowVM).DeleteFilesCommand.Execute(this);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Width < 650 || Height < 300)
                sidebarGrid.Visibility = Visibility.Collapsed;
            else
                sidebarGrid.Visibility = Visibility.Visible;
        }

        private void lstOperations_DragOver(object sender, DragEventArgs e)
        {
            var point = e.GetPosition(lstOperations);
            var windowsPoint = new Win32Point { x = (int)point.X, y = (int)point.Y };

            var windowHelper = new WindowInteropHelper(this);
            var dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragOver(ref windowsPoint, (int)e.Effects);
        }

        private void lstOperations_DragEnter(object sender, DragEventArgs e)
        {
            var point = e.GetPosition(lstOperations);
            var windowsPoint = new Win32Point { x = (int)point.X, y = (int)point.Y };

            var windowHelper = new WindowInteropHelper(this);
            var dropHelper = (IDropTargetHelper)new DragDropHelper();

            e.Effects = DragDropEffects.Move;
            dropHelper.DragEnter(windowHelper.Handle, (ComIDataObject)e.Data, ref windowsPoint, (int)e.Effects);
        }

        private void lstOperations_DragLeave(object sender, DragEventArgs e)
        {
            var dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragLeave();
        }

        private void lstOperations_Drop(object sender, DragEventArgs e)
        {
            var point = e.GetPosition(lstOperations);
            var windowsPoint = new Win32Point { x = (int)point.X, y = (int)point.Y };

            var windowHelper = new WindowInteropHelper(this);
            var dropHelper = (IDropTargetHelper)new DragDropHelper();

            e.Effects = DragDropEffects.Move;
            dropHelper.Drop((ComIDataObject)e.Data, ref windowsPoint, (int)e.Effects);
            (DataContext as MainWindowVM).HandleFileDropCommand.Execute(e.Data);
        }
    }
}
