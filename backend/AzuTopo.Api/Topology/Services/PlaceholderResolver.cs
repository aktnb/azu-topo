using System.Text.RegularExpressions;
using AzuTopo.Api.Topology.Exceptions;

namespace AzuTopo.Api.Topology.Services;

internal sealed partial class PlaceholderResolver(IReadOnlyDictionary<string, string> placeholders)
{
    public string Resolve(string value, string fieldName)
    {
        var resolved = PlaceholderPattern().Replace(value, match =>
        {
            var key = match.Groups["key"].Value;
            if (!placeholders.TryGetValue(key, out var replacement))
            {
                throw new TopologyDefinitionException($"Unknown placeholder \"{key}\" in {fieldName}.");
            }

            if (string.IsNullOrWhiteSpace(replacement))
            {
                throw new TopologyDefinitionException($"Placeholder \"{key}\" in {fieldName} resolves to an empty value.");
            }

            return replacement;
        });

        if (string.IsNullOrWhiteSpace(resolved))
        {
            throw new TopologyDefinitionException($"{fieldName} must not be empty.");
        }

        if (resolved.Contains("${", StringComparison.Ordinal))
        {
            throw new TopologyDefinitionException($"{fieldName} contains a malformed placeholder.");
        }

        return resolved;
    }

    [GeneratedRegex(@"\$\{(?<key>[A-Za-z][A-Za-z0-9_]*)\}", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderPattern();
}
