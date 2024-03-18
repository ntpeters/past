param(
    [switch]
    $Published,
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Debug'
)

. "$PSScriptRoot\TestBase.ps1"

Describe 'past get' {
    It 'Outputs clipboard history item at index' {
        # Arrange
        $expectedExitCode = 0
        $expectedContent = "This is a test!"
        Set-Clipboard $expectedContent

        # Unfortunately it seems to take a moment for the history to reflect a new item via the API
        Start-Sleep -Milliseconds 100

        # Act
        $actualContent = Invoke-NativeCommand -Command $past -Arguments @("get", "0")
        $actualExitCode = $LASTEXITCODE

        # Assert
        $actualContent | Should -BeExactly $expectedContent
        $actualExitCode | Should -BeExactly $expectedExitCode
    }
}
