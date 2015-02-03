using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// A utility class for retrieving registry settings.
    /// </summary>
    internal static class RegistryHelper
    {
        public static string GetCurrentUsersVisualStudioLocation()
        {
            string location;

            try
            {
                location = RegistryHelper.GetRegistryKeyValue<string>(
                    Registry.CurrentUser,
                    RegistryHelper.GetVSRegistryRoot(),
                    "VisualStudioLocation");
            }
            catch (Exception e)
            {
                if (ExceptionHelper.IsCriticalException(e))
                {
                    throw;
                }

                location = null;
            }

            return location == null ? string.Empty : location;
        }

        private static T GetRegistryKeyValue<T>(RegistryKey key, string subKeyPath, string subkeyName)
        {
            T keyValue = default(T);

            using (RegistryKey regKey = key.OpenSubKey(subKeyPath, RegistryKeyPermissionCheck.Default))
            {
                if (regKey != null)
                {
                    keyValue = (T)regKey.GetValue(subkeyName, default(T));
                }
            }

            return keyValue;
        }

        private static string GetVSRegistryRoot()
        {
            string registryRoot = string.Empty;

            IVsShell vsShell = (IVsShell)Package.GetGlobalService(typeof(SVsShell));
            if (vsShell != null)
            {
                object obj;
                if (ErrorHandler.Succeeded(vsShell.GetProperty((int)__VSSPROPID.VSSPROPID_VirtualRegistryRoot, out obj)))
                {
                    registryRoot = (string)obj;
                }
            }

            return registryRoot;
        }
    }
}
