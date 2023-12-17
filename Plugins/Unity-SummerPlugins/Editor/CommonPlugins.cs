using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Plugins.Unity_SummerPlugins.Editor
{
    public static class CommonPlugins
    {
        private class Plugin
        {
            internal class ScopedRegistry
            {
                [JsonProperty("name")] public string Name { get; init; }
                [JsonProperty("url")] public string Url { get; init; }
                [JsonProperty("scopes")] [ItemCanBeNull] public List<string> Scopes { get; init; }
            }

            [JsonProperty("scopedRegistries")] internal ScopedRegistry[] ScopedRegistries { get; set; }
            [JsonProperty("dependencies")] internal Dictionary<string, string> Dependencies { get; init; }
        }

        private const string ManifestPath = "Packages/manifest.json";
        private const string DependenciesPath = "Assets/Plugins/Unity-SummerPlugins/dependencies.json";

        [MenuItem("Tools/SummerPlugins/Install")]
        public static async void Install()
        {
            var plugin = await GetPluginData(DependenciesPath);
            var currentSettings = await GetPluginData(ManifestPath);
            List<string> scopes;
            if (currentSettings.ScopedRegistries is null)
            {
                scopes = new List<string>();
                currentSettings.ScopedRegistries = new Plugin.ScopedRegistry[]
                {
                    new()
                    {
                        Name = "package.openupm.com",
                        Url = "https://package.openupm.com",
                        Scopes = scopes
                    }
                };
            }
            else
            {
                scopes = currentSettings.ScopedRegistries
                    .First(e => e.Name == "package.openupm.com")
                    .Scopes;
            }
            
            bool change = false;
            foreach (var s in plugin.ScopedRegistries[0].Scopes.Where(s => !scopes.Contains(s)))
            {
                scopes.Add(s);
                change = true;
            }

            var currentPackages = currentSettings.Dependencies;
            foreach (var dependency in plugin.Dependencies)
            {
                // If does not contain => add it
                // If contain with another version => remove and add new version
                if (currentPackages.TryGetValue(dependency.Key, out var version) &&
                    version == dependency.Value) 
                    continue;
                currentPackages[dependency.Key] = dependency.Value;
                change = true;
            }
            if (!change)
            {
                Debug.Log("The package is up to date");
                return;
            }
            await SavePluginData(ManifestPath, currentSettings);
            Client.Resolve();
        }

        private static async Task SavePluginData(string path, Plugin data)
        {
            var stringData = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(path, string.Empty);
            await File.WriteAllTextAsync(path, stringData);
        }

        private static async Task<Plugin> GetPluginData(string path)
        {
            var stringData = await File.ReadAllTextAsync(path);// AssetDatabase.LoadAssetAtPath<TextAsset>(DependenciesPath);
            var data = JsonConvert.DeserializeObject<Plugin>(stringData);
            return data;
        }
    }
}