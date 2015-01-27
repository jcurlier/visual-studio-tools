using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.TextTemplating;
using NuGet.VisualStudio;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Templates.CSharp;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService
{
    [Export(typeof(ConnectedServiceHandler))]
    [ExportMetadata(Constants.ProviderId, Constants.ProviderIdValue)]
    [ExportMetadata("AppliesTo", "CSharp")]
    internal class SalesforceConnectedServiceHandler : ConnectedServiceHandler
    {
        [Import]
        internal IVsPackageInstaller PackageInstaller { get; set; }

        public override async Task AddServiceInstanceAsync(ConnectedServiceInstanceContext context, CancellationToken ct)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingConnectedService);

            SalesforceConnectedServiceInstance salesforceInstance = (SalesforceConnectedServiceInstance)context.ServiceInstance;

            try
            {
                Project project = ProjectHelper.GetProjectFromHierarchy(context.ProjectHierarchy);
                salesforceInstance.GeneratedArtifactSuffix = SalesforceConnectedServiceHandler.GetGeneratedArtifactSuffix(
                    context, project, salesforceInstance.RuntimeAuthentication.AuthStrategy);

                await SalesforceConnectedServiceHandler.CreateConnectedAppAsync(context, project, salesforceInstance);
                await SalesforceConnectedServiceHandler.UpdateConfigFileAsync(context, project, salesforceInstance);
                await this.AddNuGetPackagesAsync(context, project);
                await SalesforceConnectedServiceHandler.AddAssemblyReferencesAsync(context, project, salesforceInstance);
                await SalesforceConnectedServiceHandler.AddGeneratedCodeAsync(context, project, salesforceInstance);
                await SalesforceConnectedServiceHandler.PresentGettingStartedAsync(context, salesforceInstance);

                salesforceInstance.TelemetryHelper.TrackHandlerSucceededEvent(salesforceInstance);
                await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddedConnectedService);
            }
            catch (Exception e)
            {
                salesforceInstance.TelemetryHelper.TrackHandlerFailedEvent(salesforceInstance, e);
                await context.Logger.WriteMessageAsync(LoggerMessageCategory.Error, Resources.LogMessage_FailedAddingConnectedService, e);
                throw;
            }
        }

        private static async Task CreateConnectedAppAsync(ConnectedServiceInstanceContext context, Project project, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_CreatingConnectedApp);

            await ConnectedAppHelper.CreateConnectedAppAsync(salesforceInstance, context.Logger, project);
        }

        private static async Task UpdateConfigFileAsync(ConnectedServiceInstanceContext context, Project project, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_UpdatingConfigFile);

            using (EditableConfigHelper configHelper = new EditableConfigHelper(context.ProjectHierarchy))
            {
                foreach (ConfigSetting configSetting in salesforceInstance.RuntimeAuthentication.GetConfigSettings(salesforceInstance.ConnectedAppName))
                {
                    configHelper.SetAppSetting(
                        ConfigurationKeyNames.GetQualifiedKeyName(configSetting.Key, salesforceInstance.GeneratedArtifactSuffix),
                        configSetting.Value == null ? string.Empty : configSetting.Value.ToString(),
                        configSetting.Comment);
                }

                if (salesforceInstance.RuntimeAuthentication.AuthStrategy == AuthenticationStrategy.WebServerFlow)
                {
                    string handlerName = string.Format(CultureInfo.InvariantCulture, Constants.OAuthRedirectHandlerNameFormat, salesforceInstance.GeneratedArtifactSuffix);
                    string qualifiedHandlerTypeName = SalesforceConnectedServiceHandler.GetServiceNamespace(project, salesforceInstance.GeneratedArtifactSuffix)
                        + Type.Delimiter + Constants.OAuthRedirectHandlerTypeName;
                    string redirectUri = ((WebServerFlowInfo)salesforceInstance.RuntimeAuthentication).RedirectUri.ToString();

                    configHelper.RegisterRedirectHandler(handlerName, redirectUri, qualifiedHandlerTypeName);
                }

                configHelper.Save();
            }
        }

        private async Task AddNuGetPackagesAsync(ConnectedServiceInstanceContext context, Project project)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingNuGetPackages);

            // DeveloperForce.Force is the only NuGet package the experience has a direct dependency on, the rest are dependencies of it.
            // If the DeveloperForce.Force version is changed, the versions of its dependencies must also be updated as appropriate.
            this.PackageInstaller.InstallPackagesFromVSExtensionRepository(
                "Salesforce.VisualStudio.Services.59300730-61A5-4111-9A2B-379C3053E8C7",
                false,
                false,
                project,
                new Dictionary<string, string> {
                    { "DeveloperForce.Force", "0.6.4" },
                    { "Microsoft.Bcl", "1.1.9" },
                    { "Microsoft.Bcl.Async", "1.0.168" },
                    { "Microsoft.Bcl.Build", "1.0.14" },
                    { "Microsoft.Net.Http", "2.2.28" },
                    { "Newtonsoft.Json", "6.0.5" },
                });
        }

        private static async Task AddAssemblyReferencesAsync(ConnectedServiceInstanceContext context, Project project, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingAssemblyReferences);

            await SalesforceConnectedServiceHandler.AddAssemblyReferenceAsync(context, "System.Configuration");
            await SalesforceConnectedServiceHandler.AddAssemblyReferenceAsync(context, "System.Web");

            if (salesforceInstance.SelectedObjects.Any())
            {
                await SalesforceConnectedServiceHandler.AddAssemblyReferenceAsync(context, "System.ComponentModel.DataAnnotations");
            }
        }

        private static async Task AddAssemblyReferenceAsync(ConnectedServiceInstanceContext context, string assemblyPath)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingAssemblyReference, assemblyPath);

            HandlerHelper.AddAssemblyReference(context, assemblyPath);
        }

        private static async Task AddGeneratedCodeAsync(ConnectedServiceInstanceContext context, Project project, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_AddingGeneratedCode);

            GeneratedService generatedService = new GeneratedService()
            {
                ServiceNamespace = SalesforceConnectedServiceHandler.GetServiceNamespace(project, salesforceInstance.GeneratedArtifactSuffix),
                ModelsNamespace = SalesforceConnectedServiceHandler.GetModelsNamespace(project, salesforceInstance.GeneratedArtifactSuffix),
                DefaultNamespace = ProjectHelper.GetProjectNamespace(project),
                AuthenticationStrategy = salesforceInstance.RuntimeAuthentication.AuthStrategy,
                ConfigurationKeyNames = new ConfigurationKeyNames(salesforceInstance.GeneratedArtifactSuffix),
            };

            string serviceDirectoryName = SalesforceConnectedServiceHandler.GetServiceDirectoryName(context, salesforceInstance.GeneratedArtifactSuffix);

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

            if (salesforceInstance.SelectedObjects.Any())
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
                    SalesforceConnectedServiceHandler.GetModelsDirectoryName(salesforceInstance.GeneratedArtifactSuffix),
                    (host) => SalesforceConnectedServiceHandler.GetObjectT4Sessions(host, generatedObjects),
                    () => new SalesforceObject(),
                    (session) => ((GeneratedObject)session["generatedObject"]).Model.Name);
            }
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

        private static string GetServiceDirectoryName(ConnectedServiceInstanceContext context, string generatedArtifactSuffix)
        {
            return Path.Combine(
                HandlerHelper.GetServiceArtifactsRootFolder(context),
                SalesforceConnectedServiceHandler.GetServiceInstanceName(generatedArtifactSuffix));
        }

        private static string GetServiceNamespace(Project project, string generatedArtifactSuffix)
        {
            return ProjectHelper.GetProjectNamespace(project)
                + Type.Delimiter
                + SalesforceConnectedServiceHandler.GetServiceInstanceName(generatedArtifactSuffix);
        }

        private static string GetServiceInstanceName(string generatedArtifactSuffix)
        {
            return String.Format(CultureInfo.InvariantCulture, Constants.ServiceInstanceNameFormat, generatedArtifactSuffix);
        }

        private static string GetModelsDirectoryName(string generatedArtifactSuffix)
        {
            return Path.Combine(
                Constants.ModelsName,
                SalesforceConnectedServiceHandler.GetServiceInstanceName(generatedArtifactSuffix));
        }

        private static string GetModelsNamespace(Project project, string generatedArtifactSuffix)
        {
            return ProjectHelper.GetProjectNamespace(project)
                + Type.Delimiter
                + Constants.ModelsName
                + Type.Delimiter
                + SalesforceConnectedServiceHandler.GetServiceInstanceName(generatedArtifactSuffix);
        }

        private static async Task PresentGettingStartedAsync(ConnectedServiceInstanceContext context, SalesforceConnectedServiceInstance salesforceInstance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, Resources.LogMessage_PresentingGettingStarted);

            await HandlerHelper.AddGettingStartedAsync(
                context,
                SalesforceConnectedServiceHandler.GetServiceInstanceName(salesforceInstance.GeneratedArtifactSuffix),
                new Uri(Constants.NextStepsUrl));
        }

        /// <summary>
        /// Returns a suffix for the generated artifacts which guarantees that they don't conflict with any 
        /// existing artifacts in the project.
        /// </summary>
        private static string GetGeneratedArtifactSuffix(ConnectedServiceInstanceContext context, Project project, AuthenticationStrategy authStrategy)
        {
            using (ConfigHelper configHelper = new ConfigHelper(context.ProjectHierarchy))
            {
                return NamingUtilities.GetUniqueSuffix(suffix =>
                    configHelper.IsPrefixUsedInAppSettings(ConfigurationKeyNames.GetQualifiedKeyName(string.Empty, suffix))
                        || (authStrategy == AuthenticationStrategy.WebServerFlow
                            && configHelper.IsHandlerNameUsed(string.Format(CultureInfo.InvariantCulture, Constants.OAuthRedirectHandlerNameFormat, suffix)))
                        || SalesforceConnectedServiceHandler.IsSuffixUsedInGeneratedFilesDirectories(context, suffix, project));
            }
        }

        private static bool IsSuffixUsedInGeneratedFilesDirectories(ConnectedServiceInstanceContext context, string generatedArtifactSuffix, Project project)
        {
            string projectDir = Path.GetDirectoryName(project.FullName);
            Debug.Assert(!String.IsNullOrEmpty(projectDir));  // How can we not have a project path?
            string serviceDirectoryPath = Path.Combine(projectDir, SalesforceConnectedServiceHandler.GetServiceDirectoryName(context, generatedArtifactSuffix));
            string modelsDirectoryPath = Path.Combine(projectDir, SalesforceConnectedServiceHandler.GetModelsDirectoryName(generatedArtifactSuffix));

            return Directory.Exists(serviceDirectoryPath) || Directory.Exists(modelsDirectoryPath);
        }
    }
}
