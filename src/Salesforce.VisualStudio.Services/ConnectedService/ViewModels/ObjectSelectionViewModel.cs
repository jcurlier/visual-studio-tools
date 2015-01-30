using Microsoft.VisualStudio.ConnectedServices;
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
    internal class ObjectSelectionViewModel : SalesforceConnectedServiceWizardPage
    {
        private ObjectPickerCategory allObjectsCategory;
        private string errorMessage;
        private DesignTimeAuthentication lastDesignTimeAuthentication;
        private Task loadObjectsTask;

        public ObjectSelectionViewModel(ConnectedServiceProviderHost host, TelemetryHelper telemetryHelper, UserSettings userSettings)
            : base(host, telemetryHelper, userSettings)
        {
            this.allObjectsCategory = new ObjectPickerCategory(Resources.ObjectSelectionViewModel_AllObjects);
            this.Title = Resources.ObjectSelectionViewModel_Title;
            this.Description = Resources.ObjectSelectionViewModel_Description;
            this.Legend = Resources.ObjectSelectionViewModel_Legend;
            this.View = new ObjectSelectionPage(this);
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
                this.OnNotifyPropertyChanged();
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
        private async Task WaitOnRefreshObjectsAsync()
        {
            if (this.loadObjectsTask != null && (!this.loadObjectsTask.IsCompleted || this.loadObjectsTask.IsFaulted))
            {
                using (this.Host.StartBusyIndicator(Resources.ObjectSelectionViewModel_LoadingObjectsProgress))
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

        public override async Task<NavigationEnabledState> OnPageEnteringAsync(WizardEnteringArgs args)
        {
            await this.WaitOnRefreshObjectsAsync();

            return await base.OnPageEnteringAsync(args);
        }
    }
}
