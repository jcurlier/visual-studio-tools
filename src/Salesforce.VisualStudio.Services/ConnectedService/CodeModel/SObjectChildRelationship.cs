using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    /// <summary>
    /// The Salesforce describe metadata for a relationship between objects.
    /// </summary>
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
