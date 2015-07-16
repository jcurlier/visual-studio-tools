using Microsoft.ApplicationInsights;
using Microsoft.VisualStudio.ConnectedServices;
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
    /// <summary>
    /// A utility class for gathering and logging telemetry data.
    /// </summary>
    internal class TelemetryHelper
    {
        // Event Names
        private const string WizardStartedEventName = "SalesforceConnectedService/WizardStarted";
        private const string WizardFinishedEventName = "SalesforceConnectedService/WizardFinished";
        private const string AddServiceSucceededEventName = "SalesforceConnectedService/AddServiceSucceeded";
        private const string AddServiceFailedEventName = "SalesforceConnectedService/AddServiceFailed";
        private const string UpdateServiceSucceededEventName = "SalesforceConnectedService/UpdateServiceSucceeded";
        private const string UpdateServiceFailedEventName = "SalesforceConnectedService/UpdateServiceFailed";
        private const string CodeGeneratedEventName = "SalesforceConnectedService/CodeGenerated";
        private const string LinkClickedEventName = "SalesforceConnectedService/LinkClicked";

        // Property/Measurement Names
        private const string ProjectTypeDataName = "ProjectType";
        private const string IsUpdatingName = "IsUpdating";
        private const string EnvironmentTypeDataName = "EnvironmentType";
        private const string RuntimeAuthenticationStrategyDataName = "RuntimeAuthenticationStrategy";
        private const string UsesCustomDomainDataName = "UsesCustomDomain";
        private const string ObjectSelectedCountDataName = "SelectedCount";
        private const string ObjectAvailableCountDataName = "AvailableCount";
        private const string GeneratedCodeTemplateDataName = "Template";
        private const string GeneratedCodeUsedCustomTemplateDataName = "UsedCustomTemplate";
        private const string HelpLinkUriDataName = "Uri";
        private const string ExceptionTypeDataName = "ExceptionType";
        private const string ExceptionDetailsDataName = "ExceptionDetails";

        private bool isOptedIn;
        private TelemetryClient telemetryClient;

        public TelemetryHelper(ConnectedServiceProviderContext context)
        {
            this.isOptedIn = TelemetryHelper.InitializeIsOptedIn();

            try
            {
                if (this.isOptedIn)
                {
                    // Add the anonymous user data to the context.
                    string userName = System.Environment.UserName;
                    string fqdnName = TelemetryHelper.GetFQDN();

                    string uniqueUserRaw = string.Join("@", userName, fqdnName).ToLowerInvariant();
                    string safeUserId = TelemetryHelper.GetHashSha256(uniqueUserRaw);

                    string userDnsDomain = TelemetryHelper.GetUserDnsDomain();
                    string safeDomain = TelemetryHelper.GetHashSha256(userDnsDomain);

                    this.TelemetryClient.Context.User.Id = safeUserId;
                    this.TelemetryClient.Context.User.AccountId = safeDomain;

                    // Add the common properties/measurements to the context.
                    this.TelemetryClient.Context.Properties.Add(
                        TelemetryHelper.ProjectTypeDataName, ProjectHelper.GetCapabilities(context.ProjectHierarchy));
                    this.TelemetryClient.Context.Properties.Add(
                        TelemetryHelper.IsUpdatingName, context.IsUpdating.ToString());
                }
            }
            catch (Exception e) // Don't let a telemetry failure take down the provider
            {
                Debug.Fail(e.ToString());
            }
        }

        private TelemetryClient TelemetryClient
        {
            get
            {
                if (this.telemetryClient == null)
                {
                    this.telemetryClient = new TelemetryClient();

                    // Use the "SalesforceConnectedServiceInsights" Application Insights resource in Azure.
                    this.telemetryClient.InstrumentationKey = "f10a0520-d9c9-4105-81ed-713e0ae31074";
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
            catch (Exception e) // Don't let a telemetry failure take down the provider
            {
                Debug.Fail(e.ToString());
            }

            return isOptedIn;
        }

        private void TrackEvent(
            string eventName,
            SalesforceConnectedServiceInstance salesforceInstance,
            Action<Dictionary<string, string>> addProperties,
            Action<Dictionary<string, double>> addMeasurements)
        {
            try
            {
                if (!this.isOptedIn)
                {
                    return;
                }

                Dictionary<string, string> properties = TelemetryHelper.GetProperties(salesforceInstance, addProperties);
                Dictionary<string, double> measurements = TelemetryHelper.GetMeasurements(salesforceInstance, addMeasurements);
                this.TelemetryClient.TrackEvent(eventName, properties, measurements);
            }
            catch (Exception e) // Don't let a telemetry failure take down the provider
            {
                Debug.Fail(e.ToString());
            }
        }

        private static Dictionary<string, string> GetProperties(
            SalesforceConnectedServiceInstance salesforceInstance,
            Action<Dictionary<string, string>> addProperties)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            if (salesforceInstance != null)
            {
                properties.Add(TelemetryHelper.RuntimeAuthenticationStrategyDataName, salesforceInstance.RuntimeAuthentication.AuthStrategy.ToString());
                properties.Add(TelemetryHelper.EnvironmentTypeDataName, salesforceInstance.DesignTimeAuthentication.EnvironmentType.ToString());

                if (salesforceInstance.RuntimeAuthentication is WebServerFlowInfo)
                {
                    properties.Add(TelemetryHelper.UsesCustomDomainDataName, ((WebServerFlowInfo)salesforceInstance.RuntimeAuthentication).HasMyDomain.ToString());
                }
            }

            if (addProperties != null)
            {
                addProperties(properties);
            }

            return properties;
        }

        private static Dictionary<string, double> GetMeasurements(
            SalesforceConnectedServiceInstance salesforceInstance,
            Action<Dictionary<string, double>> addMeasurements)
        {
            Dictionary<string, double> measurements = new Dictionary<string, double>();

            if (salesforceInstance != null)
            {
                measurements.Add(TelemetryHelper.ObjectSelectedCountDataName, salesforceInstance.SelectedObjects.Count());
            }

            if (addMeasurements != null)
            {
                addMeasurements(measurements);
            }

            return measurements;
        }

        public void TrackWizardStartedEvent()
        {
            this.TrackEvent(TelemetryHelper.WizardStartedEventName, null, null, null);
        }

        public void TrackWizardFinishedEvent(SalesforceConnectedServiceInstance salesforceInstance, ObjectSelectionViewModel objectSelectionViewModel)
        {
            this.TrackEvent(
                TelemetryHelper.WizardFinishedEventName,
                salesforceInstance,
                null,
                (measurements) => measurements.Add(TelemetryHelper.ObjectAvailableCountDataName, objectSelectionViewModel.GetAvailableObjectCount()));
        }

        public void TrackAddServiceSucceededEvent(SalesforceConnectedServiceInstance salesforceInstance)
        {
            this.TrackEvent(TelemetryHelper.AddServiceSucceededEventName, salesforceInstance, null, null);
        }

        public void TrackAddServiceFailedEvent(SalesforceConnectedServiceInstance salesforceInstance, Exception e)
        {
            this.TrackFailedEvent(TelemetryHelper.AddServiceFailedEventName, salesforceInstance, e);
        }

        public void TrackUpdateServiceSucceededEvent(SalesforceConnectedServiceInstance salesforceInstance)
        {
            this.TrackEvent(TelemetryHelper.UpdateServiceSucceededEventName, salesforceInstance, null, null);
        }

        public void TrackUpdateServiceFailedEvent(SalesforceConnectedServiceInstance salesforceInstance, Exception e)
        {
            this.TrackFailedEvent(TelemetryHelper.UpdateServiceFailedEventName, salesforceInstance, e);
        }

        public void TrackFailedEvent(string eventName, SalesforceConnectedServiceInstance salesforceInstance, Exception e)
        {
            this.TrackEvent(
                eventName,
                salesforceInstance,
                (properties) =>
                {
                    properties.Add(TelemetryHelper.ExceptionTypeDataName, e.GetType().FullName);
                    properties.Add(TelemetryHelper.ExceptionDetailsDataName, e.ToString());
                },
                null);
        }

        public void TrackCodeGeneratedEvent(SalesforceConnectedServiceInstance salesforceInstance, string templateName, bool usedCustomTemplate)
        {
            this.TrackEvent(
                TelemetryHelper.CodeGeneratedEventName,
                salesforceInstance,
                (properties) =>
                    {
                        properties.Add(TelemetryHelper.GeneratedCodeTemplateDataName, templateName);
                        properties.Add(TelemetryHelper.GeneratedCodeUsedCustomTemplateDataName, usedCustomTemplate.ToString());
                    },
                null);
        }

        public void TrackLinkClickedEvent(string page)
        {
            this.TrackEvent(
                TelemetryHelper.LinkClickedEventName,
                null,
                (properties) => properties.Add(TelemetryHelper.HelpLinkUriDataName, page),
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

                if (!hostName.EndsWith(domainName, StringComparison.OrdinalIgnoreCase))
                {
                    hostName += "." + domainName;
                }
            }
            catch (Exception e) // Don't let a failure prevent the gathering of other telemetry.
            {
                Debug.Fail(e.ToString());
            }

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
            using (SHA256CryptoServiceProvider hashProvider = new SHA256CryptoServiceProvider())
            {
                byte[] hash = hashProvider.ComputeHash(bytes);

                foreach (byte x in hash)
                {
                    hashString += "{0:x2}".FormatInvariantCulture(x);
                }
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
            catch (Exception e) // Don't let a failure prevent the gathering of other telemetry.
            {
                Debug.Fail(e.ToString());
            }

            return returnValue;
        }
    }
}
