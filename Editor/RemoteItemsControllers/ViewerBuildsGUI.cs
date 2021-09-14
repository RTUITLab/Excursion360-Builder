using Packages.Excursion360_Builder.Editor.HTTP;
using Packages.Excursion360_Builder.Editor.RemoteItemsControllers;
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

namespace Packages.Excursion360_Builder.Editor.Viewer
{
    internal class ViewerBuildsGUI : RemoteControllerGUI<WebViewerBuildPack>
    {
        protected override string PacksFolderName => "viewers";

        protected override string GithubRepo => "Excursion360-Web";

        protected override List<WebViewerBuildPack> FindPacks(string rootLocation)
        {
            return Directory.GetFiles(rootLocation, "web-viewer-*-*.zip")
                            .Select(path => (path, match: Regex.Match(path, @"web-viewer-(?<tag>\S+)-(?<id>\d+).zip")))
                            .Select(b => new WebViewerBuildPack
                            {
                                Id = int.Parse(b.match.Groups["id"].Value),
                                Version = b.match.Groups["tag"].Value,
                                ArchiveLocation = b.path
                            })
                            .ToList();
        }

        protected override void RemovePack(WebViewerBuildPack pack)
        {
            File.Delete(pack.ArchiveLocation);
        }

        protected override IEnumerator DownloadPack(string rootLocation, ReleaseResponse release, Action<string> errorAction)
        {
            var targetLink = release.assets.FirstOrDefault(a => a.name == "build.zip");
            if (targetLink == null)
            {
                errorAction("No needed asset in latest release");
                yield break;
            }
            var downloadRequest = HttpHelper.InvokeGetRequest(targetLink.browser_download_url, $"Downloading {targetLink.name}",
                    handler =>
                    {
                        File.WriteAllBytes(Path.Combine(rootLocation, $"web-viewer-{release.tag_name}-{release.id}.zip"), handler.data);
                    },
                    errorAction);
            while (downloadRequest.MoveNext())
            {
                yield return downloadRequest.Current;
            }
        }
    }
    
}
