using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Brimborium.CopyProject.Tests;
public class AppConfigurationServiceTests {
    [Test]
    public async Task GetRootFolderTest() {
        var sampleFolder = GetRootFolder("sample");
        var acs1 = new AppConfigurationService(
            Options.Create<AppConfiguration>(new AppConfiguration() {
                RootFolder = sampleFolder
            }),
            new DummyLogger<AppConfigurationService>());

        var acs2 = new AppConfigurationService(
            Options.Create<AppConfiguration>(new AppConfiguration() {
                RootFolder = "./sample",
                RootMarkerFile = "sample/notbuild/marker.txt"
            }),
            new DummyLogger<AppConfigurationService>());
        await Assert.That(acs1.GetRootFolder()).IsEqualTo(sampleFolder);
        await Assert.That(acs2.GetRootFolder()).IsEqualTo(sampleFolder);
    }

    [Test]
    public async Task GetSettingsFolderTest() {
        var sampleFolder = GetRootFolder("sample");
        var sampleSettingsFolder = System.IO.Path.Combine(GetRootFolder("sample"), "settings");
        var acs1 = new AppConfigurationService(
            Options.Create<AppConfiguration>(new AppConfiguration() {
                RootFolder = sampleFolder,
                SettingsFolder = "settings"
            }),
            new DummyLogger<AppConfigurationService>());
        var acs2 = new AppConfigurationService(
            Options.Create<AppConfiguration>(new AppConfiguration() {
                RootFolder = "./sample",
                RootMarkerFile = "sample/notbuild/marker.txt",
                SettingsFolder = "settings"
            }),
            new DummyLogger<AppConfigurationService>());
        await Assert.That(acs1.GetSettingsFolder()).IsEqualTo(sampleSettingsFolder);
        await Assert.That(acs2.GetSettingsFolder()).IsEqualTo(sampleSettingsFolder);
    }

    [Test]
    public async Task GetSettingsFileTest() {
        var sampleFolder = GetRootFolder("sample");
        var sampleSettingsFile = System.IO.Path.Combine(GetRootFolder("sample"), "settings", "settings.json");
        var acs1 = new AppConfigurationService(
            Options.Create<AppConfiguration>(new AppConfiguration() {
                RootFolder = sampleFolder,
                SettingsFolder = "settings",
                SettingsFile = "settings.json"
            }),
            new DummyLogger<AppConfigurationService>());
        var acs2 = new AppConfigurationService(
            Options.Create<AppConfiguration>(new AppConfiguration() {
                RootFolder = "./sample",
                RootMarkerFile = "sample/notbuild/marker.txt",
                SettingsFolder = "settings",
                SettingsFile = "settings.json"
            }),
            new DummyLogger<AppConfigurationService>());
        await Assert.That(acs1.GetSettingsFile()).IsEqualTo(sampleSettingsFile);
        await Assert.That(acs2.GetSettingsFile()).IsEqualTo(sampleSettingsFile);
    }

    [Test]
    public async Task SaveLoadSettingsFileTest() {
        var sampleFolder = GetRootFolder("sample");
        var sampleSettingsFile = System.IO.Path.Combine(GetRootFolder("sample"), "settings", "settings.json");

        System.IO.File.WriteAllText(sampleSettingsFile, "{}");

        var acs = new AppConfigurationService(
            Options.Create<AppConfiguration>(new AppConfiguration() {
                RootFolder = sampleFolder,
                SettingsFolder = "settings",
                SettingsFile = "settings.json"
            }),
            new DummyLogger<AppConfigurationService>());
        CopyProjectSettings copyProjectSettings =
            new() {
                ExcludeFolderNames = ["bin", "obj", "node_modules"],
                Projects = [
                    new () {
                        SourcePath = "settings/project1src",
                        TargetPath = "settings/project1dst"
                    }
                ]
            };
        acs.SaveCopyProjectSettings(copyProjectSettings);

        var listCopyProjectSettingsActual = acs.LoadCopyProjectSettings();
        await Assert.That(listCopyProjectSettingsActual).IsEquivalentTo(copyProjectSettings);
    }

    private static string GetRootFolder(string name, [CallerFilePath] string path = "") {
        for (int i = 0; i < 3; i++) {
            var directory = System.IO.Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory)) { throw new InvalidOperationException("Cannot be."); }
            path = directory;
        }
        return System.IO.Path.Combine(path, name);
    }
}
