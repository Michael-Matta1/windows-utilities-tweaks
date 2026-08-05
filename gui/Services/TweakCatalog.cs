using System.IO;
using System.Reflection;
using System.Text.Json;
using WindowsTweakLauncher.Models;

namespace WindowsTweakLauncher.Services;

public static class TweakCatalog
{
    public static List<TweakItem> Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(".tweaks.manifest.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            throw new InvalidOperationException("Embedded tweaks.manifest.json not found \u2014 this build is corrupted.");

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException("Embedded tweaks.manifest.json not found \u2014 this build is corrupted.");

        string json;
        try
        {
            using var reader = new StreamReader(stream);
            json = reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read tweaks manifest: {ex.Message}", ex);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(
            namingPolicy: null));

        try
        {
            var items = JsonSerializer.Deserialize<List<TweakItem>>(json, options);
            items = items?.Where(i =>
            {
                if (i == null) return false;
                if (string.IsNullOrWhiteSpace(i.Id))
                {
                    ProcessRunner.AppendLog("TweakCatalog: skipping entry with empty Id");
                    return false;
                }
                if (i.Strategy == TweakStrategy.None)
                {
                    ProcessRunner.AppendLog($"TweakCatalog: skipping entry '{i.Id}' with Strategy=None");
                    return false;
                }
                return true;
            }).ToList();
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Tweaks manifest is empty or has no valid entries.");

            // Check for duplicate IDs
            var duplicates = items.GroupBy(i => i.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicates.Count > 0)
            {
                var msg = $"TweakCatalog: duplicate IDs found: {string.Join(", ", duplicates)}";
                ProcessRunner.AppendLog(msg);
                throw new InvalidOperationException(msg);
            }

            return items;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse tweaks manifest: {ex.Message}", ex);
        }
    }
}
