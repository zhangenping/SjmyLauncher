#pragma once

#include "./KFDBLogic/kfdbdefines.h"

namespace KFDBFinder
{
	public ref class KFDBInfoFinderClr
	{
	public:
		KFDBInfoFinderClr();
		KFDBInfoFinderClr(const char* pszGameName);
		virtual ~KFDBInfoFinderClr();

		uint GetDirAmount();
		const char* GetDirName(uint unDirId); //change use
		uint GetDirFileAmount(uint unDirId);
		int GetDirFileId(uint unDirId, uint unIndex);
		uint GetWorkDirId();
		bool SetWorkDirId(uint unDirId);
		uint GetFileAmount();
		const char* GetFilePath(uint unFileId, const char* pszCustomFile);
		int GetFileIndexField(uint unFileId);
		const char* GetFileName(uint unFileId); //change use
		const char* GetFileDesc(uint unFileId);//change use
		uint GetFieldAmount(uint unFileId);
		uchar GetFieldType(uint unFileId, uint unFieldIdx);

		const char* GetFieldName(uint unFileId, uint unFieldIdx);
		const char* GetFieldDesc(uint unFileId, uint unFieldIdx);//change use

		//是否加载成功判断
		bool IsLoad();
		void Load();
		void Load(const char* pszGameName);
		void Unload();

	private:
		KFDBInfoFinder* m_lpFinder;
	};
};

