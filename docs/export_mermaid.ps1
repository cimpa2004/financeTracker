# Extract and export Mermaid code blocks from docs/**/*.md
# Requirements: node + npm (npx will auto-install mermaid-cli if needed)
# Usage: powershell -ExecutionPolicy Bypass -File .\docs\export_mermaid.ps1

$root = Join-Path $PSScriptRoot ".." | Resolve-Path | Select-Object -ExpandProperty Path
$docs = Join-Path $root "docs"
if (!(Test-Path $docs)) { Write-Error "Docs folder not found: $docs"; exit 2 }

# Check node
$node = ""
try { $node = (node -v) 2>&1 } catch { }
if ([string]::IsNullOrEmpty($node)) {
  Write-Host "Node.js not found. Please install Node.js (https://nodejs.org/) to use mermaid-cli (npx)." -ForegroundColor Yellow
  exit 3
}

Write-Host "Node detected: $node" -ForegroundColor Green

Get-ChildItem -Path $docs -Recurse -Include *.md | ForEach-Object {
  $mdPath = $_.FullName
  $mdText = Get-Content -Raw -Path $mdPath
  $matches = [regex]::Matches($mdText, '```mermaid\s*(.*?)```', 'Singleline')
  if ($matches.Count -eq 0) { return }
  $i = 0
  foreach ($m in $matches) {
    $i++
    $mmd = $m.Groups[1].Value.Trim()
    if ($mmd -eq '') { continue }
    $outDir = Join-Path -Path $_.DirectoryName -ChildPath "diagrams"
    if (!(Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($mdPath)
    $mmdFile = Join-Path $outDir ("{0}_{1}.mmd" -f $baseName, $i)
    $pngFile = [System.IO.Path]::ChangeExtension($mmdFile, ".png")
    $svgFile = [System.IO.Path]::ChangeExtension($mmdFile, ".svg")

    Set-Content -Path $mmdFile -Value $mmd -Encoding UTF8
    Write-Host "Exporting: $mmdFile -> $pngFile and $svgFile"

    # Use npx to run mermaid-cli; --yes avoids prompts and will install if missing
    $npxArgs = "--yes @mermaid-js/mermaid-cli -i `"$mmdFile`" -o `"$pngFile`" --width 1024"
    Write-Host "Running: npx $npxArgs"
    $rc1 = (& npx --yes @mermaid-js/mermaid-cli -i $mmdFile -o $pngFile --width 1024) 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Warning "PNG render failed: $rc1" }

    # SVG
    $rc2 = (& npx --yes @mermaid-js/mermaid-cli -i $mmdFile -o $svgFile) 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Warning "SVG render failed: $rc2" }
  }
}

Write-Host "Done. Generated diagrams/ folders next to processed Markdown files." -ForegroundColor Green
