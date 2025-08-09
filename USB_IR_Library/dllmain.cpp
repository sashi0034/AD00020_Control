// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "stdafx.h"

extern HANDLE ReadWriteHandleToUSBDevice;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		if(ReadWriteHandleToUSBDevice != INVALID_HANDLE_VALUE)
		{
			CloseHandle(ReadWriteHandleToUSBDevice);
		}
		break;
	}
	return TRUE;
}

