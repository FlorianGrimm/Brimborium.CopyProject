// Copyright 2010 Google Inc.
// All Right Reserved.

/*
 * To compile with Mono:
 *   mcs Speedtest.cs ../DiffMatchPatch.cs
 * To run with Mono:
 *   mono Speedtest.exe
*/

namespace Brimborium.CopyProject.DiffMatchPatch;

public class Speedtest {
    [Test]
    public async Task Speed() {
        string text1 = System.IO.File.ReadAllText(GetFileName("Speedtest1.txt"));
        string text2 = System.IO.File.ReadAllText(GetFileName("Speedtest2.txt"));

        DiffMatchPatcher dmp = new DiffMatchPatcher();
        dmp.Diff_Timeout = 0;

        // Execute one reverse diff as a warmup.
        dmp.Diff_main(text2, text1);
        GC.Collect();
        GC.WaitForPendingFinalizers();

        DateTime ms_start = DateTime.Now;
        dmp.Diff_main(text1, text2);
        DateTime ms_end = DateTime.Now;

        var elapsed = (ms_end - ms_start).TotalMilliseconds;
        await Assert.That(elapsed).IsLessThan(1000);
        System.Console.Out.WriteLine($"Elapsed time: {elapsed} ms");
    }
    private static string GetFileName(string filename, [CallerFilePath] string path = "")
        => System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(path) ?? "",
            filename);
}
