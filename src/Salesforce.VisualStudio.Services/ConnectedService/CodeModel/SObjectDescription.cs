using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    /// <summary>
    /// The Salesforce describe metadata for an object.
    /// </summary>
    [Serializable]
    public class SObjectDescription
    {
        internal SObjectDescription()
        {
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Activateable")]
        public bool Activateable { get; set; }

        public IEnumerable<SObjectChildRelationship> ChildRelationships { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Layoutable")]
        public bool CompactLayoutable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Createable")]
        public bool Createable { get; set; }

        public bool Custom { get; set; }

        public bool CustomSetting { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deletable")]
        public bool Deletable { get; set; }

        public bool DeprecatedAndHidden { get; set; }

        public bool FeedEnabled { get; set; }

        public IEnumerable<SObjectField> Fields { get; set; }

        public string KeyPrefix { get; set; }

        public string Label { get; set; }

        public string LabelPlural { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Layoutable")]
        public bool Layoutable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mergeable")]
        public bool Mergeable { get; set; }

        public string Name { get; set; }

        public bool Queryable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Replicateable")]
        public bool Replicateable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Retrieveable")]
        public bool Retrieveable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Layoutable")]
        public bool SearchLayoutable { get; set; }

        public bool Searchable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Triggerable")]
        public bool Triggerable { get; set; }

        public bool Undeletable { get; set; }

        public bool Updateable { get; set; }
    }
}
