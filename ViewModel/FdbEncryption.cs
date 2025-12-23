
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using KFDBFinder.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows;

namespace SjmyLauncher
{
    class FdbEncryption : ViewModelBase
    {
        string m_strFdbFileDir = System.IO.Directory.GetCurrentDirectory() + "\\ini\\client\\common\\fdb";
        string m_strFdbProtocExcelPath = System.IO.Directory.GetCurrentDirectory() + "\\ini\\client\\common\\datatable";
        string m_strFdbProtocOutPath = System.IO.Directory.GetCurrentDirectory() + "\\ini\\client\\common\\databytes";

        FileSystemWatcher fdbWatcher = new FileSystemWatcher();
        FileSystemWatcher csvWatcher = new FileSystemWatcher();

        List<string> listCsvEncryptionError = new List<string>();

        private bool _bIncEncryption = false;

        public bool IsIncEncryption
        {
            get => _bIncEncryption;
            set
            {
                if (_bIncEncryption != value)
                {
                    _bIncEncryption = value;
                }
            }
        }

        private void SendMsg(MessageEventArgs msg)
        {
            Messenger.Default.Send(msg);
            Console.WriteLine(msg.Message);
        }

        public void Init()
        {
            if (!Directory.Exists(m_strFdbFileDir))
            {
                SendMsg(new ErrorMessageEventArgs { Message = "目录不存在 : " + m_strFdbFileDir });
                return;
            }

            KFDBClient.Instance.OutputMessageEvent += (sender, args) =>
            {
                SendMsg(args);
            };

            string strFdbFolderPath = System.IO.Directory.GetCurrentDirectory() + "\\ini\\client\\common\\fdb";
            string strCsvFolderPath = System.IO.Directory.GetCurrentDirectory() + "\\ini\\client\\common\\datatable";
            WatchFile(fdbWatcher, "*.txt", strFdbFolderPath, this.FdbFileSystemWatch_EventHandle);
            WatchFile(csvWatcher, "*.csv", strCsvFolderPath, this.CsvFileSystemWatch_EventHandle);

            var message = new UserEncryptionEvent(UserEncryptionEvent.EncrypEventType.InitFdbFinish);
            Messenger.Default.Send(message);
        }

        void WatchFile(FileSystemWatcher watcher, string strFileType, string strFileSourceDir, FileSystemEventHandler handler)
        {
            if (!Directory.Exists(strFileSourceDir))
            {
                SendMsg(new ErrorMessageEventArgs { Message = "目录不存在 : " + strFileSourceDir });
                return;
            }

            watcher.Path = strFileSourceDir;

            //添加监视的文件类型
            watcher.Filter = strFileType;

            // 监视文件的修改事件
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.Security | NotifyFilters.CreationTime;

            // 添加事件处理程序
            watcher.Changed += new FileSystemEventHandler(handler);

            // 启动监视
            watcher.EnableRaisingEvents = true;
        }

        public void CheckTxtFile()
        {
            KFDBClient.Instance.CheckTxtFile();
        }

        public void SetFdbWatcherStatus(bool bWatch)
        {
            if (Directory.Exists(fdbWatcher.Path))
            {
                fdbWatcher.EnableRaisingEvents = bWatch;
            }
        }

        public void SetCSVWatcherStatus(bool bWatch)
        {
            if (Directory.Exists(fdbWatcher.Path))
            {
                csvWatcher.EnableRaisingEvents = bWatch;
            }
        }

        private void FdbFileSystemWatch_EventHandle(object sender, FileSystemEventArgs e)
        {
            RebuildOneFdbFile(e.FullPath);

            var message = new UserEncryptionEvent(UserEncryptionEvent.EncrypEventType.WatchFileChanged);
            Messenger.Default.Send(message);
        }

        private void CsvFileSystemWatch_EventHandle(object sender, FileSystemEventArgs e)
        {
            RebuildOneToProtoc(e.FullPath);

            var message = new UserEncryptionEvent(UserEncryptionEvent.EncrypEventType.WatchFileChanged);
            Messenger.Default.Send(message);
        }

        public void RebuildOneFdbFile(string strFileName)
        {
            //
            var kfdb = KFDBClient.Instance.GetKFDBFiles(false);
            if (kfdb.Count > 0)
            {
                string param1 = m_strFdbFileDir;
                if (System.IO.Directory.Exists(param1))
                {
                    string[] txt = Directory.GetFiles(param1, "*.txt");
                    if (txt.Length > 0)
                    {
                        if (File.Exists(strFileName) && Path.GetExtension(strFileName) == ".txt")
                        {
                            var file = kfdb.FirstOrDefault(x => x.Name == Path.GetFileNameWithoutExtension(strFileName));
                            if (file != null)
                            {
                                bool succes = KFDBClient.Instance.ImportTXT(file, strFileName, false) && KFDBClient.Instance.ExportFDB(file, Path.Combine(Path.GetDirectoryName(strFileName), $"{file.Name}.fdb"), false);
                                if (succes)
                                {
                                    Console.WriteLine($"导出[{file.Name}]数据完成。");
                                    KFDBClient.Instance.OutputMessage($"导出[{file.Name}]数据完成。");
                                }
                                else
                                {
                                    KFDBClient.Instance.OutputMessage(new ErrorMessageEventArgs { Message = $"导出[{file.Name}]数据失败。" }); ;
                                    Console.WriteLine($"导出[{file.Name}]数据失败。");
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RebuildAllToProtoc(IProgress<int> progress = null, string strFdbProtocExcelPath = "", string strFdbProtocOutPath = "")
        {
            try
            {
                List<string> listInputPath = new List<string>();
                List<string> listOutputPath = new List<string>();
                listInputPath.Add(strFdbProtocExcelPath.Length == 0 ? m_strFdbProtocExcelPath : strFdbProtocExcelPath);
                listOutputPath.Add(strFdbProtocOutPath.Length == 0 ? m_strFdbProtocOutPath : strFdbProtocOutPath);

                // 当前目录下
                string pattern = @"^ini_\d+$";
                string[] subDirectories = Directory.GetDirectories("ini");
                foreach (string subDirectory in subDirectories)
                {
                    string dirName = Path.GetFileName(subDirectory);
                    if (Regex.IsMatch(dirName, pattern))
                    {
                        listInputPath.Add(Path.GetFullPath(subDirectory) + "\\client\\common\\datatable");
                        listOutputPath.Add(Path.GetFullPath(subDirectory) + "\\client\\common\\databytes");
                    }
                }

                if (System.IO.File.Exists("Excel2Config.exe"))
                {
                    MessageEventArgs msgBegin = new InfoMessageEventArgs { Message = "开始执行 Excel2Config.exe 重新生成 DataTable", Department = MessageDepartment.Client };
                    SendMsg(msgBegin);

                    for (int i = 0; i < listInputPath.Count; i++)
                    {
                        var inputPath = listInputPath[i];
                        var outputPath = listOutputPath[i];

                        string strExcelCmd = $"--excel_path={inputPath} --output_path={outputPath} --to_protobuf=all --protoc=protoc.exe --clean --protoc_cmd= ";
                        if (IsIncEncryption)
                        {
                            strExcelCmd += "--checkHash";
                        }

                        Process process = new Process();

                        var startInfo = new ProcessStartInfo
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "Excel2Config.exe",
                            Arguments = strExcelCmd,

                            //RedirectStandardOutput = true,  //重定向输出流
                            RedirectStandardError = true,//重定向输出流
                        };

                        process.StartInfo = startInfo;
                        // 启动外部程序
                        process.Start();

                        MessageEventArgs msg = new InfoMessageEventArgs { Message = "开始执行 Excel2Config.exe 重新生成 DataTable " + inputPath, Department = MessageDepartment.Client };
                        SendMsg(msg);

                        //日志重定向
                        string error = process.StandardError.ReadToEnd();
                        if (error.Length > 0)
                        {
                            string[] lines = error.Split(new string[] { "\n" }, StringSplitOptions.None);
                            foreach (string line in lines)
                            {
                                if (line.StartsWith("[warning]"))
                                {
                                    string outPut = line.Replace("[warning]", "");
                                    if (outPut.Length > 0)
                                    {
                                        MessageEventArgs dataTableErrorInfo = new WarnMessageEventArgs { Message = "Excel2Config.exe:" + outPut, Department = MessageDepartment.Script };
                                        SendMsg(dataTableErrorInfo);
                                    }
                                }
                                else
                                {
                                    if (line.Length > 0)
                                    {
                                        MessageEventArgs dataTableErrorInfo = new ErrorMessageEventArgs { Message = "Excel2Config.exe:" + line, Department = MessageDepartment.Script };
                                        SendMsg(dataTableErrorInfo);
                                    }
                                }
                            }
                        }

                        process.WaitForExit();
                        // 汇报进度
                        progress?.Report((int)(((i + 1) / (float)listInputPath.Count) * 100));
                    }
                    MessageEventArgs msgEnd = new InfoMessageEventArgs { Message = "执行完成 Excel2Config.exe 重新生成 DataTable", Department = MessageDepartment.Client };
                    SendMsg(msgEnd);
                }
                else
                {
                    MessageEventArgs msg = new ErrorMessageEventArgs { Message = "找不到指定程序 ： " + "Excel2Config.exe", Department = MessageDepartment.Client };
                    SendMsg(msg);
                }
            }
            catch (Exception ex)
            {
                MessageEventArgs msg = new ErrorMessageEventArgs { Message = "执行 RebuildAllToProtoc 发生了异常：" + ex.Message, Department = MessageDepartment.Client };
                SendMsg(msg);
            }            
        }

        private bool IsFileBeOccupied(string strFilePath)
        {
            bool bFileOccupied = false;
            try
            {
                using (var file = new FileStream(strFilePath, FileMode.Open, FileAccess.Read))
                {
                    file.Close();
                }
            }
            catch (IOException)
            {
                bFileOccupied = true;
            }

            return bFileOccupied;
        }

        public void RebuildCsvEncryptionError()
        {
            SetCSVWatcherStatus(false);
            for (int i = listCsvEncryptionError.Count - 1; i >= 0; i--)
            {
                if (RebuildOneToProtoc(listCsvEncryptionError[i]))
                {
                    listCsvEncryptionError.RemoveAt(i);
                }

            }
            SetCSVWatcherStatus(true);
        }

        public bool RebuildOneToProtoc(string strFdbProtocExcelPath)
        {
            if (!File.Exists(strFdbProtocExcelPath))
                return false;

            if (IsFileBeOccupied(strFdbProtocExcelPath))
            {
                if (!listCsvEncryptionError.Contains(strFdbProtocExcelPath))
                    listCsvEncryptionError.Add(strFdbProtocExcelPath);
                //MessageBox.Show($"加密失败！{strFdbProtocExcelPath}文件被占用，请关闭后重试", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageEventArgs msg = new ErrorMessageEventArgs { Message = $"加密失败！{strFdbProtocExcelPath}文件被占用，请关闭后重试", Department = MessageDepartment.Client };
                SendMsg(msg);
                return false;
            }

            bool bSuccess = true;
            try
            {
                if (System.IO.File.Exists("Excel2Config.exe"))
                {
                    var inputPath = strFdbProtocExcelPath;
                    string dirName = Path.GetDirectoryName(strFdbProtocExcelPath);
                    var outputPath = dirName.Replace("\\client\\common\\datatable", "\\client\\common\\databytes");

                    string strExcelCmd = $"--excel_path={inputPath} --output_path={outputPath} --to_protobuf=all --protoc=protoc.exe --clean --protoc_cmd= ";
                    if (IsIncEncryption)
                    {
                        strExcelCmd += "--checkHash";
                    }

                    Process process = new Process();

                    var startInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = "Excel2Config.exe",
                        Arguments = strExcelCmd,

                        //RedirectStandardOutput = true,  //重定向输出流
                        RedirectStandardError = true,//重定向输出流
                    };

                    process.StartInfo = startInfo;
                    // 启动外部程序
                    process.Start();

                    MessageEventArgs msg = new InfoMessageEventArgs { Message = "开始执行 Excel2Config.exe 重新生成 DataTable " + inputPath, Department = MessageDepartment.Client };
                    SendMsg(msg);

                    //日志重定向
                    string error = process.StandardError.ReadToEnd();
                    if (error.Length > 0)
                    {
                        string[] lines = error.Split(new string[] { "\n" }, StringSplitOptions.None);
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("[warning]"))
                            {
                                string outPut = line.Replace("[warning]", "");
                                if (outPut.Length > 0)
                                {
                                    MessageEventArgs dataTableErrorInfo = new WarnMessageEventArgs { Message = "Excel2Config.exe:" + outPut, Department = MessageDepartment.Script };
                                    SendMsg(dataTableErrorInfo);
                                    bSuccess = false;
                                }
                            }
                            else
                            {
                                if (line.Length > 0)
                                {
                                    MessageEventArgs dataTableErrorInfo = new ErrorMessageEventArgs { Message = "Excel2Config.exe:" + line, Department = MessageDepartment.Script };
                                    SendMsg(dataTableErrorInfo);
                                    bSuccess = false;
                                }
                            }
                        }
                    }

                    process.WaitForExit();
                }
                else
                {
                    MessageEventArgs msg = new ErrorMessageEventArgs { Message = "找不到指定程序 ： " + "Excel2Config.exe", Department = MessageDepartment.Client };
                    SendMsg(msg);
                    bSuccess = false;
                }
            }
            catch (Exception ex)
            {
                MessageEventArgs msg = new ErrorMessageEventArgs { Message = "执行 RebuildAllToProtoc 发生了异常：" + ex.Message, Department = MessageDepartment.Client };
                SendMsg(msg);
                bSuccess = false;
            }

            if (bSuccess)
            {
                MessageEventArgs msg = new InfoMessageEventArgs { Message = $"文件生成成功{strFdbProtocExcelPath}", Department = MessageDepartment.Client };
                SendMsg(msg);
                var message = new UserEncryptionEvent(UserEncryptionEvent.EncrypEventType.RebuildOneCsvFinish, strFdbProtocExcelPath);
                Messenger.Default.Send(message);
            }

            return bSuccess;
        }

        public string GetDataTableSourceDir()
        {
            return m_strFdbProtocExcelPath;
        }

        public void RebuildAllFdb(IProgress<int> progress = null, string strPath = "")
        {
            try
            {
                var kfdb = KFDBClient.Instance.GetKFDBFiles();
                if (kfdb.Count > 0)
                {
                    List<string> listPath = new List<string>();
                    listPath.Add(strPath.Length == 0 ? m_strFdbFileDir : strPath);

                    // 当前目录下
                    string pattern = @"^ini_\d+$";
                    string[] subDirectories = Directory.GetDirectories("ini");
                    foreach (string subDirectory in subDirectories)
                    {
                        string dirName = Path.GetFileName(subDirectory);
                        if (Regex.IsMatch(dirName, pattern))
                            listPath.Add(Path.GetFullPath(subDirectory) + "\\client\\common\\fdb");
                    }

                    int totalFiles = 0;

                    foreach (var dir in listPath)
                    {
                        if (Directory.Exists(dir))
                            totalFiles += Directory.GetFiles(dir, "*.txt").Length;
                    }

                    int processed = 0;

                    foreach (string dir in listPath)
                    {
                        if (System.IO.Directory.Exists(dir))
                        {
                            string[] txt = Directory.GetFiles(dir, "*.txt");
                            if (txt.Length > 0)
                            {
                                foreach (var fullName in txt)
                                {
                                    if (File.Exists(fullName) && Path.GetExtension(fullName) == ".txt")
                                    {
                                        bool bNeedEncryption = true;
                                        if (IsIncEncryption)
                                        {
                                            try
                                            {
                                                var sha256 = SHA256.Create();
                                                var stream = File.Open(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                                byte[] hashBytes = sha256.ComputeHash(stream);
                                                stream?.Close();
                                                var hashCode = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                                                if (!MainViewModel.HasFileChanged(fullName, hashCode))
                                                {
                                                    bNeedEncryption = false;
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                MessageEventArgs msgEnd = new InfoMessageEventArgs { Message = "Fdb文件读取失败!" + e.ToString(), Department = MessageDepartment.Client };
                                                Messenger.Default.Send(msgEnd);
                                            }
                                        }
                                        if (bNeedEncryption)
                                        {
                                            var file = kfdb.FirstOrDefault(x => x.Name == Path.GetFileNameWithoutExtension(fullName));
                                            if (file != null)
                                            {
                                                bool succes = KFDBClient.Instance.ImportTXT(file, fullName, false) && KFDBClient.Instance.ExportFDB(file, Path.Combine(Path.GetDirectoryName(fullName), $"{file.Name}.fdb"), false);
                                                if (succes)
                                                {
                                                    Console.WriteLine($"导出[{file.Name}]数据完成。");
                                                    KFDBClient.Instance.OutputMessage($"导出[{file.Name}]数据完成。");
                                                }
                                                else
                                                {
                                                    KFDBClient.Instance.OutputMessage(new ErrorMessageEventArgs { Message = $"导出[{file.Name}]数据失败。", ErrorFile = fullName }); ;
                                                    Console.WriteLine($"导出[{file.Name}]数据失败。");
                                                }
                                            }
                                        }
                                    }
                                    processed++;
                                    progress?.Report((int)((processed / (float)totalFiles) * 100));
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public void TransferTxtToCsv(string strPath)
        {
            try
            {
                const int FileDescLines = 3;
                string strOutPath = Path.Combine(m_strFdbProtocExcelPath, Path.GetFileNameWithoutExtension(strPath) + ".csv");
                List<string> listFileDesc = new List<string>();
                bool bUseCsvHeader = false;
                if (File.Exists(strOutPath))
                {
                    var firstThreeLines = File.ReadLines(strOutPath, Encoding.GetEncoding("GBK")).Take(FileDescLines);
                    foreach (var line in firstThreeLines)
                        listFileDesc.Add(line);

                    bUseCsvHeader = true;
                }
   
                List<string> badDataRecords = new List<string>();

                using (var reader = new StreamReader(strPath, Encoding.GetEncoding("GBK")))
                using (var writer = new StreamWriter(strOutPath, false, Encoding.GetEncoding("GBK")))
                using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t",
                    HasHeaderRecord = false,
                    BadDataFound = args => { } // badDataRecords.Add(args.RawRecord.Replace("\"", "\"\""))
                }))
                using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    ShouldQuote = args => args.Field.Contains(",") || args.Field.Contains("\n")
                }))
                {
                    if (bUseCsvHeader)
                    {
                        for (int i = 0; i < listFileDesc.Count; i++)
                        {
                            string[] header = listFileDesc[i].Split(',');
                            foreach (var field in header)
                            {
                                csvWriter.WriteField(field);
                            }

                            csvReader.Read();
                            csvWriter.NextRecord();
                        }
                    }

                    while (csvReader.Read())
                    {
                        foreach (var field in csvReader.Parser.Record)
                        {
                            csvWriter.WriteField(field);
                        }
                        csvWriter.NextRecord();
                    }
                }

                KFDBClient.Instance.OutputMessage($"文件[{strPath}]转换csv完成。");
            }
            catch (Exception ex)
            {
                MessageEventArgs msg = new ErrorMessageEventArgs { Message = "执行 TransferTxtToCsv 发生了异常：" + ex.Message, Department = MessageDepartment.Client };
                SendMsg(msg);
            }
        }

        public List<FdbFileInfo> GetFdbListInfo()
        {
            List<FdbFileInfo> list = new List<FdbFileInfo>();
            unsafe
            {
                try
                {
                    using (var item = new KFDBInfoFinderNet())
                    {
                        var KFDB = item.KFDB;
                        if (KFDB.IsLoad())
                        {
                            uint unDirAmt = KFDB.GetDirAmount();

                            for (uint unDirIdx = 0; unDirIdx < unDirAmt; ++unDirIdx)
                            {
                                string strResult = string.Empty;

                                IntPtr pRet = (IntPtr)KFDB.GetDirName(unDirIdx);
                                if (pRet != IntPtr.Zero)
                                    strResult = Marshal.PtrToStringAnsi(pRet);

                                uint _FILE_AMOUNT = KFDB.GetDirFileAmount(unDirIdx);

                                for (uint unFileIdx = 0; unFileIdx < _FILE_AMOUNT; ++unFileIdx)
                                {
                                    FdbFileInfo info = new FdbFileInfo();
                                    int nFileId = KFDB.GetDirFileId(unDirIdx, unFileIdx);

                                    if (0 > nFileId)
                                    {
                                        continue;
                                    }

                                    string name = string.Empty;//文件名字
                                    string desc = string.Empty;//描述
                                    string description = string.Empty;

                                    pRet = (IntPtr)KFDB.GetFileName((uint)nFileId);
                                    if (pRet != IntPtr.Zero)
                                        info.StrFileName = Marshal.PtrToStringAnsi(pRet);

                                    pRet = (IntPtr)KFDB.GetFileDesc((uint)nFileId);
                                    if (pRet != IntPtr.Zero)
                                    {
                                        info.strFileDesc = Marshal.PtrToStringAnsi(pRet);
                                    }

                                    var file = new KFDBFile(unDirIdx, (uint)nFileId, strResult, name, description, item.Name);//文件信息
                                    //info.strFileLastWrite = file.FDBInfo.LastWriteTime.ToString();
                                    info.strFileOpenPath = file.FDBInfo.DirectoryName + "\\" + info.StrFileName + ".txt";
                                    list.Add(info);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //to do 
                }


            }

            return list;
        }

    }
}
