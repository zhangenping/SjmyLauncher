#ifndef KFDBDEFINES_H
#define KFDBDEFINES_H

//#include <QtGlobal>
#include <string>
#include <vector>
#include <sstream>

#pragma pack(push)
#pragma pack(1)

typedef unsigned int  uint;
typedef unsigned char uchar;

/**
 * @brief 文件标识
 */
const char _KFDB_COPYRIGHT[] = "COPYRIGHT@KFDB";

/**
 * @brief 文件版本号
 */
#ifdef KFDB_MY
const uint _KFDB_VERSION = 20111206;
#else
const uint _KFDB_VERSION = 20100917;
#endif

/**
 * @brief 文件头
 */
struct KFDB_FILE_HEAD
{
    KFDB_FILE_HEAD() :
        nVersion(_KFDB_VERSION),
        nFieldsNum(0),
        nRecordsNum(0),
        nStringBlockSize(0)
    {}

    char szIdentify[16];    // 标示
    uint nVersion;          // 版本
    uint nFieldsNum;        // 列数
    uint nRecordsNum;       // 行数
    uint nStringBlockSize;  // 字符串区大小
};

/**
 * @brief 字段数据块
 */
struct KFDB_FILE_FIELD
{
    KFDB_FILE_FIELD() :
        uType(0),
        lpStrName(0)
    {}

    uchar uType;
    uint lpStrName;
};

/**
 * @brief 字符串字段数据
 */
struct KFDB_STR_FIELD
{
    KFDB_STR_FIELD() :
        nOffset(0),
        pString(0)
    {}
    KFDB_STR_FIELD(uint nValue) :
        pString(0)
    {
        nOffset = nValue;
    }
    KFDB_STR_FIELD(const char* pStr) :
        nOffset(0)
    {
        pString = (size_t)pStr;
    }

    uint nOffset;
    uint pString;
};

/**
 * @brief 记录索引-文件偏移映射块
 */
struct KFDB_RECORD_MAP
{
    KFDB_RECORD_MAP() :
        nFieldIndex(0),
        lFileOffset(0)
    {}
    KFDB_RECORD_MAP(uint nFieldIdx, uint nOffset) :
        nFieldIndex(nFieldIdx),
        lFileOffset(nOffset)
    {}

    uint nFieldIndex;
    uint lFileOffset;
};

#pragma pack(pop)

/**
 * @brief 字段类型
 */
public enum KFDB_FIELD_TYPE
{
    T_CHAR  = 0,
    T_UCHAR = 1,
    T_SHORT = 2,
    T_USHORT= 3,
    T_INT   = 4,
    T_UINT  = 5,
    T_FLOAT = 6,
    T_DOUBLE= 7,
    T_INT64 = 8,
    T_UINT64= 9,
    T_LPCSTR= 10
};

/**
 * @brief 字段信息
 */
struct KFDB_FIELD
{
    KFDB_FIELD() :
        eType(T_INT)
    {}
    KFDB_FIELD(KFDB_FIELD_TYPE eFieldType, const std::string& rStrName) :
        eType(eFieldType),
        strName(rStrName),
        nMinValue(0),
        nMaxValue(0)
    {}
    KFDB_FIELD(KFDB_FIELD_TYPE eFieldType, const std::string &rStrName, int nMin, int nMax) :
        eType(eFieldType),
        strName(rStrName),
        nMinValue(nMin),
        nMaxValue(nMax)
    {}

    KFDB_FIELD_TYPE eType;
    std::string strName;
    std::string strDesc;
    int nMinValue;
    int nMaxValue;
};

typedef std::vector<KFDB_FIELD> KFDB_FIELD_VEC;
typedef std::vector<KFDB_RECORD_MAP> KFDB_RECORD_MAP_VEC;

/**
 * @brief 数据文件信息
 */
struct KFDB_FILE
{
    KFDB_FILE(){}
    KFDB_FILE(const std::string& rStrName, const std::string& rStrDesc, const std::string& rStrStruct) :
        strName(rStrName),
        strDesc(rStrDesc),
        strStruct(rStrStruct),
        nIndexField(-1)
    {}

    std::string strName;
    std::string strDesc;
    std::string strStruct;
    int nIndexField;
    KFDB_FIELD_VEC vecField;

    uint unDirIdx;
    uint nRecordNum;
    uint nFileSize;
    std::string strFileTime;
};

typedef std::vector<KFDB_FILE> KFDB_FILE_VEC;
typedef std::vector<uint> KFDB_ID_VEC;
typedef std::vector<std::string> STD_VEC_STR;

/**
 * @brief 目录信息
 */
struct KFDB_DIR
{
    KFDB_DIR(){}
    KFDB_DIR(const std::string& rStrName) :
        strName(rStrName)
    {}

    std::string strName;
    KFDB_ID_VEC vecFileId;
};

typedef std::vector<KFDB_DIR> KFDB_DIR_VEC;

typedef void* LPKFDB_RECORD;

/**
 * @brief 错误信息
 */
enum KFDB_ERROR
{
    KFDB_ERR_NONE,
    KFDB_ERR_PARAM,               // 无效参数
    KFDB_ERR_FIND_FILE_INFO,      // 无法找到文件信息
    KFDB_ERR_FIND_FIELD_INFO,     // 无法找到字段信息
    KFDB_ERR_MEMERY_ALLOC,        // 内存分配失败
    KFDB_ERR_FILE_NOT_FOUND,      // 文件未找到
    KFDB_ERR_FILE_FORMAT,         // 文件格式错误
    KFDB_ERR_FIELD_FORMAT,        // 字段验证失败
    KFDB_ERR_STR_BLOCK,           // 字符串数据出错
    KFDB_ERR_FILE_CREATE,         // 文件创建失败
    KFDB_ERR_INTERVAL,            // 内部错误
    KFDB_ERR_SUBSCRIPT,           // 下标错误
    KFDB_ERR_VAL_LIMIT,           // 值域错误
    KFDB_ERR_INDEX_VAL,           // 索引字段值错误
    KFDB_ERR_FIELD_VAL,           // 无效字段值
    KFDB_ERR_FILE_OPT,            // 文件操作失败
    KFDB_ERR_FILEINFO_MODULE,     // 文件信息模块错误
    KFDB_ERR_USER_CANCEL,         // 用户取消操作
    KFDB_ERR_CODEC_VAL,           // 编码集错误
    KFDB_ERR_FIELD_FORMAT_NUM,    // 字段验证失败，个数不一
    KFDB_ERR_FIELD_FORMAT_TYPE,   // 字段验证失败，类型不一
    KFDB_ERR_FIELD_FORMAT_NAME    // 字段验证失败，名称不一
};

/**
 * @brief 数据文件格式信息查询接口
 */

interface KFDBInfoFinder
{
public:
	// 得到目录总数
	virtual UINT GetDirAmount(void) const = 0;

	// 得到数据文件目录名称
	virtual bool GetDirName(UINT unDirId, LPCSTR& rlpStrDirName) const = 0;

	// 得到某目录中文件总数
	virtual UINT GetDirFileAmount(UINT unDirId) const = 0;

	// 得到某目录数据文件ID集合
	virtual INT GetDirFileId(UINT unDirId, UINT unIndex) const = 0;

	// 得到数据文件工作目录索引(对应游戏版本的配置文件目录)
	virtual UINT GetWorkDirId(void) const = 0;

	// 设置数据文件工作目录索引(对应游戏版本的配置文件目录)
	virtual bool SetWorkDirId(UINT unDirId) = 0;

	// 得到数据文件总数
	virtual UINT GetFileAmount(void) const = 0;

	// 根据文件索引得到文件路径(对应游戏版本的文件全路径)
	virtual bool GetFilePath(UINT unFileId, std::string& rStrPath) const = 0;

	// 得到默认索引字段
	virtual INT  GetFileIndexField(UINT nFileId) = 0;

	// 使用下标获取文件名
	virtual bool GetFileName(UINT nFileId, LPCSTR& rlpStrName) = 0;

	//使用文件名获取下标
	virtual UINT  GetFileID(LPCSTR rlpStrName) = 0;

	// 使用下标获取文件描述
	virtual bool GetFileDesc(UINT nFileId, LPCSTR& rlpStrDesc) = 0;

	// 得到字段总数
	virtual UINT GetFieldAmount(UINT nFileId) = 0;

	// 得到字段类型
	virtual UCHAR GetFieldType(UINT nFileId, UINT nFieldIdx) = 0;

	// 得到字段名称
	virtual bool GetFieldName(UINT nFileId, UINT nFieldIdx, LPCTSTR& rlpStrName) = 0;

	// 得到字段描述
	virtual bool GetFieldDesc(UINT nFileId, UINT nFieldIdx, LPCTSTR& rlpStrDesc) = 0;

	// 得到字段值域
	virtual bool GetFieldLimit(UINT nFileId, UINT nFieldIdx, INT& rMinValue, INT& rMaxValue) = 0;
};


template <typename Target, typename Source>
std::string KTypeCast(Source arg)
{
    std::stringstream ss;
    ss << arg;
    return ss.str();
}

template <typename Target>
Target KTypeCast(const std::string &arg)
{
    std::stringstream ss(arg);
    Target ret;
    ss >> ret;
    return ret;
}

#endif // KFDBDEFINES_H
