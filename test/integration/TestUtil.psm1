function Get-PastPath {
    param (
            [Parameter()]
            [switch]
            $Published = $false,
            [Parameter()]
            [ValidateSet('Debug', 'Release')]
            [string]
            $Configuration = 'Debug'
          )

    $repoRoot = "$PSScriptRoot/../.."
    $baseOutputPath = "$repoRoot/src/ConsoleApp/bin/$Configuration/net6.0-windows10.0.17763.0"
    if ($Published) {
        $baseOutputPath = "$baseOutputPath/win10-x64/publish"
    }
    return "$baseOutputPath/past.exe"
}

function Invoke-NativeCommand {
    param (
            [Parameter(Mandatory=$true)]
            [string]
            $Command,
            [Parameter(Mandatory=$false)]
            [string[]]
            $Arguments
          )

    $outputPath = "$PSScriptRoot/$(Split-Path -Leaf $Command)"
    if (($null -ne $Arguments) -and ($Arguments.Count -ne 0)) {
        $outputPath = "$outputPath`_$([string]::Join('_', $Arguments))"
    }
    $outputPath = "$outputPath.output"

    Write-Debug "Invoking native command: '$Command'; Arguments: '$Arguments'; Temp Output Path: '$outputPath'"

    & $Command @Arguments > $outputPath

    $content = Get-Content -Raw $outputPath
    Remove-Item -Path $outputPath

    return $content
}

$ClipboardRegistryKey = 'HKCU:\Software\Microsoft\Clipboard'

function Set-ClipboardHistoryEnabled {
    param (
            [Parameter(Mandatory=$true)]
            [bool]
            $Value
          )

    $EnableClipboardHistory = Get-ClipboardHistoryEnabled
    if ($EnableClipboardHistory -ne $Value) {
        Set-ItemProperty -Path $ClipboardRegistryKey -Name 'EnableClipboardHistory' -Value $([int]$Value)
        Write-Debug "Set '$ClipboardRegistryKey\EnableClipboardHistory'=$Value"
    }
}

function Get-ClipboardHistoryEnabled {
    [int]$EnableClipboardHistory = Get-ItemPropertyValue -Path $ClipboardRegistryKey -Name 'EnableClipboardHistory'
    return [bool]$EnableClipboardHistory
}

function Get-ClipboardRoamingEnabled {
    return & powershell.exe -NoProfile -Command { [Windows.ApplicationModel.DataTransfer.Clipboard,Windows.ApplicationModel.DataTransfer,ContentType=WindowsRuntime] | Out-Null; [Windows.ApplicationModel.DataTransfer.Clipboard]::IsRoamingEnabled() }
}

Export-ModuleMember -Function *
