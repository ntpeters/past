param(
    [switch]
    $Published,
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Debug'
)

. "$PSScriptRoot\TestBase.ps1"

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
