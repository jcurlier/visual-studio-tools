using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    [Serializable]
    public class SObjectPickListValue
    {
        internal SObjectPickListValue()
        {
        }

        public bool Active { get; set; }

        public bool DefaultValue { get; set; }

        public string Label { get; set; }

        public object Value { get; set; }
    }
}
