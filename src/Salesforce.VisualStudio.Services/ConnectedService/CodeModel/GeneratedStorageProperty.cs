using System;

namespace Salesforce.VisualStudio.Services.ConnectedService.CodeModel
{
    [Serializable]
    public sealed class GeneratedStorageProperty
    {
        internal GeneratedStorageProperty()
        {
        }

        public Type ClrType { get; internal set; }

        public bool IsNullableType { get; internal set; }

        public bool IsKey { get; internal set; }

        public SObjectField Model { get; internal set; }
    }
}
