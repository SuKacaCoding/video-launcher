using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GongSolutions.Shell.Interop;

namespace GongSolutions.Shell
{
    class SystemImageList
    {
        public static void DrawSmallImage(Graphics g, Point point,
                                          int imageIndex, bool selected)
        {
            uint flags = (uint)(imageIndex >> 16);
            IntPtr hdc = g.GetHdc();

            try
            {
                if (selected) flags |= (int)ILD.BLEND50;
                ComCtl32.ImageList_Draw(SmallImageList, imageIndex & 0xffff,
                    hdc, point.X, point.Y, flags);
            }
            finally
            {
                g.ReleaseHdc();
            }
        }

        public static void UseSystemImageList(ListView control)
        {
            IntPtr large, small;
            int x, y;

            if (control.LargeImageList == null)
            {
                control.LargeImageList = new ImageList();
            }

            if (control.SmallImageList == null)
            {
                control.SmallImageList = new ImageList();
            }

            Shell32.FileIconInit(true);
            if (!Shell32.Shell_GetImageLists(out large, out small))
            {
                throw new Exception("Failed to get system image list");
            }

            ComCtl32.ImageList_GetIconSize(large, out x, out y);
            control.LargeImageList.ImageSize = new Size(x, y);
            ComCtl32.ImageList_GetIconSize(small, out x, out y);
            control.SmallImageList.ImageSize = new Size(x, y);

            User32.SendMessage(control.Handle, MSG.LVM_SETIMAGELIST,
                (int)LVSIL.LVSIL_NORMAL, LargeImageList);
            User32.SendMessage(control.Handle, MSG.LVM_SETIMAGELIST,
                (int)LVSIL.LVSIL_SMALL, SmallImageList);
        }

        public static void UseSystemImageList(TreeView control)
        {
            User32.SendMessage(control.Handle, MSG.TVM_SETIMAGELIST,
                0, SmallImageList);
        }

        static void InitializeImageLists()
        {
            Shell32.FileIconInit(true);
            if (!Shell32.Shell_GetImageLists(out m_LargeImageList,
                    out m_SmallImageList))
            {
                throw new Exception("Failed to get system image list");
            }
        }

        static IntPtr SmallImageList
        {
            get
            {
                if (m_SmallImageList == IntPtr.Zero)
                {
                    InitializeImageLists();
                }
                return m_SmallImageList;
            }
        }

        static IntPtr LargeImageList
        {
            get
            {
                if (m_LargeImageList == IntPtr.Zero)
                {
                    InitializeImageLists();
                }
                return m_LargeImageList;
            }
        }

        static IntPtr m_SmallImageList;
        static IntPtr m_LargeImageList;
    }
}
