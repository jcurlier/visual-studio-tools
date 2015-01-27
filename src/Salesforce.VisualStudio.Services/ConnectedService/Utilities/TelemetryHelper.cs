using Microsoft.ApplicationInsights;
using Microsoft.VisualStudio.Shell;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;


namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    internal class TelemetryHelper
    {
        // Strings for wizard data
        private const string WizardFinishedEvent = "SalesforceConnectedService/Finished";
        private const string InstanceId = "InstanceId";
        private const string EnvironmentType = "EnvironmentType";
        private const string RuntimeAuthenticationStrategy = "RuntimeAuthenticationStrategy";
        private const string UsesCustomDomain = "UsesCustomDomain";

        // Strings for object data
        private const string ObjectInformationEvent = "SalesforceConnectedService/ObjectInformation";
        private const string ObjectSelectedCount = "SelectedCount";
        private const string ObjectAvailableCount = "AvailableCount";

        // Strings for generated code data
        private const string GeneratedCodeEvent = "SalesforceConnectedService/GeneratedCode";
        private const string GeneratedCodeTemplate = "Template";
        private const string GeneratedCodeUsedCustomTemplate = "UsedCustomTemplate";

        // Strings for help link clicks
        private const string HelpLinkClickedEvent = "SalesforceConnectedService/LinkClicked";
        private const string HelpLinkUri = "Uri";

        private bool isOptedIn;
        private TelemetryClient telemetryClient;

        public TelemetryHelper()
        {
            this.isOptedIn = TelemetryHelper.InitializeIsOptedIn();

            try
            {
                if (this.isOptedIn)
                {
                    // attempt to track anonymous user data 
                    string userName = System.Environment.UserName;
                    string fqdnName = TelemetryHelper.GetFQDN();

                    string uniqueUserRaw = string.Join("@", userName, fqdnName).ToLowerInvariant();
                    string safeUserId = TelemetryHelper.GetHashSha256(uniqueUserRaw);

                    string userDnsDomain = TelemetryHelper.GetUserDnsDomain();
                    string safeDomain = TelemetryHelper.GetHashSha256(userDnsDomain);

                    this.TelemetryClient.Context.User.Id = safeUserId;
                    this.TelemetryClient.Context.User.AccountId = safeDomain;
                }
            }
            catch (Exception) { }   // don't let a telemetry failure take down the provider
        }

        private TelemetryClient TelemetryClient
        {
            get
            {
                if (this.telemetryClient == null)
                {
                    this.telemetryClient = new TelemetryClient();
                }
                return this.telemetryClient;
            }
        }

        private static bool InitializeIsOptedIn()
        {
            bool isOptedIn = false;

            try
            {
                var serviceProvider = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
                if (serviceProvider != null)
                {
                    Guid serviceGuid = typeof(SVsLog).GUID;
                    Guid interfaceGuid = typeof(IVsSqmOptinManager).GUID;
                    IntPtr ppvUnk;
                    int hResult = serviceProvider.QueryService(ref serviceGuid, ref interfaceGuid, out ppvUnk);

                    try
                    {
                        if (hResult == 0 && ppvUnk != IntPtr.Zero)
                        {
                            var sqmOptinManager = Marshal.GetObjectForIUnknown(ppvUnk) as IVsSqmOptinManager;
                            if (sqmOptinManager != null)
                            {
                                uint optinStatus, preferenceStatus;
                                sqmOptinManager.GetOptinStatus(out optinStatus, out preferenceStatus);
                                isOptedIn = optinStatus != 0;
                            }
                        }
                    }
                    finally
                    {
                        Marshal.Release(ppvUnk);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
            }
            return isOptedIn;
        }

        private void TrackEvent(string eventName, Func<Dictionary<string, string>> getProperties, Func<Dictionary<string, double>> getMeasurements)
        {
            try
            {
                if (!this.isOptedIn)
                {
                    return;
                }

                Dictionary<string, string> properties = null;
                Dictionary<string, double> measurements = null;

                if (getProperties != null)
                {
                    properties = getProperties();
                }
                if (getMeasurements != null)
                {
                    measurements = getMeasurements();
                }

                this.TelemetryClient.TrackEvent(eventName, properties, measurements);
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
            }
        }

        /// <summary>
        /// Log data gather from user selections in Wizard.
        /// </summary>
        public void LogInstanceData(SalesforceConnectedServiceInstance salesforceInstance)
        {
            this.TrackEvent(
                TelemetryHelper.WizardFinishedEvent,
                () =>
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>();
                    // Instance id
                    properties.Add(TelemetryHelper.InstanceId, salesforceInstance.InstanceId);

                    // Environment type
                    if (salesforceInstance.DesignTimeAuthentication != null)
                    {
                        properties.Add(TelemetryHelper.EnvironmentType, salesforceInstance.DesignTimeAuthentication.EnvironmentType.ToString());
                    }

                    // Runtime Authentication
                    properties.Add(TelemetryHelper.RuntimeAuthenticationStrategy, salesforceInstance.RuntimeAuthentication.AuthStrategy.ToString());

                    // Uses custom domain
                    if (salesforceInstance.RuntimeAuthentication is WebServerFlowInfo)
                    {
                        properties.Add(TelemetryHelper.UsesCustomDomain, ((WebServerFlowInfo)salesforceInstance.RuntimeAuthentication).HasMyDomain.ToString());
                    }

                    return properties;
                },
                null);
        }

        public void LogInstanceObjectData(ObjectSelectionViewModel objectSelectionViewModel)
        {
            this.TrackEvent(
                TelemetryHelper.ObjectInformationEvent,
                null,
                () => new Dictionary<string, double>()
                    {
                        { TelemetryHelper.ObjectAvailableCount, objectSelectionViewModel.GetAvailableObjectCount() },
                        { TelemetryHelper.ObjectSelectedCount, objectSelectionViewModel.GetSelectedObjects().Count() }
                    });
        }

        public void LogGeneratedCodeData(string template, bool usedCustomTemplate)
        {
            this.TrackEvent(
                TelemetryHelper.GeneratedCodeEvent,
                () => new Dictionary<string, string>()
                    {
                        { TelemetryHelper.GeneratedCodeTemplate, template },
                        { TelemetryHelper.GeneratedCodeUsedCustomTemplate, usedCustomTemplate.ToString() }
                    },
                null);
        }

        public void LogLinkClickData(string page)
        {
            this.TrackEvent(
                TelemetryHelper.HelpLinkClickedEvent,
                () => new Dictionary<string, string>() { { TelemetryHelper.HelpLinkUri, page } },
                null);
        }

        /// <summary>
        /// Reliably returns a useful string for the fully qualified domain name of the local host
        /// From http://stackoverflow.com/questions/804700/how-to-find-fqdn-of-local-machine-in-c-net
        /// </summary>
        private static string GetFQDN()
        {
            string hostName = "unknown.fqdn";
            try
            {
                string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                hostName = Dns.GetHostName();

                if (!hostName.EndsWith(domainName))
                {
                    hostName += "." + domainName;
                }
            }
            catch (Exception) { }
            return hostName;
        }

        /// <summary>
        /// Perform a 1 way hash of the user's PII (personally identifiable user information)
        /// We want to be able to track the activity streams of users without being able to determine who they are
        /// Adapted from http://stackoverflow.com/questions/12416249/hashing-a-string-with-sha256
        /// Note that we use SHA256CryptoServiceProvider so this code runs on FIPS-140 enforced machines
        /// </summary>
        private static string GetHashSha256(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            string hashString = string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256CryptoServiceProvider hashProvider = new SHA256CryptoServiceProvider();
            byte[] hash = hashProvider.ComputeHash(bytes);

            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        /// <summary>
        /// Attempt to get the logged in user's DNS Domain
        /// This is to understand organizational usage information
        /// Two users within the same Windows organization domain would have identical UserDnsDomain hashes but different userId hashes
        /// </summary>
        private static string GetUserDnsDomain()
        {
            string returnValue = "(unknown)";
            try
            {
                returnValue = System.Environment.GetEnvironmentVariable("USERDNSDOMAIN");
                if (string.IsNullOrEmpty(returnValue))
                {
                    returnValue = "(not set)";
                }

            }
            catch (Exception) { }
            return returnValue;
        }

    }
}
