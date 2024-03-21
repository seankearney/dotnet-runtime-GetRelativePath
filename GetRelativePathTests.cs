using System.Diagnostics;
using Xunit;

namespace CaseSensitiveGetRelativePath;

public class GetRelativePathTests
{
    private readonly string _caseSensitiveRoot;
    private readonly string _folder_a,
                            _folder_A,
                            _folder_b;

    public GetRelativePathTests()
    {
        _caseSensitiveRoot = Path.Combine(Path.GetTempPath(), "case-sensitive-directory");
        if (!Directory.Exists(_caseSensitiveRoot))
        {
            Directory.CreateDirectory(_caseSensitiveRoot);
            TrySetDirectoryCaseSensitivity(true, _caseSensitiveRoot);
        }

        _folder_a = Path.Combine(_caseSensitiveRoot, "a");
        _folder_A = Path.Combine(_caseSensitiveRoot, "A");
        _folder_b = Path.Combine(_caseSensitiveRoot, "b");

        Directory.CreateDirectory(_folder_a);
        Directory.CreateDirectory(_folder_A);
        Directory.CreateDirectory(_folder_b);

        /*
        C:\Users\_you_\AppData\Local\Temp\case-sensitive-directory
            +---A
            +---a
            +---b
         */
    }

    [Fact]
    public void a_and_A__should_not_be_the_same()
    {
        string a_relativeTo_a = Path.GetRelativePath(_folder_a, _folder_a);
        string A_relativeTo_A = Path.GetRelativePath(_folder_A, _folder_A);
        string a_relativeTo_b = Path.GetRelativePath(_folder_b, _folder_a);
        string a_relativeTo_A = Path.GetRelativePath(_folder_A, _folder_a);

        Assert.True(A_relativeTo_A == ".");
        Assert.True(a_relativeTo_a == ".");
        Assert.True(a_relativeTo_b == "..\\a");

        // `b` and `A` are siblings and should have the same relative path to `a`
        // However Path.GetRelativePath _thinks_ that `A` and `a` are the same folder and returns `.`
        Assert.Multiple(
            () => Assert.True(a_relativeTo_A == "..\\a", "`a` is a sibling to `A`, should return `..\\a`"),
            () => Assert.False(a_relativeTo_A == ".", "`a` is not `A`")
            );
    }

    // Taken from https://github.com/dotnet/runtime/issues/39370
    private static bool TrySetDirectoryCaseSensitivity(bool enable, string path)
    {
        var status = enable ? "enable" : "disable";
        var fsutilpsi = new ProcessStartInfo("fsutil",
            $"file SetCaseSensitiveInfo \"{path}\" {status}")
        {
            CreateNoWindow = true,
            LoadUserProfile = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        var fsutil = Process.Start(fsutilpsi);
        var @out = fsutil.StandardOutput.ReadToEnd();
        var err = fsutil.StandardError.ReadToEnd();
        fsutil.WaitForExit();
        if (!string.IsNullOrEmpty(@out)) Debug.WriteLine(@out);
        if (!string.IsNullOrEmpty(err)) Debug.WriteLine($"ERROR: {err}");
        return fsutil.ExitCode == 0 && string.IsNullOrEmpty(err);
    }
}