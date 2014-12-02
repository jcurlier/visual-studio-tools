using Microsoft.VisualStudio.ConnectedServices;
using System.ComponentModel;

namespace Salesforce.VisualStudio.Services.ConnectedService.ViewModels
{
    internal interface IWizardPage : IConnectedServiceWizardPage, INotifyPropertyChanged
    {
        bool IsValid { get; }

        new bool IsEnabled { get; set; }
    }
}
