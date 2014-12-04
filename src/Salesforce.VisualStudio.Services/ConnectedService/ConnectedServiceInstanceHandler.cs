using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.TextTemplating;
using NuGet.VisualStudio;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
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
    [Export(typeof(IConnectedServiceInstanceHandler))]
    [ExportMetadata(Constants.ProviderId, Constants.ProviderIdValue)]
    [ExportMetadata("AppliesTo", "CSharp | VB")]
    [ExportMetadata(Constants.Version, Constants.VersionValue)]
    internal class ConnectedServiceInstanceHandler : IConnectedServiceInstanceHandler
    {
        [Import]
        internal IVsPackageInstaller PackageInstaller { get; set; }

        public async Task AddServiceInstanceAsync(IConnectedServiceInstanceContext context, CancellationToken ct)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddingConnectedService);

            ConnectedServiceInstance salesforceInstance = (ConnectedServiceInstance)context.ServiceInstance;

            try
            {
                Project project = ProjectHelper.GetProjectFromHierarchy(context.ProjectHierarchy);
                string generatedArtifactSuffix = ConnectedServiceInstanceHandler.GetGeneratedArtifactSuffix(
                    context, project, salesforceInstance.RuntimeAuthentication.AuthStrategy);

                await ConnectedServiceInstanceHandler.CreateConnectedApp(context, project, salesforceInstance);
                ConnectedServiceInstanceHandler.UpdateConfigFile(context, project, salesforceInstance, generatedArtifactSuffix);
                this.AddNuGetPackages(context, project);
                ConnectedServiceInstanceHandler.AddAssemblyReferences(context, project, salesforceInstance);
                await ConnectedServiceInstanceHandler.AddGeneratedCode(context, project, salesforceInstance, generatedArtifactSuffix);
                await ConnectedServiceInstanceHandler.PresentGettingStarted(context, generatedArtifactSuffix);

                context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddedConnectedService);
            }
            catch (Exception e)
            {
                context.Logger.WriteMessage(LoggerMessageCategory.Error, Resources.LogMessage_FailedAddingConnectedService, e);
                throw;
            }

            salesforceInstance.TelemetryHelper.LogInstanceData(salesforceInstance);
        }

        private static async Task CreateConnectedApp(IConnectedServiceInstanceContext context, Project project, ConnectedServiceInstance salesforceInstance)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_CreatingConnectedApp);

            await ConnectedAppHelper.CreateConnectedApp(salesforceInstance, context.Logger, project);
        }

        private static void UpdateConfigFile(IConnectedServiceInstanceContext context, Project project, ConnectedServiceInstance salesforceInstance, string generatedArtifactSuffix)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_UpdatingConfigFile);

            using (EditableConfigHelper configHelper = new EditableConfigHelper(context.ProjectHierarchy))
            {
                foreach (ConfigSetting configSetting in salesforceInstance.RuntimeAuthentication.GetConfigSettings(salesforceInstance.ConnectedAppName))
                {
                    configHelper.SetAppSetting(
                        ConfigurationKeyNames.GetQualifiedKeyName(configSetting.Key, generatedArtifactSuffix),
                        configSetting.Value == null ? string.Empty : configSetting.Value.ToString(),
                        configSetting.Comment);
                }

                if (salesforceInstance.RuntimeAuthentication.AuthStrategy == AuthenticationStrategy.WebServerFlow)
                {
                    string handlerTypeName = Constants.OAuthRedirectHandlerName;
                    string handlerName = handlerTypeName + generatedArtifactSuffix;
                    string qualifiedHandlerTypeName =
                        ConnectedServiceInstanceHandler.GetServiceNamespace(project, generatedArtifactSuffix) + Type.Delimiter + handlerTypeName;
                    string redirectUri = ((WebServerFlowInfo)salesforceInstance.RuntimeAuthentication).RedirectUri.ToString();

                    configHelper.RegisterRedirectHandler(handlerName, redirectUri, qualifiedHandlerTypeName);
                }

                configHelper.Save();
            }
        }

        private void AddNuGetPackages(IConnectedServiceInstanceContext context, Project project)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddingNuGetPackages);

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

        private static void AddAssemblyReferences(IConnectedServiceInstanceContext context, Project project, ConnectedServiceInstance salesforceInstance)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddingAssemblyReferences);

            ConnectedServiceInstanceHandler.AddAssemblyReference(context, "System.Configuration");
            ConnectedServiceInstanceHandler.AddAssemblyReference(context, "System.Web");

            if (salesforceInstance.SelectedObjects.Any())
            {
                ConnectedServiceInstanceHandler.AddAssemblyReference(context, "System.ComponentModel.DataAnnotations");
            }
        }

        private static void AddAssemblyReference(IConnectedServiceInstanceContext context, string assemblyPath)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddingAssemblyReference, assemblyPath);

            HandlerHelper.AddAssemblyReference(context, assemblyPath);
        }

        private static async Task AddGeneratedCode(IConnectedServiceInstanceContext context, Project project, ConnectedServiceInstance salesforceInstance, string generatedArtifactSuffix)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddingGeneratedCode);

            GeneratedService generatedService = new GeneratedService()
            {
                ServiceNamespace = ConnectedServiceInstanceHandler.GetServiceNamespace(project, generatedArtifactSuffix),
                ModelsNamespace = ConnectedServiceInstanceHandler.GetModelsNamespace(project, generatedArtifactSuffix),
                DefaultNamespace = ProjectHelper.GetProjectNamespace(project),
                AuthenticationStrategy = salesforceInstance.RuntimeAuthentication.AuthStrategy,
                ConfigurationKeyNames = new ConfigurationKeyNames(generatedArtifactSuffix),
            };

            string serviceDirectoryName = ConnectedServiceInstanceHandler.GetServiceDirectoryName(context, generatedArtifactSuffix);

            await GeneratedCodeHelper.AddGeneratedCode(
                context,
                project,
                "SalesforceService",
                serviceDirectoryName,
                (host) => ConnectedServiceInstanceHandler.GetServiceT4Sessions(host, generatedService),
                (session) => "SalesforceService");

            if (salesforceInstance.RuntimeAuthentication.AuthStrategy == AuthenticationStrategy.WebServerFlow)
            {
                await GeneratedCodeHelper.AddGeneratedCode(
                    context,
                    project,
                    Constants.OAuthRedirectHandlerName,
                    serviceDirectoryName,
                    (host) => ConnectedServiceInstanceHandler.GetServiceT4Sessions(host, generatedService),
                    (session) => Constants.OAuthRedirectHandlerName);
            }

            if (salesforceInstance.SelectedObjects.Any())
            {
                context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_BuildingObjectModel);
                IEnumerable<GeneratedObject> generatedObjects = await CodeModelBuilder.BuildObjectModel(
                    salesforceInstance.SelectedObjects,
                    salesforceInstance.DesignTimeAuthentication,
                    generatedService,
                    context.Logger);

                context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddingGeneratedCodeForObjects, generatedObjects.Count());
                await GeneratedCodeHelper.AddGeneratedCode(
                    context,
                    project,
                    "SalesforceObject",
                    ConnectedServiceInstanceHandler.GetModelsDirectoryName(generatedArtifactSuffix),
                    (host) => ConnectedServiceInstanceHandler.GetObjectT4Sessions(host, generatedObjects),
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

        private static string GetServiceDirectoryName(IConnectedServiceInstanceContext context, string generatedArtifactSuffix)
        {
            return Path.Combine(
                HandlerHelper.GetServiceArtifactsRootFolder(context),
                ConnectedServiceInstanceHandler.GetServiceInstanceName(generatedArtifactSuffix));
        }

        private static string GetServiceNamespace(Project project, string generatedArtifactSuffix)
        {
            return ProjectHelper.GetProjectNamespace(project)
                + Type.Delimiter
                + ConnectedServiceInstanceHandler.GetServiceInstanceName(generatedArtifactSuffix);
        }

        private static string GetServiceInstanceName(string generatedArtifactSuffix)
        {
            return String.Format(CultureInfo.InvariantCulture, Constants.ServiceInstanceNameFormat, generatedArtifactSuffix);
        }

        private static string GetModelsDirectoryName(string generatedArtifactSuffix)
        {
            return Path.Combine(
                Constants.ModelsName,
                ConnectedServiceInstanceHandler.GetServiceInstanceName(generatedArtifactSuffix));
        }

        private static string GetModelsNamespace(Project project, string generatedArtifactSuffix)
        {
            return ProjectHelper.GetProjectNamespace(project)
                + Type.Delimiter
                + Constants.ModelsName
                + Type.Delimiter
                + ConnectedServiceInstanceHandler.GetServiceInstanceName(generatedArtifactSuffix);
        }

        private static async Task PresentGettingStarted(IConnectedServiceInstanceContext context, string generatedArtifactSuffix)
        {
            context.Logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_PresentingGettingStarted);

            await HandlerHelper.AddGettingStartedAsync(
                context,
                ConnectedServiceInstanceHandler.GetServiceInstanceName(generatedArtifactSuffix),
                new Uri(Constants.NextStepsUrl));
        }

        /// <summary>
        /// Returns a suffix for the generated artifacts which guarantees that they don't conflict with any 
        /// existing artifacts in the project.
        /// </summary>
        private static string GetGeneratedArtifactSuffix(IConnectedServiceInstanceContext context, Project project, AuthenticationStrategy authStrategy)
        {
            using (ConfigHelper configHelper = new ConfigHelper(context.ProjectHierarchy))
            {
                return NamingUtilities.GetUniqueSuffix(suffix =>
                    configHelper.IsPrefixUsedInAppSettings(ConfigurationKeyNames.GetQualifiedKeyName(string.Empty, suffix))
                        || authStrategy == AuthenticationStrategy.WebServerFlow && configHelper.IsHandlerNameUsed(Constants.OAuthRedirectHandlerName + suffix)
                        || ConnectedServiceInstanceHandler.IsSuffixUsedInGeneratedFilesDirectories(context, suffix, project));
            }
        }

        private static bool IsSuffixUsedInGeneratedFilesDirectories(IConnectedServiceInstanceContext context, string generatedArtifactSuffix, Project project)
        {
            string projectDir = Path.GetDirectoryName(project.FullName);
            Debug.Assert(!String.IsNullOrEmpty(projectDir));  // How can we not have a project path?
            string serviceDirectoryPath = Path.Combine(projectDir, ConnectedServiceInstanceHandler.GetServiceDirectoryName(context, generatedArtifactSuffix));
            string modelsDirectoryPath = Path.Combine(projectDir, ConnectedServiceInstanceHandler.GetModelsDirectoryName(generatedArtifactSuffix));

            return Directory.Exists(serviceDirectoryPath) || Directory.Exists(modelsDirectoryPath);
        }
    }
}
