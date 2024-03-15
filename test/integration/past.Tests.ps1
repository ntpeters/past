param(
    [switch]
    $Published,
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Debug'
)

BeforeDiscovery {
    Import-Module "$PSScriptRoot/TestUtil.psm1" -Force

    $Script:past = Get-PastPath -Configuration $Configuration -Published:$Published
    Write-Debug "Using Past Path: $past"
}

Describe 'past' {
    It 'Outputs current clipboard contents' {
        # Arrange
        $expectedExitCode = 0
        $expectedContent = "This is a test!"
        Set-Clipboard $expectedContent

        # Act
        $actualContent = Invoke-NativeCommand -Command $past
        $actualExitCode = $LASTEXITCODE

        # Assert
        $actualContent | Should -BeExactly $expectedContent
        $actualExitCode | Should -BeExactly $expectedExitCode
    }
}
