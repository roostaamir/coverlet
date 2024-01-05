﻿// Copyright (c) Toni Solarin-Sodara
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Coverlet.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Coverlet.Integration.Tests
{
  public class Msbuild : BaseTest
  {
    private readonly string _buildConfiguration;
    private readonly string _buildTargetFramework;
    private readonly ITestOutputHelper _output;

    public Msbuild(ITestOutputHelper output)
    {
      _buildConfiguration = TestUtils.GetAssemblyBuildConfiguration().ToString();
      _buildTargetFramework = TestUtils.GetAssemblyTargetFramework();
      _output = output;
    }

    private ClonedTemplateProject PrepareTemplateProject()
    {
      ClonedTemplateProject clonedTemplateProject = CloneTemplateProject();
      UpdateNugetConfigWithLocalPackageFolder(clonedTemplateProject.ProjectRootPath!);
      AddCoverletMsbuildRef(clonedTemplateProject.ProjectRootPath!);
      return clonedTemplateProject;
    }

    [Fact]
    public void TestMsbuild()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      bool result = DotnetCli($"test -c {_buildConfiguration} -f {_buildTargetFramework} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\", out string standardOutput, out string standardError);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.True(result);
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);
      string coverageFileName = $"coverage.{_buildTargetFramework}.json";
      Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, coverageFileName)));
      AssertCoverage(clonedTemplateProject, coverageFileName);
    }

    [Fact]
    public void TestMsbuild_NoCoverletOutput()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      bool result = DotnetCli($"test -c {_buildConfiguration} -f {_buildTargetFramework} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true", out string standardOutput, out string standardError);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.True(result);
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);
      string coverageFileName = $"coverage.{_buildTargetFramework}.json";
      Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, coverageFileName)));
      AssertCoverage(clonedTemplateProject, coverageFileName);
    }

    [Fact]
    public void TestMsbuild_CoverletOutput_Folder_FileNameWithoutExtension()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      bool result = DotnetCli($"test -c {_buildConfiguration} -f {_buildTargetFramework} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file", out string standardOutput, out string standardError);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.True(result); Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);
      string coverageFileName = $"file.{_buildTargetFramework}.json";
      Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, coverageFileName)));
      AssertCoverage(clonedTemplateProject, coverageFileName);
    }

    [Fact]
    public void TestMsbuild_CoverletOutput_Folder_FileNameExtension()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      Assert.True(DotnetCli($"test -c {_buildConfiguration} -f {_buildTargetFramework} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file.ext", out string standardOutput, out string standardError), standardOutput);
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);
      string coverageFileName = $"file.{_buildTargetFramework}.ext";
      Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, coverageFileName)));
      AssertCoverage(clonedTemplateProject, coverageFileName);
    }

    [Fact]
    public void TestMsbuild_CoverletOutput_Folder_FileNameExtension_SpecifyFramework()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      string[] targetFrameworks = new string[] { "net6.0" };
      UpdateProjectTargetFramework(clonedTemplateProject, targetFrameworks);
      Assert.False(clonedTemplateProject.IsMultipleTargetFramework());
      string framework = clonedTemplateProject.GetTargetFrameworks().Single();
      DotnetCli($"test -c {_buildConfiguration} -f {framework} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file.ext", out string standardOutput, out string standardError);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);
      Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, "file.ext")));
      AssertCoverage(clonedTemplateProject, "file.ext");
    }

    [Fact]
    public void TestMsbuild_CoverletOutput_Folder_FileNameWithDoubleExtension()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      DotnetCli($"test -c {_buildConfiguration} -f {_buildTargetFramework} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file.ext1.ext2", out string standardOutput, out string standardError);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);
      string coverageFileName = $"file.ext1.{_buildTargetFramework}.ext2";
      Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, coverageFileName)));
      AssertCoverage(clonedTemplateProject, coverageFileName);
    }

    [Fact]
    public void Test_MultipleTargetFrameworkReport_NoCoverletOutput()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      string[] targetFrameworks = new string[] { "net6.0", "net7.0", "net8.0" };
      UpdateProjectTargetFramework(clonedTemplateProject, targetFrameworks);
      DotnetCli($"test -c {_buildConfiguration} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true", out string standardOutput, out string standardError, clonedTemplateProject.ProjectRootPath!);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);

      foreach (string targetFramework in targetFrameworks)
      {
        Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, $"coverage.{targetFramework}.json")));
      }

      AssertCoverage(clonedTemplateProject, "coverage.*.json");
    }

    [Fact]
    public void Test_MultipleTargetFrameworkReport_CoverletOutput_Folder()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      string[] targetFrameworks = new string[] { "net6.0", "net7.0", "net8.0" };
      UpdateProjectTargetFramework(clonedTemplateProject, targetFrameworks);
      bool result = DotnetCli($"test -c {_buildConfiguration} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\", out string standardOutput, out string standardError, clonedTemplateProject.ProjectRootPath!);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.True(result);
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);

      foreach (string targetFramework in targetFrameworks)
      {
        string fileToCheck = Path.Combine(clonedTemplateProject.ProjectRootPath, $"coverage.{targetFramework}.json");
        Assert.True(File.Exists(fileToCheck), $"Expected file '{fileToCheck}'\nOutput:\n{standardOutput}");
      }

      AssertCoverage(clonedTemplateProject, "coverage.*.json");
    }

    [Fact]
    public void Test_MultipleTargetFrameworkReport_CoverletOutput_Folder_FileNameWithoutExtension()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      string[] targetFrameworks = new string[] { "net6.0", "net7.0", "net8.0" };
      UpdateProjectTargetFramework(clonedTemplateProject, targetFrameworks);
      DotnetCli($"test -c {_buildConfiguration} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file", out string standardOutput, out string standardError, clonedTemplateProject.ProjectRootPath!);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);

      foreach (string targetFramework in targetFrameworks)
      {
        Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, $"file.{targetFramework}.json")));
      }

      AssertCoverage(clonedTemplateProject, "file.*.json");
    }

    [Fact]
    public void Test_MultipleTargetFrameworkReport_CoverletOutput_Folder_FileNameWithExtension_SpecifyFramework()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      string[] targetFrameworks = new string[] { "net6.0", "net7.0", "net8.0" };
      UpdateProjectTargetFramework(clonedTemplateProject, targetFrameworks);
      Assert.True(clonedTemplateProject.IsMultipleTargetFramework());
      string[] frameworks = clonedTemplateProject.GetTargetFrameworks();
      Assert.Equal(3, frameworks.Length);
      string framework = frameworks.FirstOrDefault()!;
      DotnetCli($"test -c {_buildConfiguration} -f {framework} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file.ext", out string standardOutput, out string standardError, clonedTemplateProject.ProjectRootPath!);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);

      foreach (string targetFramework in targetFrameworks)
      {
        if (framework == targetFramework)
        {
          Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, $"file.{targetFramework}.ext")));
        }
        else
        {
          Assert.False(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, $"file.{targetFramework}.ext")));
        }
      }

      AssertCoverage(clonedTemplateProject, "file.*.ext");
    }

    [Fact]
    public void Test_MultipleTargetFrameworkReport_CoverletOutput_Folder_FileNameWithExtension()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      string[] targetFrameworks = new string[] { "net6.0", "net7.0", "net8.0" };
      UpdateProjectTargetFramework(clonedTemplateProject, targetFrameworks);
      DotnetCli($"test -c {_buildConfiguration} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file.ext", out string standardOutput, out string standardError, clonedTemplateProject.ProjectRootPath!);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);

      foreach (string targetFramework in targetFrameworks)
      {
        Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, $"file.{targetFramework}.ext")));
      }

      AssertCoverage(clonedTemplateProject, "file.*.ext");
    }

    [Fact]
    public void Test_MultipleTargetFrameworkReport_CoverletOutput_Folder_FileNameWithDoubleExtension()
    {
      using ClonedTemplateProject clonedTemplateProject = PrepareTemplateProject();
      string[] targetFrameworks = new string[] { "net6.0", "net7.0", "net8.0" };
      UpdateProjectTargetFramework(clonedTemplateProject, targetFrameworks);
      DotnetCli($"test -c {_buildConfiguration} \"{clonedTemplateProject.ProjectRootPath}\" /p:CollectCoverage=true /p:Include=\"[{ClonedTemplateProject.AssemblyName}]*DeepThought\" /p:IncludeTestAssembly=true /p:CoverletOutput=\"{clonedTemplateProject.ProjectRootPath}\"\\file.ext1.ext2", out string standardOutput, out string standardError, clonedTemplateProject.ProjectRootPath!);
      if (!string.IsNullOrEmpty(standardError))
      {
        _output.WriteLine(standardError);
      }
      else
      {
        _output.WriteLine(standardOutput);
      }
      Assert.Contains("Passed!", standardOutput, StringComparison.Ordinal);
      Assert.Contains("| coverletsamplelib.integration.template | 100% | 100%   | 100%   |", standardOutput, StringComparison.Ordinal);

      foreach (string targetFramework in targetFrameworks)
      {
        Assert.True(File.Exists(Path.Combine(clonedTemplateProject.ProjectRootPath, $"file.ext1.{targetFramework}.ext2")));
      }

      AssertCoverage(clonedTemplateProject, "file.ext1.*.ext2");
    }
  }
}
