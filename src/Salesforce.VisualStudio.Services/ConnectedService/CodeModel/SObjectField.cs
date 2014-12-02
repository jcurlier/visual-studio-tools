using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    [Serializable]
    public class SObjectField
    {
        internal SObjectField()
        {
        }

        public bool AutoNumber { get; set; }

        public int ByteLength { get; set; }

        public bool Calculated { get; set; }

        public string CalculatedFormula { get; set; }

        public bool CascadeDelete { get; set; }

        public bool CaseSensitive { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Createable")]
        public bool Createable { get; set; }

        public bool Custom { get; set; }

        public object DefaultValue { get; set; }

        public string DefaultValueFormula { get; set; }

        public bool DefaultedOnCreate { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Picklist")]
        public bool DependentPicklist { get; set; }

        public bool DeprecatedAndHidden { get; set; }

        public int Digits { get; set; }

        public bool DisplayLocationInDecimal { get; set; }

        public bool ExternalId { get; set; }

        public string ExtraTypeInfo { get; set; }

        public bool Filterable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Groupable")]
        public bool Groupable { get; set; }

        public bool HtmlFormatted { get; set; }

        public bool IdLookup { get; set; }

        public string InlineHelpText { get; set; }

        public string Label { get; set; }

        public int Length { get; set; }

        public string Name { get; set; }

        public bool NameField { get; set; }

        public bool NamePointing { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nillable")]
        public bool Nillable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Permissionable")]
        public bool Permissionable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Picklist")]
        public IEnumerable<SObjectPickListValue> PicklistValues { get; set; }

        public int Precision { get; set; }

        public bool QueryByDistance { get; set; }

        public IEnumerable<string> ReferenceTo { get; set; }

        public string RelationshipName { get; set; }

        public string RelationshipOrder { get; set; }

        public bool RestrictedDelete { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Picklist")]
        public bool RestrictedPicklist { get; set; }

        public int Scale { get; set; }

        public string SoapType { get; set; }

        public bool Sortable { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public string Type { get; set; }

        public bool Unique { get; set; }

        public bool Updateable { get; set; }

        public bool WriteRequiresMasterRead { get; set; }
    }
}
