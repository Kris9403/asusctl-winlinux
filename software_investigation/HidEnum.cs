using System;
using System.Runtime.InteropServices;
using System.Text;

class HidEnum
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, byte[] lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, uint MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, uint DeviceInterfaceDetailDataSize, out uint RequiredSize, IntPtr DeviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("hid.dll")]
    static extern void HidD_GetHidGuid(out Guid HidGuid);

    [DllImport("hid.dll", SetLastError = true)]
    static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, out IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    static extern int HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

    [DllImport("hid.dll", SetLastError = true)]
    static extern bool HidD_GetAttributes(IntPtr HidDeviceObject, out HIDD_ATTRIBUTES Attributes);

    [StructLayout(LayoutKind.Sequential)]
    struct HIDD_ATTRIBUTES
    {
        public int Size;
        public ushort VendorID;
        public ushort ProductID;
        public ushort VersionNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HIDP_CAPS
    {
        public ushort Usage;
        public ushort UsagePage;
        public ushort InputReportByteLength;
        public ushort OutputReportByteLength;
        public ushort FeatureReportByteLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public ushort[] Reserved;
        public ushort NumberLinkCollectionNodes;
        public ushort NumberInputButtonCaps;
        public ushort NumberInputValueCaps;
        public ushort NumberInputDataIndices;
        public ushort NumberOutputButtonCaps;
        public ushort NumberOutputValueCaps;
        public ushort NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps;
        public ushort NumberFeatureValueCaps;
        public ushort NumberFeatureDataIndices;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SP_DEVICE_INTERFACE_DATA
    {
        public int cbSize;
        public Guid InterfaceClassGuid;
        public int Flags;
        public IntPtr Reserved;
    }

    const uint DIGCF_PRESENT = 0x02;
    const uint DIGCF_DEVICEINTERFACE = 0x10;
    const uint GENERIC_READ = 0x80000000;
    const uint GENERIC_WRITE = 0x40000000;
    const uint FILE_SHARE_READ = 0x1;
    const uint FILE_SHARE_WRITE = 0x2;
    const uint OPEN_EXISTING = 3;
    const uint IOCTL_HID_GET_REPORT_DESCRIPTOR = 0x000B0006;

    static string GetDevicePath(IntPtr infoSet, ref SP_DEVICE_INTERFACE_DATA ifData)
    {
        uint requiredSize = 0;
        SetupDiGetDeviceInterfaceDetail(infoSet, ref ifData, IntPtr.Zero, 0, out requiredSize, IntPtr.Zero);
        if (requiredSize == 0) return null;
        IntPtr detailDataBuffer = Marshal.AllocHGlobal((int)requiredSize);
        Marshal.WriteInt32(detailDataBuffer, IntPtr.Size == 8 ? 8 : 6);
        uint reqSize2;
        bool ok = SetupDiGetDeviceInterfaceDetail(infoSet, ref ifData, detailDataBuffer, requiredSize, out reqSize2, IntPtr.Zero);
        string path = null;
        if (ok)
        {
            path = Marshal.PtrToStringAuto(new IntPtr(detailDataBuffer.ToInt64() + 4));
        }
        Marshal.FreeHGlobal(detailDataBuffer);
        return path;
    }

    static void Main()
    {
        Guid hidGuid;
        HidD_GetHidGuid(out hidGuid);
        IntPtr infoSet = SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
        uint idx = 0;
        while (true)
        {
            SP_DEVICE_INTERFACE_DATA ifData = new SP_DEVICE_INTERFACE_DATA();
            ifData.cbSize = Marshal.SizeOf(ifData);
            bool ok = SetupDiEnumDeviceInterfaces(infoSet, IntPtr.Zero, ref hidGuid, idx, ref ifData);
            if (!ok) break;
            string path = GetDevicePath(infoSet, ref ifData);
            idx++;
            if (path == null) continue;
            if (path.ToLower().IndexOf("vid_0b05") >= 0 && path.ToLower().IndexOf("pid_19b6") >= 0)
            {
                Console.WriteLine("PATH: " + path);
                IntPtr h = CreateFile(path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (h == new IntPtr(-1))
                {
                    Console.WriteLine("  open failed err=" + Marshal.GetLastWin32Error());
                    continue;
                }

                HIDD_ATTRIBUTES attrs;
                if (HidD_GetAttributes(h, out attrs))
                {
                    Console.WriteLine(string.Format("  VID={0:x4} PID={1:x4} Version={2:x4}", attrs.VendorID, attrs.ProductID, attrs.VersionNumber));
                }

                IntPtr preparsed;
                if (HidD_GetPreparsedData(h, out preparsed))
                {
                    HIDP_CAPS caps;
                    int status = HidP_GetCaps(preparsed, out caps);
                    Console.WriteLine(string.Format("  UsagePage={0:x4} Usage={1:x4} InputLen={2} OutputLen={3} FeatureLen={4}",
                        caps.UsagePage, caps.Usage, caps.InputReportByteLength, caps.OutputReportByteLength, caps.FeatureReportByteLength));
                    HidD_FreePreparsedData(preparsed);
                }

                byte[] buf = new byte[4096];
                uint bytesReturned;
                bool ioctlOk = DeviceIoControl(h, IOCTL_HID_GET_REPORT_DESCRIPTOR, IntPtr.Zero, 0, buf, (uint)buf.Length, out bytesReturned, IntPtr.Zero);
                if (ioctlOk)
                {
                    Console.WriteLine("  Report Descriptor (" + bytesReturned + " bytes):");
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < bytesReturned; i++) sb.AppendFormat("{0:x2} ", buf[i]);
                    Console.WriteLine("  " + sb.ToString());
                }
                else
                {
                    Console.WriteLine("  IOCTL_HID_GET_REPORT_DESCRIPTOR failed err=" + Marshal.GetLastWin32Error());
                }
                CloseHandle(h);
                Console.WriteLine();
            }
        }
        SetupDiDestroyDeviceInfoList(infoSet);
    }
}
