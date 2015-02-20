using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    /// <summary>
    /// The view model for the Object Selection wizard page.
    /// </summary>
    internal class ObjectSelectionViewModel : SalesforceConnectedServiceWizardPage
    {
        private ObjectPickerCategory allObjectsCategory;
        private string errorMessage;
        private DesignTimeAuthentication lastDesignTimeAuthentication;
        private Task loadObjectsTask;

        public ObjectSelectionViewModel(SalesforceConnectedServiceWizard wizard)
            : base(wizard)
        {
            this.allObjectsCategory = new ObjectPickerCategory(Resources.ObjectSelectionViewModel_AllObjects);
            this.Title = Resources.ObjectSelectionViewModel_Title;
            this.Description = Resources.ObjectSelectionViewModel_Description;
            this.Legend = Resources.ObjectSelectionViewModel_Legend;
            this.View = new ObjectSelectionPage(this);

            // Because the ObjectPicker is a scrollable control itself, the page's scroll bar functionality
            // needs to be disabled in order to force the control to be sized within the page and thus
            // show scroll bars.
            this.DisableScrollBars = true;
        }

        public IEnumerable<ObjectPickerCategory> Categories
        {
            get { yield return this.allObjectsCategory; }
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// Refreshes the objects displayed in the picker asynchronously.  The objects are only refreshed if the specified
        /// authentication is different than the last time the objects were retrieved.
        /// </summary>
        public void BeginRefreshObjects(DesignTimeAuthentication authentication)
        {
            // In the future, consider preserving any previously selected objects.  This is not currently done because
            // there is no mechanism to indicate to the user when previously selected objects are no longer available.

            if (this.lastDesignTimeAuthentication == null ||
                !this.lastDesignTimeAuthentication.Equals(authentication))
            {
                this.allObjectsCategory.Children = null;
                this.ErrorMessage = null;
                this.lastDesignTimeAuthentication = authentication;
                this.loadObjectsTask = this.LoadObjectsAsync(authentication);
            }
        }

        private async Task LoadObjectsAsync(DesignTimeAuthentication authentication)
        {
            try
            {
                IEnumerable<SObjectDescription> objects = await MetadataLoader.LoadObjectsAsync(authentication);
                this.allObjectsCategory.Children = objects
                    .Select(o => new ObjectPickerObject(this.allObjectsCategory, o.Name) { State = o })
                    .ToArray();
                this.allObjectsCategory.IsSelected = true;

                if (this.Wizard.Context.IsUpdating)
                {
                    await this.InitializeObjectSelectionState(this.allObjectsCategory.Children);
                }
            }
            catch (Exception ex)
            {
                if (ExceptionHelper.IsCriticalException(ex))
                {
                    throw;
                }

                this.ErrorMessage = String.Format(CultureInfo.CurrentCulture, Resources.ObjectSelectionViewModel_LoadingError, ex);
            }
        }

        /// <summary>
        /// Initialize the selection state of the objects in the object picker based on which objects have been previously
        /// scaffolded.
        /// </summary>
        private async Task InitializeObjectSelectionState(IEnumerable<ObjectPickerObject> children)
        {
            Project project = CodeAnalysisHelper.GetProject(this.Wizard.Context.ProjectHierarchy, this.Wizard.VisualStudioWorkspace);
            Compilation compilation = await project?.GetCompilationAsync();
            if (compilation != null)
            {
                string modelsNamespaceName =
                    ProjectHelper.GetProjectNamespace(ProjectHelper.GetProjectFromHierarchy(this.Wizard.Context.ProjectHierarchy)) 
                    + Type.Delimiter 
                    + this.Wizard.DesignerData.GetDefaultedModelsHintPath().Replace(Path.DirectorySeparatorChar, Type.Delimiter);
                INamespaceSymbol modelsNamespace = CodeAnalysisHelper.GetNamespace(modelsNamespaceName, compilation);
                if (modelsNamespace != null)
                {
                    foreach (INamedTypeSymbol type in modelsNamespace.GetTypeMembers())
                    {
                        // Types are matched only on type name, it is hard to do much more because users are free to change
                        // the scaffolded code in a number of ways.
                        ObjectPickerObject pickerObject = children.FirstOrDefault(c => c.Name == type.Name);
                        if (pickerObject != null)
                        {
                            pickerObject.IsChecked = true;
                            pickerObject.IsEnabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Waits for the RefreshObjects task to complete if it is not already completed.  While waiting a busy indicator will be displayed.
        /// </summary>
        private async Task WaitOnRefreshObjectsAsync()
        {
            if (this.loadObjectsTask != null && (!this.loadObjectsTask.IsCompleted || this.loadObjectsTask.IsFaulted))
            {
                using (this.Wizard.Context.StartBusyIndicator(Resources.ObjectSelectionViewModel_LoadingObjectsProgress))
                {
                    await this.loadObjectsTask;
                }
            }

            this.loadObjectsTask = null;
        }

        /// <summary>
        /// Gets the objects that were selected by the user.  In the case of update, only the objects that were
        /// selected during update are returned.
        /// </summary>
        public IEnumerable<SObjectDescription> GetSelectedObjects()
        {
            return this.allObjectsCategory.Children
                .Where(c => c.IsChecked && c.IsEnabled)
                .Select(c => c.State)
                .Cast<SObjectDescription>();
        }

        public int GetAvailableObjectCount()
        {
            return this.allObjectsCategory.Children.Count();
        }

        public override async Task OnPageEnteringAsync(WizardEnteringArgs args)
        {
            await this.WaitOnRefreshObjectsAsync();

            await base.OnPageEnteringAsync(args);
        }
    }
}