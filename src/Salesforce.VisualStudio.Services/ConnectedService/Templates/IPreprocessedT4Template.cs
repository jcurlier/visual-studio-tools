using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService.Templates
{
    internal interface IPreprocessedT4Template
    {
        IDictionary<string, object> Session { get; set; }

        void Initialize();

        string TransformText();
    }
}
