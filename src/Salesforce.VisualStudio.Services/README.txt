Two changes must be manually made with regards to the SalesforceMetadata anytime the Salesforce Service
Reference is updated:

1.  In "Web References\SalesforceMetadata\Reference.cs", the expression "typeof(QuickActionLayoutItem)"
    in the XmlArrayItemAttribute for QuickActionLayout.quickActionLayoutColumns must be changed to 
    "typeof(QuickActionLayoutItem[])".  For more information, see 
    http://www.fishofprey.com/2013/10/importing-salesforce-winter-13-metadata.html

2.  In "Web References\SalesforceMetadata\Reference.cs", the statement at the beginning of the MetadataService
    constructor setting this.Url should be removed, because that value is set as part of the design-time experience.
    Similarly, the generated Settings files in Properties (Properties\Settings.settings and 
    Properties\Settings.Designer.cs) should be deleted, as should the section which was added to app.config 
    regarding SalesforceMetadata.