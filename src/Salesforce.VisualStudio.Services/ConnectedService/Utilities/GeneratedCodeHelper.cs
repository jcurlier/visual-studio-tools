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
        private static ITextTemplating TextTemplating
        {
            get { return (ITextTemplating)Shell.Package.GetGlobalService(typeof(STextTemplating)); }
        }

        public static async Task AddGeneratedCodeAsync(
            ConnectedServiceInstanceContext context,
            Project project,
            string templateFileName,
            string generatedFilesDirectory,
            Func<ITextTemplatingSessionHost, IEnumerable<ITextTemplatingSession>> getSessions,
            Func<ITextTemplatingSession, string> getArtifactName)
        {
            string templatePath = Path.Combine(
                    Path.GetDirectoryName(typeof(SalesforceConnectedServiceHandler).Assembly.Location),
                    "ConnectedService\\Templates",
                    GeneratedCodeHelper.GetTemplateFolderName(project),
                    templateFileName + ".tt");
            string template = File.ReadAllText(templatePath);
            ITextTemplating textTemplating = GeneratedCodeHelper.TextTemplating;
            ITextTemplatingSessionHost sessionHost = (ITextTemplatingSessionHost)textTemplating;

            foreach (ITextTemplatingSession session in getSessions(sessionHost))
            {
                sessionHost.Session = session;
                string content = textTemplating.ProcessTemplate(templatePath, template);
                string targetPath = Path.Combine(
                    generatedFilesDirectory,
                    getArtifactName(session) + "." + GeneratedCodeHelper.GetCodeFileExtension(project));
                await HandlerHelper.AddFileAsync(context, GeneratedCodeHelper.CreateTempFile(content), targetPath);
            }
        }

        private static string GetTemplateFolderName(Project project)
        {
            string extension;

            if (project.Kind.Equals(Constants.CSharpProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                extension = Constants.CSharpTemplateFolderName;
            }
            else if (project.Kind.Equals(Constants.VBProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                extension = Constants.VBTemplateFolderName;
            }
            else
            {
                throw new NotSupportedException();
            }

            return extension;
        }

        private static string GetCodeFileExtension(Project project)
        {
            string extension;

            if (project.Kind.Equals(Constants.CSharpProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                extension = Constants.CSharpFileExtensions;
            }
            else if (project.Kind.Equals(Constants.VBProjectKind, StringComparison.OrdinalIgnoreCase))
            {
                extension = Constants.VBFileExtensions;
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
