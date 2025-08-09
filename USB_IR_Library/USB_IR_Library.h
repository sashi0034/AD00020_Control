#include "stdafx.h"

#ifdef __cplusplus
#define EXPORT extern "C" __declspec (dllexport)
#else
#define EXPORT __declspec (dllexport)
#endif

EXPORT HANDLE __stdcall openUSBIR(HANDLE hRecipient);
EXPORT int __stdcall closeUSBIR(HANDLE HandleToUSBDevice);
EXPORT int __stdcall writeUSBIR(HANDLE HandleToUSBDevice, int format_type, unsigned char *code, int code_len);
EXPORT int __stdcall writeUSBIRex(HANDLE HandleToUSBDevice, int format_type, unsigned char *code, int code_len1, int code_len2);

typedef enum
{
    AEHA = 1,
    NEC,
    SONY,
    MITSUBISHI
}IR_FORMAT;

