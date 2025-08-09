// USB_IR_Library.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include "USB_IR_Library.h"
#include <stdexcept>

using namespace std;


#define MY_DEVICE_ID  "Vid_22ea&Pid_001E"
#define MY_DEVICE_ID2  "Mi_03"
//

GUID InterfaceClassGuid = {0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30};
PSP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA;	//Global

BOOL AttachedState = FALSE;						//Need to keep track of the USB device attachment status for proper plug and play operation.
HANDLE ReadWriteHandleToUSBDevice = INVALID_HANDLE_VALUE;

#ifdef UNICODE
static wchar_t DevicePath[256];
#else
static char DevicePath[256];
#endif


bool CheckIfPresentAndGetUSBDevicePath(void)
{
	HDEVINFO DeviceInfoTable = INVALID_HANDLE_VALUE;
	PSP_DEVICE_INTERFACE_DATA InterfaceDataStructure = new SP_DEVICE_INTERFACE_DATA;
//		PSP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA;	//Globally declared instead
	SP_DEVINFO_DATA DevInfoData;

	DWORD InterfaceIndex = 0;
	DWORD StatusLastError = 0;
	DWORD dwRegType;
	DWORD dwRegSize;
	DWORD StructureSize = 0;
	PBYTE PropertyValueBuffer;
	bool MatchFound = false;
	bool MatchFound2 = false;
	DWORD ErrorStatus;
	BOOL BoolStatus = FALSE;
	DWORD LoopCounter = 0;

	//#ifdef UNICODE
	//wchar_t tmpDevicePath[256];
	//#else
	//char tmpDevicePath[256];
	//#endif

	#ifdef UNICODE
	wchar_t DeviceIDToFind[256] = {0};
	wchar_t DeviceIDToFind2[256] = {0};
	#else
	char DeviceIDToFind[256] = {0};
	char DeviceIDToFind2[256] = {0};
	#endif
	
	//First populate a list of plugged in devices (by specifying "DIGCF_PRESENT"), which are of the specified class GUID. 
	DeviceInfoTable = SetupDiGetClassDevs(&InterfaceClassGuid, NULL, NULL, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);


	//Now look through the list we just populated.  We are trying to see if any of them match our device. 
	while(true)
	{
		InterfaceDataStructure->cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
		if(SetupDiEnumDeviceInterfaces(DeviceInfoTable, NULL, &InterfaceClassGuid, InterfaceIndex, InterfaceDataStructure))
		{
			ErrorStatus = GetLastError();
			if(ErrorStatus == ERROR_NO_MORE_ITEMS)	//Did we reach the end of the list of matching devices in the DeviceInfoTable?
			{	//Cound not find the device.  Must not have been attached.
				SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
				return FALSE;
			}
		}
		else	//Else some other kind of unknown error ocurred...
		{
			ErrorStatus = GetLastError();
			SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
			return FALSE;
		}

		//Now retrieve the hardware ID from the registry.  The hardware ID contains the VID and PID, which we will then 
		//check to see if it is the correct device or not.

		//Initialize an appropriate SP_DEVINFO_DATA structure.  We need this structure for SetupDiGetDeviceRegistryProperty().
		DevInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
		SetupDiEnumDeviceInfo(DeviceInfoTable, InterfaceIndex, &DevInfoData);

		//First query for the size of the hardware ID, so we can know how big a buffer to allocate for the data.
		SetupDiGetDeviceRegistryProperty(DeviceInfoTable, &DevInfoData, SPDRP_HARDWAREID, &dwRegType, NULL, 0, &dwRegSize);

		//Allocate a buffer for the hardware ID.
		PropertyValueBuffer = (BYTE *) malloc (dwRegSize);
		if(PropertyValueBuffer == NULL)	//if null, error, couldn't allocate enough memory
		{	//Can't really recover from this situation, just exit instead.
			SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
			delete InterfaceDataStructure;
			return FALSE;		
		}

		//Retrieve the hardware IDs for the current device we are looking at.  PropertyValueBuffer gets filled with a 
		//REG_MULTI_SZ (array of null terminated strings).  To find a device, we only care about the very first string in the
		//buffer, which will be the "device ID".  The device ID is a string which contains the VID and PID, in the example 
		//format "Vid_04d8&Pid_003f".
		SetupDiGetDeviceRegistryProperty(DeviceInfoTable, &DevInfoData, SPDRP_HARDWAREID, &dwRegType, PropertyValueBuffer, dwRegSize, NULL);

		#ifdef UNICODE
		wchar_t device_str_c1[256] = {0};
		wchar_t *dev_cp = (wchar_t *)PropertyValueBuffer;
		#else
		char device_str_c1[256] = {0};
		char *dev_cp = (char *)PropertyValueBuffer;
		#endif

		int count = 0;
		// 文字列コピーをコピーして小文字に変換
		while(*dev_cp != 0x00)
		{
			// 文字列コピー
			device_str_c1[count] = *dev_cp;
			// 小文字に変換
			device_str_c1[count] = tolower(device_str_c1[count]);

			count++;
			dev_cp++;
			//PropertyValueBuffer += sizeof(wchar_t);
		}

		count = 0;
		while(MY_DEVICE_ID[count] != 0)
		{
			DeviceIDToFind[count] = MY_DEVICE_ID[count];
			DeviceIDToFind[count] = tolower(DeviceIDToFind[count]);
			count++;
		}
		count = 0;
		while(MY_DEVICE_ID2[count] != 0)
		{
			DeviceIDToFind2[count] = MY_DEVICE_ID2[count];
			DeviceIDToFind2[count] = tolower(DeviceIDToFind2[count]);
			count++;
		}

		
		#ifdef UNICODE
		if(wcsstr(device_str_c1, DeviceIDToFind) != NULL)
		{
			MatchFound = true;
		}
		else
		{
			MatchFound = false;
		}
		if(wcsstr(device_str_c1, DeviceIDToFind2) != NULL)
		{
			MatchFound2 = true;
		}
		else
		{
			MatchFound2 = false;
		}
		#else
		if(strstr(device_str_c1, DeviceIDToFind) != NULL)
		{
			MatchFound = true;
		}
		else
		{
			MatchFound = false;
		}
		if(strstr(device_str_c1, DeviceIDToFind2) != NULL)
		{
			MatchFound2 = true;
		}
		else
		{
			MatchFound2 = false;
		}
		#endif

		free(PropertyValueBuffer);		//No longer need the PropertyValueBuffer, free the memory to prevent potential memory leaks

		if((MatchFound == true) && (MatchFound2 == true))
		{
			//Device must have been found.  Open WinUSB interface handle now.  In order to do this, we will need the actual device path first.
			//We can get the path by calling SetupDiGetDeviceInterfaceDetail(), however, we have to call this function twice:  The first
			//time to get the size of the required structure/buffer to hold the detailed interface data, then a second time to actually 
			//get the structure (after we have allocated enough memory for the structure.)
			DetailedInterfaceDataStructure->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
			//First call populates "StructureSize" with the correct value
			SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, InterfaceDataStructure, NULL, NULL, &StructureSize, NULL);	
			DetailedInterfaceDataStructure = (PSP_DEVICE_INTERFACE_DETAIL_DATA)(malloc(StructureSize));		//Allocate enough memory
			if(DetailedInterfaceDataStructure == NULL)	//if null, error, couldn't allocate enough memory
			{	//Can't really recover from this situation, just exit instead.
				SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
				return FALSE;		
			}
			DetailedInterfaceDataStructure->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
			 //Now call SetupDiGetDeviceInterfaceDetail() a second time to receive the goods.  
			SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, InterfaceDataStructure, DetailedInterfaceDataStructure, StructureSize, NULL, NULL); 

			//We now have the proper device path, and we can finally open a device handle to the device.
			//WinUSB requires the device handle to be opened with the FILE_FLAG_OVERLAPPED attribute.
			SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
			return TRUE;
		}

		InterfaceIndex++;	
		//Keep looping until we either find a device with matching VID and PID, or until we run out of devices to check.
		//However, just in case some unexpected error occurs, keep track of the number of loops executed.
		//If the number of loops exceeds a very large number, exit anyway, to prevent inadvertent infinite looping.
		LoopCounter++;
		if(LoopCounter == 10000000)	//Surely there aren't more than 10 million devices attached to any forseeable PC...
		{
			return FALSE;
		}
	}//end of while(true)
}


HANDLE __stdcall openUSBIR(HANDLE hRecipient)
{
	//PSP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA;	//Global

	DEV_BROADCAST_DEVICEINTERFACE MyDeviceBroadcastHeader;// = new DEV_BROADCAST_HDR;
	MyDeviceBroadcastHeader.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
	MyDeviceBroadcastHeader.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
	MyDeviceBroadcastHeader.dbcc_reserved = 0;	//Reserved says not to use...
	MyDeviceBroadcastHeader.dbcc_classguid = InterfaceClassGuid;
	RegisterDeviceNotification(hRecipient, &MyDeviceBroadcastHeader, DEVICE_NOTIFY_WINDOW_HANDLE);

	
	if(CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
	{	// USBデバイス見つかった
		if(AttachedState == FALSE)
		{
			DWORD ErrorStatusReadWrite;

			//We now have the proper device path, and we can finally open read and write handles to the device.
			ReadWriteHandleToUSBDevice = CreateFile(DetailedInterfaceDataStructure->DevicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, 0);
			ErrorStatusReadWrite = GetLastError();
			if(ErrorStatusReadWrite == ERROR_SUCCESS)
			{
				AttachedState = TRUE;		//Let the rest of the PC application know the USB device is connected, and it is safe to read/write to it
			}
			else //for some reason the device was physically plugged in, but one or both of the read/write handles didn't open successfully...
			{
				AttachedState = FALSE;		//Let the rest of this application known not to read/write to the device.

				if(ReadWriteHandleToUSBDevice != INVALID_HANDLE_VALUE)
				{
					CloseHandle(ReadWriteHandleToUSBDevice);
					ReadWriteHandleToUSBDevice = INVALID_HANDLE_VALUE;
				}
			}
		}
		else
		{
		}
	}
	else	//Device must not be connected (or not programmed with correct firmware)
	{	// USBデバイスなし
		AttachedState = FALSE;

		if(ReadWriteHandleToUSBDevice != INVALID_HANDLE_VALUE)
		{
			CloseHandle(ReadWriteHandleToUSBDevice);
			ReadWriteHandleToUSBDevice = INVALID_HANDLE_VALUE;
		}
	}
	// 接続成功
	if(AttachedState == TRUE)
	{
		return ReadWriteHandleToUSBDevice;
	}
	else
	{
		return NULL;
	}
}

int __stdcall closeUSBIR(HANDLE HandleToUSBDevice)
{
	int i_ret = 0;


	if(ReadWriteHandleToUSBDevice != INVALID_HANDLE_VALUE && HandleToUSBDevice != NULL && ReadWriteHandleToUSBDevice == HandleToUSBDevice)
	{
		AttachedState = FALSE;

		CloseHandle(ReadWriteHandleToUSBDevice);
		ReadWriteHandleToUSBDevice = INVALID_HANDLE_VALUE;
	}
	else
	{
		i_ret = -1;
	}

	return i_ret;
}


int __stdcall writeUSBIR(HANDLE HandleToUSBDevice, int format_type, unsigned char *code, int code_len)
{
	int i_ret_val = -1;
	BYTE OUTBuffer[65];	//Allocate a memory buffer equal to the OUT endpoint size + 1
	//BYTE INBuffer[65];		//Allocate a memory buffer equal to the IN endpoint size + 1
	DWORD BytesWritten = 0;
	DWORD BytesRead = 0;
	
	if(ReadWriteHandleToUSBDevice != INVALID_HANDLE_VALUE && HandleToUSBDevice != NULL && ReadWriteHandleToUSBDevice == HandleToUSBDevice && code_len > 0)
	{
		OUTBuffer[0] = 0;
		OUTBuffer[1] = 0x60;
		//サイズ＋フォーマットタイプ
		OUTBuffer[2] = (BYTE)((((code_len / 4) << 4) & 0xF0) | ((int)format_type & 0x0F));
		
		int code_len_check = (int)(code_len / 8);
		if((code_len % 8) > 0)
		{
			code_len_check++;
		}

		if(0 < code_len_check && code_len_check <= 62)
		{
			// 赤外線コードコピー
			for (int fi = 0; fi < code_len_check; fi++)
			{
				OUTBuffer[fi + 3] = code[fi];
			}
			if (WriteFile(HandleToUSBDevice, &OUTBuffer, 65, &BytesWritten, 0))
			{
				i_ret_val = 0;
			}
		}
	}
	else
	{
		// パラメータエラー
		i_ret_val = -2;
	}

	return i_ret_val;
}

int __stdcall writeUSBIRex(HANDLE HandleToUSBDevice, int format_type, unsigned char *code, int code_len1, int code_len2)
{
	int i_ret_val = -1;
	BYTE OUTBuffer[65];	//Allocate a memory buffer equal to the OUT endpoint size + 1
	//BYTE INBuffer[65];		//Allocate a memory buffer equal to the IN endpoint size + 1
	DWORD BytesWritten = 0;
	DWORD BytesRead = 0;
	
	if(ReadWriteHandleToUSBDevice != INVALID_HANDLE_VALUE && HandleToUSBDevice != NULL && ReadWriteHandleToUSBDevice == HandleToUSBDevice && code_len1 > 0 && code_len2 >= 0)
	{
		OUTBuffer[0] = 0;
		OUTBuffer[1] = 0x61;
		//フォーマットタイプ
		OUTBuffer[2] = (BYTE)((int)format_type & 0xFF);
		//サイズ
		OUTBuffer[3] = (BYTE)(code_len1 & 0xFF);
		OUTBuffer[4] = (BYTE)(code_len2 & 0xFF);
		
		int code_len_check = (int)((code_len1 + code_len2) / 8);
		if(((code_len1 + code_len2) % 8) > 0)
		{
			code_len_check++;
		}

		if(0 < code_len_check && code_len_check <= 60)
		{
			// 赤外線コードコピー
			for (int fi = 0; fi < code_len_check; fi++)
			{
				OUTBuffer[fi + 5] = code[fi];
			}
			if (WriteFile(HandleToUSBDevice, &OUTBuffer, 65, &BytesWritten, 0))
			{
				i_ret_val = 0;
			}
		}
	}
	else
	{
		// パラメータエラー
		i_ret_val = -2;
	}

	return i_ret_val;
}


