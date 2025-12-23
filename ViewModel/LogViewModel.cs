using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using KFDBFinder.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;

namespace SjmyLauncher
{
    public class MyMessage
    {
        public enum cmd_type
        {
            EXPORT_LOG = 100,
        };
        public cmd_type _type { get; set; }
    }

    class LogViewModel : ViewModelBase
    {
        private bool m_bIsShowNormalMsg = true;
        public bool IsShowNormalMsg
        {
            get { return m_bIsShowNormalMsg; }
            set
            {
                m_bIsShowNormalMsg = value;
                ResetLogInfo();
                RaisePropertyChanged();
            }
        }

        private string m_SelectLog = string.Empty;
        public string SelectLog
        {
            get { return m_SelectLog; }
            set
            {
                m_SelectLog = value;
                RaisePropertyChanged();
            }
        }


        private bool m_bIsShowWarnlMsg = true;
        public bool IsShowWarnMsg
        {
            get { return m_bIsShowWarnlMsg; }
            set
            {
                m_bIsShowWarnlMsg = value;
                ResetLogInfo();
                RaisePropertyChanged();
            }
        }

        private bool m_bIsErrorMsg = true;
        public bool IsShowErrorMsg
        {
            get { return m_bIsErrorMsg; }
            set
            {
                m_bIsErrorMsg = value;
                ResetLogInfo();
                RaisePropertyChanged();
            }
        }

        private bool m_bMerge = true;
        public bool IsMerge
        {
            get { return m_bMerge; }
            set
            {
                m_bMerge = value;
                ResetLogInfo();
                RaisePropertyChanged();
            }
        }

        //显示部门
        private bool m_bIsShowClient = true;
        public bool IsShowClient
        {
            get { return m_bIsShowClient; }
            set
            {
                m_bIsShowClient = value;
                ResetLogInfo();
                RaisePropertyChanged();
            }
        }

        private int m_nLogInfoDepartmentMask = (int)MessageDepartment.Client | (int)MessageDepartment.Resource | (int)MessageDepartment.Script | (int)MessageDepartment.Engine;

        public int LogInfoDepartmentMask
        {
            get { return m_nLogInfoDepartmentMask; }
            set
            {
                m_nLogInfoDepartmentMask = value;
                ResetLogInfo();
            }
        }


        private ObservableCollection<RunResult> m_AllLogInfo;  //日志管理
        private ObservableCollection<RunResult> m_collectLogInfo;  //日志管理

        private string search = string.Empty;
        public string Search
        {
            get { return search; }
            set
            {
                search = value;
                ResetLogInfo();
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<RunResult> CollectLogInfo
        {
            get
            {
                if (null == m_collectLogInfo)
                {
                    m_collectLogInfo = new ObservableCollection<RunResult>();
                }

                return m_collectLogInfo;
            }

            set { m_collectLogInfo = value; RaisePropertyChanged(); }
        }

        private int nErrorMsgNumber = 0;  //
        private int nWarningMsgNumber = 0;//
        private int nInfoMsgNumber = 0;//

        private string m_strErrorMsg = "错误";
        public String ErrMsgNumber
        {
            get { return m_strErrorMsg; }
            set { m_strErrorMsg = value; RaisePropertyChanged(); }
        }
        private string m_strWarningMsg = "警告";
        public String WarningMsgNumber
        {
            get { return m_strWarningMsg; }
            set { m_strWarningMsg = value; RaisePropertyChanged(); }
        }

        private string m_strNormalMsg = "信息";
        public String NormalMsgNumber
        {
            get { return m_strNormalMsg; }
            set { m_strNormalMsg = value; RaisePropertyChanged(); }
        }

        public LogViewModel()
        {
            m_AllLogInfo = new ObservableCollection<RunResult>();

            Messenger.Default.Register<MessageEventArgs>(this, SetLog);
            Messenger.Default.Register<InfoMessageEventArgs>(this, SetLog);
            Messenger.Default.Register<DebugMessageEventArgs>(this, SetLog);
            Messenger.Default.Register<WarnMessageEventArgs>(this, SetLog);
            Messenger.Default.Register<ErrorMessageEventArgs>(this, SetLog);

            Messenger.Default.Register<UserEvent.UserEventType>(this, (eventType) =>
            {
                if (eventType == UserEvent.UserEventType.ClearLog)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        m_AllLogInfo.Clear();
                        m_collectLogInfo.Clear();
                        nErrorMsgNumber = 0;
                        nWarningMsgNumber = 0;
                        nInfoMsgNumber = 0;
                        SelectLog = string.Empty;

                        NormalMsgNumber = string.Concat("信息", " ", nInfoMsgNumber.ToString());
                        WarningMsgNumber = string.Concat("信息", " ", nWarningMsgNumber.ToString());
                        ErrMsgNumber = string.Concat("信息", " ", nErrorMsgNumber.ToString());
                    });
                }
            });

            Messenger.Default.Register<MyMessage>(this, (message) =>
            {
                if (message._type == MyMessage.cmd_type.EXPORT_LOG)
                {
                    string filePath = "log.txt";
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        foreach (var value in CollectLogInfo)
                        {
                            string strLog = $"时间[{value.m_StrTime}] 归属部门[{value.m_strDepartment}] 数量[{value.m_nResultNumber}] 日志内容[{value.m_StrRunResult}] ";
                            writer.WriteLine(strLog);
                        }
                    }

                    MessageBox.Show("日志已导出 " + filePath);
                }
            });
        }

        private void ResetLogInfo()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                CollectLogInfo.Clear();

                if (IsMerge)
                {
                    Dictionary<string, RunResult> mergedLogs = new Dictionary<string, RunResult>();

                    foreach (var item in m_AllLogInfo)
                    {
                        if (Filter(item))
                        {
                            if (mergedLogs.ContainsKey(item.m_StrRunResult))
                            {
                                mergedLogs[item.m_StrRunResult].StrResultNumber++;
                                mergedLogs[item.m_StrRunResult].StrTime = item.StrTime;
                            }
                            else
                            {
                                item.StrResultNumber = 1;
                                mergedLogs[item.m_StrRunResult] = item;
                            }
                        }
                    }

                    foreach (var log in mergedLogs.Values)
                    {
                        CollectLogInfo.Add(log);
                    }
                }
                else
                {
                    foreach (var item in m_AllLogInfo)
                    {
                        if(item.StrResultNumber!=1)
                        {
                            item.StrResultNumber = 1;
                        }
                        if (Filter(item))
                        {
                            CollectLogInfo.Add(item);
                        }
                    }
                }
            });
        }


        private bool Filter(RunResult value)
        {
            if (value.m_StrRunResult.Contains(search))
            {
                if (value.type == MessageType.Error)
                {
                    if (!m_bIsErrorMsg)
                    {
                        return false;
                    }
                }
                else if (value.type == MessageType.Warn)
                {
                    if (!m_bIsShowWarnlMsg)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!IsShowNormalMsg)
                    {
                        return false;
                    }
                }

                if ((LogInfoDepartmentMask & (int)(value).Department) == 0)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void SetLog(MessageEventArgs args)
        {
            if (args.Type != MessageType.None && args.Type != MessageType.Info && args.Department != MessageDepartment.Script)
            {
                var message = new UserEncryptionEvent(UserEncryptionEvent.EncrypEventType.FileEncryptionError, args.ErrorFile);
                Messenger.Default.Send(message);
            }
            DispatcherHelper.CheckBeginInvokeOnUI(() => 
            {
                RunResult sult = new RunResult();
                sult.StrTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sult.StrRunResult = args.Message;
                sult.type = args.Type;
                sult.Department = args.Department;

                if (args.Department == MessageDepartment.Engine)
                {
                    sult.strDepartment = "引擎";
                }
                else if (args.Department == MessageDepartment.Script)
                {
                    sult.strDepartment = "脚本";
                }
                else if (args.Department == MessageDepartment.Resource)
                {
                    sult.strDepartment = "资源";
                }
                else
                {
                    sult.strDepartment = "程序";
                }

                if (sult.type == MessageType.Error)
                {
                    nErrorMsgNumber++;
                    sult.StrTimeForeground = "red";
                    sult.StrRunResultForeground = "red";
                    sult.strErrorFile = args.ErrorFile;
                }
                else if (sult.type == MessageType.Warn)
                {
                    nWarningMsgNumber++;
                    sult.StrTimeForeground = "Orange";
                    sult.StrRunResultForeground = "Orange";
                    sult.strErrorFile = args.ErrorFile;
                }
                else
                {
                    nInfoMsgNumber++;
                }

                if (m_bMerge)
                {
                    bool bFind = false;
                    for (int i = 0; i < CollectLogInfo.Count; i++)
                    {
                        sult.StrResultNumber = 1;
                        if (CollectLogInfo[i].m_StrRunResult == sult.m_StrRunResult)
                        {
                            CollectLogInfo[i].StrResultNumber++;
                            CollectLogInfo[i].StrTime = sult.StrTime;
                            bFind = true;
                            break;
                        }
                    }

                    if (!bFind)
                    {
                        sult.StrResultNumber = 1;
                        if (Filter(sult))
                        {
                            CollectLogInfo.Add(sult);
                        }
                    }
                }
                else
                {
                   sult.StrResultNumber = 1;
                   if (Filter(sult))
                   {
                       CollectLogInfo.Add(sult);
                   }
                }
                m_AllLogInfo.Add(sult);

                NormalMsgNumber = string.Concat("信息：",  nInfoMsgNumber.ToString());
                WarningMsgNumber = string.Concat("警告：",  nWarningMsgNumber.ToString());
                ErrMsgNumber = string.Concat("错误：",  nErrorMsgNumber.ToString());
            });
        }
    }

    public class RunResult : INotifyPropertyChanged
    {
        public string m_StrTime { set; get; } = "";
        public string m_StrRunResult { set; get; } = "";
        public string m_StrTimeForeground { set; get; } = "black";
        public string m_StrRunResultForeground { set; get; } = "black";

        public string m_strDepartment { set; get; } = "";

        public MessageDepartment Department { set; get; }

        public string strErrorFile { set; get; } = "";

        public int m_nResultNumber;


        public string StrTime
        {
            get { return m_StrTime; }
            set { m_StrTime = value; OnPropertyChanged("StrTime"); }
        }

        public string StrRunResult
        {
            get { return m_StrRunResult; }
            set { m_StrRunResult = value; OnPropertyChanged("StrRunResult"); }
        }

        public string StrTimeForeground
        {
            get { return m_StrTimeForeground; }
            set { m_StrTimeForeground = value; OnPropertyChanged("StrTimeForeground"); }
        }

        public string StrRunResultForeground
        {
            get { return m_StrRunResultForeground; }
            set { m_StrRunResultForeground = value; OnPropertyChanged("StrRunResultForeground"); }
        }

        public string strDepartment
        {
            get { return m_strDepartment; }
            set { m_strDepartment = value; OnPropertyChanged("strDepartment"); }
        }

        public int StrResultNumber
        {
            get { return m_nResultNumber; }
            set { m_nResultNumber = value; OnPropertyChanged("StrResultNumber"); }
        }


        public MessageType type { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    };

    public class MsgStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string message && !string.IsNullOrEmpty(message))
            {
                // 检查字符串是否以"错误："或"警告："开头
                bool startsWithKeyword = message.StartsWith("错误：") || message.StartsWith("警告：");
                if (startsWithKeyword)
                {
                    // 尝试移除前缀后解析剩余部分作为整数
                    int errorCode;
                    bool isValidNumber = int.TryParse(message.Substring(message.IndexOf('：') + 1), out errorCode);
                    if (isValidNumber && errorCode > 0)
                    {
                        return Visibility.Visible;
                    }
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
