using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Blish_HUD._Utils {
    public static class ProcessUtil {
        #region PInvoke Methods

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern NTSTATUS NtQueryInformationProcess(IntPtr processHandle, PROCESSINFOCLASS processInformationClass, byte[] processInformation, int processInformationLength, out IntPtr returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, out UIntPtr lpNumberOfBytesRead);
        #endregion PInvoke Methods

        #region PInvoke Types
        private enum PROCESSINFOCLASS : uint {
            ProcessBasicInformation = 0x00,
            ProcessCommandLineInformation = 0x3C,
        }

        private enum NTSTATUS : uint {
            STATUS_SUCCESS = 0x00000000,
            STATUS_INFO_LENGTH_MISMATCH = 0xC0000004,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING {
            public ushort length;
            public ushort maximumLength;
            public IntPtr buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION {
            private IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            private IntPtr Reserved2_0;
            private IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            private IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PEB {
            // Some padding
            private IntPtr Padding1_0;
            private IntPtr Padding1_1;
            private IntPtr Padding1_2;
            private IntPtr Padding1_3;
            public IntPtr ProcessParameters;
            // not interested in anything further
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RtlUserProcessParameters {
            public uint MaximumLength;
            public uint Length;
            public uint Flags;
            public uint DebugFlags;
            public IntPtr ConsoleHandle;
            public uint ConsoleFlags;
            public IntPtr StandardInput;
            public IntPtr StandardOutput;
            public IntPtr StandardError;
            public UNICODE_STRING CurrentDirectory;
            public IntPtr CurrentDirectoryHandle;
            public UNICODE_STRING DllPath;
            public UNICODE_STRING ImagePathName;
            public UNICODE_STRING CommandLine;
        }
        #endregion PInvoke Types

        public static string GetCommandLine(this Process process) {
            // Only Winnt
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                return string.Empty;
            }

            Version osVersion = Environment.OSVersion.Version;

            switch (osVersion.Major, osVersion.Minor) {
                case (6, 1):  // win7/2008r2
                case (6, 2):  // win8
                    return GetCommandLineInternalLegacy(process.Handle);

                case (6, 3):  // win8.1
                case (10, 0): // win10 & 11
                    return GetCommandLineInternal(process.Handle);

                default:
                    return string.Empty;
            }
        }

        private static string GetCommandLineInternal(IntPtr hProcess) {
            byte[] buffer = Array.Empty<byte>();

            // Pass 0 for length, required length is returned in bufSz
            NTSTATUS status = NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessCommandLineInformation, buffer, buffer.Length, out IntPtr bufSz);
            if (status != NTSTATUS.STATUS_INFO_LENGTH_MISMATCH) {
                throw new Win32Exception();
            }

            // Allocate a buffer
            buffer = new byte[bufSz.ToInt32()];
            status = NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessCommandLineInformation, buffer, buffer.Length, out _);
            if (status != NTSTATUS.STATUS_SUCCESS) {
                throw new Win32Exception();
            }

            // The returned buffer is a UNICODE_STRING structure - the entire string is contained there.
            int offset = Marshal.SizeOf<UNICODE_STRING>();
            UNICODE_STRING str = MemoryMarshal.Read<UNICODE_STRING>(buffer);
            return Encoding.Unicode.GetString(buffer, offset, str.length);
        }

        private static string GetCommandLineInternalLegacy(IntPtr hProcess) {
            byte[] buffer = new byte[Marshal.SizeOf<PROCESS_BASIC_INFORMATION>()];

            NTSTATUS status = NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessBasicInformation, buffer, buffer.Length, out IntPtr _);
            if (status != NTSTATUS.STATUS_SUCCESS) {
                throw new Win32Exception();
            }

            PROCESS_BASIC_INFORMATION basicInformation = MemoryMarshal.Read<PROCESS_BASIC_INFORMATION>(buffer);

            PEB peb = ReadStructFromProcessMemory<PEB>(hProcess, basicInformation.PebBaseAddress);
            RtlUserProcessParameters userProcessParameters = ReadStructFromProcessMemory<RtlUserProcessParameters>(hProcess, peb.ProcessParameters);

            return ReadUnicodeStringFromProcessMemory(hProcess, userProcessParameters.CommandLine);
        }

        private static string ReadUnicodeStringFromProcessMemory(IntPtr hProcess, in UNICODE_STRING unicodeString) {
            byte[] buffer = new byte[unicodeString.length];
            if (!ReadProcessMemory(hProcess, unicodeString.buffer, buffer, new UIntPtr((uint)buffer.Length), out UIntPtr _)) {
                throw new Win32Exception();
            }

            return Encoding.Unicode.GetString(buffer);
        }

        private static TStruct ReadStructFromProcessMemory<TStruct>(IntPtr hProcess, IntPtr lpBaseAddress)
            where TStruct : struct {

            int size = Marshal.SizeOf<TStruct>();
            byte[] buf = new byte[size];

            if (!ReadProcessMemory(hProcess, lpBaseAddress, buf, new UIntPtr((uint)buf.Length), out UIntPtr _)) {
                throw new Win32Exception();
            }

            return MemoryMarshal.Read<TStruct>(buf);
        }
    }
}
