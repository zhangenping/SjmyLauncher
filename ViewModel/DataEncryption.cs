using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using KFDBFinder.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static KFDBFinder.Extensions.IOutputMessage;
using System.Security.Cryptography;
using System.Diagnostics;

namespace SjmyLauncher
{
    class DataEncryption : ViewModelBase
    {
        public enum DATA_ENCRYPTION_INIT_ERROR
        {
            DATA_ENCRYPTION_INIT_FOLDER_LOSS = 1,
        };

        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(string path);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr lib, String funcName);

        //加密task.xml文件
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int EncryptTaskXmlWrapper(byte[] strSrcFile, byte[] strDstFile);

        // 定义委托，需要与DLL中的函数签名一致
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int EncryptFileWrapper(byte[] strSrcFile, byte[] strDstFile, byte[] pszKey);

        static string m_strIniFileFolderSource = "\\client\\debug";
        static string m_strIniFileFolderTarget = "\\client\\release";
        string strIniFileSourceDir = System.IO.Directory.GetCurrentDirectory() + "\\ini" + m_strIniFileFolderSource;
        string strIniMapdesFileSourceDir = System.IO.Directory.GetCurrentDirectory() + "\\ini" + m_strIniFileFolderSource + "\\mapdestination";

        string strUserTargetIniDir = ""; // 用户设置的目标目录

        const string DllFilePath = "BaseCode.dll";
        private IntPtr hLib;
        private EncryptFileWrapper encryptFileWrapperDelegate;//加密非taskxml 委托接口
        private EncryptTaskXmlWrapper encryptTaskXmlDelegate;//加密taskxml 委托接口

        private IntPtr pAddressOfEncryptFileWrapper;
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

        public DataEncryption()
        {
            InitLib();
            InitWatch();
        }

        private void SendMsg(MessageEventArgs msg)
        {
            Messenger.Default.Send(msg);

            Console.WriteLine(msg.Message);
        }

        public void InitWatch()
        {
            //监视文件
            WatchFile("*.ini", strIniFileSourceDir, this.FdbFileSystemWatch_EventHandle);
            WatchFile("*.xml", strIniFileSourceDir, this.FdbFileSystemWatch_EventHandle);

            WatchFile("*.xml", strIniMapdesFileSourceDir, this.FdbFileSystemWatch_EventHandle);
        }

        private int nErrorCode = 0;

        private void InitLib()
        {
            //加载basecode中的EncryptFileWrapper接口
            hLib = LoadLibrary(Path.Combine(System.IO.Directory.GetCurrentDirectory(), DllFilePath));

            if (hLib == IntPtr.Zero)
            {
                Console.WriteLine("BaseCode 库加载错误");
                nErrorCode = Marshal.GetLastWin32Error();
            }
            else
            {
                // 获取函数的地址
                pAddressOfEncryptFileWrapper = GetProcAddress(hLib, "EncryptFileWrapper");

                // 检查是否获取成功
                if (pAddressOfEncryptFileWrapper == IntPtr.Zero)
                {
                    SendMsg(new ErrorMessageEventArgs { Message = "EncryptFileWrapper 接口获取失败" });
                }
                else
                {
                    // 转换为委托
                    encryptFileWrapperDelegate = (EncryptFileWrapper)Marshal.GetDelegateForFunctionPointer(pAddressOfEncryptFileWrapper, typeof(EncryptFileWrapper));
                }
            }
        }
        void WatchFile(string strFileType, string strFileSourceDir, FileSystemEventHandler handler)
        {
            if (!Directory.Exists(strIniFileSourceDir))
            {
                SendMsg(new ErrorMessageEventArgs { Message = "目录不存在 : " + strIniFileSourceDir });
                return;
            }

            // 创建一个文件系统监视器
            FileSystemWatcher watcher = new FileSystemWatcher();
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

        public string GetFileSourceDir()
        {
            return strIniFileSourceDir;
        }            

        public void SetSouceFolder(string strFolder)
        {
            strIniFileSourceDir = strFolder;
        }

        public void SetTargetFolder(string strFolder)
        {
            strUserTargetIniDir = strFolder;
        }

        public void RebuildAllIniInDirectory(string strDirectory)
        {
            if (Directory.Exists(strDirectory))
            {
                string[] extends = new string[] { "*.ini ", "*.xml" };
                foreach (var ex in extends)
                {
                    foreach (var file in Directory.GetFiles(strDirectory, ex))
                    {
                        RebuildOneIniFile(file);                     
                    }
                }
            }
        }

        public void RebuildAllIni(IProgress<int> progress = null)
        {
            List<string> listPath = new List<string>();
            listPath.Add(strIniFileSourceDir);

            // 当前目录下
            string pattern = @"^ini_\d+$";
            string[] subDirectories = Directory.GetDirectories("ini");
            foreach (string subDirectory in subDirectories)
            {
                string dirName = Path.GetFileName(subDirectory);
                if (Regex.IsMatch(dirName, pattern))
                    listPath.Add(Path.GetFullPath(subDirectory) + "\\client\\debug");
            }

            int totalDirs = 0;
            foreach (string dirName in listPath)
            {
                if (Directory.Exists(dirName))
                {
                    totalDirs += Directory.GetDirectories(dirName).Length + 1; // 子目录 + 当前目录
                }
            }

            int processed = 0;

            foreach (string dirName in listPath)
            {
                if (Directory.Exists(dirName))
                {
                    string[] dirs = Directory.GetDirectories(dirName);
                    foreach (var dir in dirs)
                    {
                        RebuildAllIniInDirectory(dir);
                        processed++;
                        progress?.Report(processed * 100 / totalDirs);
                    }
                    RebuildAllIniInDirectory(dirName);

                    processed++;
                    progress?.Report(processed * 100 / totalDirs);

                    var message = new InfoMessageEventArgs { Message = "重新加密完成" };
                    SendMsg(message);
                }
            }
        }

        public void AutoMergeConfig(string strSrc, string strTarget)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string strUiSourceDir = Path.Combine(currentDir, strSrc);
            string strUiTargetDir = Path.Combine(currentDir, "configpack", strTarget);
            string autoMergeExePath = Path.Combine(currentDir, "AutoMergeTool.exe");

            // 验证目录和文件存在性
            if (!Directory.Exists(strUiSourceDir))
            {
                Messenger.Default.Send(new ErrorMessageEventArgs { Message = "源目录不存在" });
                return;
            }
            if (!File.Exists(autoMergeExePath))
            {
                Messenger.Default.Send(new ErrorMessageEventArgs { Message = "AutoMergeTool.exe未找到" });
                return;
            }

            string arguments = $"\"{strUiSourceDir}\" \"{strUiTargetDir}\"";

            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = autoMergeExePath,
                    Arguments = arguments,
                    RedirectStandardError = true,
                };

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                Messenger.Default.Send(new InfoMessageEventArgs { Message = "文件合并完成", Department = MessageDepartment.Client });
            }
        }

        public void RebuildOneIniFile(string strFileName)
        {
            string strTargetFile = ""; //最终文件 （路径+文件名）
            string strTargetFileDir = "";
            string strTargetFileName = System.IO.Path.ChangeExtension(strFileName, "dat");
            strTargetFileName = Path.GetFileName(strTargetFileName);

            if (strUserTargetIniDir.Length > 0)
            {
                strTargetFileDir = strUserTargetIniDir;

                string lastFolder = Path.GetDirectoryName(strFileName).Split(Path.DirectorySeparatorChar).Last();
                bool isMapDestination = lastFolder.Equals("mapdestination", StringComparison.OrdinalIgnoreCase);
                if (isMapDestination)
                {
                    strTargetFileDir += "\\mapdestination";
                }
            }
            else 
            {
                strTargetFileDir = Path.GetDirectoryName(strFileName);
                if (strTargetFileDir.Contains(m_strIniFileFolderSource))
                {
                    strTargetFileDir = strTargetFileDir.Replace(m_strIniFileFolderSource, m_strIniFileFolderTarget);
                }
            }

            strTargetFile = strTargetFileDir + "\\"+ strTargetFileName;

            if (!Directory.Exists(strTargetFileDir))
            {
                Directory.CreateDirectory(strTargetFileDir);
                SendMsg(new InfoMessageEventArgs { Message = "创建目录成功:" + strTargetFileDir });
            }

            bool bNeedEncryption = true;
            if (IsIncEncryption)
            {
                try
                {
                    var sha256 = SHA256.Create();
                    var stream = File.Open(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    stream?.Close();
                    var hashCode = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    if (!MainViewModel.HasFileChanged(strFileName, hashCode))
                    {
                        bNeedEncryption = false;
                    }
                }
                catch (Exception e)
                {
                    MessageEventArgs msgEnd = new InfoMessageEventArgs { Message = "Ini文件读取失败!" + e.ToString(), Department = MessageDepartment.Client };
                    Messenger.Default.Send(msgEnd);
                }

            }
            if (bNeedEncryption)
            {
                if (Path.GetFileName(strFileName) == "task.xml")
                {
                    if (EncryTaskFileByDll(strFileName, strTargetFile))
                    {
                        SendMsg(new InfoMessageEventArgs { Message = "文件加密成功:" + strTargetFile });
                    }
                }
                else if (false == EncryptFileByDll(strFileName, strTargetFile, ""))
                {
                    SendMsg(new ErrorMessageEventArgs { Message = "文件加密错误:" + strTargetFile, ErrorFile = strFileName });
                }
            }
        }

        private void FdbFileSystemWatch_EventHandle(object sender, FileSystemEventArgs e)
        {
            string strFileName = e.FullPath;
            RebuildOneIniFile(strFileName);

            var message = new UserEncryptionEvent(UserEncryptionEvent.EncrypEventType.WatchFileChanged);
            Messenger.Default.Send(message);
        }

        public byte[] GetKey(string strFileName, string strKey)
        {
            if (strFileName.StartsWith("slient_ex"))
            {
                byte[] btKey = Encoding.Default.GetBytes("■中国2008奥运会北京■");
                return btKey;
            }
            else if (strFileName.StartsWith("eudpurchasetype"))
            {
                byte[] btKey = Encoding.Default.GetBytes("★魔★域★突★破★100★万★");
                return btKey;
            }
            else
            {
                string strUseKey = Path.GetFileNameWithoutExtension(strFileName);
                byte[] btKey = Encoding.Default.GetBytes(strUseKey);
                return btKey;
            }
        }

        bool EncryTaskFileByDll(string strSrcFile, string strDstFile)
        {
            if (encryptTaskXmlDelegate == null)
            {
                IntPtr libGameLogic = LoadLibrary(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "GameLogic.dll"));
                if (libGameLogic == null)
                {
                    Console.WriteLine("GameLogic 库加载错误");
                }
                else
                {
                    IntPtr pAddressOfEncryptTask = GetProcAddress(libGameLogic, "EncryptTaskXmlWrapper");

                    if (pAddressOfEncryptTask == IntPtr.Zero)
                    {
                        Console.WriteLine("EncryptTaskXmlWrapper 接口获取失败");
                    }
                    else
                    {
                        // 转换为委托
                        encryptTaskXmlDelegate = (EncryptTaskXmlWrapper)Marshal.GetDelegateForFunctionPointer(pAddressOfEncryptTask, typeof(EncryptTaskXmlWrapper));
                    }
                }
            }

            if (encryptTaskXmlDelegate != null)
            {
                byte[] btSrcFile = Encoding.Default.GetBytes(strSrcFile);
                byte[] btDstFile = Encoding.Default.GetBytes(strDstFile);
                int nCode = encryptTaskXmlDelegate(btSrcFile, btDstFile);
                if (nCode != 0)
                {
                    SendMsg(new ErrorMessageEventArgs { Message = "文件加密错误:" + strSrcFile + "ErrorCode=" + nCode, ErrorFile = strSrcFile });
                }
                return nCode == 0;
            }
            else 
            {
                SendMsg(new ErrorMessageEventArgs { Message = "encryptTaskXmlDelegate接口初始化失败", ErrorFile = strSrcFile });
                return false;
            }
        }

        bool EncryptFileByDll(string strSrcFile, string strDstFile, string strKey)
        {
            if (hLib == IntPtr.Zero)
            {
                string strPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), DllFilePath);
                SendMsg(new ErrorMessageEventArgs { Message = "加载库失败：错误码  " + nErrorCode + " 库路径 ： " + strPath });
                return false;
            }


            if (hLib == IntPtr.Zero || pAddressOfEncryptFileWrapper == IntPtr.Zero)
            {
                return false;
            }

            //加密文件返回的CODE

            int ENCRYPT_FILE_CODE_OK = 10;//成功
            int ENCRYPT_FILE_ERROR_CODE_1 = 11;//错误码1 打开源文件或者目标文件失败
            int ENCRYPT_FILE_ERROR_CODE_2 = 12;//加密Key需小于128位!
            int ENCRYPT_FILE_ERROR_CODE_3 = 13;//key 内存申请失败
            int ENCRYPT_FILE_ERROR_CODE_4 = 14;//Key加密失败!
            int ENCRYPT_FILE_ERROR_CODE_5 = 15;//Line加密失败!
            int ENCRYPT_FILE_ERROR_CODE_6 = 16;//传入参数有为空
            int ENCRYPT_FILE_ERROR_CODE_7 = 17;//传入参数有为空

            byte[] btSrcFile = Encoding.Default.GetBytes(strSrcFile);
            byte[] btDstFile = Encoding.Default.GetBytes(strDstFile);

            int nCode;
            if (strKey.Length == 0)
            {
                string strFileNameWithoutExtension = Path.GetFileNameWithoutExtension(strDstFile);
                nCode = encryptFileWrapperDelegate(btSrcFile, btDstFile, GetKey(strFileNameWithoutExtension, strKey));
            }
            else
            {
                byte[] btKey = Encoding.Default.GetBytes(strKey);
                nCode = encryptFileWrapperDelegate(btSrcFile, btDstFile, btKey);
            }


            if (nCode == ENCRYPT_FILE_CODE_OK)
            {
                SendMsg(new InfoMessageEventArgs { Message = "成功" + strDstFile });
            }
            else if (nCode == ENCRYPT_FILE_ERROR_CODE_1)
            {
                SendMsg(new ErrorMessageEventArgs { Message = "打开源文件失败 : " + strSrcFile, ErrorFile = strSrcFile });
            }
            else if (nCode == ENCRYPT_FILE_ERROR_CODE_7)
            {
                SendMsg(new ErrorMessageEventArgs { Message = "打开目标文件失败  " + strDstFile, ErrorFile = strSrcFile });
            }
            else if (nCode == ENCRYPT_FILE_ERROR_CODE_2)
            {
                SendMsg(new ErrorMessageEventArgs { Message = "加密Key需小于128位!", ErrorFile = strSrcFile });
            }
            else if (nCode == ENCRYPT_FILE_ERROR_CODE_3)
            {
                SendMsg(new ErrorMessageEventArgs { Message = "key 内存申请失败", ErrorFile = strSrcFile });
            }
            else if (nCode == ENCRYPT_FILE_ERROR_CODE_4)
            {
                SendMsg(new ErrorMessageEventArgs { Message = "Key加密失败!", ErrorFile = strSrcFile });
            }
            else if (nCode == ENCRYPT_FILE_ERROR_CODE_5)
            {
                SendMsg(new ErrorMessageEventArgs { Message = "Line加密失败!", ErrorFile = strSrcFile });
            }
            else if (nCode == ENCRYPT_FILE_ERROR_CODE_6)
            {
                SendMsg(new ErrorMessageEventArgs { Message = "传入参数有为空", ErrorFile = strSrcFile });
            }

            return true;
        }

    }
}
