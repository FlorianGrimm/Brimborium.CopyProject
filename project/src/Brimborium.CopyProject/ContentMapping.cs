using Microsoft.Extensions.Logging;

namespace Brimborium.CopyProject;

public sealed class ContentMapping {
    private readonly ILogger _Logger;

    public ContentMapping(
        CopyProjectMap copyProjectMap,
        RepoContentList src,
        RepoContentList dst,
        RepoContentList? diff,
        ILogger logger
        ) {
        this.CopyProjectMap = copyProjectMap;
        this.Src = src;
        this.Dst = dst;
        this.Diff = diff;
        this._Logger = logger;
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
                    this._Logger.UpdateAction(relativeFile.RelativePath, relativeFile.ActionE);
                    continue;

                } else if (string.IsNullOrEmpty(srcFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    this._Logger.UpdateAction(relativeFile.RelativePath, relativeFile.ActionE);
                    continue;

                } else if (string.IsNullOrEmpty(dstFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    this._Logger.UpdateAction(relativeFile.RelativePath, relativeFile.ActionE);
                    continue;

                } else {
                    relativeFile.ActionE = CopyFileSettingsAction.Diff;
                    result = true;
                    this._Logger.UpdateAction(relativeFile.RelativePath, relativeFile.ActionE);
                }
            }

            if (relativeFile.ActionE == CopyFileSettingsAction.Copy) {
                string srcFileContent = this.Src.ReadFile(relativeFile.RelativePath);
                string dstFileContent = this.Dst.ReadFile(relativeFile.RelativePath);
                if (this.CompareFileContent(srcFileContent, dstFileContent)) {
                    // ok
                } else if (string.IsNullOrEmpty(srcFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    this._Logger.UpdateAction(relativeFile.RelativePath, relativeFile.ActionE);
                    continue;

                } else if (string.IsNullOrEmpty(dstFileContent)) {
                    relativeFile.ActionE = CopyFileSettingsAction.Delete;
                    result = true;
                    this._Logger.UpdateAction(relativeFile.RelativePath, relativeFile.ActionE);
                    continue;

                } else {
                    relativeFile.ActionE = CopyFileSettingsAction.Diff;
                    result = true;
                    this._Logger.UpdateAction(relativeFile.RelativePath, relativeFile.ActionE);
                }

            }

            if (relativeFile.ActionE == CopyFileSettingsAction.Delete) {
                //
            }

            if (relativeFile.ActionE == CopyFileSettingsAction.Diff) {
                //
                // TODO: create diff
                /*
                if (this.Diff is { }) {
                    string srcFileContent = this.Src.ReadFile(relativeFile.RelativePath);
                    string dstFileContent = this.Dst.ReadFile(relativeFile.RelativePath);
                    var diffAbsolutePath = this.Diff.GetAbsolutePath(relativeFile.RelativePath);
                    Brimborium.CopyProject.DiffMatchPatch.DiffMatchPatcher diffMatchPatcher = new();
                    var listDiff = diffMatchPatcher.Diff_main(srcFileContent, dstFileContent);
                    using (var stream = System.IO.File.Create(diffAbsolutePath)) { 
                        System.Text.Json.JsonSerializer.Serialize<List<Brimborium.CopyProject.DiffMatchPatch.Diff>>(
                            stream, listDiff);
                        stream.Flush();
                    }
                }
                */
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
                    this._Logger.ExecuteAction(relativeFile.RelativePath, relativeFile.ActionE);
                } else {
                    string srcFileContent = this.Src.ReadFile(relativeFile.RelativePath);
                    string dstFileContent = this.Dst.ReadFile(relativeFile.RelativePath);
                    if (this.CompareFileContent(srcFileContent, dstFileContent)) {
                        // no change
                    } else {
                        this.Dst.WriteFile(relativeFile.RelativePath, srcFileContent);
                        this._Logger.ExecuteAction(relativeFile.RelativePath, relativeFile.ActionE);
                    }
                }
                continue;
            }

            if (relativeFile.ActionE == CopyFileSettingsAction.Delete) {
                if (this.Dst.DeleteFile(relativeFile.RelativePath)) {
                    this._Logger.ExecuteAction(relativeFile.RelativePath, relativeFile.ActionE);
                }
                continue;
            }

            if (relativeFile.ActionE == CopyFileSettingsAction.Diff) {
                // patch - apply diff
                // this._Logger.ExecuteAction(relativeFile.RelativePath, relativeFile.ActionE);
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
                    this._Logger.DiffAction(relativeFile.RelativePath);
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
                    Path = AppConfigurationUtility.ConvertPathForSettings(relativePath),
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
