

using System.Security.Cryptography;

namespace Brimborium.CopyProject;
public class ContentMapping {
    public ContentMapping(
        CopyProjectMap copyProjectMap,
        RepoContentList src,
        RepoContentList dst,
        RepoContentList? diff
        ) {
        this.CopyProjectMap = copyProjectMap;
        this.Src = src;
        this.Dst = dst;
        this.Diff = diff;
    }

    public CopyProjectMap CopyProjectMap { get; }
    public RepoContentList Src { get; }
    public RepoContentList Dst { get; }
    public RepoContentList? Diff { get; }

    public void ScanFolder() {
        var s = this.Src.ScanFolderInit();
        _ = this.Dst.ScanFolderInit();
        this.Src.RescanFolder();
        this.Dst.RescanFolder();
        this.Src.ScanFolderDone(false, s);
    }

    public bool UpdateAction() {
        var result = false;
        foreach (var relativeFile in this.Src.DictFileAction.Values) {
            if (relativeFile.ActionE == CopyFileSettingsAction.Default) {
                string srcFileContent = this.Src.ReadFile(relativeFile.RelativePath);
                string dstFileContent = this.Dst.ReadFile(relativeFile.RelativePath);
                if (this.CompareFileContent(srcFileContent, dstFileContent)) {
                    // ok
                    relativeFile.ActionE = CopyFileSettingsAction.Copy;
                    result = true;
                    System.Console.Out.WriteLine($"copy:   {relativeFile.RelativePath}");

                } else if (string.IsNullOrEmpty(srcFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    System.Console.Out.WriteLine($"delete: {relativeFile.RelativePath}");

                } else if (string.IsNullOrEmpty(dstFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    System.Console.Out.WriteLine($"delete: {relativeFile.RelativePath}");
                }

            } else if (relativeFile.ActionE == CopyFileSettingsAction.Copy) {
                string srcFileContent = this.Src.ReadFile(relativeFile.RelativePath);
                string dstFileContent = this.Dst.ReadFile(relativeFile.RelativePath);
                if (this.CompareFileContent(srcFileContent, dstFileContent)) {
                    // ok
                } else if (string.IsNullOrEmpty(srcFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    System.Console.Out.WriteLine($"delete: {relativeFile.RelativePath}");

                } else if (string.IsNullOrEmpty(dstFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    System.Console.Out.WriteLine($"delete: {relativeFile.RelativePath}");

                } else {
                    relativeFile.ActionE = CopyFileSettingsAction.Diff;
                    result = true;
                    System.Console.Out.WriteLine($"diff:   {relativeFile.RelativePath}");
                }

            } else if (relativeFile.ActionE == CopyFileSettingsAction.Delete) {
                //

            } else if (relativeFile.ActionE == CopyFileSettingsAction.Diff) {
                //
                // TODO: create diff
            }
        }

        // changed?
        return result;
    }

    public void ExecuteAction() {
        foreach (var relativeFile in this.Src.DictFileAction.Values) {
            if (relativeFile.ActionE is CopyFileSettingsAction.Copy or CopyFileSettingsAction.Default) {
                string srcPath = this.Src.GetAbsolutePath(relativeFile.RelativePath);
                string dstPath = this.Dst.GetAbsolutePath(relativeFile.RelativePath);
                if (!System.IO.File.Exists(dstPath)) {
                    var dstDirName = System.IO.Path.GetDirectoryName(dstPath);
                    if (dstDirName is { Length: > 0 }) {
                        System.IO.Directory.CreateDirectory(dstDirName);
                    }

                }
                if (System.IO.File.Exists(srcPath) && !System.IO.File.Exists(dstPath)) {
                    System.IO.File.Copy(srcPath, dstPath, true);
                    System.Console.Out.WriteLine($"copy:   {relativeFile.RelativePath}");
                } else {
                    string srcFileContent = this.Src.ReadFile(relativeFile.RelativePath);
                    string dstFileContent = this.Dst.ReadFile(relativeFile.RelativePath);
                    if (this.CompareFileContent(srcFileContent, dstFileContent)) {
                        // no change
                    } else {
                        this.Dst.WriteFile(relativeFile.RelativePath, srcFileContent);
                        System.Console.Out.WriteLine($"copy:   {relativeFile.RelativePath}");
                    }
                }
                continue;
            }

            if (relativeFile.ActionE == CopyFileSettingsAction.Delete) {
                if (this.Dst.DeleteFile(relativeFile.RelativePath)) {
                    System.Console.Out.WriteLine($"delete:   {relativeFile.RelativePath}");
                }
                continue;
            }

            if (relativeFile.ActionE == CopyFileSettingsAction.Diff) {
                // patch - apply diff
                continue;
            }
        }
    }

    public void ShowDiff() {
        foreach (var relativeFile in this.Src.DictFileAction.Values) {
            if (relativeFile.ActionE is CopyFileSettingsAction.Copy or CopyFileSettingsAction.Default) {
                string srcFileContent = this.Src.ReadFile(relativeFile.RelativePath);
                string dstFileContent = this.Dst.ReadFile(relativeFile.RelativePath);
                if (this.CompareFileContent(srcFileContent, dstFileContent)) {
                    // no change
                } else {
                    System.Console.Out.WriteLine($"diff: {relativeFile.RelativePath}");
                }
            }
        }
    }

    public void SaveCopyFileSettings(
        AppConfigurationService appConfigurationService) {
        var settingsName = this.CopyProjectMap.SettingsName;
        if (string.IsNullOrEmpty(settingsName)) {
            throw new InvalidOperationException("SettingsName is empty.");
        }
        var listFiles = this.Src.GetFiles().OrderBy(item => item.RelativePath).ToList();
        var rootFolder = appConfigurationService.GetRootFolder();
        var settingsNameJson = AppConfigurationUtility.EnsureJsonExtension(settingsName);

        List<CopyFileSettings> listCopyFileSettings = [];
        foreach (var file in listFiles) {
            if (file.RelativePath is { Length: > 0 } relativePath) {
                listCopyFileSettings.Add(new CopyFileSettings {
                    Path = relativePath,
                    ActionE = file.ActionE
                });
            }
        }
        appConfigurationService.SaveCopyFileSettings(settingsNameJson, listCopyFileSettings);
    }


    public bool CompareFileContent(string srcFileContent, string dstFileContent) {
        if (string.Equals(srcFileContent, dstFileContent, StringComparison.Ordinal)) {
            return true;
        }
        srcFileContent = srcFileContent.ReplaceLineEndings();
        dstFileContent = dstFileContent.ReplaceLineEndings();
        if (string.Equals(srcFileContent, dstFileContent, StringComparison.Ordinal)) {
            return true;
        }
        return false;
    }
}
