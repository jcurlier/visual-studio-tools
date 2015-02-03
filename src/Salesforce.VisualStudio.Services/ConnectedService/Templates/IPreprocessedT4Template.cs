using System.Collections.Generic;

namespace Salesforce.VisualStudio.Services.ConnectedService.Templates
{
    /// <summary>
    /// A preprocessed T4 template that can be invoked to generate code.
    /// </summary>
    internal interface IPreprocessedT4Template
    {
        IDictionary<string, object> Session { get; set; }

        void Initialize();

        string TransformText();
    }
}
