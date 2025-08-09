// Decompiled with JetBrains decompiler
// Type: USB_IR_Library.USBIR
// Assembly: USB_IR_Library, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED27352A-243B-4E35-83EF-992F6D5AAC47
// Assembly location: F:\Downloads\USB_IR_sample\USB_IR_sample\USB_IR_Library.dll

#nullable disable
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AD00020_Control;

public class USBIR
{
    internal const uint DIGCF_PRESENT = 2;
    internal const uint DIGCF_DEVICEINTERFACE = 16 /*0x10*/;
    internal const uint GENERIC_WRITE = 1073741824 /*0x40000000*/;
    internal const uint OPEN_EXISTING = 3;
    internal const uint FILE_SHARE_READ = 1;
    internal const uint FILE_SHARE_WRITE = 2;
    internal const uint DBT_DEVTYP_DEVICEINTERFACE = 5;
    internal const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0;
    internal const uint ERROR_SUCCESS = 0;
    internal const uint ERROR_NO_MORE_ITEMS = 259;
    internal const uint SPDRP_HARDWAREID = 1;
    private static string DevicePath = (string)null;

    private static Guid InterfaceClassGuid = new Guid(1293833650U, (ushort)61807, (ushort)4559, (byte)136, (byte)203,
        (byte)0, (byte)17, (byte)17, (byte)0, (byte)0, (byte)48 /*0x30*/);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr SetupDiGetClassDevs(
        ref Guid ClassGuid,
        IntPtr Enumerator,
        IntPtr hwndParent,
        uint Flags);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetupDiEnumDeviceInterfaces(
        IntPtr DeviceInfoSet,
        IntPtr DeviceInfoData,
        ref Guid InterfaceClassGuid,
        uint MemberIndex,
        ref USBIR.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetupDiEnumDeviceInfo(
        IntPtr DeviceInfoSet,
        uint MemberIndex,
        ref USBIR.SP_DEVINFO_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetupDiGetDeviceRegistryProperty(
        IntPtr DeviceInfoSet,
        ref USBIR.SP_DEVINFO_DATA DeviceInfoData,
        uint Property,
        ref uint PropertyRegDataType,
        IntPtr PropertyBuffer,
        uint PropertyBufferSize,
        ref uint RequiredSize);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(
        IntPtr DeviceInfoSet,
        ref USBIR.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
        IntPtr DeviceInterfaceDetailData,
        uint DeviceInterfaceDetailDataSize,
        ref uint RequiredSize,
        IntPtr DeviceInfoData);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetupDiGetDeviceInterfaceDetail(
        IntPtr DeviceInfoSet,
        ref USBIR.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
        IntPtr DeviceInterfaceDetailData,
        uint DeviceInterfaceDetailDataSize,
        IntPtr RequiredSize,
        IntPtr DeviceInfoData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr RegisterDeviceNotification(
        IntPtr hRecipient,
        IntPtr NotificationFilter,
        uint Flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToWrite,
        ref uint lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    public static SafeFileHandle openUSBIR(IntPtr hRecipient)
    {
        SafeFileHandle safeFileHandle = (SafeFileHandle)null;
        try
        {
            USBIR.DEV_BROADCAST_DEVICEINTERFACE structure = new USBIR.DEV_BROADCAST_DEVICEINTERFACE()
            {
                dbcc_devicetype = 5
            };
            structure.dbcc_size = (uint)Marshal.SizeOf((object)structure);
            structure.dbcc_reserved = 0U;
            structure.dbcc_classguid = USBIR.InterfaceClassGuid;
            IntPtr zero = IntPtr.Zero;
            IntPtr num = Marshal.AllocHGlobal(Marshal.SizeOf((object)structure));
            Marshal.StructureToPtr((object)structure, num, false);
            USBIR.RegisterDeviceNotification(hRecipient, num, 0U);
            if (USBIR.CheckIfPresentAndGetUSBDevicePath())
            {
                safeFileHandle = USBIR.CreateFile(USBIR.DevicePath, 1073741824U /*0x40000000*/, 3U, IntPtr.Zero, 3U, 0U,
                    IntPtr.Zero);
                if (Marshal.GetLastWin32Error() != 0)
                    safeFileHandle = (SafeFileHandle)null;
            }
        }
        catch
        {
        }

        return safeFileHandle;
    }

    public static int closeUSBIR(SafeFileHandle HandleToUSBDevice)
    {
        int num = -1;
        try
        {
            if (HandleToUSBDevice != null)
            {
                HandleToUSBDevice.Close();
                num = 0;
            }
        }
        catch
        {
        }

        return num;
    }

    public static int writeUSBIR(
        SafeFileHandle HandleToUSBDevice,
        USBIR.IR_FORMAT format_type,
        byte[] code,
        int code_len)
    {
        int num1 = -1;
        byte[] lpBuffer = new byte[65];
        uint lpNumberOfBytesWritten = 0;
        try
        {
            if (HandleToUSBDevice != null)
            {
                if (code_len > 0)
                {
                    lpBuffer[0] = (byte)0;
                    lpBuffer[1] = (byte)96 /*0x60*/;
                    lpBuffer[2] = (byte)((USBIR.IR_FORMAT)(code_len / 4 << 4 & 240 /*0xF0*/) |
                                         format_type & (USBIR.IR_FORMAT)15);
                    int num2 = code_len / 8;
                    if (code_len % 8 != 0)
                        ++num2;
                    if (code.Length >= num2)
                    {
                        if (1 <= num2)
                        {
                            if (num2 <= 62)
                            {
                                for (int index = 0; index < num2; ++index)
                                    lpBuffer[index + 3] = code[index];
                                if (USBIR.WriteFile(HandleToUSBDevice, lpBuffer, 65U, ref lpNumberOfBytesWritten,
                                        IntPtr.Zero))
                                    num1 = 0;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
        }

        return num1;
    }

    public static int writeUSBIRex(
        SafeFileHandle HandleToUSBDevice,
        USBIR.IR_FORMAT format_type,
        byte[] code,
        int code_len1,
        int code_len2)
    {
        int num1 = -1;
        byte[] lpBuffer = new byte[65];
        uint lpNumberOfBytesWritten = 0;
        try
        {
            if (HandleToUSBDevice != null)
            {
                if (code_len1 > 0)
                {
                    if (code_len2 >= 0)
                    {
                        lpBuffer[0] = (byte)0;
                        lpBuffer[1] = (byte)97;
                        lpBuffer[2] = (byte)(format_type & (USBIR.IR_FORMAT)255 /*0xFF*/);
                        lpBuffer[3] = (byte)(code_len1 & (int)byte.MaxValue);
                        lpBuffer[4] = (byte)(code_len2 & (int)byte.MaxValue);
                        int num2 = (code_len1 + code_len2) / 8;
                        if ((code_len1 + code_len2) % 8 != 0)
                            ++num2;
                        if (code.Length >= num2)
                        {
                            if (1 <= num2)
                            {
                                if (num2 <= 60)
                                {
                                    for (int index = 0; index < num2; ++index)
                                        lpBuffer[index + 5] = code[index];
                                    if (USBIR.WriteFile(HandleToUSBDevice, lpBuffer, 65U, ref lpNumberOfBytesWritten,
                                            IntPtr.Zero))
                                        num1 = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
        }

        return num1;
    }

    private static bool CheckIfPresentAndGetUSBDevicePath()
    {
        try
        {
            IntPtr zero1 = IntPtr.Zero;
            USBIR.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = new USBIR.SP_DEVICE_INTERFACE_DATA();
            USBIR.SP_DEVICE_INTERFACE_DETAIL_DATA structure1 = new USBIR.SP_DEVICE_INTERFACE_DETAIL_DATA();
            USBIR.SP_DEVINFO_DATA structure2 = new USBIR.SP_DEVINFO_DATA();
            uint MemberIndex = 0;
            uint PropertyRegDataType = 0;
            uint RequiredSize1 = 0;
            uint RequiredSize2 = 0;
            uint RequiredSize3 = 0;
            IntPtr zero2 = IntPtr.Zero;
            uint num1 = 0;
            string str1 = "Vid_22ea&Pid_001e";
            string str2 = "Mi_03";
            IntPtr classDevs = USBIR.SetupDiGetClassDevs(ref USBIR.InterfaceClassGuid, IntPtr.Zero, IntPtr.Zero, 18U);
            if (!(classDevs != IntPtr.Zero))
                return false;
            do
            {
                DeviceInterfaceData.cbSize = (uint)Marshal.SizeOf((object)DeviceInterfaceData);
                if (USBIR.SetupDiEnumDeviceInterfaces(classDevs, IntPtr.Zero, ref USBIR.InterfaceClassGuid, MemberIndex,
                        ref DeviceInterfaceData))
                {
                    if (Marshal.GetLastWin32Error() == 259)
                    {
                        USBIR.SetupDiDestroyDeviceInfoList(classDevs);
                        return false;
                    }

                    structure2.cbSize = (uint)Marshal.SizeOf((object)structure2);
                    USBIR.SetupDiEnumDeviceInfo(classDevs, MemberIndex, ref structure2);
                    USBIR.SetupDiGetDeviceRegistryProperty(classDevs, ref structure2, 1U, ref PropertyRegDataType,
                        IntPtr.Zero, 0U, ref RequiredSize1);
                    IntPtr num2 = Marshal.AllocHGlobal((int)RequiredSize1);
                    USBIR.SetupDiGetDeviceRegistryProperty(classDevs, ref structure2, 1U, ref PropertyRegDataType, num2,
                        RequiredSize1, ref RequiredSize2);
                    string stringUni = Marshal.PtrToStringUni(num2);
                    Marshal.FreeHGlobal(num2);
                    string lowerInvariant = stringUni.ToLowerInvariant();
                    str1 = str1.ToLowerInvariant();
                    str2 = str2.ToLowerInvariant();
                    bool flag1 = lowerInvariant.Contains(str1);
                    bool flag2 = lowerInvariant.Contains(str2);
                    if (flag1 && flag2)
                    {
                        structure1.cbSize = (uint)Marshal.SizeOf((object)structure1);
                        USBIR.SetupDiGetDeviceInterfaceDetail(classDevs, ref DeviceInterfaceData, IntPtr.Zero, 0U,
                            ref RequiredSize3, IntPtr.Zero);
                        IntPtr zero3 = IntPtr.Zero;
                        IntPtr num3 = Marshal.AllocHGlobal((int)RequiredSize3);
                        structure1.cbSize = 6U;
                        Marshal.StructureToPtr((object)structure1, num3, false);
                        if (USBIR.SetupDiGetDeviceInterfaceDetail(classDevs, ref DeviceInterfaceData, num3,
                                RequiredSize3, IntPtr.Zero, IntPtr.Zero))
                        {
                            USBIR.DevicePath = Marshal.PtrToStringUni(new IntPtr((long)(uint)(num3.ToInt32() + 4)));
                            USBIR.SetupDiDestroyDeviceInfoList(classDevs);
                            Marshal.FreeHGlobal(num3);
                            return true;
                        }

                        Marshal.GetLastWin32Error();
                        USBIR.SetupDiDestroyDeviceInfoList(classDevs);
                        Marshal.FreeHGlobal(num3);
                        return false;
                    }

                    ++MemberIndex;
                    ++num1;
                }
                else
                {
                    uint lastWin32Error = (uint)Marshal.GetLastWin32Error();
                    USBIR.SetupDiDestroyDeviceInfoList(classDevs);
                    return false;
                }
            } while (num1 != 10000000U);

            return false;
        }
        catch
        {
            return false;
        }
    }

    public enum IR_FORMAT
    {
        AEHA = 1,
        NEC = 2,
        SONY = 3,
        MITSUBISHI = 4,
    }

    internal struct SP_DEVICE_INTERFACE_DATA
    {
        internal uint cbSize;
        internal Guid InterfaceClassGuid;
        internal uint Flags;
        internal uint Reserved;
    }

    internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        internal uint cbSize;
        internal char[] DevicePath;
    }

    internal struct SP_DEVINFO_DATA
    {
        internal uint cbSize;
        internal Guid ClassGuid;
        internal uint DevInst;
        internal uint Reserved;
    }

    internal struct DEV_BROADCAST_DEVICEINTERFACE
    {
        internal uint dbcc_size;
        internal uint dbcc_devicetype;
        internal uint dbcc_reserved;
        internal Guid dbcc_classguid;
        internal char[] dbcc_name;
    }
}