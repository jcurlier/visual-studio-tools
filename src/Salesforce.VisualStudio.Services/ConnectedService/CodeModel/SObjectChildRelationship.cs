using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    [Serializable]
    public class SObjectChildRelationship
    {
        internal SObjectChildRelationship()
        {
        }

        public bool CascadeDelete { get; set; }

        public string ChildSObject { get; set; }

        public bool DeprecatedAndHidden { get; set; }

        public string Field { get; set; }

        public string RelationshipName { get; set; }

        public bool RestrictedDelete { get; set; }
    }
}
