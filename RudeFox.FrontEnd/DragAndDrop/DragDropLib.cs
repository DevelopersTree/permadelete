/*
 * Drag and Drop library
 * By: Garry Trinder
 * Link: https://blogs.msdn.microsoft.com/adamroot/2008/02/19/shell-style-drag-and-drop-in-net-wpf-and-winforms/
 */

namespace DragDropLib
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    #region Native structures

    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Point
    {
        public int x;
        public int y;
    }

    #endregion // Native structures

    #region IDropTargetHelper

    [ComVisible(true)]
    [ComImport]
    [Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDropTargetHelper
    {
        void DragEnter(
            [In] IntPtr hwndTarget,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
            [In] ref Win32Point pt,
            [In] int effect);

        void DragLeave();

        void DragOver(
            [In] ref Win32Point pt,
            [In] int effect);

        void Drop(
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
            [In] ref Win32Point pt,
            [In] int effect);

        void Show(
            [In] bool show);
    }

    #endregion // IDropTargetHelper

    #region DragDropHelper

    [ComImport]
    [Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
    public class DragDropHelper { }

    #endregion // DragDropHelper
}