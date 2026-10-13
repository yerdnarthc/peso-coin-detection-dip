# Run from any directory after building the application in Debug configuration.
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$output = Join-Path $repository 'CoinDenominationDIP\bin\Debug'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$application = Join-Path $output 'CoinDenominationDIP.exe'
if (-not (Test-Path $application)) { throw 'Build the Debug application first.' }
& $compiler /nologo "/out:$output\AnalyzerChecks.exe" /r:System.Drawing.dll /r:System.Core.dll /r:System.Windows.Forms.dll "/r:$application" (Join-Path $PSScriptRoot 'AnalyzerChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Check harness compilation failed.' }
& (Join-Path $output 'AnalyzerChecks.exe') (Join-Path $repository 'input-images\philippine-peso-coins.jpeg') $output
if ($LASTEXITCODE -ne 0) { throw 'Verification failed.' }
