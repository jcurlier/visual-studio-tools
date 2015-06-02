using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.Threading;
using NuGet.VisualStudio;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Templates.CSharp;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Shell = Microsoft.VisualStudio.Shell;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    /// <summary>
    /// A ConnectedServiceHandler that is responsible for configuring a project (adding NuGet packages, assembly references,
    /// generated code, config settings, etc.) so that it can be used to connect to a Salesforce service.  The handler also
    /// creates a Connected App within Salesforce for this project.
    /// </summary>
    // Support the following C# projects - Console, WinForms, WPF, Class Libs, ASP.net (pre 5), Windows Services
    // Exclude the following C# projects - Windows Store, Windows Phone, Universal, Shared Class Libs, ASP.net 5, Silverlight
    [ConnectedServiceHandlerExport(
        Constants.ProviderId,
        AppliesTo = "CSharp + !WindowsAppContainer + !WindowsPhone + !SharedAssetsProject + !MultiTarget + !ProjectK",
        SupportedProjectTypes = "!A1591282-1198-4647-A2B1-27E5FF5F6F3B" /* Excluding Silverlight */)]
    internal class SalesforceConnectedServiceHandler : ConnectedServiceHandler
    {
        // DeveloperForce.Force is the only NuGet package the experience has a direct dependency on, the rest are dependencies it has.
        // If the DeveloperForce.Force version is changed, the versions of its dependencies must also be updated as appropriate.
        private static Tuple<string, Version>[] requiredPackages = new Tuple<string, Version>[]
        {
            Tuple.Create("Newtonsoft.Json", new Version("6.0.5")),
            Tuple.Create("Microsoft.Net.Http", new Version("2.2.29")),
            Tuple.Create("Microsoft.Bcl.Build", new Version("1.0.21")),
            Tuple.Create("Microsoft.Bcl.Async", new Version("1.0.168")),
            Tuple.Create("Microsoft.Bcl", new Version("1.1.10")),
            Tuple.Create("DeveloperForce.Force", new Version("1.0.0")),
        };

        [Import]
        internal IVsPackageInstaller PackageInstaller { get; set; }

        [Import]
        internal IVsPackageInstallerServices PackageInstallerServices { get; set; }

        public override async Task<AddServiceInstanceResult> AddServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken ct)
        {
            SalesforceConnectedServiceInstance salesforceInstance = (SalesforceConnectedServiceInstance)context.ServiceInstance;

            try
            {
                Project project = ProjectHelper.GetProjectFromHierarchy(context.ProjectHierarchy);
                string generatedArtifactSuffix = SalesforceConnectedServiceHandler.GetGeneratedArtifactSuffix(
                    context, project, salesforceInstance.RuntimeAuthentication.AuthStrategy);
                salesforceInstance.DesignerData.ServiceName = SalesforceConnectedServiceHandler.GetServiceInstanceName(generatedArtifactSuffix);

                await TaskScheduler.Default; // Switch to a worker thread to avoid blocking the UI thread (e.g. the progress dialog).

                await SalesforceConnectedServiceHandler.CreateConnectedAppAsync(context, project, salesforceInstance);
                await SalesforceConnectedServiceHandler.UpdateConfigFileAsync(context, project, salesforceInstance);
                await this.AddNuGetPackagesAsync(context, project);
                await SalesforceConnectedServiceHandler.AddAssemblyReferencesAsync(context, salesforceInstance);
                await SalesforceConnectedServiceHandler.AddGeneratedCodeAsync(context, project, salesforceInstance);

                salesforceInstance.DesignerData.StoreExtendedDesignerData(context);

                salesforceInstance.TelemetryHelper.TrackAddServiceSucceededEvent(salesforceInstance);

                return new AddServiceInstanceResult(
                    salesforceInstance.DesignerData.ServiceName,
                    new Uri(Constants.NextStepsUrl));
            }
            catch (Exception e)
            {
                salesforceInstance.TelemetryHelper.TrackAddServiceFailedEvent(salesforceInstance, e);
                throw;
            }
        }

        public override async Task<UpdateServiceInstanceResult> UpdateServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken ct)
        {
            SalesforceConnectedServiceInstance salesforceInstance = (SalesforceConnectedServiceInstance)context.ServiceInstance;

            try
            {
                Project project = ProjectHelper.GetProjectFromHierarchy(context.ProjectHierarchy);

                await TaskScheduler.Default; // Switch to a worker thread to avoid blocking the UI thread (e.g. the progress dialog).

                // Update currently only supports adding additional scaffolded objects.  Before they are added, ensure the correct NuGets are
                // installed.  Additionally, ensure the required assemblies exist as not all are added initially if no objects are scaffolded.
                await this.AddNuGetPackagesAsync(context, project);
                await SalesforceConnectedServiceHandler.AddAssemblyReferencesAsync(context, salesforceInstance);

                try
                {
                    await SalesforceConnectedServiceHandler.AddGeneratedCodeAsync(context, project, salesforceInstance);
                }
                catch (COMException comException)
                {
                    if (comException.HResult == -2147467259)
                    {
                        // Provide a better exception message for when an invalid path ModelsHintPath was specified.
                        throw new InvalidOperationException(
                            Resources.LogMessage_InvalidModelsHintPath.FormatCurrentCulture(salesforceInstance.DesignerData.ModelsHintPath),
                            comException);
                    }
                }

                salesforceInstance.DesignerData.StoreExtendedDesignerData(context);

                salesforceInstance.TelemetryHelper.TrackUpdateServiceSucceededEvent(salesforceInstance);

                return new UpdateServiceInstanceResult();
            }
            catch (Exception e)
            {
                salesforceInstance.TelemetryHelper.TrackUpdateServiceFailedEvent(salesforceInstance, e);
                throw;
            }
        }

        private static async Task CreateConnectedAppAsync(ConnectedServiceHandlerContext context, Project project, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_CreatingConnectedApp);

            await ConnectedAppHelper.CreateConnectedAppAsync(salesforceInstance, context.Logger, project);
        }

        private static async Task UpdateConfigFileAsync(ConnectedServiceHandlerContext context, Project project, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); // The EditableConfigHelper must run on the UI thread.

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_UpdatingConfigFile);

            using (EditableXmlConfigHelper configHelper = context.CreateEditableXmlConfigHelper())
            {
                foreach (ConfigSetting configSetting in salesforceInstance.RuntimeAuthentication.GetConfigSettings(salesforceInstance.ConnectedAppName))
                {
                    configHelper.SetAppSetting(
                        ConfigurationKeyNames.GetQualifiedKeyName(configSetting.Key, salesforceInstance.DesignerData.ServiceName),
                        configSetting.Value == null ? string.Empty : configSetting.Value.ToString(),
                        configSetting.Comment);
                }

                if (salesforceInstance.RuntimeAuthentication.AuthStrategy == AuthenticationStrategy.WebServerFlow)
                {
                    string handlerName = Constants.OAuthRedirectHandlerNameFormat.FormatInvariantCulture(salesforceInstance.DesignerData.ServiceName);
                    string qualifiedHandlerTypeName = SalesforceConnectedServiceHandler.GetServiceNamespace(project, salesforceInstance.DesignerData.ServiceName)
                        + Type.Delimiter + Constants.OAuthRedirectHandlerTypeName;
                    string redirectUri = ((WebServerFlowInfo)salesforceInstance.RuntimeAuthentication).RedirectUri.ToString();

                    configHelper.RegisterRedirectHandler(handlerName, redirectUri, qualifiedHandlerTypeName);
                }

                configHelper.Save();
            }
        }

        private async Task AddNuGetPackagesAsync(ConnectedServiceHandlerContext context, Project project)
        {
            IEnumerable<IVsPackageMetadata> installedPackages = this.PackageInstallerServices.GetInstalledPackages();
            Dictionary<string, string> packagesToInstall = new Dictionary<string, string>();

            foreach (Tuple<string, Version> requiredPackage in SalesforceConnectedServiceHandler.requiredPackages)
            {
                IVsPackageMetadata installedPackage = installedPackages.FirstOrDefault(p => p.Id == requiredPackage.Item1);
                if (installedPackage == null)
                {
                    // The package does not exist - notify and install the package.
                    await context.Logger.WriteMessageAsync(
                        LoggerMessageCategory.Information,
                        Resources.LogMessage_AddingNuGetPackage,
                        requiredPackage.Item1,
                        requiredPackage.Item2.ToString());
                }
                else
                {
                    Version installedVersion = SalesforceConnectedServiceHandler.GetNuGetPackageVersion(installedPackage);
                    if (installedVersion == null)
                    {
                        // Unable to parse the version - continue.
                        continue;
                    }
                    else if (installedVersion.Major < requiredPackage.Item2.Major)
                    {
                        // An older potentially non-compatible version of the package already exists - warn and upgrade the package.
                        await context.Logger.WriteMessageAsync(
                            LoggerMessageCategory.Warning,
                            Resources.LogMessage_OlderMajorVersionNuGetPackageExists,
                            requiredPackage.Item1,
                            installedPackage.VersionString,
                            requiredPackage.Item2.ToString());
                    }
                    else if (installedVersion.Major > requiredPackage.Item2.Major)
                    {
                        // A newer potentially non-compatible version of the package already exists - warn and continue.
                        await context.Logger.WriteMessageAsync(
                            LoggerMessageCategory.Warning,
                            Resources.LogMessage_NewerMajorVersionNuGetPackageExists,
                            requiredPackage.Item1,
                            requiredPackage.Item2.ToString(),
                            installedPackage.VersionString);

                        continue;
                    }
                    else if (installedVersion >= requiredPackage.Item2)
                    {
                        // A newer semantically compatible version of the package already exists - continue.
                        continue;
                    }
                    else
                    {
                        // An older semantically compatible version of the package exists - notify and upgrade the package.
                        await context.Logger.WriteMessageAsync(
                            LoggerMessageCategory.Information,
                            Resources.LogMessage_UpgradingNuGetPackage,
                            requiredPackage.Item1,
                            installedPackage.VersionString,
                            requiredPackage.Item2.ToString());
                    }
                }

                packagesToInstall.Add(requiredPackage.Item1, requiredPackage.Item2.ToString());
            }

            if (packagesToInstall.Any())
            {
                this.PackageInstaller.InstallPackagesFromVSExtensionRepository(
                    "Salesforce.VisualStudio.Services.55451A07-E6E0-47CC-855D-8A25B0B59409",
                    false,
                    false,
                    project,
                    packagesToInstall);
            }
        }

        private static Version GetNuGetPackageVersion(IVsPackageMetadata package)
        {
            Version version;
            string versionString = package.VersionString;
            int dashIndex = versionString.IndexOf('-');
            if (dashIndex != -1)
            {
                // Trim off any pre-release versions.  Because the handler should never install pre-release
                // versions they can be ignored when comparing versions.
                versionString = versionString.Substring(0, dashIndex);
            }

            if (!Version.TryParse(versionString, out version))
            {
                Debug.Fail("Unable to parse the NuGet package version " + versionString);
            }

            return version;
        }

        private static async Task AddAssemblyReferencesAsync(ConnectedServiceHandlerContext context, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingAssemblyReferences);

            await SalesforceConnectedServiceHandler.AddAssemblyReferenceAsync(context, "System.Configuration");
            await SalesforceConnectedServiceHandler.AddAssemblyReferenceAsync(context, "System.Web");

            if (salesforceInstance.SelectedObjects.Any())
            {
                await SalesforceConnectedServiceHandler.AddAssemblyReferenceAsync(context, "System.ComponentModel.DataAnnotations");
            }
        }

        private static async Task AddAssemblyReferenceAsync(ConnectedServiceHandlerContext context, string assemblyPath)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingAssemblyReference, assemblyPath);

            context.HandlerHelper.AddAssemblyReference(assemblyPath);
        }

        private static async Task AddGeneratedCodeAsync(ConnectedServiceHandlerContext context, Project project, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingGeneratedCode);

            string modelsHintPath = salesforceInstance.DesignerData.GetDefaultedModelsHintPath();
            GeneratedService generatedService = new GeneratedService()
            {
                ServiceNamespace = SalesforceConnectedServiceHandler.GetServiceNamespace(project, salesforceInstance.DesignerData.ServiceName),
                ModelsNamespace = SalesforceConnectedServiceHandler.GetModelsNamespace(project, modelsHintPath),
                DefaultNamespace = ProjectHelper.GetProjectNamespace(project),
                AuthenticationStrategy = salesforceInstance.RuntimeAuthentication.AuthStrategy,
                ConfigurationKeyNames = new ConfigurationKeyNames(salesforceInstance.DesignerData.ServiceName),
            };

            if (!context.IsUpdating)
            {
                await SalesforceConnectedServiceHandler.AddGeneratedServiceCodeAsync(context, project, salesforceInstance, generatedService);
            }

            if (salesforceInstance.SelectedObjects.Any())
            {
                // Only set the ModelsHintPath in the case where objects are generated so that it is only added to
                // the ConnectedService.json when it is applicable.
                salesforceInstance.DesignerData.ModelsHintPath = modelsHintPath;

                await SalesforceConnectedServiceHandler.AddGeneratedObjectCodeAsync(context, project, salesforceInstance, generatedService, modelsHintPath);
            }
        }

        private static async Task AddGeneratedServiceCodeAsync(
            ConnectedServiceHandlerContext context,
            Project project,
            SalesforceConnectedServiceInstance salesforceInstance,
            GeneratedService generatedService)
        {
            string serviceDirectoryName = SalesforceConnectedServiceHandler.GetServiceDirectoryName(context, salesforceInstance.DesignerData.ServiceName);

            await GeneratedCodeHelper.AddGeneratedCodeAsync(
                context,
                project,
                "SalesforceService",
                serviceDirectoryName,
                (host) => SalesforceConnectedServiceHandler.GetServiceT4Sessions(host, generatedService),
                () => new SalesforceService(),
                (session) => "SalesforceService");

            if (salesforceInstance.RuntimeAuthentication.AuthStrategy == AuthenticationStrategy.WebServerFlow)
            {
                await GeneratedCodeHelper.AddGeneratedCodeAsync(
                    context,
                    project,
                    Constants.OAuthRedirectHandlerTypeName,
                    serviceDirectoryName,
                    (host) => SalesforceConnectedServiceHandler.GetServiceT4Sessions(host, generatedService),
                    () => new SalesforceOAuthRedirectHandler(),
                    (session) => Constants.OAuthRedirectHandlerTypeName);
            }
        }

        private static async Task AddGeneratedObjectCodeAsync(
            ConnectedServiceHandlerContext context,
            Project project,
            SalesforceConnectedServiceInstance salesforceInstance,
            GeneratedService generatedService,
            string modelsHintPath)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_BuildingObjectModel);
            IEnumerable<GeneratedObject> generatedObjects = await CodeModelBuilder.BuildObjectModelAsync(
                salesforceInstance.SelectedObjects,
                salesforceInstance.DesignTimeAuthentication,
                generatedService,
                context.Logger);

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingGeneratedCodeForObjects, generatedObjects.Count());
            await GeneratedCodeHelper.AddGeneratedCodeAsync(
                context,
                project,
                "SalesforceObject",
                modelsHintPath,
                (host) => SalesforceConnectedServiceHandler.GetObjectT4Sessions(host, generatedObjects),
                () => new SalesforceObject(),
                (session) => ((GeneratedObject)session["generatedObject"]).Model.Name);
        }

        private static IEnumerable<ITextTemplatingSession> GetServiceT4Sessions(ITextTemplatingSessionHost host, GeneratedService generatedService)
        {
            ITextTemplatingSession session = host.CreateSession();
            session["generatedService"] = generatedService;

            yield return session;
        }

        private static IEnumerable<ITextTemplatingSession> GetObjectT4Sessions(ITextTemplatingSessionHost host, IEnumerable<GeneratedObject> generatedObjects)
        {
            foreach (GeneratedObject generatedObject in generatedObjects)
            {
                ITextTemplatingSession session = host.CreateSession();
                session["generatedObject"] = generatedObject;

                yield return session;
            }
        }

        private static string GetServiceDirectoryName(ConnectedServiceHandlerContext context, string serviceName)
        {
            return Path.Combine(
                context.HandlerHelper.GetServiceArtifactsRootFolder(),
                serviceName);
        }

        private static string GetServiceNamespace(Project project, string serviceName)
        {
            return ProjectHelper.GetProjectNamespace(project) + Type.Delimiter + serviceName;
        }

        private static string GetServiceInstanceName(string generatedArtifactSuffix)
        {
            return Constants.ServiceInstanceNameFormat.FormatInvariantCulture(generatedArtifactSuffix);
        }

        public static string GetModelsDirectoryName(string serviceName)
        {
            return Path.Combine(Constants.ModelsName, serviceName);
        }

        private static string GetModelsNamespace(Project project, string modelsFolder)
        {
            return ProjectHelper.GetProjectNamespace(project)
                + Type.Delimiter
                + modelsFolder.Replace(Path.DirectorySeparatorChar, Type.Delimiter);
        }

        /// <summary>
        /// Returns a suffix for the generated artifacts which guarantees that they don't conflict with any 
        /// existing artifacts in the project.
        /// </summary>
        private static string GetGeneratedArtifactSuffix(ConnectedServiceHandlerContext context, Project project, AuthenticationStrategy authStrategy)
        {
            using (XmlConfigHelper configHelper = context.CreateReadOnlyXmlConfigHelper())
            {
                return GeneralUtilities.GetUniqueSuffix(suffix =>
                {
                    string serviceName = SalesforceConnectedServiceHandler.GetServiceInstanceName(suffix);
                    return configHelper.IsPrefixUsedInAppSettings(serviceName)
                        || (authStrategy == AuthenticationStrategy.WebServerFlow
                            && configHelper.IsHandlerNameUsed(Constants.OAuthRedirectHandlerNameFormat.FormatInvariantCulture(serviceName)))
                        || SalesforceConnectedServiceHandler.IsSuffixUsedInGeneratedFilesDirectories(context, serviceName, project);
                });
            }
        }

        private static bool IsSuffixUsedInGeneratedFilesDirectories(ConnectedServiceHandlerContext context, string serviceName, Project project)
        {
            string projectDir = Path.GetDirectoryName(project.FullName);
            Debug.Assert(!String.IsNullOrEmpty(projectDir));  // How can we not have a project path?
            string serviceDirectoryPath = Path.Combine(projectDir, SalesforceConnectedServiceHandler.GetServiceDirectoryName(context, serviceName));
            string modelsDirectoryPath = Path.Combine(projectDir, SalesforceConnectedServiceHandler.GetModelsDirectoryName(serviceName));

            return Directory.Exists(serviceDirectoryPath) || Directory.Exists(modelsDirectoryPath);
        }
    }
}
