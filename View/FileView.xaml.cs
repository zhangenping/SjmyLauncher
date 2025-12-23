using System.Diagnostics;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;
using GalaSoft.MvvmLight.Messaging;

namespace SjmyLauncher.View
{
    /// <summary>
    /// FileView.xaml 的交互逻辑
    /// </summary>
    public partial class FileView : UserControl
    {
        private readonly FileViewModel _model;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        public FileView()
        {
            InitializeComponent();

            _model = new FileViewModel();
            this.DataContext = _model;

            ListViewFile.PreviewMouseRightButtonDown += FileListViewRightClick;

            Messenger.Default.Register<UserEncryptionEvent>(this, message =>
            {
                if (message._eType == UserEncryptionEvent.EncrypEventType.RebuildOneCsvFinish)
                {
                    SendReloadMessage(message._strData);
                }
            });
        }

        private void FileListViewRightClick(object sender, MouseButtonEventArgs e)
        {
            if (_model.SelectedFileDisplayType == FileDisplayType.CSV)
            {
                var contextMenu = new ContextMenu();
                var menuItem = new MenuItem { Header = "热重载CSV数据" };
                menuItem.Click += MenuItemReloadCSVClick;
                contextMenu.Items.Add(menuItem);
                contextMenu.PlacementTarget = sender as UIElement;
                contextMenu.IsOpen = true;
                e.Handled = true; 
            }
        }

        private void FileListViewClick(object sender, MouseButtonEventArgs e)
        {
            FdbFileInfo data = ListViewFile.SelectedItem as FdbFileInfo;
            if (null == data)
                return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    //打开 .txt 文件
                    if (File.Exists(data.strFileOpenPath))
                    {
                        OpenFile(data.strFileOpenPath);
                    }
                    break;
                // case MouseButton.Right:
                //     SendReloadMessage(data.StrFileName);
                //     break;
            }
        }

        private void MenuItemReloadCSVClick(object sender, RoutedEventArgs e)
        {
            FdbFileInfo data = ListViewFile.SelectedItem as FdbFileInfo;

            if (null == data)
                return;

            SendReloadMessage(data.StrFileName);         
        }

        private void OpenFile(string strFileName)
        {
            if (!File.Exists(strFileName))
            {
                MessageBox.Show("文件不存在! 文件名: " + strFileName);
                return;
            }

            Process.Start(strFileName);
        }

        private void SendReloadMessage(string strFileName)
        {
            if (Path.GetExtension(strFileName) != ".csv")
                return;

            const int WM_COPYDATA = 0x004A;
            const int PM_FLAG = 100;

            StarSjMyModel.listLinkWnd.ForEach((IntPtr hWnd) =>
            {
                string message = "/ReloadDataTable " + Path.GetFileNameWithoutExtension(strFileName);

                COPYDATASTRUCT cds;
                cds.dwData = new IntPtr(PM_FLAG);
                cds.cbData = (uint)(message.Length + 1) * 2;
                cds.lpData = Marshal.StringToHGlobalAnsi(message);

                SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            });
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FileViewModel viewModel)
            {
                viewModel.ChangeShowFileList?.Execute(viewModel.SelectedFileDisplayType);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public uint cbData;
            public IntPtr lpData;
        }
    }
}
