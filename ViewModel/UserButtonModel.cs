using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using KFDBFinder.Extensions;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SjmyLauncher
{
    public class UserEncryptionEvent
    {
        public enum EncrypEventType
        {
            InitFdbFinish = 1,//加密ini完成
            WatchFileChanged = 2,//监听到文件发生变化
            FileEncryptionError = 3, //监听文件加密出现的异常，警告以上
            RebuildOneCsvFinish = 4, //监听Csv变化，并且重新加密成功
        };
        public EncrypEventType _eType { get; set; }
        public string _strData { get; set; } = "";
        public UserEncryptionEvent(EncrypEventType eType, string strData = "")
        {
            _eType = eType;
            _strData = strData;
        }
    };

    public class UserEvent
    {
        public enum UserEventType
        {
            ClearLog = 1, //清除日志
            BeginEncryption = 2, //开始加密
            EndEncryption = 3, //结束加密
        };

        public UserEvent _eType { get; set; }

        public UserEvent(UserEvent eType)
        {
            _eType = eType;
        }
    };

    class UserButtonModel : ViewModelBase
    {
        #region Command
        public RelayCommand cmd_RebuildIni { set; get; }
        public RelayCommand cmd_RebuildDataTable { set; get; }  //生成PB 文件
        public RelayCommand cmd_StarKFEBEditor { set; get; }

        public RelayCommand cmd_RebuildAllFdbToTxt { set; get; }
        public RelayCommand cmd_ExportLog { set; get; }
        public RelayCommand cmd_RebuildOneIni { set; get; }
        public RelayCommand cmd_RebuildOneFileFdb { set; get; }
        public RelayCommand cmd_RebuildFdb { set; get; }

        public RelayCommand cmd_TxtToCsv { set; get; }
        public RelayCommand cmd_ClearLog { set; get; }
        public RelayCommand cmd_ExportDepartmentErr { set; get; }
        #endregion
        private bool m_isClickBtn;
        public bool IsClickBtn
        {
            get { return m_isClickBtn; }
            set
            {
                if (m_isClickBtn != value)
                {
                    m_isClickBtn = value;
                    RaisePropertyChanged();
                }
            }
        }
        public UserButtonModel()
        {            
            Messenger.Default.Register<UserEvent.UserEventType>(this, (eventType) =>
            {
                if (eventType == UserEvent.UserEventType.BeginEncryption)
                {
                    SetButtonClick(false);
                }
                else if (eventType == UserEvent.UserEventType.EndEncryption)
                {
                    SetButtonClick(true);
                }
            });          

            InitCmd();
        }

        private void InitCmd()
        {
            cmd_RebuildFdb = new RelayCommand(RebuildFdb);
            cmd_RebuildIni = new RelayCommand(RebuildIni);
            cmd_RebuildDataTable = new RelayCommand(RebuildDataTable);

            cmd_RebuildAllFdbToTxt = new RelayCommand(RebuildAllFdbToTxt);

            cmd_StarKFEBEditor = new RelayCommand(StarKFEBEditor);
            cmd_ExportLog = new RelayCommand(ExportLog);
            cmd_RebuildOneIni = new RelayCommand(RebuildOneIni);
            cmd_RebuildOneFileFdb = new RelayCommand(RebuildOneFileFdb);
            
            cmd_TxtToCsv = new RelayCommand(TransferTxtToCsv);

            cmd_ClearLog = new RelayCommand(() =>
            {
                Messenger.Default.Send(UserEvent.UserEventType.ClearLog);
            });
        }

        private void StarKFEBEditor()
        {
            string strExeDir = System.IO.Directory.GetCurrentDirectory() + "\\KFDBEditor(WPF).exe";

            if (File.Exists(strExeDir) == false)
            {
                MessageEventArgs msg = new ErrorMessageEventArgs { Message = "找不到指定程序 ： " + strExeDir, Department = MessageDepartment.Client };
                Messenger.Default.Send(msg);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = strExeDir,
            };

            // 启动外部程序
            Process.Start(startInfo);
        }

        private void RebuildAllFdbToTxt()
        {
            Messenger.Default.Send(UserEvent.UserEventType.BeginEncryption);

            SimpleIoc.Default.GetInstance<FdbEncryption>().SetFdbWatcherStatus(false);
            SimpleIoc.Default.GetInstance<FdbEncryption>().CheckTxtFile();
            SimpleIoc.Default.GetInstance<FdbEncryption>().SetFdbWatcherStatus(true);

            Messenger.Default.Send(UserEvent.UserEventType.EndEncryption);
        }

        private void ExportLog()
        {
            MyMessage msg = new MyMessage();
            msg._type = MyMessage.cmd_type.EXPORT_LOG;
            Messenger.Default.Send<MyMessage, LogViewModel>(msg);
        }

        private void RebuildOneIni()
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            SimpleIoc.Default.GetInstance<DataEncryption>().RebuildOneIniFile(dialog.FileName);
        }

        private void RebuildOneFileFdb()
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            SimpleIoc.Default.GetInstance<FdbEncryption>().SetFdbWatcherStatus(false);
            SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildOneFdbFile(dialog.FileName);
            SimpleIoc.Default.GetInstance<FdbEncryption>().SetFdbWatcherStatus(true);
        }

        private void TransferTxtToCsv()
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            SimpleIoc.Default.GetInstance<FdbEncryption>().TransferTxtToCsv(dialog.FileName);
        }

        private async void RebuildFdb()
        {
            Messenger.Default.Send(UserEvent.UserEventType.BeginEncryption);

            SimpleIoc.Default.GetInstance<FdbEncryption>().IsIncEncryption = false;
            await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllFdb());
            SimpleIoc.Default.GetInstance<FdbEncryption>().SetCSVWatcherStatus(false);
            await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllToProtoc());
            SimpleIoc.Default.GetInstance<FdbEncryption>().SetCSVWatcherStatus(true);

            Messenger.Default.Send(UserEvent.UserEventType.EndEncryption);
        }

        private async void RebuildIni()
        {
            Messenger.Default.Send(UserEvent.UserEventType.BeginEncryption);

            SimpleIoc.Default.GetInstance<DataEncryption>().IsIncEncryption = false;
            await Task.Run(() => SimpleIoc.Default.GetInstance<DataEncryption>().RebuildAllIni());

            Messenger.Default.Send(UserEvent.UserEventType.EndEncryption);
        }

        private async void RebuildDataTable()
        {
            Messenger.Default.Send(UserEvent.UserEventType.BeginEncryption);

            SimpleIoc.Default.GetInstance<FdbEncryption>().SetCSVWatcherStatus(false);
            SimpleIoc.Default.GetInstance<FdbEncryption>().IsIncEncryption = false;
            await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllToProtoc());
            SimpleIoc.Default.GetInstance<FdbEncryption>().SetCSVWatcherStatus(true);

            Messenger.Default.Send(UserEvent.UserEventType.EndEncryption);
        }

        private void SetButtonClick(bool bClick)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                IsClickBtn = bClick;
            });
        }
    }
}
