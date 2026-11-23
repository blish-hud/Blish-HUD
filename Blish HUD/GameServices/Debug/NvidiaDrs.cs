using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Blish_HUD.Debug {
    internal static class NvidiaDrs {

        // Blish HUD is always x64
        private const string NVAPI_DLL = "nvapi64.dll";

        #region NVAPI Constants

        // nvapi_QueryInterface function IDs (from the public NVAPI SDK)
        private const uint ID_NVAPI_INITIALIZE                  = 0x0150E828;
        private const uint ID_NVAPI_UNLOAD                      = 0xD22BDD7E;
        private const uint ID_NVAPI_DRS_CREATESESSION           = 0x0694D52E;
        private const uint ID_NVAPI_DRS_DESTROYSESSION          = 0xDAD9CFF8;
        private const uint ID_NVAPI_DRS_LOADSETTINGS            = 0x375DBD6B;
        private const uint ID_NVAPI_DRS_FINDAPPLICATIONBYNAME   = 0xEEE566B2;
        private const uint ID_NVAPI_DRS_GETCURRENTGLOBALPROFILE = 0x617BFF9F;
        private const uint ID_NVAPI_DRS_GETSETTING              = 0x73BF8338;

        private const int NVAPI_OK                   = 0;
        private const int NVAPI_SETTING_NOT_FOUND    = -160;
        private const int NVAPI_EXECUTABLE_NOT_FOUND = -166;

        private const int NVDRS_DWORD_TYPE = 0;

        // NvAPI_UnicodeString is NvU16[2048]
        private const int UNICODE_STRING_BYTES = 2048 * sizeof(ushort);

        // NVDRS_SETTING_UNION: largest member is NVDRS_BINARY_SETTING (NvU32 valueLength + NvU8[4096])
        private const int SETTING_UNION_BYTES = sizeof(uint) + 4096;

        private const int SETTING_TYPE_OFFSET   = sizeof(uint) + UNICODE_STRING_BYTES + sizeof(uint);
        private const int SETTING_CURRENT_VALUE = sizeof(uint) + UNICODE_STRING_BYTES + 5 * sizeof(uint) + SETTING_UNION_BYTES;
        private const int SETTING_V1_BYTES      = SETTING_CURRENT_VALUE + SETTING_UNION_BYTES; // 12320
        private const uint NVDRS_SETTING_VER    = SETTING_V1_BYTES | (1u << 16);

        private const int APPLICATION_V3_BYTES     = 2 * sizeof(uint) + 4 * UNICODE_STRING_BYTES + sizeof(uint); // 16396
        private const uint NVDRS_APPLICATION_VER_V3 = APPLICATION_V3_BYTES | (3u << 16);

        #endregion

        #region Native

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string lpLibFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr QueryInterfaceFn(uint id);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int StatusFn();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CreateSessionFn(out IntPtr hSession);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SessionFn(IntPtr hSession);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetProfileFn(IntPtr hSession, out IntPtr hProfile);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int FindApplicationByNameFn(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr)] string appName, out IntPtr hProfile, [In, Out] byte[] application);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetSettingFn(IntPtr hSession, IntPtr hProfile, uint settingId, [In, Out] byte[] setting);

        #endregion

        public static Dictionary<uint, uint> GetDwordSettings(string exePath, IEnumerable<uint> settingIds) {
            // Load from System32 and quickly exit on non-NVIDIA systems
            IntPtr hNvapi = LoadLibraryW(Path.Combine(Environment.SystemDirectory, NVAPI_DLL));
            if (hNvapi == IntPtr.Zero) return null;

            IntPtr pQueryInterface = GetProcAddress(hNvapi, "nvapi_QueryInterface");
            if (pQueryInterface == IntPtr.Zero) return null;

            var queryInterface = Marshal.GetDelegateForFunctionPointer<QueryInterfaceFn>(pQueryInterface);

            var initialize = GetFunction<StatusFn>(queryInterface, ID_NVAPI_INITIALIZE);
            if (initialize == null || initialize() != NVAPI_OK) return null;

            var createSession         = GetFunction<CreateSessionFn>(queryInterface, ID_NVAPI_DRS_CREATESESSION);
            var destroySession        = GetFunction<SessionFn>(queryInterface, ID_NVAPI_DRS_DESTROYSESSION);
            var loadSettings          = GetFunction<SessionFn>(queryInterface, ID_NVAPI_DRS_LOADSETTINGS);
            var findApplicationByName = GetFunction<FindApplicationByNameFn>(queryInterface, ID_NVAPI_DRS_FINDAPPLICATIONBYNAME);
            var getGlobalProfile      = GetFunction<GetProfileFn>(queryInterface, ID_NVAPI_DRS_GETCURRENTGLOBALPROFILE);
            var getSetting            = GetFunction<GetSettingFn>(queryInterface, ID_NVAPI_DRS_GETSETTING);

            if (createSession == null || destroySession == null || loadSettings == null
             || findApplicationByName == null || getGlobalProfile == null || getSetting == null
             || createSession(out IntPtr hSession) != NVAPI_OK) {
                return null;
            }

            try {
                if (loadSettings(hSession) != NVAPI_OK) return null;

                // NVAPI returns the profile the driver will apply when the provided executable runs.
                // Apps without a profile get the current global profile.
                var application = new byte[APPLICATION_V3_BYTES];
                WriteUInt32(application, 0, NVDRS_APPLICATION_VER_V3);

                int status = findApplicationByName(hSession, exePath, out IntPtr hProfile, application);

                if (status == NVAPI_EXECUTABLE_NOT_FOUND) {
                    status = getGlobalProfile(hSession, out hProfile);
                }

                if (status != NVAPI_OK || hProfile == IntPtr.Zero) return null;

                var result  = new Dictionary<uint, uint>();
                var setting = new byte[SETTING_V1_BYTES];

                foreach (uint settingId in settingIds) {
                    Array.Clear(setting, 0, setting.Length);
                    WriteUInt32(setting, 0, NVDRS_SETTING_VER);

                    status = getSetting(hSession, hProfile, settingId, setting);

                    if (status == NVAPI_SETTING_NOT_FOUND) continue;
                    if (status != NVAPI_OK) return null;

                    if (BitConverter.ToInt32(setting, SETTING_TYPE_OFFSET) == NVDRS_DWORD_TYPE) {
                        result[settingId] = BitConverter.ToUInt32(setting, SETTING_CURRENT_VALUE);
                    }
                }

                return result;
            } finally {
                destroySession(hSession);
                GetFunction<StatusFn>(queryInterface, ID_NVAPI_UNLOAD)?.Invoke();
            }
        }

        private static T GetFunction<T>(QueryInterfaceFn queryInterface, uint id) where T : Delegate {
            IntPtr pFunction = queryInterface(id);

            return pFunction == IntPtr.Zero
                       ? null
                       : Marshal.GetDelegateForFunctionPointer<T>(pFunction);
        }

        private static void WriteUInt32(byte[] buffer, int offset, uint value) {
            buffer[offset]     = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

    }
}
