using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using KFDBFinder.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

namespace SjmyLauncher
{
    class StarSjMyModel : ViewModelBase
    {
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        //app 启动目录
        private string appPath = string.Empty;

        public static List<IntPtr> listLinkWnd = new List<IntPtr>();

        public string AppPath
        {
            get { return appPath; }
            set { appPath = value; RaisePropertyChanged(); }
        }

        //是否关闭微端
        private bool isCloseWD = true;
        public bool IsCloseWD
        {
            get { return isCloseWD; }
            set { isCloseWD = value; RaisePropertyChanged(); }
        }

        //是否SDK登录
        private bool isSdk = false;
        public bool IsSdk
        {
            get { return isSdk; }
            set { isSdk = value; RaisePropertyChanged(); }
        }

        //app 启动参数用dx 或者 openGl
        private string m_renderModeCommand = "-dx11";  //开启模式 默认dx11

        //声明命令 按钮所有操作直接绑定命令
        #region Command
        public RelayCommand cmd_StarSoulApp { get; set; } //启动手机魔域命令
        public RelayCommand<string> cmd_ModifyStartAppParam { set; get; }//修改启动手机魔域使用 gl 还是 dx

        private RelayCommand _OpenFAQ { get; set; }
        public RelayCommand OpenFAQ
        {
            get
            {
                if (_OpenFAQ == null)
                {
                    _OpenFAQ = new RelayCommand(() =>
                    {
                        string url = "https://docs.qq.com/doc/DTWtBTk16UWhEYUNE";
                        Process.Start(new ProcessStartInfo(url));
                    });
                }
                return _OpenFAQ;
            }
        }
        #endregion

        public StarSjMyModel()
        {
            AppPath = "路径：" + Directory.GetCurrentDirectory();

            InitCommand();
        }

        private void InitCommand()
        {
            cmd_StarSoulApp = new RelayCommand(StarSoulApp);
            cmd_ModifyStartAppParam = new RelayCommand<string>(ModifyStartAppParam);
        }

        private void ModifyStartAppParam(string strParma)
        {
            m_renderModeCommand = strParma;
        }

        private IntPtr FindWindowByProcessId(int processId)
        {
            IntPtr foundHwnd = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                if (pid == processId)
                {
                    foundHwnd = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            return foundHwnd;

        }

        public void StarSoulApp()
        {
            string strExeDir = "";
#if DEBUG
            strExeDir = System.IO.Directory.GetCurrentDirectory() + "\\Soul_d.exe";
#else
            strExeDir = System.IO.Directory.GetCurrentDirectory() + "\\Soul.exe";
#endif


            if (File.Exists(strExeDir) == false)
            {
                MessageEventArgs msg = new ErrorMessageEventArgs { Message = "找不到指定程序 ： " + strExeDir, Department = MessageDepartment.Client };
                Messenger.Default.Send(msg);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = strExeDir,
                Arguments = (IsCloseWD ? "CloseWD " : "") + m_renderModeCommand + (IsSdk ? " UseNdSdk" : "") + " IsLauncher",
            };

            try
            {
                // 启动外部程序
                Process process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForInputIdle();

                    // 游戏窗口
                    IntPtr hWnd = FindWindowByProcessId(process.Id);
                    if (hWnd != IntPtr.Zero)
                        listLinkWnd.Add(hWnd);
                }
            }
            catch (Exception ex)
            {
                MessageEventArgs msg = new ErrorMessageEventArgs { Message = "手机魔域启动失败 :" + ex, Department = MessageDepartment.Client };
                Messenger.Default.Send(msg);
            }
        }

    }
}
