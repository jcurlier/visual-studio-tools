using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Xml;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// Provides methods for loading and saving user settings from isolated storage.
    /// </summary>
    public static class UserSettingsHelper
    {
        /// <summary>
        /// Saves user settings to isolated storage.  The data is stored with the user's roaming profile.
        /// </summary>
        /// <param name="userSettings">
        /// The user settings to be saved.  A DataContractSerializer is used to store the data, so this object must
        /// specify a System.Runtime.Serialization.DataContractAttribute.
        /// </param>
        /// <param name="providerId">
        /// The ProviderId of the Connected Service Provider invoking this method.
        /// </param>
        /// <param name="name">
        /// A unique name for the user settings being saved.  If an isolated storage file for the current user exists 
        /// for the specified providerId and name, it will be overwritten with the specified userSettings.
        /// </param>
        /// <param name="onSaved">
        /// An optional delegate which, if specified, will be executed immediately after a successful save operation.
        /// </param>
        /// <remarks>
        /// Non-critical exceptions are handled by writing an error message in the output window.
        /// </remarks>
        public static void Save(object userSettings, string providerId, string name, Action onSaved = null)
        {
            if (userSettings == null)
            {
                throw new ArgumentNullException(nameof(userSettings));
            }
            if (providerId == null)
            {
                throw new ArgumentNullException(nameof(providerId));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            string fileName = UserSettingsHelper.GetStorageFileName(providerId, name);

            UserSettingsHelper.ExecuteNoncriticalOperation(
                () =>
                {
                    using (IsolatedStorageFile file = UserSettingsHelper.GetIsolatedStorageFile())
                    {
                        IsolatedStorageFileStream stream = null;
                        try
                        {
                            // note: this overwrites existing settings file if it exists
                            stream = file.OpenFile(fileName, FileMode.Create);
                            using (XmlWriter writer = XmlWriter.Create(stream))
                            {
                                stream = null;

                                DataContractSerializer dcs = new DataContractSerializer(userSettings.GetType());
                                dcs.WriteObject(writer, userSettings);

                                writer.Flush();
                            }
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                    }

                    if (onSaved != null)
                    {
                        onSaved();
                    }
                },
                Resources.UserSettingsHelper_FailedSaving,
                fileName);
        }

        /// <summary>
        /// Loads user settings from isolated storage.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the userSettings to load.  A DataContractSerializer is used to store the data, so this 
        /// type must specify a System.Runtime.Serialization.DataContractAttribute.
        /// </typeparam>
        /// <param name="providerId">
        /// The ProviderId of the Connected Service Provider invoking this method.
        /// </param>
        /// <param name="name">
        /// The name of the user settings to be loaded.
        /// </param>
        /// <param name="onLoaded">
        /// An optional delegate which, if specified, will be executed immediately after a successful load operation.
        /// </param>
        /// <returns>
        /// The specified user settings if they exist; else returns null.
        /// </returns>
        /// <remarks>
        /// Non-critical exceptions are handled by writing an error message in the output window and 
        /// returning null.
        /// </remarks>
        public static T Load<T>(string providerId, string name, Action<T> onLoaded = null) where T : class
        {
            if (providerId == null)
            {
                throw new ArgumentNullException(nameof(providerId));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            string fileName = UserSettingsHelper.GetStorageFileName(providerId, name);
            T result = null;

            UserSettingsHelper.ExecuteNoncriticalOperation(
                () =>
                {
                    using (IsolatedStorageFile file = UserSettingsHelper.GetIsolatedStorageFile())
                    {
                        if (file.FileExists(fileName))
                        {
                            IsolatedStorageFileStream stream = null;
                            try
                            {
                                stream = file.OpenFile(fileName, FileMode.Open);
                                using (XmlReader reader = XmlReader.Create(stream))
                                {
                                    stream = null;

                                    DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                                    result = dcs.ReadObject(reader) as T;
                                }
                            }
                            finally
                            {
                                if (stream != null)
                                {
                                    stream.Dispose();
                                }
                            }

                            if (onLoaded != null && result != null)
                            {
                                onLoaded(result);
                            }
                        }
                    }
                },
                Resources.UserSettingsHelper_FailedLoading,
                fileName);

            return result;
        }

        private static string GetStorageFileName(string providerId, string name)
        {
            return providerId + "_" + name + ".xml";
        }

        private static IsolatedStorageFile GetIsolatedStorageFile()
        {
            return IsolatedStorageFile.GetStore(
                IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming, null, null);
        }

        private static void ExecuteNoncriticalOperation(
            Action operation,
            string failureMessage,
            string failureMessageArg)
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                if (ExceptionHelper.IsCriticalException(ex))
                {
                    throw;
                }

                UserSettingsHelper.WriteOutputWindowMessage(failureMessage, failureMessageArg, ex);
            }
        }

        private static void WriteOutputWindowMessage(string format, params object[] args)
        {
            IVsOutputWindowPane outputPane = ServiceProvider.GlobalProvider.GetService(typeof(SVsGeneralOutputWindowPane)) as IVsOutputWindowPane;
            if (outputPane != null)
            {
                // Write out the message. If it fails, do nothing.
                outputPane.Activate();
                string msg = string.Format(CultureInfo.CurrentCulture, format, args);
                outputPane.OutputStringThreadSafe(msg);
            }
        }
    }
}
