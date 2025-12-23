using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;

namespace SjmyLauncher
{
    public enum FileDisplayType
    {
        FDB,
        INI,
        CSV,
    }
    class FileViewModel : ViewModelBase
    {
        private ObservableCollection<FdbFileInfo> m_collectAllInfo;
        private ObservableCollection<FdbFileInfo> m_collectFileInfo;

        // ini 文件的文件数据 
        private List<FdbFileInfo> m_listIniFile = new List<FdbFileInfo>();
        // csv 文件的文件数据
        private List<FdbFileInfo> m_listCSVFile = new List<FdbFileInfo>();

        private bool m_bIsDoAllFileEncryption = false;

        public ObservableCollection<FdbFileInfo> CollectFileInfo
        {
            get { return m_collectFileInfo; }
            set { m_collectFileInfo = value; RaisePropertyChanged(); }
        }

        public List<FileDisplayType> FileDisplayTypes { get; }
        private FileDisplayType m_eFileDisplayType = FileDisplayType.FDB;
        public FileDisplayType SelectedFileDisplayType
        {
            get { return m_eFileDisplayType; }
            set
            {
                m_eFileDisplayType = value;
                RaisePropertyChanged(nameof(SelectedFileDisplayType));
            }
        }

        private string search = string.Empty;
        public string Search
        {
            get { return search; }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    search = value;
                    m_collectFileInfo.Clear();

                    foreach (var item in m_collectAllInfo)
                    {
                        if (item.m_StrFileName.Contains(search)) // 根据某个属性进行过滤
                        {
                            m_collectFileInfo.Add(item);
                        }
                    }
                });
                RaisePropertyChanged();
            }
        }


        public RelayCommand<TextChangedEventArgs> TextChangedCommand { get; private set; }

        public RelayCommand<FileDisplayType> ChangeShowFileList { get; private set; }

        private void OnTextChanged(TextChangedEventArgs e)
        {

        }

        public FileViewModel()
        {
            m_collectFileInfo = new ObservableCollection<FdbFileInfo>();
            m_collectAllInfo = new ObservableCollection<FdbFileInfo>();

            TextChangedCommand = new RelayCommand<TextChangedEventArgs>(OnTextChanged);

            FileDisplayTypes = Enum.GetValues(typeof(FileDisplayType)).Cast<FileDisplayType>().ToList();

            ChangeShowFileList = new RelayCommand<FileDisplayType>((var) => 
            {
                SelectedFileDisplayType = var;

                FlushFdbFileListView();
            });

            Messenger.Default.Register<UserEncryptionEvent>(this, message =>
            {
                if (message._eType == UserEncryptionEvent.EncrypEventType.InitFdbFinish)
                {
                    FlushFdbFileListView();
                }
                else if (message._eType == UserEncryptionEvent.EncrypEventType.WatchFileChanged)
                {
                    if (!m_bIsDoAllFileEncryption)
                    {
                        m_listIniFile.Clear();
                        FlushFdbFileListView();
                    }     
                }
            });

            Messenger.Default.Register<UserEvent.UserEventType>(this, (eventType) =>
            {
                if (eventType == UserEvent.UserEventType.BeginEncryption)
                {
                    m_bIsDoAllFileEncryption = true;
                }
                else if (eventType == UserEvent.UserEventType.EndEncryption)
                {
                    m_bIsDoAllFileEncryption = false;
                }
            });
        }

        private void FlushFdbFileListView()//刷新文件
        {
            m_collectAllInfo.Clear();
            switch(SelectedFileDisplayType)
            {
                case FileDisplayType.FDB:
                    List<FdbFileInfo> TempData = SimpleIoc.Default.GetInstance<FdbEncryption>().GetFdbListInfo();

                    for (int i = 0; i < TempData.Count; i++)
                    {
                        if (File.Exists(TempData[i].strFileOpenPath))
                        {
                            TempData[i].strFileLastWrite = File.GetLastWriteTime(TempData[i].strFileOpenPath).ToString();
                            m_collectAllInfo.Add(TempData[i]);
                        }
                    }
                    break;
                case FileDisplayType.INI:
                    string[] extends = new string[] { "*.ini ", "*.xml"};
                    if (m_listIniFile.Count == 0)
                    {
                        Stack<string> directories = new Stack<string>();
                        directories.Push(SimpleIoc.Default.GetInstance<DataEncryption>().GetFileSourceDir());
                        directories.Push(SimpleIoc.Default.GetInstance<FdbEncryption>().GetDataTableSourceDir());

                        //遍历文件夹-->
                        while (directories.Count > 0)
                        {
                            string currentDir = directories.Pop();

                            foreach (var ex in extends)
                            {
                                foreach (var file in Directory.GetFiles(currentDir, ex))
                                {
                                    var iniInfo = new FdbFileInfo()
                                    {
                                        m_StrFileName = System.IO.Path.GetFileName(file),
                                        strFileOpenPath = file,
                                        strFileLastWrite = File.GetLastWriteTime(file).ToString()
                                    };
                                    m_listIniFile.Add(iniInfo);
                                }
                            }

                            foreach (var dir in Directory.GetDirectories(currentDir))
                            {
                                directories.Push(dir);
                            }
                        }
                    }

                    for (int i = 0; i < m_listIniFile.Count; i++)
                    {
                        if (File.Exists(m_listIniFile[i].strFileOpenPath))
                        {
                            m_collectAllInfo.Add(m_listIniFile[i]);
                        }
                    }                   
                    break;
                case FileDisplayType.CSV:
                    string[] csvExtends = new string[] { "*.csv" };
                    if (m_listCSVFile.Count == 0)
                    {
                        Stack<string> directories = new Stack<string>();
                        directories.Push(SimpleIoc.Default.GetInstance<DataEncryption>().GetFileSourceDir());
                        directories.Push(SimpleIoc.Default.GetInstance<FdbEncryption>().GetDataTableSourceDir());

                        //遍历文件夹-->
                        while (directories.Count > 0)
                        {
                            string currentDir = directories.Pop();
                            foreach (var ex in csvExtends)
                            {
                                foreach (var file in Directory.GetFiles(currentDir, ex))
                                {
                                    var csvInfo = new FdbFileInfo()
                                    {
                                        m_StrFileName = System.IO.Path.GetFileName(file),
                                        strFileOpenPath = file,
                                        strFileLastWrite = File.GetLastWriteTime(file).ToString()
                                    };
                                    m_listCSVFile.Add(csvInfo);
                                }
                            }
                            foreach (var dir in Directory.GetDirectories(currentDir))
                            {
                                directories.Push(dir);
                            }
                        }
                    }
                    for (int i = 0; i < m_listCSVFile.Count; i++)
                    {
                        if (File.Exists(m_listCSVFile[i].strFileOpenPath))
                        {
                            m_collectAllInfo.Add(m_listCSVFile[i]);
                        }
                    }                   
                    break;
                default:
                    break;
            }

            Search = search;
        }
    }

    public class FdbFileInfo : INotifyPropertyChanged
    {
        public string m_StrFileName { set; get; } = "";//文件名
        public string m_strFileDesc { set; get; } = "";//文件描述
        public string m_strFileLastWrite { set; get; } = "";//文件最后修改时间

        public string strFileOpenPath { set; get; } = "";//文件打开的路径

        public string StrFileName
        {
            get { return m_StrFileName; }
            set { m_StrFileName = value; OnPropertyChanged("StrFileName"); }
        }

        public string strFileDesc
        {
            get { return m_strFileDesc; }
            set { m_strFileDesc = value; OnPropertyChanged("strFileDesc"); }
        }

        public string strFileLastWrite
        {
            get { return m_strFileLastWrite; }
            set { m_strFileLastWrite = value; OnPropertyChanged("strFileLastWrite"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    };

    //枚举和布尔值之间的转换器
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? false : value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(true) ? parameter : Binding.DoNothing; 
        }
    }
}
