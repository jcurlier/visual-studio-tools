using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    internal static class ProjectHelper
    {
        public static Project GetProjectFromHierarchy(IVsHierarchy projectHierarchy)
        {
            object projectObject;
            int result = projectHierarchy.GetProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out projectObject);
            ErrorHandler.ThrowOnFailure(result);
            return (Project)projectObject;
        }

        public static string GetProjectNamespace(Project project)
        {
            return project.Properties.Item("DefaultNamespace").Value.ToString();
        }
    }
}
