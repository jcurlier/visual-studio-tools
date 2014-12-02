using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Connected.CredentialStorage;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    [DataContract]
    internal class UserSettings
    {
        private const string CredentialFeatureName = "Salesforce.VisualStudio.Services";
        private const string Name = "Settings";
        private const int MaxMruEntries = 10;

        private IVsCredentialStorageService credentialService;

        public UserSettings()
        {
            this.MruDesignTimeAuthentications = new ObservableCollection<DesignTimeAuthentication>();
            this.MruMyDomains = new ObservableCollection<string>();
        }

        [DataMember]
        public ObservableCollection<string> MruMyDomains { get; private set; }

        // Note:  The storage of the authentication info here is a short term minimal invested solution until
        // the appropriate VS infrastructure is added.
        [DataMember]
        public ObservableCollection<DesignTimeAuthentication> MruDesignTimeAuthentications { get; private set; }

        private IVsCredentialStorageService CredentialService
        {
            get
            {
                if (this.credentialService == null)
                {
                    this.credentialService = (IVsCredentialStorageService)ServiceProvider.GlobalProvider.GetService(typeof(SVsCredentialStorageService));
                }

                return this.credentialService;
            }
        }

        public static UserSettings Load()
        {
            return UserSettingsHelper.Load<UserSettings>(Constants.ProviderIdValue, UserSettings.Name, UserSettings.OnLoaded) ?? new UserSettings();
        }

        private static void OnLoaded(UserSettings userSettings)
        {
            for (int i = userSettings.MruDesignTimeAuthentications.Count() - 1; i >= 0; i--)
            {
                DesignTimeAuthentication authentication = userSettings.MruDesignTimeAuthentications[i];
                if (authentication.Version != DesignTimeAuthentication.CurrentVersion)
                {
                    // Currently if the authentication info cached in the MRU does not match the current version
                    // then it is simply removed.  At anytime this logic could be modified to perform an upgrade
                    // if the necessary requirements exist.
                    userSettings.MruDesignTimeAuthentications.Remove(authentication);
                    continue;
                }

                IVsCredentialKey key = userSettings.GetCredentialKey(authentication);
                IVsCredential credential = userSettings.CredentialService.Retrieve(key);
                authentication.RefreshToken = credential == null ? null : credential.TokenValue;
            }
        }

        public void Save()
        {
            UserSettingsHelper.Save(this, Constants.ProviderIdValue, UserSettings.Name, this.OnSaved);
        }

        private void OnSaved()
        {
            // Instead of attempting to track/detect what tokens have changed, simply remove all and re-add the current ones.
            foreach (IVsCredential credential in this.CredentialService.RetrieveAll(UserSettings.CredentialFeatureName))
            {
                this.credentialService.Remove(credential);
            }
            foreach (DesignTimeAuthentication authentication in this.MruDesignTimeAuthentications.Where(a => a.RefreshToken != null))
            {
                IVsCredentialKey key = this.GetCredentialKey(authentication);
                this.CredentialService.Add(key, authentication.RefreshToken);
            }
        }

        public static void AddToTopOfMruList<T>(ObservableCollection<T> mruList, T item)
        {
            int index = mruList.IndexOf(item);
            if (index > 0)
            {
                // The item is in the MRU list but it is not at the top.
                mruList.Move(index, 0);
            }
            else if (index == -1)
            {
                // The item is not in the MRU list, make room for it by clearing out the oldest item.
                while (mruList.Count >= UserSettings.MaxMruEntries)
                {
                    mruList.RemoveAt(mruList.Count - 1);
                }

                mruList.Insert(0, item);
            }
        }

        private IVsCredentialKey GetCredentialKey(DesignTimeAuthentication authentication)
        {
            return this.CredentialService.CreateCredentialKey(
                UserSettings.CredentialFeatureName,
                authentication.Domain.ToString(),
                authentication.UserName,
                "OAuthRefresh");
        }
    }
}
