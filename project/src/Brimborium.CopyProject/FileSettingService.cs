using System.Text.Json;

namespace Brimborium.CopyProject;

public class FileSettingService {
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions() {
        WriteIndented = true,
    };

    public FileSettingService() {
    }

    // CopyProjectSettings
    
    public async Task<List<CopyProjectSettings>?> LoadCopyProjectSettingsAsync(
        string filepath,
        JsonSerializerOptions? options,
         CancellationToken cancellationToken) {
        if (System.IO.File.Exists(filepath)) {
            using (var json = System.IO.File.OpenRead(filepath)) {
                var optionsToUse = options ?? this.JsonSerializerOptions;
                return await System.Text.Json.JsonSerializer.DeserializeAsync<List<CopyProjectSettings>>(
                    json, optionsToUse, cancellationToken);
            }
        }
        return null;
    }
    public async Task SaveCopyProjectSettingsAsync(
        string filepath,
        List<CopyProjectSettings> listSettings,
        JsonSerializerOptions? options,
         CancellationToken cancellationToken) {
        using (var json = System.IO.File.Create(filepath)) {
            var optionsToUse = options ?? this.JsonSerializerOptions;
            await System.Text.Json.JsonSerializer.SerializeAsync(json, listSettings, optionsToUse, cancellationToken);
        }
    }

    // CopyFileSettings

    public async Task<List<CopyFileSettings>?> LoadCopyFileSettingsAsync(
        string filepath,
        JsonSerializerOptions? options,
         CancellationToken cancellationToken) {
        if (System.IO.File.Exists(filepath)) {
            using (var json = System.IO.File.OpenRead(filepath)) {
                var optionsToUse = options ?? this.JsonSerializerOptions;
                return await System.Text.Json.JsonSerializer.DeserializeAsync<List<CopyFileSettings>>(
                    json, optionsToUse, cancellationToken);
            }
        }
        return null;
    }
    public async Task SaveCopyFileSettingsAsync(
        string filepath,
        List<CopyFileSettings> listSettings,
        JsonSerializerOptions? options,
         CancellationToken cancellationToken) {
        using (var json = System.IO.File.Create(filepath)) {
            var optionsToUse = options ?? this.JsonSerializerOptions;
            await System.Text.Json.JsonSerializer.SerializeAsync(json, listSettings, optionsToUse, cancellationToken);
        }
    }
}