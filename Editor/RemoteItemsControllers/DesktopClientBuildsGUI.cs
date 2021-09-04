using Packages.Excursion360_Builder.Editor.WebBuild;
using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
using Packages.tour_creator.Editor.WebBuild.GitHubAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.Excursion360_Builder.Editor.RemoteItemsControllers
{
    internal class DesktopClientBuildsGUI : RemoteControllerGUI<DesktopClientBuildPack>
    {
        protected override string PacksFolderName => "desktop_clients";

        protected override string GithubRepo => "Excursion360-Desktop";

        protected override List<DesktopClientBuildPack> FindPacks(string rootLocation)
        {
            return Directory.GetDirectories(rootLocation)
               .Select(path => (path, match: Regex.Match(path, @"(?<tag>[^\\/]+)-(?<id>\d+)")))
               .Select(b => new DesktopClientBuildPack
               {
                   Id = int.Parse(b.match.Groups["id"].Value),
                   Version = b.match.Groups["tag"].Value,
                   FolderLocation = b.path
               })
               .ToList();

        }

        protected override void RemovePack(DesktopClientBuildPack pack)
        {
            Directory.Delete(pack.FolderLocation, true);
        }

        protected override IEnumerator DownloadPack(string rootLocation, ReleaseResponse release, Action<string> errorAction)
        {
            var exeAssets = release.assets.Where(a => a.name.EndsWith(".exe")).ToArray();
            if (exeAssets.Length == 0)
            {
                errorAction("No needed asset in latest release");
                yield break;
            }

            var versionDirectory = Path.Combine(rootLocation, $"{release.tag_name}-{release.id}");
            Directory.CreateDirectory(versionDirectory);

            foreach (var exe in exeAssets)
            {
                var downloadRequest = GitHubApi.InvokeGetRequest(exe.browser_download_url, $"Downloading {exe.name}",
                    handler =>
                    {
                        File.WriteAllBytes(Path.Combine(versionDirectory, exe.name), handler.data);
                    },
                    errorAction);
                while (downloadRequest.MoveNext())
                {
                    yield return downloadRequest.Current;
                }
            }
        }
    }

}
