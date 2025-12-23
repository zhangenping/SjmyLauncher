using KFDBFinder.Extensions;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SjmyLauncher.View
{
    /// <summary>
    /// LogView.xaml 的交互逻辑
    /// </summary>
    public partial class LogView : UserControl
    {
        private LogViewModel _model;
        public LogView()
        {
            InitializeComponent();

            _model = new LogViewModel();
            this.DataContext = _model;
        }

        private void ListViewLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((ListViewLog.SelectedItem as RunResult) != null)
            {
                if (_model != null)
                {
                    _model.SelectLog = (ListViewLog.SelectedItem as RunResult).StrRunResult; ;
                }
            }
        }

        private void ListViewLog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RunResult data = null;// ListViewLog.SelectedItem as RunResult;
            if (data != null && data.type == MessageType.Error)
            {
                if (File.Exists(data.strErrorFile))
                {
                    OpenFile(data.strErrorFile);
                }
            }
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

        private void DepartmentComboChoose(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox == null) return;

            LogViewModel model = this.DataContext as LogViewModel;
            if (null == model)
            {
                return;
            }

            switch (checkBox.Content.ToString())
            {
                case "程序":
                    if (checkBox.IsChecked == true)
                    {
                        model.LogInfoDepartmentMask |= (int)MessageDepartment.Client;
                    }
                    else
                    {
                        model.LogInfoDepartmentMask ^= (1 << 0);
                    }
                    break;
                case "脚本":
                    if (checkBox.IsChecked == true)
                    {
                        model.LogInfoDepartmentMask |= (int)MessageDepartment.Script;
                    }
                    else
                    {
                        model.LogInfoDepartmentMask ^= (1 << 1);
                    }
                    break;
                case "资源":
                    if (checkBox.IsChecked == true)
                    {
                        model.LogInfoDepartmentMask |= (int)MessageDepartment.Resource;
                    }
                    else
                    {
                        model.LogInfoDepartmentMask ^= (1 << 2);
                    }
                    break;
                case "引擎":
                    if (checkBox.IsChecked == true)
                    {
                        model.LogInfoDepartmentMask |= (int)MessageDepartment.Engine;
                    }
                    else
                    {
                        model.LogInfoDepartmentMask ^= (1 << 3);
                    }
                    break;
                case "信息":
                    if (checkBox.IsChecked == true)
                    {
                        model.IsShowNormalMsg = true;
                    }
                    else
                    {
                        model.IsShowNormalMsg = false;
                    }
                    break;
                case "警告":
                    if (checkBox.IsChecked == true)
                    {
                        model.IsShowWarnMsg = true;
                    }
                    else
                    {
                        model.IsShowWarnMsg = false;
                    }
                    break;
                case "错误":
                    if (checkBox.IsChecked == true)
                    {
                        model.IsShowErrorMsg = true;
                    }
                    else
                    {
                        model.IsShowErrorMsg = false;
                    }
                    break;
                case "日志折叠":
                    if (checkBox.IsChecked == true)
                    {
                        model.IsMerge = true;
                    }
                    else
                    {
                        model.IsMerge = false;
                    }
                    break;               
                default:
                    break;
            }
        }

        private void ComboBoxMouseEnter(object sender, MouseEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.IsDropDownOpen = true;
            }
        }

        private void ComboBoxMouseLeave(object sender, MouseEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null) 
            {
                comboBox.IsDropDownOpen = false;
            }
        }
    }
}
