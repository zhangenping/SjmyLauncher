#include "pch.h"
#include <windows.h>
#include "kfdb_clr.h"
#include <iostream>

using namespace std;
using namespace KFDBFinder;


typedef KFDBInfoFinder* (*LPINFOFINDER)	(void);

HMODULE hDLL;
KFDBInfoFinderClr::KFDBInfoFinderClr()
{
	m_lpFinder = NULL;
	Load();
}

KFDBInfoFinderClr::KFDBInfoFinderClr(const char* pszGameName)
{
	m_lpFinder = NULL;
	Load(pszGameName);
}

void KFDBInfoFinderClr::Load()
{
	hDLL = ::LoadLibrary("GameBase_d.dll");   //加载动态链接库

	if (!hDLL)
	{
		hDLL = ::LoadLibrary("GameBase.dll");   //加载动态链接库
	}

	if (!hDLL)
	{
		FreeLibrary(hDLL);
		return;
	}

	LPINFOFINDER lpGetFinderFun = (LPINFOFINDER)GetProcAddress(hDLL, "_KFDBGetFileInfo");
	if (lpGetFinderFun)
	{
		m_lpFinder = lpGetFinderFun();
	}
}

void KFDBInfoFinderClr::Load(const char* pszGameName)
{
	std::cout << pszGameName << std::endl;

	hDLL = ::LoadLibrary("GameBase_d.dll");   //加载动态链接库

	if (!hDLL)
	{
		hDLL = ::LoadLibrary(pszGameName);   //加载动态链接库
	}

	if (!hDLL)
	{
		FreeLibrary(hDLL);
		return;
	}

	LPINFOFINDER lpGetFinderFun = (LPINFOFINDER)GetProcAddress(hDLL, "_KFDBGetFileInfo");
	if (lpGetFinderFun)
	{
		m_lpFinder = lpGetFinderFun();
	}
}

void KFDBInfoFinderClr::Unload()
{
	FreeLibrary(hDLL);
	m_lpFinder = NULL;
}

bool KFDBInfoFinderClr::IsLoad()
{
	return m_lpFinder != NULL;
}

KFDBInfoFinderClr::~KFDBInfoFinderClr()
{
	Unload();
}

uint KFDBInfoFinderClr::GetDirAmount()
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetDirAmount();
	}

	return 0;
}

const char* KFDBInfoFinderClr::GetDirName(uint unDirId)
{
	const char* p = NULL;
	if (m_lpFinder)
	{
		if (m_lpFinder->GetDirName(unDirId, p) && p != NULL)
		{
			return p;
		}
	}

	return p;
}

uint KFDBInfoFinderClr::GetDirFileAmount(uint unDirId)
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetDirFileAmount(unDirId);
	}

	return 0;
}

int KFDBInfoFinderClr::GetDirFileId(uint unDirId, uint unIndex)
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetDirFileId(unDirId, unIndex);
	}

	return -1;
}

uint KFDBInfoFinderClr::GetWorkDirId()
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetWorkDirId();
	}

	return 0;
}

bool KFDBInfoFinderClr::SetWorkDirId(uint unDirId)
{
	if (m_lpFinder)
	{
		return m_lpFinder->SetWorkDirId(unDirId);
	}

	return -1;
}

uint KFDBInfoFinderClr::GetFileAmount()
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetFileAmount();
	}

	return 0;
}

const char* KFDBInfoFinderClr::GetFilePath(uint unFileId, const char* pszCustomFile)
{
	const char* p = NULL;
	string rStrPath;
	if (m_lpFinder && m_lpFinder->GetFilePath(unFileId, rStrPath))
	{
		p = rStrPath.c_str();
	}

	return p;
}

int KFDBInfoFinderClr::GetFileIndexField(uint unFileId)
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetFileIndexField(unFileId);
	}
	return -1;
}

const char* KFDBInfoFinderClr::GetFileName(uint unFileId)
{
	if (m_lpFinder)
	{
		const char* p = NULL;
		if (m_lpFinder)
		{
			if (m_lpFinder->GetFileName(unFileId, p) && p != NULL)
			{
				return p;
			}
		}
	}
	return NULL;
}

const char* KFDBInfoFinderClr::GetFileDesc(uint unFileId)
{
	if (m_lpFinder)
	{
		const char* p = NULL;
		if (m_lpFinder)
		{
			if (m_lpFinder->GetFileDesc(unFileId, p) && p != NULL)
			{
				return p;
			}
		}
	}
	return NULL;
}

uint KFDBInfoFinderClr::GetFieldAmount(uint unFileId)
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetFieldAmount(unFileId);
	}
	return 0;
}

uchar KFDBInfoFinderClr::GetFieldType(uint unFileId, uint unFieldIdx)
{
	if (m_lpFinder)
	{
		return m_lpFinder->GetFieldType(unFileId, unFieldIdx);
	}
	return 0;
}

const char* KFDBInfoFinderClr::GetFieldName(uint unFileId, uint unFieldIdx)
{
	if (m_lpFinder)
	{
		const char* p = NULL;
		if (m_lpFinder)
		{
			if (m_lpFinder->GetFieldName(unFileId, unFieldIdx, p) && p != NULL)
			{
				return p;
			}
		}
	}
	return NULL;
}

const char* KFDBInfoFinderClr::GetFieldDesc(uint unFileId, uint unFieldIdx)
{
	if (m_lpFinder)
	{
		const char* p = NULL;
		if (m_lpFinder)
		{
			if (m_lpFinder->GetFieldDesc(unFileId, unFieldIdx, p) && p != NULL)
			{
				return p;
			}
		}
	}
	return NULL;
}






