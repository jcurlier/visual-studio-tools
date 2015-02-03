namespace Salesforce.VisualStudio.Services.ConnectedService.Models
{
    /// <summary>
    /// Represents an AppSetting within a web/app config file.
    /// </summary>
    internal class ConfigSetting
    {
        public ConfigSetting(string key, object value)
            : this(key, value, null)
        {
        }

        public ConfigSetting(string key, object value, string comment)
        {
            this.Key = key;
            this.Value = value;
            this.Comment = comment;
        }

        public string Key { get; private set; }

        public object Value { get; private set; }

        public string Comment { get; private set; }
    }
}
