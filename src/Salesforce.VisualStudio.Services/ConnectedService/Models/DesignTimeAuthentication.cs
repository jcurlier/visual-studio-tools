using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    [DataContract]
    internal class DesignTimeAuthentication : IEquatable<DesignTimeAuthentication>, INotifyPropertyChanged
    {
        public const int CurrentVersion = 2;
        public const string AccessTokenPropertyName = "AccessToken";
        public const string EnvironmentTypePropertyName = "EnvironmentType";
        private const string IsNewIdentityPropertyName = "IsNewIdentity";
        private const string SummaryPropertyName = "Summary";

        [DataMember]
        private string userName;

        [DataMember]
        private EnvironmentType environmentType;

        [DataMember]
        private Uri myDomain;

        [DataMember]
        private string metadataServiceUrl;

        [DataMember]
        private int version;

        private string accessToken;

        public DesignTimeAuthentication()
        {
            this.version = DesignTimeAuthentication.CurrentVersion;
        }

        public string UserName
        {
            get { return this.userName; }
            set
            {
                this.userName = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(DesignTimeAuthentication.SummaryPropertyName);
                this.RaisePropertyChanged(DesignTimeAuthentication.IsNewIdentityPropertyName);
            }
        }

        public EnvironmentType EnvironmentType
        {
            get { return this.environmentType; }
            set
            {
                this.environmentType = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(DesignTimeAuthentication.SummaryPropertyName);
            }
        }

        public Uri MyDomain
        {
            get { return this.myDomain; }
            set
            {
                this.myDomain = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(DesignTimeAuthentication.SummaryPropertyName);
            }
        }

        public string AccessToken
        {
            get { return this.accessToken; }
            set
            {
                this.accessToken = value;
                this.RaisePropertyChanged();
            }
        }

        public string RefreshToken { get; set; }

        public string MetadataServiceUrl
        {
            get
            {
                // The backing field's value contains a {version} placeholder. The placeholder is only replaced upon read rather than set
                // because this state is cached in the identity MRU.  If this VSIX is rev'd the version it depends on may change.
                return this.metadataServiceUrl == null ? null : this.metadataServiceUrl.Replace("{version}", Constants.SalesforceApiVersion);
            }
            set { this.metadataServiceUrl = value; }
        }

        [DataMember]
        public string InstanceUrl { get; set; }

        public bool IsNewIdentity
        {
            get { return this.UserName == null; }
        }

        public Uri Domain
        {
            get
            {
                switch (this.EnvironmentType)
                {
                    case EnvironmentType.Production:
                        return new Uri(Constants.ProductionDomainUrl);
                    case EnvironmentType.Sandbox:
                        return new Uri(Constants.SandboxDomainUrl);
                    case EnvironmentType.Custom:
                        return this.MyDomain;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public string Summary
        {
            get
            {
                string result;

                if (this.IsNewIdentity)
                {
                    result = Resources.DesignTimeAuthentication_NewIdentityName;
                }
                else
                {
                    switch (this.EnvironmentType)
                    {
                        case EnvironmentType.Production:
                            result = string.Format(CultureInfo.CurrentCulture, Resources.DesignTimeAuthentication_Summary_Production, this.UserName);
                            break;
                        case EnvironmentType.Sandbox:
                            result = string.Format(CultureInfo.CurrentCulture, Resources.DesignTimeAuthentication_Summary_Sandbox, this.UserName);
                            break;
                        case EnvironmentType.Custom:
                            result = string.Format(CultureInfo.CurrentCulture, Resources.DesignTimeAuthentication_Summary_Custom, this.UserName, this.MyDomain);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

                return result;
            }
        }

        public int Version
        {
            get { return this.version; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool Equals(DesignTimeAuthentication other)
        {
            return other != null
                && this.UserName == other.UserName
                && this.EnvironmentType == other.EnvironmentType
                && this.MyDomain == other.MyDomain;
        }

        public override string ToString()
        {
            return this.Summary;
        }
    }
}
