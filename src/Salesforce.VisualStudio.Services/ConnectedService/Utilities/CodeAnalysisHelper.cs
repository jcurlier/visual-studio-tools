using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    /// <summary>
    /// A helper class for analyzing what code exists within the project.
    /// </summary>
    internal static class CodeAnalysisHelper
    {
        public static Project GetProject(IVsHierarchy projectHierarchy, VisualStudioWorkspace visualStudioWorkspace)
        {
            return visualStudioWorkspace.CurrentSolution.Projects
                .FirstOrDefault(p => projectHierarchy == visualStudioWorkspace.GetHierarchy(p.Id));
        }

        public static INamespaceSymbol GetNamespace(string nspace, Compilation compilation)
        {
            return CodeAnalysisHelper.GetNamespace(nspace, compilation.GlobalNamespace);
        }

        private static INamespaceSymbol GetNamespace(string nspace, INamespaceSymbol namespaceSymbol)
        {
            namespaceSymbol = namespaceSymbol.GetNamespaceMembers()
                .FirstOrDefault(ns =>
                    {
                        string fullNamespace = CodeAnalysisHelper.GetFullNamespace(ns);
                        return nspace == fullNamespace || nspace.StartsWith(fullNamespace + Type.Delimiter);
                    });

            if (namespaceSymbol != null && CodeAnalysisHelper.GetFullNamespace(namespaceSymbol) != nspace)
            {
                namespaceSymbol = CodeAnalysisHelper.GetNamespace(nspace, namespaceSymbol);
            }

            return namespaceSymbol;
        }

        private static string GetFullNamespace(INamespaceSymbol namespaceSymbol)
        {
            string fullNamespace = namespaceSymbol.Name;

            if (!string.IsNullOrEmpty(fullNamespace))
            {
                string parentNamespace = CodeAnalysisHelper.GetFullNamespace(namespaceSymbol.ContainingNamespace);
                if (!string.IsNullOrEmpty(parentNamespace))
                {
                    fullNamespace = parentNamespace + Type.Delimiter + fullNamespace;
                }
            }

            return fullNamespace;
        }
    }
}
