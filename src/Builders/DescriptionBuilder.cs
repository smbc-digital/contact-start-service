using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace contact_start_service.Builders
{
    [ExcludeFromCodeCoverage]
    public class DescriptionBuilder
    {
        private string Description { get; set; } = "";

        public DescriptionBuilder Add(string key, string[] values, string delimiter = " ")
        {
            Description += $"{key}: {string.Join(delimiter, values.ToList().Where(_ => !string.IsNullOrEmpty(_)))} \n";

            return this;
        }

        public DescriptionBuilder Add(string key, string value)
        {
            Description += $"{key}: {value} \n";

            return this;
        }

        public DescriptionBuilder Add(string value)
        {
            Description += $"{value} \n";

            return this;
        }

        public string Build() => Description;
    }
}
