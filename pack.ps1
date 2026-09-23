param(
  # Release配置
  [string]$Configuration = "Release",
  # 输出目录
  [string]$Output = "./nupkg"
)

$ErrorActionPreference = "Stop"

Write-Host "获取 Git 最新 Tag..."

# 获取最新tag
$tag = git describe --tags --abbrev=0

if ([string]::IsNullOrWhiteSpace($tag)) {
  throw "没有找到 Git Tag"
}

Write-Host "Git Tag: $tag"


# 去掉 v 前缀
$version = $tag.TrimStart("v")

Write-Host "Package Version: $version"


# 简单SemVer检查
if ($version -notmatch '^\d+\.\d+\.\d+([\-\.].+)?$') {
  throw "Tag格式错误: $tag，要求类似 v1.0.0"
}


# 清理输出目录
if (Test-Path $Output) {
  Remove-Item $Output -Recurse -Force
}

New-Item $Output -ItemType Directory | Out-Null


Write-Host ""
Write-Host "开始打包..."

dotnet pack `
  -c $Configuration `
  -o $Output `
  /p:Version=$version `
  src/SuCaiFlow.Playwright/SuCaiFlow.Playwright.csproj

dotnet pack `
  -c $Configuration `
  -o $Output `
  /p:Version=$version `
  src/SuCaiFlow.Abstractions/SuCaiFlow.Abstractions.csproj

dotnet pack `
  -c $Configuration `
  -o $Output `
  /p:Version=$version `
  src/SuCaiFlow.Engine/SuCaiFlow.Engine.csproj

dotnet pack `
  -c $Configuration `
  -o $Output `
  /p:Version=$version `
  src/SuCaiFlow.EntityFrameworkCore.Models/SuCaiFlow.EntityFrameworkCore.Models.csproj

dotnet pack `
  -c $Configuration `
  -o $Output `
  /p:Version=$version `
  src/SuCaiFlow.EntityFrameworkCore/SuCaiFlow.EntityFrameworkCore.csproj

if ($LASTEXITCODE -ne 0) {
  throw "dotnet pack失败"
}


Write-Host ""
Write-Host "NuGet包生成完成:"
Get-ChildItem $Output -Filter "*.nupkg"
