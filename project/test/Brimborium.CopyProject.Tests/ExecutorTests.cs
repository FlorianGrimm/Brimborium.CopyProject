namespace Brimborium.CopyProject.Tests;

public class ExecutorTests {
    [Test]
    public async Task ExecutorTest() {
        var folderPath = GetFolderPath();
        var rootPath = System.IO.Path.Combine(folderPath, "sample1");
        var settingsPath = System.IO.Path.Combine(rootPath, "settings", "copy.json");

        var aPath = System.IO.Path.Combine(rootPath, "settings", "a.json");
        System.IO.File.WriteAllText(aPath, "[]");

        var bPath = System.IO.Path.Combine(rootPath, "settings", "b.json");
        System.IO.File.WriteAllText(bPath, "[]");

        var dstPath = System.IO.Path.Combine(rootPath, "dst");
        if (System.IO.Directory.Exists(dstPath)) {
            System.IO.Directory.Delete(dstPath, true);
        }
        System.IO.Directory.CreateDirectory(dstPath);

        // 1 ScanFolder
        {
            var program = new Program();
            var result = await program.RunAsync(new string[] {
                "ScanFolder",
                "--RootFolder", rootPath,
                "--SettingsFile", @$"{rootPath}\settings\copy.json"
            });
            await Assert.That(result).IsEqualTo(0);
            await Assert.That(System.IO.File.ReadAllText(aPath)).IsEqualTo(
                """
                [
                  {
                    "Path": "a1.txt",
                    "Action": ""
                  },
                  {
                    "Path": "a2.txt",
                    "Action": ""
                  },
                  {
                    "Path": "a3/a4.txt",
                    "Action": ""
                  }
                ]
                """);
            await Assert.That(System.IO.File.ReadAllText(bPath)).IsEqualTo(
                """
                [
                  {
                    "Path": "b1.txt",
                    "Action": ""
                  },
                  {
                    "Path": "b2/b3.txt",
                    "Action": ""
                  }
                ]
                """);
        }

        // 2 Copy
        {
            var program = new Program();
            var result = await program.RunAsync(new string[] {
                "Copy",
                "--RootFolder", rootPath,
                "--SettingsFile", @$"{rootPath}\settings\copy.json"
            });
            await Assert.That(result).IsEqualTo(0);
        }

        // 3 Update
        {
            var program = new Program();
            var result = await program.RunAsync(new string[] {
                "Update",
                "--RootFolder", rootPath,
                "--SettingsFile", @$"{rootPath}\settings\copy.json"
            });
            await Assert.That(result).IsEqualTo(0);

            await Assert.That(System.IO.File.ReadAllText(aPath)).IsEqualTo(
                """
                [
                  {
                    "Path": "a1.txt",
                    "Action": "copy"
                  },
                  {
                    "Path": "a2.txt",
                    "Action": "copy"
                  },
                  {
                    "Path": "a3/a4.txt",
                    "Action": "copy"
                  }
                ]
                """);
            await Assert.That(System.IO.File.ReadAllText(bPath)).IsEqualTo(
                """
                [
                  {
                    "Path": "b1.txt",
                    "Action": "copy"
                  },
                  {
                    "Path": "b2/b3.txt",
                    "Action": "copy"
                  }
                ]
                """);
        }

        // 4 Update
        {
            System.IO.File.WriteAllText(bPath,
                """
                [
                  {
                    "Path": "b1.txt",
                    "Action": "copy"
                  },
                  {
                    "Path": "b2/b3.txt",
                    "Action": ""
                  }
                ]
                """);
            System.IO.File.Delete(@"C:\github\FlorianGrimm\Brimborium.CodeAsCode\submodule\Brimborium.CopyProject\project\test\Brimborium.CopyProject.Tests\sample1\dst\b\b2\b3.txt");
            {
                var program = new Program();
                var result = await program.RunAsync(new string[] {
                    "Update",
                    "--RootFolder", rootPath,
                    "--SettingsFile", @$"{rootPath}\settings\copy.json"
                });
                await Assert.That(result).IsEqualTo(0);
            }
            await Assert.That(System.IO.File.ReadAllText(bPath)).IsEqualTo(
                """
                [
                  {
                    "Path": "b1.txt",
                    "Action": "copy"
                  },
                  {
                    "Path": "b2/b3.txt",
                    "Action": "delete"
                  }
                ]
                """);
        }
    }

    private static string GetFolderPath([CallerFilePath] string path = "")
        => System.IO.Path.GetDirectoryName(path) ?? throw new Exception("cannotbe");
}
