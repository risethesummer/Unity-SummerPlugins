using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Editor
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

            [JsonProperty("scopedRegistries")] internal ScopedRegistry[] ScopedRegistries { get; init; }
            [JsonProperty("dependencies")] internal Dictionary<string, string> Dependencies { get; init; }
        }

        private const string ManifestPath = "Packages/manifest.json";
        private const string DependenciesPath = "Assets/Plugins/Unity-SummerPlugins/dependencies.json";

        [MenuItem("Tools/SummerPlugins/Install")]
        public static async void Install()
        {
            var plugin = await GetPluginData(DependenciesPath);
            var currentSettings = await GetPluginData(ManifestPath);
            bool change = false;
            var scopes = currentSettings.ScopedRegistries
                .First(e => e.Name == "package.openupm.com")
                .Scopes;
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
        }

        private static async UniTask SavePluginData(string path, Plugin data)
        {
            var stringData = JsonConvert.SerializeObject(data);
            await File.WriteAllTextAsync(path, string.Empty);
            await File.WriteAllTextAsync(path, stringData);
        }

        private static async UniTask<Plugin> GetPluginData(string path)
        {
            var stringData = await File.ReadAllTextAsync(path);// AssetDatabase.LoadAssetAtPath<TextAsset>(DependenciesPath);
            var data = JsonConvert.DeserializeObject<Plugin>(stringData);
            return data;
        }
    }
}