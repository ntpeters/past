param(
    [switch]
    $Published,
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Debug'
)

. "$PSScriptRoot\TestBase.ps1"

BeforeDiscovery {
    $testCases = @(
        @{ HistoryEnabled = $true }
        @{ HistoryEnabled = $false }
    )
}

Describe 'past status' {
    BeforeEach {
        $originalClipboardHistoryEnabled = Get-ClipboardHistoryEnabled
        $roamingEnabled = Get-ClipboardRoamingEnabled
    }

    AfterEach {
        Set-ClipboardHistoryEnabled -Value $originalClipboardHistoryEnabled
    }

    It 'Output and exit code match clipboard history state - HistoryEnabled=<historyEnabled>' -ForEach $testCases {
        # Arrange
        Set-ClipboardHistoryEnabled -Value $historyEnabled
        $expectedOutput = "Clipboard History Enabled: $historyEnabled$([Environment]::NewLine)Clipboard Roaming Enabled: $roamingEnabled$([Environment]::NewLine)"
        $expectedExitCode = [int]$historyEnabled + ([int]$roamingEnabled * 2)

        # Act
        $actualOutput = Invoke-NativeCommand -Command $past -Arguments "status"
        $actualExitCode = $LASTEXITCODE

        # Assert
        $actualOutput | Should -BeExactly $expectedOutput
        $actualExitCode | Should -BeExactly $expectedExitCode
    }
}

