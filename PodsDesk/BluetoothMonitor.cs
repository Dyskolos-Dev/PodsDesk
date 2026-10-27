using System.Runtime.InteropServices;
using System.Text;
namespace PodsDesk;
internal static class BluetoothMonitor {
 [StructLayout(LayoutKind.Sequential)] struct RadioParams { public uint size; }
 [StructLayout(LayoutKind.Sequential)] struct SearchParams { public uint size; [MarshalAs(UnmanagedType.Bool)] public bool authenticated,remembered,unknown,connected,issueInquiry; public byte timeout; public IntPtr radio; }
 [StructLayout(LayoutKind.Sequential)] struct SystemTime { public ushort year,month,dayOfWeek,day,hour,minute,second,milliseconds; }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct DeviceInfo { public uint size; public ulong address; public uint deviceClass; [MarshalAs(UnmanagedType.Bool)] public bool connected,remembered,authenticated; public SystemTime lastSeen,lastUsed; [MarshalAs(UnmanagedType.ByValTStr,SizeConst=248)] public string name; }
 [DllImport("bthprops.cpl",SetLastError=true)] static extern IntPtr BluetoothFindFirstRadio(ref RadioParams p,out IntPtr radio);
 [DllImport("bthprops.cpl",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] static extern bool BluetoothFindNextRadio(IntPtr find,out IntPtr radio);
 [DllImport("bthprops.cpl")] [return:MarshalAs(UnmanagedType.Bool)] static extern bool BluetoothFindRadioClose(IntPtr find);
 [DllImport("bthprops.cpl",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr BluetoothFindFirstDevice(ref SearchParams p,ref DeviceInfo info);
 [DllImport("bthprops.cpl",CharSet=CharSet.Unicode,SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] static extern bool BluetoothFindNextDevice(IntPtr find,ref DeviceInfo info);
 [DllImport("bthprops.cpl")] [return:MarshalAs(UnmanagedType.Bool)] static extern bool BluetoothFindDeviceClose(IntPtr find);
 [DllImport("kernel32.dll")] [return:MarshalAs(UnmanagedType.Bool)] static extern bool CloseHandle(IntPtr handle);
 internal record Device(string Address,string Name);
 internal static List<Device> ConnectedAirPods(){var result=new List<Device>();var rp=new RadioParams{size=(uint)Marshal.SizeOf<RadioParams>()};IntPtr radio,find=BluetoothFindFirstRadio(ref rp,out radio);if(find==IntPtr.Zero)return result;try{do{try{var sp=new SearchParams{size=(uint)Marshal.SizeOf<SearchParams>(),connected=true,radio=radio};var info=new DeviceInfo{size=(uint)Marshal.SizeOf<DeviceInfo>(),name=""};var devices=BluetoothFindFirstDevice(ref sp,ref info);if(devices!=IntPtr.Zero){try{do{if(info.connected && !string.IsNullOrEmpty(info.name) && info.name.Contains("airpods",StringComparison.OrdinalIgnoreCase))result.Add(new Device((info.address & 0xffffffffffffUL).ToString("X12"),info.name));info.size=(uint)Marshal.SizeOf<DeviceInfo>();}while(BluetoothFindNextDevice(devices,ref info));}finally{BluetoothFindDeviceClose(devices);}}}finally{CloseHandle(radio);}}while(BluetoothFindNextRadio(find,out radio));}finally{BluetoothFindRadioClose(find);}return result;}
}
