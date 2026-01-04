using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KFDBFinder.Extensions
{
    public static class KFDB_STATIC
    {
        public readonly static uint KFDB_FILE_HEAD_SIZE = (uint)Marshal.SizeOf<KFDB_FILE_HEAD>();
        public readonly static uint KFDB_RECORD_MAP_SIZE = (uint)Marshal.SizeOf<KFDB_RECORD_MAP>();
        public readonly static uint KFDB_STR_FIELD_SIZE = (uint)Marshal.SizeOf<KFDB_STR_FIELD>();
        public readonly static uint KFDB_FILE_FIELD_SIZE = (uint)Marshal.SizeOf<KFDB_FILE_FIELD>();

        public readonly static uint T_CHAR_SIZE = (uint)Marshal.SizeOf(typeof(Char));
        public readonly static uint T_UCHAR_SIZE = (uint)Marshal.SizeOf(typeof(Byte));
        public readonly static uint T_SHORT_SIZE = (uint)Marshal.SizeOf(typeof(Int16));
        public readonly static uint T_USHORT_SIZE = (uint)Marshal.SizeOf(typeof(UInt16));
        public readonly static uint T_INT_SIZE = (uint)Marshal.SizeOf(typeof(Int32));
        public readonly static uint T_UINT_SIZE = (uint)Marshal.SizeOf(typeof(UInt32));
        public readonly static uint T_FLOAT_SIZE = (uint)Marshal.SizeOf(typeof(Single));
        public readonly static uint T_DOUBLE_SIZE = (uint)Marshal.SizeOf(typeof(Double));
        public readonly static uint T_INT64_SIZE = (uint)Marshal.SizeOf(typeof(Int64));
        public readonly static uint T_UINT64_SIZE = (uint)Marshal.SizeOf(typeof(UInt64));
        public readonly static uint T_LPCSTR_SIZE = (uint)Marshal.SizeOf(typeof(Int32));

        public readonly static Type T_CHAR_DATATYPE = typeof(Char);
        public readonly static Type T_UCHAR_DATATYPE = typeof(Byte);
        public readonly static Type T_SHORT_DATATYPE = typeof(Int16);
        public readonly static Type T_USHORT_DATATYPE = typeof(UInt16);
        public readonly static Type T_INT_DATATYPE = typeof(Int32);
        public readonly static Type T_UINT_DATATYPE = typeof(UInt32);
        public readonly static Type T_FLOAT_DATATYPE = typeof(Single);
        public readonly static Type T_DOUBLE_DATATYPE = typeof(Double);
        public readonly static Type T_INT64_DATATYPE = typeof(Int64);
        public readonly static Type T_UINT64_DATATYPE = typeof(UInt64);
        public readonly static Type T_LPCSTR_DATATYPE = typeof(String);

        public static UInt32 GetSize(KFDB_FIELD_TYPE uType)
        {
            switch (uType)
            {
                case KFDB_FIELD_TYPE.T_CHAR: return T_CHAR_SIZE;
                case KFDB_FIELD_TYPE.T_UCHAR: return T_UCHAR_SIZE;
                case KFDB_FIELD_TYPE.T_SHORT: return T_SHORT_SIZE;
                case KFDB_FIELD_TYPE.T_USHORT: return T_USHORT_SIZE;
                case KFDB_FIELD_TYPE.T_INT: return T_INT_SIZE;
                case KFDB_FIELD_TYPE.T_UINT: return T_UINT_SIZE;
                case KFDB_FIELD_TYPE.T_FLOAT: return T_FLOAT_SIZE;
                case KFDB_FIELD_TYPE.T_DOUBLE: return T_DOUBLE_SIZE;
                case KFDB_FIELD_TYPE.T_INT64: return T_INT64_SIZE;
                case KFDB_FIELD_TYPE.T_UINT64: return T_UINT64_SIZE;
                case KFDB_FIELD_TYPE.T_LPCSTR: return T_LPCSTR_SIZE;
                default:
                    return 0;
            }
        }

        private readonly static Dictionary<Type, KFDB_FIELD_TYPE> _caseFieldType = new Dictionary<Type, KFDB_FIELD_TYPE>
        {
            [typeof(Char)] = KFDB_FIELD_TYPE.T_CHAR,
            [typeof(Byte)] = KFDB_FIELD_TYPE.T_UCHAR,
            [typeof(Int16)] = KFDB_FIELD_TYPE.T_SHORT,
            [typeof(UInt16)] = KFDB_FIELD_TYPE.T_USHORT,
            [typeof(Int32)] = KFDB_FIELD_TYPE.T_INT,
            [typeof(UInt32)] = KFDB_FIELD_TYPE.T_UINT,
            [typeof(Single)] = KFDB_FIELD_TYPE.T_FLOAT,
            [typeof(Double)] = KFDB_FIELD_TYPE.T_DOUBLE,
            [typeof(Int64)] = KFDB_FIELD_TYPE.T_INT64,
            [typeof(UInt64)] = KFDB_FIELD_TYPE.T_UINT64,
            [typeof(String)] = KFDB_FIELD_TYPE.T_LPCSTR,
        };

        public static KFDB_FIELD_TYPE GetType(Type code)
        {
            if (_caseFieldType.ContainsKey(code))
                return _caseFieldType[code];
            else
                return KFDB_FIELD_TYPE.T_LPCSTR;
        }

        public static Type GetType(KFDB_FIELD_TYPE FIELD_TYPE)
        {
            switch (FIELD_TYPE)
            {

                case KFDB_FIELD_TYPE.T_CHAR:
                    {
                        return T_CHAR_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_UCHAR:
                    {
                        return T_UCHAR_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_SHORT:
                    {
                        return T_SHORT_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_USHORT:
                    {
                        return T_USHORT_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_INT:
                    {
                        return T_INT_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_UINT:
                    {
                        return T_UINT_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_FLOAT:
                    {
                        return T_FLOAT_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_DOUBLE:
                    {
                        return T_DOUBLE_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_INT64:
                    {
                        return T_INT64_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_UINT64:
                    {
                        return T_UINT64_DATATYPE;
                    }
                case KFDB_FIELD_TYPE.T_LPCSTR:
                    {
                        return T_LPCSTR_DATATYPE;
                    }
                default:
                    {
                        return T_LPCSTR_DATATYPE;
                    }
            }
        }

        public static T GetValue<T>(byte[] szBuf)
        {
            return (T)GetValue(szBuf, GetType(typeof(T)));
        }

        public static object GetValue(byte[] szBuf, KFDB_FIELD_TYPE eFieldType)
        {
            switch (eFieldType)
            {
                case KFDB_FIELD_TYPE.T_CHAR:
                    {
                        return BitConverter.ToChar(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_UCHAR:
                    {
                        return szBuf[0];
                    }
                case KFDB_FIELD_TYPE.T_SHORT:
                    {
                        return BitConverter.ToInt16(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_USHORT:
                    {
                        return BitConverter.ToUInt16(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_INT:
                    {
                        return BitConverter.ToInt32(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_UINT:
                    {
                        return BitConverter.ToUInt32(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_FLOAT:
                    {
                        return BitConverter.ToSingle(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_DOUBLE:
                    {
                        return BitConverter.ToDouble(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_INT64:
                    {
                        return BitConverter.ToInt64(szBuf, 0);
                    }
                case KFDB_FIELD_TYPE.T_UINT64:
                    {
                        return BitConverter.ToUInt64(szBuf, 0);
                    }
                default:
                    {
                        return BitConverter.ToInt32(szBuf, 0);
                    }
            }
        }



        public static byte[] ConverterToBytes(object obj, KFDB_FIELD_TYPE eFieldType)
        {
            if (obj is string str)
            {
                switch (eFieldType)
                {
                    case KFDB_FIELD_TYPE.T_CHAR:
                        {
                            obj = char.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_UCHAR:
                        {
                            obj = byte.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_SHORT:
                        {
                            obj = Int16.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_USHORT:
                        {
                            obj = UInt16.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_INT:
                        {
                            obj = Int32.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_UINT:
                        {
                            obj = UInt32.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_FLOAT:
                        {
                            obj = float.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_DOUBLE:
                        {
                            obj = double.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_INT64:
                        {
                            obj = Int64.Parse(str);
                            break;
                        }
                    case KFDB_FIELD_TYPE.T_UINT64:
                        {
                            obj = UInt64.Parse(str);
                            break;
                        }
                    default:
                        {
                            obj = Int32.Parse(str);
                            break;
                        }
                }
            }

            switch (eFieldType)
            {
                case KFDB_FIELD_TYPE.T_CHAR:
                    {
                        if (obj is char value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(char));
                    }
                case KFDB_FIELD_TYPE.T_UCHAR:
                    {
                        if (obj is byte value)
                            return new byte[] { value };

                        return new byte[] { 0 };
                    }
                case KFDB_FIELD_TYPE.T_SHORT:
                    {
                        if (obj is Int16 value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(Int16));
                    }
                case KFDB_FIELD_TYPE.T_USHORT:
                    {
                        if (obj is UInt16 value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(UInt16));
                    }
                case KFDB_FIELD_TYPE.T_INT:
                    {
                        if (obj is Int32 value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(Int32));
                    }
                case KFDB_FIELD_TYPE.T_UINT:
                    {
                        if (obj is UInt32 value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(UInt32));
                    }
                case KFDB_FIELD_TYPE.T_FLOAT:
                    {
                        if (obj is float value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(float));
                    }
                case KFDB_FIELD_TYPE.T_DOUBLE:
                    {
                        if (obj is double value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(double));
                    }
                case KFDB_FIELD_TYPE.T_INT64:
                    {
                        if (obj is Int64 value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(Int64));
                    }
                case KFDB_FIELD_TYPE.T_UINT64:
                    {
                        if (obj is UInt64 value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes(default(UInt64));
                    }
                default:
                    {
                        if (obj is Int32 value)
                            return BitConverter.GetBytes(value);

                        return BitConverter.GetBytes((Int32)obj);
                    }
            }
        }
    }

    public enum KFDB_FIELD_TYPE
    {
        T_NULL = -1,

        T_CHAR = 0,
        T_UCHAR = 1,
        T_SHORT = 2,
        T_USHORT = 3,
        T_INT = 4,
        T_UINT = 5,
        T_FLOAT = 6,
        T_DOUBLE = 7,
        T_INT64 = 8,
        T_UINT64 = 9,
        T_LPCSTR = 10
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KFDB_FILE_HEAD
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string szIdentify;    // 标示
        public UInt32 nVersion;          // 版本
        public UInt32 nFieldsNum;        // 列数
        public UInt32 nRecordsNum;       // 行数
        public UInt32 nStringBlockSize;	// 字符串区大小
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KFDB_RECORD_MAP
    {
        public UInt32 nFieldIndex;
        public UInt32 lFileOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KFDB_STR_FIELD
    {
        public UInt32 nOffset;
        //public IntPtr pString;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KFDB_FILE_FIELD
    {
        public Byte uType;
        public UInt32 lpStrName;
    }

    public class KFDB_DATAROW
    {
        public object this[int column]
        {
            get
            {
                return ItemArray[column];
            }
            set
            {
                ItemArray[column] = value;
            }
        }

        public object[] ItemArray { get; }

        internal KFDB_DATAROW(KFDB_DATATABLE dataTable)
        {
            ItemArray = new object[dataTable.Columns.Count];
        }

        internal KFDB_DATAROW(object[] array)
        {
            ItemArray = array;
        }
    }

    public class KFDB_DATACOLUMN
    {
        public int Index { get; }

        public string Name { get; }

        public string Description { get; }

        public Type DataType { get; }

        internal KFDB_DATACOLUMN(int index, string name, string description, Type dataType)
        {
            Index = index;
            Name = name;
            Description = description;
            DataType = dataType;
        }
    }

    public class KFDB_DATATABLE
    {
        public Collection<KFDB_DATACOLUMN> Columns { get; } = new Collection<KFDB_DATACOLUMN>();

        public Collection<KFDB_DATAROW> Rows { get; } = new Collection<KFDB_DATAROW>();

        public KFDB_DATAROW this[int row]
        {
            get 
            {
                return Rows[row];
            }
        }

        public KFDB_DATAROW NewRow()
        {
            return new KFDB_DATAROW(this);
        }

        public KFDB_DATAROW NewRow(bool init)
        {
            if (init)
            {
                KFDB_DATAROW ROW = NewRow();

                for (int i = 0; i < Columns.Count; i++)
                {
                    ROW[i] = Columns[i].DataType.IsValueType ? Activator.CreateInstance(Columns[i].DataType) : null;
                }

                return ROW;
            }
            else
                return NewRow();
        }

        public void AddNewRow()
        {
            Rows.Add(NewRow());
        }

        public void AddRow(KFDB_DATAROW row)
        {
            Rows.Add(row);
        }

        public void AddRow(object[] row)
        {
            Rows.Add(new KFDB_DATAROW(row));
        }

        public void AddColumn(int index, string name, string description, Type dataType)
        {
            Columns.Add(new KFDB_DATACOLUMN(index, name, description, dataType));
        }
    }

    public class KFDBFileFieldData
    {
        public int Index { get; set; } = -1;

        public string Name { get; set; }

        public string Description { get; set; }

        public KFDB_FIELD_TYPE FieldType { get; set; } = KFDB_FIELD_TYPE.T_NULL;

        public uint Offset { get; set; }
    }

    public class KFDBFileHead
    {
        public KFDB_FILE_HEAD Head;

        public KFDBFileHead(KFDB_FILE_HEAD head)
        {
            Head = head;
        }
    }

    public class KFDBFile
    {
        public string ModelName { get; }

        public uint DirIndex { get; }

        public uint FileIndex { get; }

        public string ShortDir { get; }

        public string Name { get; }

        public string Description { get; }

        private uint recordCount;

        public uint RecordCount
        {
            get
            {
                if (source != null)
                    recordCount = (uint)source.Rows.Count;
                else if (recordCount != 0)
                    return recordCount;
                else if (head != null)
                    return head.Head.nRecordsNum;

                return recordCount;
            }
        }

        private KFDB_DATATABLE source;

        public KFDB_DATATABLE Source
        {
            get
            {
                if (source != null)
                    return source;
                else
                {
                    source = KFDBClient.Instance.GetKFDBFileData(this, FDBSource);
                    return source;
                }
            }
        }

        internal void ResetSource(KFDB_DATATABLE dest)
        {
            source = dest;
        }

        internal void ResetSource(string fdb)
        {
            source = null;
            fdbSource = fdb;
            head = getKFDBFileHead();
        }

        public void Clear()
        {
            source = null;
            head = null;
        }

        public bool HasSource
        {
            get => source != null;
        }

        public List<KFDBFileFieldData> FDBFields { get; internal set; }

        public List<KFDBFileFieldData> Fields { get; internal set; }

        private string fdbSource;

        private string FDBSource
        {
            get
            {
                return fdbSource ?? FDBPath;
            }
        }

        private string fdbPath;

        public string FDBPath
        {
            get
            {
                if (fdbPath != null)
                    return fdbPath;
                else
                {
                    fdbPath = Path.Combine(Path.Combine(System.IO.Directory.GetCurrentDirectory(), this.ShortDir, $"{this.Name}.fdb"));
                    return fdbPath;
                }

            }
        }

        private string txtPath;

        public string TXTPath
        {
            get
            {
                if (txtPath != null)
                    return txtPath;
                else
                {
                    txtPath = Path.Combine(Path.Combine(System.IO.Directory.GetCurrentDirectory(), this.ShortDir, $"{this.Name}.txt"));
                    return txtPath;
                }

            }
        }

        private FileInfo fDBInfo;

        public FileInfo FDBInfo
        {
            get
            {
                if (fDBInfo != null)
                    return fDBInfo;
                else
                {
                    fDBInfo = new FileInfo(FDBPath);
                    return fDBInfo;
                }
            }
        }

        private KFDBFileHead getKFDBFileHead()
        {
            if (FDBInfo.Exists)
            {
                try
                {
                    using (var fdb = File.Open(FDBInfo.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        var _m_sFileHead = new byte[KFDB_STATIC.KFDB_FILE_HEAD_SIZE];
                        fdb.Read(_m_sFileHead, 0, _m_sFileHead.Length);
                        KFDB_FILE_HEAD m_sFileHead = (KFDB_FILE_HEAD)ByteHelper.BytesToStruct(_m_sFileHead, typeof(KFDB_FILE_HEAD));
                        return new KFDBFileHead(m_sFileHead);
                    }
                }
                catch (Exception ex)
                {
                    KFDBClient.Instance.OutputMessage(new ErrorMessageEventArgs
                    {
                        Message = ex.Message,
                        Exception = ex,
                    });
                }
            }

            return null;
        }

        private KFDBFileHead head;

        public KFDB_FILE_HEAD Head
        {
            get
            {
                if (head == null)
                    head = getKFDBFileHead();

                if (head == null)
                    head = new KFDBFileHead(new KFDB_FILE_HEAD { szIdentify = "COPYRIGHT@KFDB", nVersion = 20111206, });

                return head != null ? head.Head : new KFDB_FILE_HEAD
                {
                    szIdentify = "COPYRIGHT@KFDB",
                    nVersion = 20111206,
                };
            }
        }

        public string HeadTag
        {
            get
            {
                if (HasHead)
                    return $"{Head.szIdentify}{Head.nVersion}";
                else
                    return string.Empty;
            }
        }

        public void ResetHead()
        {
            if (HasHead)
                ResetHead(ref head.Head);
        }

        public void ResetHead(ref KFDB_FILE_HEAD FILE_HEAD)
        {
            FILE_HEAD.szIdentify = "COPYRIGHT@KFDB";
            FILE_HEAD.nVersion = 20111206;
        }

        public bool VaildHead()
        {
            if (HasHead)
                return VaildHead(Head);

            return true;
        }

        public bool VaildHead(KFDB_FILE_HEAD FILE_HEAD)
        {
            return (FILE_HEAD.szIdentify == "COPYRIGHT@KFDB") && (FILE_HEAD.nVersion == 20111206);
        }

        public bool HasHead
        {
            get
            {
                if (head == null)
                    head = getKFDBFileHead();

                return head != null;
            }
        }

        public KFDBFile(uint dirIndex, uint fileIndex, string shortDir, string name, string description, string modelName)
        {
            DirIndex = dirIndex;
            FileIndex = fileIndex;
            ShortDir = shortDir;
            Name = name;
            Description = description;
            ModelName = modelName;
        }

        public bool CompareFields()
        {
            if (Fields == null || FDBFields == null)
                return true;

            if (Fields.Count != FDBFields.Count)
                return false;

            for (int i = 0; i < Fields.Count; i++)
            {
                var a = Fields[i];
                var b = FDBFields[i];

                if (a.Name != b.Name)
                    return false;

                if (a.FieldType != b.FieldType)
                    return false;
            }

            return true;
        }
    }

    public class KFDBInfoFinderNet : IDisposable
    {
        public string Name { get; private set; } = "GameBase";

        public KFDBInfoFinderClr KFDB { get; }

        private IntPtr Ptr { get; }

        public KFDBInfoFinderNet()
        {
            if (Directory.Exists(@"ini\client\common\fdb"))
            {
                try
                {
                    KFDB = new KFDBInfoFinderClr();
                }
                catch (Exception ex)
                {
                    KFDBClient.Instance.OutputMessage(ex);
                }
            }
            else
            {
                Name = "GameData";
                Ptr = Marshal.StringToHGlobalAnsi("GameData.dll");
                // 打印当前工作目录，排查路径基准问题
                Console.WriteLine("当前工作目录：" + Environment.CurrentDirectory);
                unsafe
                {
                    try
                    {
                        KFDB = new KFDBInfoFinderClr((sbyte*)Ptr);
                    }
                    catch (Exception ex)
                    {


                        KFDBClient.Instance.OutputMessage(ex);
                    }
                }
            }
        }
        public void Dispose()
        {
            Marshal.FreeHGlobal(Ptr);
            KFDB?.Dispose();
        }
    }

    public partial class KFDBClient : IOutputMessage
    {
        public bool ExportFDB(string fileName)
        {
            if (File.Exists(fileName) && Path.GetExtension(fileName) == ".txt" && this.IsLoaded)
            {
                var file = this.GetKFDBFiles().FirstOrDefault(x => x.Name == Path.GetFileNameWithoutExtension(fileName));
                if (file != null)
                {
                    return this.ImportTXT(file, fileName, true) && this.ExportFDB(file, Path.Combine(Path.GetDirectoryName(fileName), $"{file.Name}.fdb"), true);
                }
            }

            return false;
        }

        private static readonly Lazy<KFDBClient> instance = new Lazy<KFDBClient>(() => new KFDBClient());

        public static KFDBClient Instance
        {
            get
            {
                if (!instance.Value.IsLoaded)
                    instance.Value.Init();

                return instance.Value;
            }
        }

        public bool IsLoaded { get; private set; }

        public void Reset()
        {
            IsLoaded = false;
            Init();
        }

        private void Init()
        {
            if (!IsLoaded)
            {
                try
                {
                    unsafe
                    {
                        using (var item = new KFDBInfoFinderNet())
                        {
                            var KFDB = item.KFDB;
                            if (KFDB.IsLoad())
                            {
                                IsLoaded = true;
                            }
                            else
                            {
                                OutputMessage(new ErrorMessageEventArgs
                                {
                                    Message = $"未找到模块[{item.Name}]。",
                                });
                            }
                        }
                    }
                }
                catch { }
            }
        }

        //private KFDBInfoFinderClr KFDB;

        public event IOutputMessage.OutputMessageHandler OutputMessageEvent;

        public void OutputMessage(string message)
        {
            OutputMessage(new InfoMessageEventArgs
            {
                Message = message,
            });
        }

        public void OutputMessage(Exception ex)
        {
            OutputMessage(new ErrorMessageEventArgs
            {
                Message = ex.Message,
                Exception = ex
            });
        }

        public void OutputMessage(MessageEventArgs args)
        {
            //LogHelper.Auto(args);
            OutputMessageEvent?.Invoke(this, args);
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<KFDBFile> GetKFDBFiles(bool bShowMsg = true)
        {
            List<KFDBFile> result = new List<KFDBFile>();

            try
            {
                unsafe
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

                                if (bShowMsg)
                                {
                                    this.OutputMessage($"加载数据信息模块[{item.Name}]成功，读取({_FILE_AMOUNT})个文件信息。");
                                    this.OutputMessage($"开始检查数据文件。(0/{_FILE_AMOUNT})");
                                }

                                for (uint unFileIdx = 0; unFileIdx < _FILE_AMOUNT; ++unFileIdx)
                                {
                                    int nFileId = KFDB.GetDirFileId(unDirIdx, unFileIdx);

                                    if (0 > nFileId)
                                    {
                                        continue;
                                    }

                                    string name = string.Empty;
                                    string description = string.Empty;

                                    pRet = (IntPtr)KFDB.GetFileName((uint)nFileId);
                                    if (pRet != IntPtr.Zero)
                                        name = Marshal.PtrToStringAnsi(pRet);


                                    pRet = (IntPtr)KFDB.GetFileDesc((uint)nFileId);
                                    if (pRet != IntPtr.Zero)
                                        description = Marshal.PtrToStringAnsi(pRet);

                                    var file = new KFDBFile(unDirIdx, (uint)nFileId, strResult, name, description, item.Name);
                                    file.Fields = GetKFDBFieldData(file, KFDB);

                                    if (file.FDBInfo.Exists)
                                    {
                                        file.FDBFields = GetKFDBFileColumn(file, null);

                                        if (file.Head.nRecordsNum <= 0 && bShowMsg)
                                        {
                                            this.OutputMessage($"检查文件[{file.Name}]未发现数据。");
                                        }
                                    }
                                    else
                                    {
                                        if (bShowMsg)
                                        {
                                            this.OutputMessage($"检查文件[{file.Name}]未找到。");
                                        }
                                    }

                                    result.Add(file);
                                }
                                if (bShowMsg)
                                {
                                    this.OutputMessage($"完成数据文件检查。({result.Count}/{_FILE_AMOUNT})");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputMessage(ex);
            }

            return result.OrderBy(x => x.Name).ToList();
        }

        static string strFdbFileDir = System.IO.Directory.GetCurrentDirectory() + "\\ini\\client\\common\\fdb";
        public void CheckTxtFile()
        {
            try
            {
                unsafe
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
                                    int nFileId = KFDB.GetDirFileId(unDirIdx, unFileIdx);

                                    if (0 > nFileId)
                                    {
                                        continue;
                                    }

                                    string name = string.Empty;
                                    string description = string.Empty;

                                    pRet = (IntPtr)KFDB.GetFileName((uint)nFileId);
                                    if (pRet != IntPtr.Zero)
                                        name = Marshal.PtrToStringAnsi(pRet);
                                    var file = new KFDBFile(unDirIdx, (uint)nFileId, strResult, name, description, item.Name);
                                    file.Fields = GetKFDBFieldData(file, KFDB);

                                    string strTxtFile = strFdbFileDir + "\\" + name + ".txt";
                                    if (!File.Exists(strTxtFile))
                                    {
                                        File.WriteAllText(strTxtFile, string.Empty);
                                        File.AppendAllText(strTxtFile, string.Join("\t", file.Fields.Select(x => string.Join("", x.Name))), Encoding.GetEncoding("gb2312"));
                                        File.AppendAllText(strTxtFile, "\r\n");
                                        File.AppendAllText(strTxtFile, string.Join("\t", file.Fields.Select(x => string.Join("", x.FieldType))), Encoding.GetEncoding("gb2312"));
                                        File.AppendAllText(strTxtFile, "\r\n");
                                        File.AppendAllText(strTxtFile, string.Join("\t", file.Fields.Select(x => string.Join("", x.Description))), Encoding.GetEncoding("gb2312"));
                                        File.AppendAllText(strTxtFile, "\r\n");
                                        File.AppendAllText(strTxtFile, string.Join("\r\n", file.Source.Rows.Select(x => string.Join("\t", x.ItemArray))), Encoding.GetEncoding("gb2312"));
                                        this.OutputMessage("检查新增TXT文件 : " + strTxtFile);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputMessage(ex);
            }
        }

        internal List<KFDBFileFieldData> GetKFDBFieldData(KFDBFile kFDBFile, KFDBInfoFinderClr KFDB)
        {
            List<KFDBFileFieldData> result = new List<KFDBFileFieldData>();

            if (KFDB.IsLoad())
            {
                unsafe
                {
                    try
                    {
                        uint nFieldAmt = KFDB.GetFieldAmount(kFDBFile.FileIndex);

                        for (uint i = 0; i < nFieldAmt; ++i)
                        {
                            string name = string.Empty;
                            string description = string.Empty;

                            IntPtr pRet = (IntPtr)KFDB.GetFieldName(kFDBFile.FileIndex, i);
                            if (pRet != IntPtr.Zero)
                                name = Marshal.PtrToStringAnsi(pRet);

                            pRet = (IntPtr)KFDB.GetFieldDesc(kFDBFile.FileIndex, i);
                            if (pRet != IntPtr.Zero)
                                description = Marshal.PtrToStringAnsi(pRet);

                            KFDB_FIELD_TYPE fieldType = (KFDB_FIELD_TYPE)KFDB.GetFieldType(kFDBFile.FileIndex, i);

                            result.Add(new KFDBFileFieldData { Index = result.Count + 1, FieldType = fieldType, Name = name, Description = description });
                        }
                    }
                    catch (Exception ex)
                    {
                        OutputMessage(ex);
                    }
                }
            }

            return result;
        }

        internal List<KFDBFileFieldData> GetKFDBFieldData(KFDBFile kFDBFile)
        {
            using (var KFDB = new KFDBInfoFinderClr())
            {
                return GetKFDBFieldData(kFDBFile, KFDB);
            }
        }

        internal List<KFDBFileFieldData> GetKFDBFileColumn(KFDBFile kFDBFile, string fdbPath)
        {
            if (IsLoaded && kFDBFile.FDBInfo.Exists && kFDBFile.HasHead)
            {
                //C# 读取

                var fdb = File.Open(string.IsNullOrWhiteSpace(fdbPath) ? kFDBFile.FDBPath : fdbPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                byte[] bytes = new byte[fdb.Length];
                fdb.Read(bytes, 0, bytes.Length);
                fdb.Dispose();

                KFDB_FILE_HEAD m_sFileHead = kFDBFile.Head;

                uint m_skipSize = KFDB_STATIC.T_UCHAR_SIZE * 5;
                uint m_unRecordSize = 0;

                var m_Fields = new List<KFDBFileFieldData>();
                for (int col = 0; col < m_sFileHead.nFieldsNum; col++)
                {
                    //这里取5个单位, 后4位保存KFDB_FILE_FIELD 中 LPCSTR 字符串指针,现已无法使用
                    byte[] _uType = new byte[KFDB_STATIC.KFDB_FILE_FIELD_SIZE];
                    Array.Copy(bytes, KFDB_STATIC.KFDB_FILE_HEAD_SIZE + (col * m_skipSize), _uType, 0, _uType.Length);
                    KFDB_FILE_FIELD m_FileField = (KFDB_FILE_FIELD)ByteHelper.BytesToStruct(_uType, typeof(KFDB_FILE_FIELD));
                    KFDB_FIELD_TYPE eFieldType = (KFDB_FIELD_TYPE)m_FileField.uType;

                    KFDBFileFieldData data = new KFDBFileFieldData
                    {
                        Index = col,
                        FieldType = eFieldType,
                        Offset = m_FileField.lpStrName,
                    };

                    m_Fields.Add(data);
                    m_unRecordSize += KFDB_STATIC.GetSize(data.FieldType);
                }

                uint m_lStrBlockFileOffset
                = (KFDB_STATIC.KFDB_FILE_HEAD_SIZE
                + m_sFileHead.nFieldsNum * 5
                + m_sFileHead.nRecordsNum * KFDB_STATIC.KFDB_RECORD_MAP_SIZE
                + m_sFileHead.nRecordsNum * m_unRecordSize);

                foreach (KFDBFileFieldData fileField in m_Fields)
                {
                    List<byte> szBuf = new List<byte>();

                    while (bytes.Length > (m_lStrBlockFileOffset + fileField.Offset + szBuf.Count) && bytes[m_lStrBlockFileOffset + fileField.Offset + szBuf.Count] is byte val && val != '\0')
                    {
                        szBuf.Add(val);
                    }

                    string pszString = Encoding.Default.GetString(szBuf.ToArray());
                    fileField.Name = pszString;
                }

                return m_Fields;
            }

            return null;
        }

        internal KFDB_DATATABLE GetKFDBFileData(KFDBFile kFDBFile, string fdbPath)
        {
            KFDB_DATATABLE result = new KFDB_DATATABLE();

            if (IsLoaded)
            {
                foreach (var field in kFDBFile.Fields)
                {
                    result.AddColumn(field.Index, field.Name, field.Description, KFDB_STATIC.GetType(field.FieldType));
                }

                if (kFDBFile.FDBInfo.Exists && kFDBFile.HasHead)
                {
                    //C# 读取
                    var fdb = File.Open(string.IsNullOrWhiteSpace(fdbPath) ? kFDBFile.FDBPath : fdbPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    byte[] bytes = new byte[fdb.Length];
                    fdb.Read(bytes, 0, bytes.Length);
                    fdb.Dispose();

                    KFDB_FILE_HEAD m_sFileHead = kFDBFile.Head;

                    uint m_skipSize = KFDB_STATIC.T_UCHAR_SIZE * 5;
                    uint m_unRecordSize = 0;

                    var m_Fields = new List<KFDBFileFieldData>();
                    for (int col = 0; col < m_sFileHead.nFieldsNum; col++)
                    {
                        //这里取5个单位, 后4位保存KFDB_FILE_FIELD 中 LPCSTR 字符串指针,现已无法使用
                        byte[] _uType = new byte[KFDB_STATIC.KFDB_FILE_FIELD_SIZE];
                        Array.Copy(bytes, KFDB_STATIC.KFDB_FILE_HEAD_SIZE + (col * m_skipSize), _uType, 0, _uType.Length);
                        KFDB_FILE_FIELD m_FileField = (KFDB_FILE_FIELD)ByteHelper.BytesToStruct(_uType, typeof(KFDB_FILE_FIELD));
                        KFDB_FIELD_TYPE eFieldType = (KFDB_FIELD_TYPE)m_FileField.uType;

                        KFDBFileFieldData data = new KFDBFileFieldData
                        {
                            Index = col,
                            FieldType = eFieldType,
                            Offset = m_FileField.lpStrName,
                        };

                        m_Fields.Add(data);
                        m_unRecordSize += KFDB_STATIC.GetSize(data.FieldType);
                    }

                    uint m_lStrBlockFileOffset
                    = (KFDB_STATIC.KFDB_FILE_HEAD_SIZE
                    + m_sFileHead.nFieldsNum * 5
                    + m_sFileHead.nRecordsNum * KFDB_STATIC.KFDB_RECORD_MAP_SIZE
                    + m_sFileHead.nRecordsNum * m_unRecordSize);

                    foreach (KFDBFileFieldData fileField in m_Fields)
                    {
                        List<byte> szBuf = new List<byte>();

                        while (bytes.Length > (m_lStrBlockFileOffset + fileField.Offset + szBuf.Count) && bytes[m_lStrBlockFileOffset + fileField.Offset + szBuf.Count] is byte val && val != '\0')
                        {
                            szBuf.Add(val);
                        }

                        string pszString = Encoding.Default.GetString(szBuf.ToArray());
                        fileField.Name = pszString;
                    }

                    kFDBFile.FDBFields = m_Fields;

                    //筛选
                    int[] _select = new int[m_Fields.Count];
                    for (int i = 0; i < m_Fields.Count; ++i)
                    {
                        KFDBFileFieldData y = m_Fields[i];
                        _select[i] = kFDBFile.Fields.IndexOf(kFDBFile.Fields.FirstOrDefault(x => (x.Name == y.Name) && (x.FieldType == y.FieldType)));
                    }

                    if (m_sFileHead.nFieldsNum != m_Fields.Count)
                    {
                        this.OutputMessage(new ErrorMessageEventArgs { Message = $"导入fdb[{kFDBFile.Name}]字段数({m_Fields.Count})与模块[{$"{kFDBFile.ModelName}"}]字段数({m_sFileHead.nFieldsNum})不一致！" });
                    }

                    for (int row = 0; row < m_sFileHead.nRecordsNum; row++)
                    {
                        byte[] _sRecordMap = new byte[KFDB_STATIC.KFDB_RECORD_MAP_SIZE];
                        Array.Copy(bytes, KFDB_STATIC.KFDB_FILE_HEAD_SIZE + (m_sFileHead.nFieldsNum * 5) + (_sRecordMap.Length * row), _sRecordMap, 0, _sRecordMap.Length);

                        KFDB_RECORD_MAP sRecordMap = (KFDB_RECORD_MAP)ByteHelper.BytesToStruct(_sRecordMap, typeof(KFDB_RECORD_MAP));

                        uint lFileOffset = sRecordMap.lFileOffset;
                        uint unFieldOffset = 0;

                        object[] dataRow = new object[kFDBFile.Fields.Count];
                        result.AddRow(dataRow);

                        for (int i = 0; i < m_Fields.Count; ++i)
                        {
                            int index = _select[i];
                            KFDBFileFieldData field = m_Fields[i];
                            KFDB_FIELD_TYPE eFieldType = field.FieldType;

                            if (index < 0)
                            {

                            }
                            else
                            {
                                //if (index != i && row == 0)
                                //    this.OutputMessage(new ErrorMessageEventArgs { Message = $"导入fdb[{kFDBFile.Name}]字段[{field.Name}]({i})在模块[{"GameBase"}]字段[{field.Name}]({index})序列不一致, 已对齐！" });

                                if (eFieldType == KFDB_FIELD_TYPE.T_LPCSTR)
                                {
                                    byte[] _nOffset = new byte[KFDB_STATIC.T_LPCSTR_SIZE];
                                    Array.Copy(bytes, lFileOffset + unFieldOffset, _nOffset, 0, _nOffset.Length);
                                    int nOffset = KFDB_STATIC.GetValue<int>(_nOffset);

                                    List<byte> szBuf = new List<byte>();
                                    while (bytes[m_lStrBlockFileOffset + nOffset + szBuf.Count] is byte val && val != '\0')
                                    {
                                        szBuf.Add(val);
                                    }

                                    string pszString = Encoding.Default.GetString(szBuf.ToArray());
                                    dataRow[index] = pszString;
                                }
                                else
                                {
                                    byte[] szBuf = new byte[KFDB_STATIC.GetSize(eFieldType)];
                                    Array.Copy(bytes, lFileOffset + unFieldOffset, szBuf, 0, szBuf.Length);

                                    dataRow[index] = (KFDB_STATIC.GetValue(szBuf, eFieldType));
                                }
                            }

                            unFieldOffset += KFDB_STATIC.GetSize(eFieldType);
                        }
                    }
                }
            }

            return result;
        }

        public bool ImportFDB(KFDBFile file, string openPath, bool isOutputMessage = true)
        {
            if (isOutputMessage)
                this.OutputMessage($"开始导入fdb到[{file.Name}]");

            if (File.Exists(openPath))
            {
                file.ResetSource(openPath);

                if (isOutputMessage)
                    this.OutputMessage($"导入fdb到[{file.Name}]完成，共导入({file.RecordCount})条记录。");

                return true;
            }
            else
            {
                return false;
            }
        }

        private class ConverterResult
        {
            public bool IsValid { get; set; }

            public object Result { get; set; }
        }

        public bool ImportTXT(KFDBFile file, FileInfo txt, bool isOutputMessage = true)
        {
            try
            {
                if (txt.Exists)
                {
                    if (file.Fields.Count > 0)
                    {
                        if (!file.HasSource)
                        {
                            KFDB_DATATABLE source = new KFDB_DATATABLE();

                            foreach (var field in file.Fields)
                            {
                                source.AddColumn(field.Index, field.Name, field.Description, KFDB_STATIC.GetType(field.FieldType));
                            }

                            file.ResetSource(source);
                        }
                        else
                        {
                            file.Source.Rows.Clear();
                        }

                        //原始数据
                        //List<string[]> szStr = File.ReadAllLines(txt.FullName, Encoding.GetEncoding("gb2312")).Skip(3).Select(x => x.Split('\t')).ToList();
                        //流读取避免文件被占用
                        List<string[]> szStr = new List<string[]>();
                        FileStream fs = new FileStream(txt.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Write);

                        if (fs != null)
                        {
                            StreamReader streamReader = new StreamReader(fs, Encoding.GetEncoding("gb2312"));
                            string strSourceline = "";
                            int nJumpLine = 0;
                            while ((strSourceline = streamReader.ReadLine()) != null)
                            {
                                if (nJumpLine > 2)
                                {
                                    string[] temp = strSourceline.Split('\t');// line.Select(x => x.Split('\t')).ToList();
                                    szStr.Add(temp);
                                }
                                else
                                {
                                    nJumpLine++;
                                }
                            }
                            streamReader.Dispose();
                        }

                        //真实数据
                        IEnumerable<string[]> _szStr = szStr.Where(x => x.Length > 0 && !string.IsNullOrWhiteSpace(x[0]) && x[0] != ";");

                        foreach (string[] str in _szStr.Where(x => x.Length != file.Fields.Count))
                        {
                            this.OutputMessage(new ErrorMessageEventArgs { ErrorFile = txt.FullName, Message = $"导入txt[{txt.FullName}]字段数({str.Length}) at line {szStr.IndexOf(str) + 1}: 与模块[{$"{file.ModelName}"}]字段数({file.Fields.Count})不一致！" }); ;
                            break;
                        }

                        if (isOutputMessage)
                            this.OutputMessage($"开始导入txt到[{file.Name}]");

                        int errorValid = -1;
                        Dictionary<Type, TypeConverter> converterRecords = new Dictionary<Type, TypeConverter>();
                        Dictionary<string, Dictionary<TypeConverter, ConverterResult>> validRecords = new Dictionary<string, Dictionary<TypeConverter, ConverterResult>>();

                        int nLineIndex = 0;  
                        foreach (string[] line in _szStr)
                        {
                            nLineIndex++;
                            object[] dataRow = new object[file.Fields.Count];

                            for (int i = 0; i < file.Fields.Count; ++i)
                            {
                                KFDBFileFieldData field = file.Fields[i];
                                Type eFieldType = KFDB_STATIC.GetType(field.FieldType);

                                if (i < line.Length)
                                {
                                    TypeConverter converter = converterRecords.ContainsKey(eFieldType) ? converterRecords[eFieldType] : null;

                                    if (converter == null)
                                    {
                                        converter = TypeDescriptor.GetConverter(eFieldType);
                                        converterRecords.Add(eFieldType, converter);
                                    }

                                    string val = line[i];

                                    //这里验证下值类型, 调试模式下会非常耗时
                                    if (!validRecords.ContainsKey(val))
                                        validRecords.Add(val, new Dictionary<TypeConverter, ConverterResult>());

                                    if (!validRecords[val].ContainsKey(converter))
                                    {
                                        bool valid = converter.IsValid(val);

                                        validRecords[val].Add(converter, new ConverterResult
                                        {
                                            IsValid = valid,
                                            Result = !valid ? (eFieldType.IsValueType ? Activator.CreateInstance(eFieldType) : null) : converter.ConvertFrom(val)
                                        });
                                    }

                                    bool isValid = validRecords[val][converter].IsValid;

                                    dataRow[i] = validRecords[val][converter].Result;

                                    if (!isValid && (errorValid < 0 || errorValid == szStr.IndexOf(line)))
                                    {
                                        errorValid = szStr.IndexOf(line);
                                        this.OutputMessage(new ErrorMessageEventArgs { ErrorFile = file.TXTPath, Message = $"导入txt[{file.Name}]字段[{i}] at line {errorValid + 1}: 与模块[{$"{file.ModelName}"}]字段类型[{field.FieldType}]不一致！" });
                                    }

                                    //字段检测接口，后续特殊字段检查添加到此处
                                    CheckSpecialField(file, field, nLineIndex, validRecords[val][converter].Result);
                                }
                                else
                                {
                                    dataRow[i] = eFieldType.IsValueType ? Activator.CreateInstance(eFieldType) : null;
                                }
                            }

                            file.Source.AddRow(dataRow);
                        }
                    }
                }

                if (isOutputMessage)
                    this.OutputMessage($"导入txt到[{file.Name}]完成，共导入({file.RecordCount})条记录。");

                return true;
            }
            catch (Exception ex)
            {
                OutputMessage(ex);
                //throw ex;
            }

            return false;
        }

        //检查特殊字段
        private void CheckSpecialField(KFDBFile file, KFDBFileFieldData field, int nLine, object _value)
        {
            if (null == _value)
            {
                return;
            }

            if (file.Name == "magictype" 
                && (field.Name == "szIntoneSound" || field.Name == "szSenderSound" || field.Name == "szTargetSound"))
            {
                string strSound = (string)_value;

                if (strSound == "NULL" || strSound == "0" || strSound.Length == 0)
                {
                    return;
                }

                if (!strSound.StartsWith("sound/") || !strSound.EndsWith(".aac"))
                {
                    this.OutputMessage(new ErrorMessageEventArgs { Department = MessageDepartment.Resource, ErrorFile = file.TXTPath, 
                        Message = $"导入txt[{file.Name}]字段[{field.Name}] at line {nLine} 内容为 {strSound}， 此字段必须以 [sound/] 开头 [.aac]结尾 " });
                }
            }


            if (file.Name == "magictype" && field.Name == "dwMagicBreak")
            {
                int nValue = Convert.ToInt32(_value);
                if (nValue > 0 && nValue < 400)
                {
                    this.OutputMessage(new ErrorMessageEventArgs
                    {
                        Department = MessageDepartment.Resource,
                        ErrorFile = file.TXTPath,
                        Message = $"导入txt[{file.Name}]字段[{field.Name}] at line {nLine} 内容为 {nValue}，dwMagicBreak 配置非0值 必须配置>=400 "
                    });
                }
            }
        }

        public bool ImportTXT(KFDBFile file, string openPath, bool isOutputMessage = true)
        {
            return ImportTXT(file, new FileInfo(openPath), isOutputMessage);
        }

        public bool ExportTXT(KFDBFile file, bool isOutputMessage = true)
        {
            return ExportTXT(file, file.TXTPath, isOutputMessage);
        }

        /// <summary>
        /// 导出txt
        /// </summary>
        /// <param name="file"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public bool ExportTXT(KFDBFile file, string savePath, bool isOutputMessage = true)
        {
            try
            {
                string saveDir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                File.WriteAllText(savePath, string.Empty);
                File.AppendAllText(savePath, string.Join("\t", file.Fields.Select(x => string.Join("", x.Name))), Encoding.GetEncoding("gb2312"));
                File.AppendAllText(savePath, "\r\n");
                File.AppendAllText(savePath, string.Join("\t", file.Fields.Select(x => string.Join("", x.FieldType))), Encoding.GetEncoding("gb2312"));
                File.AppendAllText(savePath, "\r\n");
                File.AppendAllText(savePath, string.Join("\t", file.Fields.Select(x => string.Join("", x.Description))), Encoding.GetEncoding("gb2312"));
                File.AppendAllText(savePath, "\r\n");
                File.AppendAllText(savePath, string.Join("\r\n", file.Source.Rows.Select(x => string.Join("\t", x.ItemArray))), Encoding.GetEncoding("gb2312"));

                if (isOutputMessage)
                    this.OutputMessage($"导出[{file.Name}]数据完成。");

                return true;
            }
            catch (Exception ex)
            {
                OutputMessage(ex);
                //throw ex;
            }

            return false;
        }

        public bool ExportFDB(KFDBFile file, bool isOutputMessage = true)
        {
            return ExportFDB(file, file.FDBPath, isOutputMessage);
        }

        public bool ExportFDB(KFDBFile file, string savePath, bool isOutputMessage = true)
        {
            try
            {
                //这里重置下文件头信息
                file.ResetHead();

                KFDB_FILE_HEAD m_sFileHead = file.Head;
                KFDB_DATATABLE source = file.Source;

                if (file.HasSource)
                {
                    List<byte> strBlockData = new List<byte>();
                    List<KFDBFileFieldData> m_vecFields = file.Fields;
                    int nFieldNum = m_vecFields.Count;
                    List<byte> vecFieldData = new List<byte>();

                    uint m_nRecordSize = 0;
                    for (int i = 0; i < nFieldNum; i++)
                    {
                        KFDBFileFieldData rFieldInfo = m_vecFields[i];
                        vecFieldData.Add((byte)rFieldInfo.FieldType);
                        vecFieldData.AddRange(BitConverter.GetBytes((uint)strBlockData.Count));

                        strBlockData.AddRange(Encoding.GetEncoding("gb2312").GetBytes(rFieldInfo.Name));
                        strBlockData.Add(0);

                        m_nRecordSize += KFDB_STATIC.GetSize(rFieldInfo.FieldType);
                    }

                    uint m_nRecordsNum = file.RecordCount;
                    List<byte> vecRecordMaps = new List<byte>();
                    uint nHeaderSize = KFDB_STATIC.KFDB_FILE_HEAD_SIZE;
                    uint nFieldBlockSize = (uint)nFieldNum * KFDB_STATIC.KFDB_FILE_FIELD_SIZE;
                    uint nRecordMapSize = KFDB_STATIC.KFDB_RECORD_MAP_SIZE;
                    uint nRecordMapBlockSize = m_nRecordsNum * nRecordMapSize;
                    uint nRecordBlockSize = nHeaderSize + nFieldBlockSize + nRecordMapBlockSize;

                    Dictionary<string, byte[]> mapStrSets = new Dictionary<string, byte[]>();
                    List<byte> vecRecords = new List<byte>();

                    for (uint i = 0; i < m_nRecordsNum; ++i)
                    {
                        KFDB_DATAROW row = source[(int)i];

                        vecRecordMaps.AddRange(KFDB_STATIC.ConverterToBytes(row[0], KFDB_FIELD_TYPE.T_UINT));
                        vecRecordMaps.AddRange(KFDB_STATIC.ConverterToBytes(nRecordBlockSize + i * m_nRecordSize, KFDB_FIELD_TYPE.T_UINT));

                        for (int j = 0; j < nFieldNum; ++j)
                        {
                            KFDB_DATACOLUMN col = source.Columns[j];
                            if (m_vecFields[j].FieldType != KFDB_FIELD_TYPE.T_LPCSTR)
                            {
                                vecRecords.AddRange(KFDB_STATIC.ConverterToBytes(row[j], KFDB_STATIC.GetType(col.DataType)));
                                continue;
                            }

                            string rStrValue = row[j]?.ToString() ?? (col.DataType.IsValueType ? Activator.CreateInstance(col.DataType).ToString() : "");
                            byte[] iter = mapStrSets.ContainsKey(rStrValue) ? mapStrSets[rStrValue] : null;

                            if (iter != null)
                            {
                                vecRecords.AddRange(iter);
                            }
                            else
                            {
                                byte[] bytes = BitConverter.GetBytes((uint)strBlockData.Count);
                                mapStrSets.Add(rStrValue, bytes);
                                vecRecords.AddRange(bytes);
                                strBlockData.AddRange(Encoding.GetEncoding("gb2312").GetBytes(rStrValue));
                                strBlockData.Add(0);
                            }
                        }
                    }

                    m_sFileHead.nFieldsNum = (uint)nFieldNum;
                    m_sFileHead.nRecordsNum = m_nRecordsNum;
                    m_sFileHead.nStringBlockSize = (uint)strBlockData.Count;

                    List<byte> export = new List<byte>();

                    //文件信息的描述
                    export.AddRange(ByteHelper.StructToBytes(m_sFileHead));
                    //所有列头的描述
                    export.AddRange(vecFieldData);
                    //所有行头的描述
                    export.AddRange(vecRecordMaps);
                    //所有行的值描述
                    export.AddRange(vecRecords);
                    //所有字符串类型的值的集合
                    export.AddRange(strBlockData);

                    string saveDir = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(saveDir))
                        Directory.CreateDirectory(saveDir);

                    if (File.Exists(savePath))
                    {
                        using (var fdb = File.Open(savePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            fdb.Write(export.ToArray(), 0, export.Count);
                            fdb.Seek(0, SeekOrigin.Begin);
                        }
                    }
                    else
                    {
                        File.WriteAllBytes(savePath, export.ToArray());
                    }

                    if (isOutputMessage)
                        this.OutputMessage($"导出[{file.Name}]数据完成。");

                    return true;
                }
            }
            catch (Exception ex)
            {
                OutputMessage(ex);
                //throw ex;
            }

            return false;
        }
    }
}
