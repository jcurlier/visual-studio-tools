using Microsoft.ApplicationInsights;
using Microsoft.VisualStudio.Shell;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

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

        // Strings for help link clicks
        private const string HelpLinkClickedEvent = "SalesforceConnectedService/LinkClicked";
        private const string HelpLinkUri = "Uri";

        private bool isOptedIn;
        private TelemetryClient telemetryClient;

        public TelemetryHelper()
        {
            isOptedIn = TelemetryHelper.InitializeIsOptedIn();
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
        public void LogInstanceData(ConnectedServiceInstance salesforceInstance)
        {
            this.TrackEvent(TelemetryHelper.WizardFinishedEvent,
                () =>
                {
                    Dictionary<string, string> properties = new Dictionary<string, string>();
                    //Instance id
                    properties.Add(TelemetryHelper.InstanceId, salesforceInstance.InstanceId);

                    // Environment type 
                    if (salesforceInstance.DesignTimeAuthentication != null)
                    {
                        properties.Add(TelemetryHelper.EnvironmentType, salesforceInstance.DesignTimeAuthentication.EnvironmentType.ToString());
                    }

                    // Runtime Authentication
                    properties.Add(TelemetryHelper.RuntimeAuthenticationStrategy, salesforceInstance.RuntimeAuthentication.AuthStrategy.ToString());

                    //Uses custom domain
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
                () =>
                new Dictionary<string, double>()
                {
                    { TelemetryHelper.ObjectAvailableCount, objectSelectionViewModel.GetAvailableObjectCount() },
                    { TelemetryHelper.ObjectSelectedCount, objectSelectionViewModel.GetSelectedObjects().Count() }
                });
        }

        public void LogLinkClickData(string page)
        {
            this.TrackEvent(
                TelemetryHelper.HelpLinkClickedEvent,
                () => new Dictionary<string, string>() { { TelemetryHelper.HelpLinkUri, page } },
                null);
        }
    }
}
