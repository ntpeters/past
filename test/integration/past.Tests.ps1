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
        $expectedContent = "This is a test!"
        Set-Clipboard $expectedContent

        # Act
        $actualContent = Invoke-NativeCommand -Command $past

        # Assert
        $actualContent | Should -BeExactly $expectedContent
    }
}
