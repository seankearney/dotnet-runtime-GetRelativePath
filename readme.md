`System.IO.Path.GetRelativePath` is returning incorrect values when used within a case-sensitive directory.
============================================================================================================

## Description

`System.IO.Path.GetRelativePath` is returning incorrect values when used within a case-sensitive directory.

## Repro

Run the provided unit test, or setup a case-sensitive directory structure as follows:

```
C:\Temp\Case-Sensitive-Directory
    +---A
    +---a
    +---b
```

Within an Admin Command Prompt, run the following commands:

```cmd
cd %temp%
md case-sensitive-directory
fsutil file setCaseSensitiveInfo case-sensitive-directory enable
cd case-sensitive-directory
md a
md A
md b
```

Run the following C# Code

```csharp
string path_a = Path.Combine(Path.GetTempPath(), "case-sensitive-directory", "a");
string path_b = Path.Combine(Path.GetTempPath(), "case-sensitive-directory", "b");
string path_A = Path.Combine(Path.GetTempPath(), "case-sensitive-directory", "A");

string a_relativeTo_a = Path.GetRelativePath(path_a, path_a);
string A_relativeTo_A = Path.GetRelativePath(path_A, path_A);
string a_relativeTo_b = Path.GetRelativePath(path_b, path_a);
string a_relativeTo_A = Path.GetRelativePath(path_A, path_a);

Console.WriteLine(a_relativeTo_a);
Console.WriteLine(A_relativeTo_A);
Console.WriteLine(a_relativeTo_b);
Console.WriteLine(a_relativeTo_A);
```

## Expected Output

```
.
.
..\a
..\a
```

## Actual Output

```
.
.
..\a
.
```

- :white_check_mark: A call to `System.IO.Path.GetRelativePath( A, A )` will return `.` which indicates that `A` is the same as `A`
- :white_check_mark: A call to `System.IO.Path.GetRelativePath( a, a )` will return `.` which indicates that `a` is the same as `a`
- :white_check_mark: A call to `System.IO.Path.GetRelativePath( b, a )` will return `..\a` which indicates that `b` is a sibling of `a`.
- :x: A call to `System.IO.Path.GetRelativePath( A, a )` **should** return `..\a` which indicates that `A` is a sibling of `a`. However, it returns `a` which indicates that `A` is `a`.


## Configuration

- net 8.0.100
- Windows 10
- x64



