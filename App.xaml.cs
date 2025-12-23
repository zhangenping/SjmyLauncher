using GalaSoft.MvvmLight.Ioc;
using KFDBFinder.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace SjmyLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //:看日志 接一个重定向：> log.txt;  
        //: fdb 指的是fdb加密 后面可以接 0 ， 1。 表示要不要删除源文件
        //:SjmyLauncher.exe fdb E:\\source\\2023_Q4_SjmyLauncher\\env\\ini\\client\\common\\fdb 

        //pb 方式生成 fdb 文件 入参 
        //:SjmyLauncher.exe fdb_pb E:\\source\\sjmy_Developer\\2024_Q4\ini\\client\\common\\datatable E:\\source\\sjmy_Developer\\2024_Q4\\ini\\client\\common\\databytes

        //:dat 加密
        //SjmyLauncher.exe dat (参数2 :文件名 或者目录) (参数3：目标目录(可不写 不写就是默认目录))
        //SjmyLauncher.exe dat E:\source\trunk_xsj\env\ini\client\debug

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            SimpleIoc.Default.Register<DataEncryption>();
            SimpleIoc.Default.Register<FdbEncryption>();

            if (e.Args.Length > 0)
            {
                if ("fdb" == e.Args[0])
                {
                    Console.WriteLine("FdbEncryptionByCmd");
                    FdbEncryptionByCmd(sender, e);
                }
                else if ("dat" == e.Args[0])
                {
                    Console.WriteLine("DatEncryptionByCmd");
                    DatEncryptionByCmd(sender, e);
                }
                else if ("fdb_pb" == e.Args[0])
                {
                    Console.WriteLine("FdbEncryptionByCmd to pb");
                    FdbEncryptionToPbByCmd(e);
                }
                else
                {
                    Console.WriteLine("cmd is error --> ");
                }

                Application.Current.Shutdown();
            }
            else
            {
                SimpleIoc.Default.GetInstance<DataEncryption>().IsIncEncryption = true;
                SimpleIoc.Default.GetInstance<FdbEncryption>().IsIncEncryption = true;
                showMainWindow();
            }
        }

        void showMainWindow()
        {
            MainWindow win = new MainWindow();
            win.Show();
        }


        private void DatEncryptionByCmd(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 1)
            {
                string SourceFolder = e.Args[1];

                //目标目标是缺省 可以不写
                if (e.Args.Length > 2)
                {
                    string TargetFolder = e.Args[2];
                    SimpleIoc.Default.GetInstance<DataEncryption>().SetTargetFolder(TargetFolder);
                }

                if (File.Exists(SourceFolder)) //如果是文件
                {
                    SimpleIoc.Default.GetInstance<DataEncryption>().RebuildOneIniFile(SourceFolder);
                }
                else
                {
                    //如果是目录
                    SimpleIoc.Default.GetInstance<DataEncryption>().SetSouceFolder(SourceFolder);

                    string[] dirs = Directory.GetDirectories(SourceFolder);
                    foreach (var dir in dirs)
                    {
                        SimpleIoc.Default.GetInstance<DataEncryption>().RebuildAllIniInDirectory(dir);
                    }
                    SimpleIoc.Default.GetInstance<DataEncryption>().RebuildAllIniInDirectory(SourceFolder);
                }
            }
        }

        private void FdbEncryptionToPbByCmd(StartupEventArgs e)
        {
            if (e.Args.Length >= 3)
            {
                string param1 = e.Args[1];
                string param2 = e.Args[2];

                SimpleIoc.Default.GetInstance<FdbEncryption>().RebuildAllToProtoc(null, param1, param2);
            }
            else
            {
                Console.WriteLine("FdbEncryptionToPbByCmd 参数不足3个");
            }
        }

        private void FdbEncryptionByCmd(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 1)
            {
                try
                {
                    var kfdb = KFDBClient.Instance.GetKFDBFiles();

                    if (kfdb.Count > 0)
                    {
                        string param1 = e.Args[1];
                        if (System.IO.Directory.Exists(param1))
                        {
                            string[] txt = Directory.GetFiles(param1, "*.txt");
                            if (txt.Length > 0)
                            {
                                bool delete = e.Args.Length > 2 ? e.Args[2] == "1" : false;
                                foreach (var fullName in txt)
                                {
                                    if (File.Exists(fullName) && Path.GetExtension(fullName) == ".txt")
                                    {
                                        var file = kfdb.FirstOrDefault(x => x.Name == Path.GetFileNameWithoutExtension(fullName));
                                        if (file != null)
                                        {
                                            bool succes = KFDBClient.Instance.ImportTXT(file, fullName, false) && KFDBClient.Instance.ExportFDB(file, Path.Combine(Path.GetDirectoryName(fullName), $"{file.Name}.fdb"), false);
                                            if (succes)
                                            {
                                                Console.WriteLine($"导出[{file.Name}]数据完成。");

                                                if (delete)
                                                    File.Delete(fullName);
                                            }
                                            else
                                            {
                                                Console.WriteLine($"导出[{file.Name}]数据失败。");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}
