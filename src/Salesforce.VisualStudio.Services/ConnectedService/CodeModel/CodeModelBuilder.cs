using Microsoft.VisualStudio.ConnectedServices;
using Salesforce.VisualStudio.Services.ConnectedService.Models;
using Salesforce.VisualStudio.Services.ConnectedService.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    internal static class CodeModelBuilder
    {
        public static async Task<IEnumerable<GeneratedObject>> BuildObjectModel(
            IEnumerable<SObjectDescription> sObjects,
            DesignTimeAuthentication authentication,
            GeneratedService generatedService,
            ConnectedServiceLogger logger)
        {
            IEnumerable<SObjectDescription> sObjectsWithDetails = await MetadataLoader.LoadObjectDetails(sObjects, authentication);

            IEnumerable<GeneratedObject> generatedObjects = sObjectsWithDetails
                .Select(o => new GeneratedObject()
                {
                    Model = o,
                    Service = generatedService,
                    StorageProperties = CodeModelBuilder.BuildStorageProperties(o, logger)
                })
                .ToArray();

            return generatedObjects;
        }

        private static IEnumerable<GeneratedStorageProperty> BuildStorageProperties(SObjectDescription objDescription, ConnectedServiceLogger logger)
        {
            return objDescription.Fields
                .Select(f => CodeModelBuilder.BuildStorageProperty(
                    f,
                    (soapType) => logger.WriteMessageAsync(LoggerMessageCategory.Error, Resources.LogMessage_UnsupportedSoapType, soapType, objDescription.Name, f.Name)))
                .ToArray();
        }

        private static GeneratedStorageProperty BuildStorageProperty(SObjectField field, Action<string> onUnsupportedType)
        {
            Type clrType = CodeModelBuilder.GetClrType(field.SoapType, onUnsupportedType);

            return new GeneratedStorageProperty()
            {
                ClrType = clrType,
                IsNullableType = field.Nillable && clrType.IsValueType,
                IsKey = field.Type == "id" && !field.Nillable,
                Model = field
            };
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Type GetClrType(string soapType, Action<string> onUnsupportedType)
        {
            switch (soapType)
            {
                case "xsd:string":
                case "tns:ID":
                    return typeof(string);
                case "xsd:anyType":
                case "urn:address":
                    return typeof(object);
                case "xsd:base64Binary":
                    return typeof(byte[]);
                case "xsd:boolean":
                    return typeof(bool);
                case "xsd:double":
                    return typeof(double);
                case "xsd:int":
                    return typeof(int);
                case "xsd:date":
                case "xsd:time":
                case "xsd:dateTime":
                    return typeof(DateTimeOffset);
                default:
                    onUnsupportedType(soapType);
                    return typeof(object);
            }
        }
    }
}
