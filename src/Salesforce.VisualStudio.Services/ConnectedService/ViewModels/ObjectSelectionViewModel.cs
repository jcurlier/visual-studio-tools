using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.ConnectedServices.Controls;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using Salesforce.VisualStudio.Services.ConnectedService.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class ObjectSelectionViewModel : PageViewModel
    {
        private ObjectPickerCategory allObjectsCategory;
        private string errorMessage;
        private DesignTimeAuthentication lastDesignTimeAuthentication;
        private IConnectedServiceProviderHost providerHost;
        private Task loadObjectsTask;

        public ObjectSelectionViewModel(IConnectedServiceProviderHost providerHost)
        {
            this.allObjectsCategory = new ObjectPickerCategory(Resources.ObjectSelectionViewModel_AllObjects);
            this.providerHost = providerHost;
            this.View = new ObjectSelectionPage(this);
        }

        public override string Title
        {
            get { return Resources.ObjectSelectionViewModel_Title; }
        }

        public override string Description
        {
            get { return Resources.ObjectSelectionViewModel_Description; }
        }

        public override string Legend
        {
            get { return Resources.ObjectSelectionViewModel_Legend; }
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
                this.RaisePropertyChanged();
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
                this.loadObjectsTask = this.LoadObjects(authentication);
            }
        }

        private async Task LoadObjects(DesignTimeAuthentication authentication)
        {
            try
            {
                IEnumerable<SObjectDescription> objects = await MetadataLoader.LoadObjects(authentication);
                this.allObjectsCategory.Children = objects
                    .Select(o => new ObjectPickerObject(this.allObjectsCategory, o.Name) { State = o })
                    .ToArray();
                this.allObjectsCategory.IsSelected = true;
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
        /// Waits for the RefreshObjects task to complete if it is not already completed.  While waiting a busy indicator will be displayed.
        /// </summary>
        private async Task WaitOnRefreshObjects()
        {
            if (this.loadObjectsTask != null && (!this.loadObjectsTask.IsCompleted || this.loadObjectsTask.IsFaulted))
            {
                using (this.providerHost.StartBusyIndicator(Resources.ObjectSelectionViewModel_LoadingObjectsProgress))
                {
                    await this.loadObjectsTask;
                }
            }

            this.loadObjectsTask = null;
        }

        public IEnumerable<SObjectDescription> GetSelectedObjects()
        {
            return this.allObjectsCategory.Children
                .Where(c => c.IsChecked)
                .Select(c => c.State)
                .Cast<SObjectDescription>();
        }

        public int GetAvailableObjectCount()
        {
            return this.allObjectsCategory.Children.Count();
        }

        public override async Task<NavigationEnabledState> OnPageEntering()
        {
            await this.WaitOnRefreshObjects();

            return await base.OnPageEntering();
        }
    }
}
