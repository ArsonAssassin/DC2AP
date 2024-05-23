using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.PCSX2.Util
{
    public class Memory
    {
        #region kernel functions
        public const uint PROCESS_VM_READ = 0x0010;
        public const uint PROCESS_VM_WRITE = 0x0020;
        public const uint PROCESS_VM_OPERATION = 0x0008;
        public const uint PROCESS_SUSPEND_RESUME = 0x0800;

        public const uint PAGE_EXECUTE_READWRITE = 0x40;

        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;


        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processID); 
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr processH, int lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr processH, int lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]  
        internal static extern bool CloseHandle(IntPtr processH);
        [DllImport("kernel32.dll", SetLastError = true)] 
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.ThisCall)]
        public static extern bool VirtualProtect(IntPtr processH, int lpAddress, int lpBuffer, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr processH, int lpAddress, int lpBuffer, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr Arguments);
        
        private static int GetProcessID(string procName)

        {
            Process[] Processes = Process.GetProcessesByName(procName); 
            if (Processes.Any()) 
            {
                IntPtr hWnd = Processes.First().MainWindowHandle;
                GetWindowThreadProcessId(hWnd, out int PID);
                return PID;
            }
            else 
            {
                //application is not running
                CloseHandle(processH);
                return 0;
            }
        }

        //Make PID available anywhere within the program.
        internal static readonly int PCSX2_PROCESSID = GetProcessID("pcsx2");   
        internal static readonly IntPtr processH = OpenProcess(PROCESS_VM_OPERATION | PROCESS_SUSPEND_RESUME | PROCESS_VM_READ | PROCESS_VM_WRITE, false, PCSX2_PROCESSID);

        internal static string GetSystemMessage(uint errorCode)
        {
            return Marshal.PtrToStringAnsi(IntPtr.Zero);
        }
        #endregion
        public static byte ReadByte(int address) 
        {
            byte[] dataBuffer = new byte[1];
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
            return dataBuffer[0];
        }

        public static byte[] ReadByteArray(int address, int numBytes)
        {
            byte[] dataBuffer = new byte[numBytes];
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _); 
            return dataBuffer;
        }

        public static ushort ReadUShort(int address) 
        {
            byte[] dataBuffer = new byte[2];
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
            return BitConverter.ToUInt16(dataBuffer, 0);
        }

        public static short ReadShort(int address)
        {
            byte[] dataBuffer = new byte[2]; 
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
            return BitConverter.ToInt16(dataBuffer, 0);
        }

        public static uint ReadUInt(int address)
        {
            byte[] dataBuffer = new byte[4];
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
            return BitConverter.ToUInt32(dataBuffer, 0);
        }

        public static int ReadInt(int address)
        {
            byte[] dataBuffer = new byte[4];
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
            return BitConverter.ToInt32(dataBuffer, 0);
        }

        public static float ReadFloat(int address)
        {
            byte[] dataBuffer = new byte[8];
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
            return BitConverter.ToSingle(dataBuffer, 0);
        }

        public static double ReadDouble(int address)
        {
            byte[] dataBuffer = new byte[8];
            ReadProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
            return BitConverter.ToDouble(dataBuffer, 0);
        }

        public static string ReadString(int address, int length)
        {
            byte[] dataBuffer = new byte[length];
            ReadProcessMemory(processH, address, dataBuffer, length, out _);
            var converter = Encoding.GetEncoding(10000);
            var output = converter.GetString(dataBuffer);
            return output;
        }

        public static bool Write(int address, byte[] value)
        {
            return WriteProcessMemory(processH, address, value, value.Length, out _);
        }

        public static bool WriteString(int address, string stringToWrite)
        {
            byte[] dataBuffer = Encoding.GetEncoding(10000).GetBytes(stringToWrite);
            return WriteProcessMemory(processH, address, dataBuffer, dataBuffer.Length, out _);
        }

        public static bool WriteByte(int address, byte value)
        {
            return Write(address, [value] );
        }

        public static void WriteByteArray(int address, byte[] byteArray)
        {
            bool successful;
            successful = VirtualProtectEx(processH, address, byteArray.Length, PAGE_EXECUTE_READWRITE, out _);
            if (successful == false) 
                Console.WriteLine(GetLastError() + " - " + GetSystemMessage(GetLastError())); 
            successful = WriteProcessMemory(processH, address, byteArray, byteArray.Length, out _);
            if (successful == false)
                Console.WriteLine(GetLastError() + " - " + GetSystemMessage(GetLastError()));
        }

        public static bool Write(int address, ushort value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public static bool Write(int address, int value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public static bool Write(int address, uint value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public static bool Write(int address, float value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public static bool Write(int address, double value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
    }
}
