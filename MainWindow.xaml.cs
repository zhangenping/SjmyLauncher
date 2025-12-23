using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using KFDBFinder.Extensions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Collections.Generic;

namespace SjmyLauncher
{
    public partial class MainWindow : Window
    {
        public enum ENCRYPTION_EVENT
        {
            ENCRYPTION_BEGIN,
            ENCRYPTION_END,
        };

        public MainWindow()
        {
            InitializeComponent();

            MainViewModel MainModel = new MainViewModel();
            this.DataContext = MainModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            foreach (var wirter in m_departmentErrWriters)
            {
                wirter.Value.Close();
            }

            m_departmentErrWriters.Clear();
        }


        //注册窗口监听消息
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 注册窗口
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private async void Grid_Drop(object sender, DragEventArgs e)//拖拽到界面进行加密
        {
            var fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            if (File.Exists(fileName))//如果是文件
            {
                SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildOneFdbFile(fileName);
            }
            else
            {
                await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllFdb());
                await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllToProtoc());
            }
        }

        // 定义WM_COPYDATA消息
        private const int WM_COPYDATA = 0x004A;

        // 拆分日志输出
        private Dictionary<MessageDepartment, String> m_departmentNames = new Dictionary<MessageDepartment, String>();
        private Dictionary<MessageDepartment, StreamWriter> m_departmentErrWriters = new Dictionary<MessageDepartment, StreamWriter>();

        private void AddDepartmentNames()
        {
            m_departmentNames.Add(MessageDepartment.Client, "client");
            m_departmentNames.Add(MessageDepartment.Resource, "resource");
            m_departmentNames.Add(MessageDepartment.Script, "script");
            m_departmentNames.Add(MessageDepartment.Engine, "engine");
        }

        private void AddDepartmentWriter(MessageDepartment department)
        {
            String departmentName = "other";

            if (m_departmentNames.Count == 0)
            {
                AddDepartmentNames();
            }

            if (m_departmentNames.ContainsKey(department))
            {
                departmentName = m_departmentNames[department];
            }

            DateTime dateTime = DateTime.Now;
            String filePath = $"./debug/{dateTime.Year}_{dateTime.Month}_{dateTime.Day}_{departmentName}_error.log";
            StreamWriter writer = new StreamWriter(filePath);
            writer.WriteLine("时间\t\t\t日志");
            m_departmentErrWriters.Add(department, writer);
        }

        private void WriteDepartmentErr(MessageDepartment department, String msg)
        {
            if(!m_departmentErrWriters.ContainsKey(department))
            {
                AddDepartmentWriter(department);
            }

            string strLog = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t\t{msg}";
            m_departmentErrWriters[department].WriteLine(strLog);
        }

        // 注册窗口过程
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                if (lParam != IntPtr.Zero)
                {
                    IntPtr originalDataPointer = lParam;

                    try
                    {
                        COPYDATASTRUCT copyData = Marshal.PtrToStructure<COPYDATASTRUCT>(originalDataPointer);

                        string pattern = @"\[([^\]]+)\]";

                        MatchCollection matches = Regex.Matches(copyData.lpData, pattern);

                        if (matches.Count >= 3)
                        {
                            MessageDepartment Department;
                            if (matches[2].Groups[1].Value == "script")
                            {
                                Department = MessageDepartment.Script;
                            }
                            else if (matches[2].Groups[1].Value == "resource")
                            {
                                Department = MessageDepartment.Resource;
                            }
                            else if (matches[2].Groups[1].Value == "engine")
                            {
                                Department = MessageDepartment.Engine;
                            }
                            else
                            {
                                Department = MessageDepartment.Client;  //上面没有的类型都是客户端问题
                            }

                            //去除标识，只显示日志部分内容
                            string strOrginal = copyData.lpData;
                            string strDelete = matches[0].Groups[0].Value + " " + matches[1].Groups[0].Value + " " + matches[2].Groups[0].Value;
                            string strShow = strOrginal.Replace(strDelete, "");

                            if (matches[0].Groups[1].Value == "ERROR" || matches[0].Groups[1].Value == "FATAL")
                            {
                                var message = new ErrorMessageEventArgs { Message = strShow, Department = Department };
                                Messenger.Default.Send(message);
                                WriteDepartmentErr(Department, strShow);
                            }
                            else if (matches[0].Groups[1].Value == "WARN")
                            {
                                var message = new WarnMessageEventArgs { Message = strShow, Department = Department };
                                Messenger.Default.Send(message);
                                WriteDepartmentErr(Department, strShow);
                            }
                            else
                            {
                                var message = new InfoMessageEventArgs { Message = strShow, Department = Department };
                                Messenger.Default.Send(message);
                            }
                        }
                        else
                        {
                            var message = new ErrorMessageEventArgs { Message = copyData.lpData, Department = MessageDepartment.Client };
                            Messenger.Default.Send(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        string strMsg = "启动器接受消息解析错误: " + ex.Message;
                        var message = new ErrorMessageEventArgs { Message = strMsg, Department = MessageDepartment.Client };
                        Messenger.Default.Send(message);

                        Console.WriteLine("Error during structure conversion: " + ex.Message);
                    }                    
                }                
            }

            return IntPtr.Zero;
        }

        // 用于COPYDATA消息的结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        private void UserButton_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }

}
