using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace KE.VSIX.Packages
{
    public static class PackageHelper
    {
        #region Var
        private static readonly Version RoslynVersion = new Version(2, 9, 0);
        private const string RoslynURL = "https://www.nuget.org/api/v2/package/Microsoft.Net.Compilers";

        private static readonly object _sync = new object();
        #endregion

        #region Functions
        public static PathContainer Initialize<TPackage>() where TPackage : AsyncPackage => Initialize<TPackage>(false);

        public static PathContainer Initialize<TPackage>(bool NeedRoslyn) where TPackage : AsyncPackage
        {
            PathContainer container = new PathContainer(typeof(TPackage));
            string wrongDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), container.PackageName);
            if (Directory.Exists(wrongDir))
            {
                if (Directory.Exists(container.Path))
                {
                    foreach (string x in Directory.EnumerateDirectories(container.Path, "*", SearchOption.AllDirectories))
                        Directory.CreateDirectory(x.Replace(container.Path, wrongDir));
                    foreach (string x in Directory.EnumerateFiles(container.Path, "*", SearchOption.AllDirectories))
                        File.Copy(x, x.Replace(container.Path, wrongDir), true);
                    Directory.Delete(container.Path, true);
                }
                Directory.Move(wrongDir, container.Path);
            }
            if (NeedRoslyn)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                InitializeRoslynAsync(container.RoslynPath);
#pragma warning restore CS4014
            }
            return container;
        }

        private static async Task InitializeRoslynAsync(string RoslynPath)
        {
            Version minVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            foreach (string exe in new[] { "csc", "csi", "vbc", "VBCSCompiler" }.Select(x => Path.Combine(RoslynPath, $"{x}.exe")))
            {
                if (!File.Exists(exe))
                {
                    await InstallRoslynAsync(RoslynPath);
                    return;
                }
                Version.TryParse(FileVersionInfo.GetVersionInfo(exe).FileVersion, out Version version);
                minVersion = version < minVersion ? version : minVersion;
            }

            if (minVersion < RoslynVersion)
                await InstallRoslynAsync(RoslynPath);
        }

        private static async Task InstallRoslynAsync(string RoslynPath)
        {
            if (Monitor.TryEnter(_sync))
                try
                {
                    if (Directory.Exists(RoslynPath))
                        Directory.Delete(RoslynPath, true);
                    Directory.CreateDirectory(RoslynPath);

                    using (HttpClient client = new HttpClient())
                    {
                        byte[] bytes = await client.GetByteArrayAsync($"{RoslynURL}/{RoslynVersion}");
                        string tmp = Path.GetTempFileName();
                        File.WriteAllBytes(tmp, bytes);
                        try
                        {
                            using (ZipArchive zip = ZipFile.OpenRead(tmp))
                            {
                                var tools = zip.Entries.Where(x => Path.GetFileNameWithoutExtension(Path.GetDirectoryName(x.FullName)) == "tools");
                                foreach (ZipArchiveEntry entry in tools)
                                    entry.ExtractToFile(Path.Combine(RoslynPath, entry.Name), true);
                            }
                        }
                        finally
                        {
                            File.Delete(tmp);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
        }
        #endregion
    }
}
