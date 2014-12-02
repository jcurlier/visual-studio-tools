using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using VSLangProj;
using OleInterop = Microsoft.VisualStudio.OLE.Interop;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    internal static class ProjectHelper
    {
        public static bool AddAssemblyReference(Project project, IVsHierarchy projectHierarchy, string assemblyPath, ILogger logger)
        {
            bool result;

            using (ServiceProvider serviceProvider = new ServiceProvider((OleInterop.IServiceProvider)project.DTE))
            {
                Dictionary<uint, bool> virtualFolders = ProjectHelper.GetVirtualFoldersExpanded(projectHierarchy, serviceProvider);

                try
                {
                    if (project.Object != null)
                    {
                        ((VSProject)project.Object).References.Add(assemblyPath);
                    }

                    ProjectHelper.SetVirtualFoldersExpanded(projectHierarchy, serviceProvider, virtualFolders);

                    logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_AddedAssemblyReference, assemblyPath);
                    result = true;
                }
                catch (COMException e)
                {
                    if (e.ErrorCode == -2147166395) // A reference to the component '...' already exists in the project.
                    {
                        logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_DetectedExistingAssemblyReference, assemblyPath);
                        result = true;
                    }
                    else
                    {
                        logger.WriteMessage(LoggerMessageCategory.Information, Resources.LogMessage_FailedAddingAssemblyReference, assemblyPath, e);
                        result = false;
                    }
                }
            }

            return result;
        }

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

        private static Dictionary<uint, bool> GetVirtualFoldersExpanded(IVsHierarchy projectHierarchy, IServiceProvider provider)
        {
            Dictionary<uint, bool> virtualFolders = new Dictionary<uint, bool>();

            Debug.Assert(projectHierarchy != null, "projectHierarchy cannot be null");
            Debug.Assert(provider != null, "provider cannot be null");
            if (projectHierarchy == null || provider == null)
            {
                return virtualFolders;
            }

            IVsUIHierarchyWindow window = VsShellUtilities.GetUIHierarchyWindow(provider, new Guid(EnvDTE.Constants.vsWindowKindSolutionExplorer));

            object item;
            int result = projectHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_FirstChild, out item);
            if (result >= 0)
            {
                uint itemId = (uint)(int)item;

                //Iterate through first level children of 'project' to find all the virtual folders
                while (itemId != VSConstants.VSITEMID_NIL)
                {
                    Guid guid;
                    result = projectHierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out guid);
                    if (result < 0)
                    {
                        break;
                    }

                    if (guid == VSConstants.ItemTypeGuid.VirtualFolder_guid)
                    {
                        uint state;
                        uint stateMask = (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded;
                        result = window.GetItemState(projectHierarchy as IVsUIHierarchy, itemId, stateMask, out state);
                        if (result < 0)
                        {
                            break;
                        }

                        virtualFolders[itemId] = ((__VSHIERARCHYITEMSTATE)state).HasFlag(__VSHIERARCHYITEMSTATE.HIS_Expanded);
                    }

                    result = projectHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_NextSibling, out item);
                    if (result < 0)
                    {
                        break;
                    }

                    itemId = (uint)(int)item;
                }
            }

            return virtualFolders;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsUIHierarchyWindow.ExpandItem(Microsoft.VisualStudio.Shell.Interop.IVsUIHierarchy,System.UInt32,Microsoft.VisualStudio.Shell.Interop.EXPANDFLAGS)")]
        private static void SetVirtualFoldersExpanded(IVsHierarchy projectHierarchy, IServiceProvider provider, Dictionary<uint, bool> virtualFolders)
        {
            Debug.Assert(projectHierarchy != null, "projectHierarchy cannot be null");
            Debug.Assert(provider != null, "provider cannot be null");
            Debug.Assert(virtualFolders != null, "virtualFolders can't be null");
            if (virtualFolders == null || projectHierarchy == null || provider == null)
            {
                return;
            }

            IVsUIHierarchyWindow window = VsShellUtilities.GetUIHierarchyWindow(provider, new Guid(EnvDTE.Constants.vsWindowKindSolutionExplorer));

            foreach (KeyValuePair<uint, bool> keyValue in virtualFolders)
            {
                EXPANDFLAGS action = keyValue.Value ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder;
                window.ExpandItem((IVsUIHierarchy)projectHierarchy, keyValue.Key, action);
            }
        }
    }
}
