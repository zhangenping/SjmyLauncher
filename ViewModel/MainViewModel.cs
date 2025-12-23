using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using KFDBFinder.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;

namespace SjmyLauncher
{
    public class MainViewModel : ViewModelBase
    {
        private int _progress;
        private static bool _bNeedSaveFileHashs;
        private static Dictionary<string, string> _fileHashs = new Dictionary<string, string>();
        private string HASH_FILE_PATH = "debug/filehashs.log";
        private DispatcherTimer _timer;
        public int ProgressCount
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    RaisePropertyChanged(nameof(ProgressCount));
                }
            }
        }

        private bool _isProgressVisible = true;

        public bool IsProgressVisible
        {
            get { return _isProgressVisible; }
            set
            {
                if (_isProgressVisible != value)
                {
                    _isProgressVisible = value;
                    RaisePropertyChanged(() => IsProgressVisible);
                }
            }
        }

        public MainViewModel()
        {
            Messenger.Default.Register<UserEncryptionEvent>(this, message =>
            {
                if (message._eType == UserEncryptionEvent.EncrypEventType.FileEncryptionError)
                {
                    if (message._strData == "")
                    {
                        _fileHashs.Clear();
                    }
                    else
                    {
                        _fileHashs[message._strData] = "0";
                    }
                    _bNeedSaveFileHashs = true;
                }
            });

            DispatcherHelper.Initialize();
            LoadData();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsProgressVisible)
            {
                Messenger.Default.Send(UserEvent.UserEventType.BeginEncryption);
                SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildCsvEncryptionError();
                Messenger.Default.Send(UserEvent.UserEventType.EndEncryption);
            }
        }

        private async void LoadData()
        {
            Messenger.Default.Send(UserEvent.UserEventType.BeginEncryption);
            IsProgressVisible = true;

            LoadFileHashs();

            MessageEventArgs msgBegin = new InfoMessageEventArgs { Message = "===开始加密全客户端配置文件===", Department = MessageDepartment.Client };
            Messenger.Default.Send(msgBegin);

            ProgressCount = 0;
            await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().Init());

            var progressFdb = new Progress<int>(value => ProgressCount = value / 3);
            await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllFdb(progressFdb));

            var progressProto = new Progress<int>(value => ProgressCount = 33 + value / 3);
            SimpleIoc.Default.GetInstance<FdbEncryption>().SetCSVWatcherStatus(false);
            await Task.Run(() => SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllToProtoc(progressProto));
            SimpleIoc.Default.GetInstance<FdbEncryption>().SetCSVWatcherStatus(true);

            var progressIni = new Progress<int>(value => ProgressCount = 66 + value / 3);
            await Task.Run(() => SimpleIoc.Default.GetInstance<DataEncryption>().RebuildAllIni(progressIni));

            await Task.Run(() => SimpleIoc.Default.GetInstance<DataEncryption>().AutoMergeConfig("ui", "ui.pack"));
            await Task.Run(() => SimpleIoc.Default.GetInstance<DataEncryption>().AutoMergeConfig("ini\\client\\common\\skill", "skill.pack"));

            ProgressCount = 100;
            IsProgressVisible = false;

            if (_bNeedSaveFileHashs)
            {
                SaveFileHashs();
            }

            MessageEventArgs msgEnd = new InfoMessageEventArgs { Message = "===完成全客户端配置文件加密===", Department = MessageDepartment.Client };
            Messenger.Default.Send(msgEnd);

            Messenger.Default.Send(UserEvent.UserEventType.EndEncryption);
        }

        public static bool HasFileChanged(string strFilePath, string strFileHash)
        {
            string strFileHashHistroy;
            if (_fileHashs.TryGetValue(strFilePath, out strFileHashHistroy))
            {
                if (strFileHash == strFileHashHistroy)
                {
                    return false;
                }
            }
            _fileHashs[strFilePath] = strFileHash;
            _bNeedSaveFileHashs = true;
            return true;
        }

        private void LoadFileHashs()
        {
            if (!Directory.Exists("debug"))
            {
                Directory.CreateDirectory("debug");
            }
            if (!File.Exists(HASH_FILE_PATH))
                return;

            foreach (string line in File.ReadAllLines(HASH_FILE_PATH))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) // 跳过空行和注释
                    continue;

                int index = line.IndexOf('=');
                if (index > 0)
                {
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();
                    _fileHashs[key] = value;
                }
            }
        }

        private void SaveFileHashs()
        {
            List<string> _originalLines = new List<string>();
            foreach (var data in _fileHashs)
            {
                _originalLines.Add($"{data.Key}={data.Value}");
            }
            try
            {
                File.WriteAllLines(HASH_FILE_PATH, _originalLines);
            }
            catch (Exception ex)
            {
                MessageEventArgs msgEnd = new InfoMessageEventArgs { Message = "文件Hash值更新失败!" + ex.ToString(), Department = MessageDepartment.Client };
                Messenger.Default.Send(msgEnd);
            }
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
 
}