using Microsoft.VisualStudio.ConnectedServices.Controls;
using Salesforce.VisualStudio.Services.ConnectedService.CodeModel;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal class ObjectSelectionViewModel : ViewModel
    {
        private ObjectPickerCategory allObjectsCategory;
        private string errorMessage;

        public ObjectSelectionViewModel()
        {
            this.allObjectsCategory = new ObjectPickerCategory(Resources.ObjectSelectionViewModel_AllObjects);
        }

        public async Task RefreshObjects(DesignTimeAuthentication authentication)
        {
            // In the future, consider preserving any previously selected objects.  This is not currently done because
            // there is no mechanism to indicate to the user when previously selected objects are no longer available.

            this.allObjectsCategory.Children = null;
            this.ErrorMessage = null;

            try
            {
                IEnumerable<SObjectDescription> objects = await MetadataLoader.LoadObjects(authentication);
                this.allObjectsCategory.Children = objects
                    .Select(o => new ObjectPickerObject(this.allObjectsCategory, o.Name) { State = o })
                    .ToArray();
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

        public IEnumerable<SObjectDescription> GetSelectedObjects()
        {
            return this.allObjectsCategory.Children
                .Where(c => c.IsSelected)
                .Select(c => c.State)
                .Cast<SObjectDescription>();
        }

        public int GetAvailableObjectCount()
        {
            return this.allObjectsCategory.Children.Count();
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
    }
}
