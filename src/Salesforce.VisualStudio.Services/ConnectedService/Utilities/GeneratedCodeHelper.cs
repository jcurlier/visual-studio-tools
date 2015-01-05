using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Shell = Microsoft.VisualStudio.Shell;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    internal static class GeneratedCodeHelper
    {
        private const string CSharpProjectKind = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        private const string VBProjectKind = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";

        private static ITextTemplating TextTemplating
        {
            get { return (ITextTemplating)Shell.Package.GetGlobalService(typeof(STextTemplating)); }
        }

        public static async Task AddGeneratedCodeAsync(
            ConnectedServiceInstanceContext context,
            Project project,
            string templateFileName,
            string outputDirectory,
            Func<ITextTemplatingSessionHost, IEnumerable<ITextTemplatingSession>> getSessions,
            Func<ITextTemplatingSession, string> getArtifactName)
        {
            string usersLanguageFolderName = GeneratedCodeHelper.GetUsersLanguageFolderName(project);
            string templatePath = Path.Combine(
                    RegistryHelper.GetCurrentUsersVisualStudioLocation(),
                    "Templates\\ConnectedServiceTemplates",
                    usersLanguageFolderName,
                    "Salesforce",
                    templateFileName + ".tt");
            bool useCustomTemplate = File.Exists(templatePath);

            if (!useCustomTemplate)
            {
                templatePath = Path.Combine(
                    Path.GetDirectoryName(typeof(SalesforceConnectedServiceHandler).Assembly.Location),
                    "ConnectedService\\Templates",
                    GeneratedCodeHelper.GetLanguageFolderName(project),
                    templateFileName + ".tt");
            }

            ((SalesforceConnectedServiceInstance)context.ServiceInstance).TelemetryHelper.LogGeneratedCodeData(
                templateFileName, usersLanguageFolderName, useCustomTemplate);

            string template = File.ReadAllText(templatePath);
            ITextTemplating textTemplating = GeneratedCodeHelper.TextTemplating;
            ITextTemplatingSessionHost sessionHost = (ITextTemplatingSessionHost)textTemplating;

            foreach (ITextTemplatingSession session in getSessions(sessionHost))
            {
                sessionHost.Session = session;
                string content = textTemplating.ProcessTemplate(templatePath, template);
                string targetPath = Path.Combine(
                    outputDirectory,
                    getArtifactName(session) + "." + GeneratedCodeHelper.GetCodeFileExtension(project));
                await HandlerHelper.AddFileAsync(context, GeneratedCodeHelper.CreateTempFile(content), targetPath);
            }
        }

        private static string GetLanguageFolderName(Project project)
        {
            string folderName;

            if (project.Kind.Equals(GeneratedCodeHelper.CSharpProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                folderName = "CSharp";
            }
            else if (project.Kind.Equals(GeneratedCodeHelper.VBProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                folderName = "VisualBasic";
            }
            else
            {
                throw new NotSupportedException();
            }

            return folderName;
        }

        private static string GetUsersLanguageFolderName(Project project)
        {
            string folderName;

            if (project.Kind.Equals(GeneratedCodeHelper.CSharpProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                folderName = "Visual C#";
            }
            else if (project.Kind.Equals(GeneratedCodeHelper.VBProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                folderName = "Visual Basic";
            }
            else
            {
                throw new NotSupportedException();
            }

            return folderName;
        }

        private static string GetCodeFileExtension(Project project)
        {
            string extension;

            if (project.Kind.Equals(GeneratedCodeHelper.CSharpProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                extension = "cs";
            }
            else if (project.Kind.Equals(GeneratedCodeHelper.VBProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                extension = "vb";
            }
            else
            {
                throw new NotSupportedException();
            }

            return extension;
        }

        private static string CreateTempFile(string contents)
        {
            string tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, contents);

            return tempFileName;
        }
    }
}
