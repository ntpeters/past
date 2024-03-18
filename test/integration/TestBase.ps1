BeforeDiscovery {
    if (-not (Get-Variable 'Configuration' -Scope Local -ErrorAction Ignore)) {
        throw "Test must define 'Configuration' parameter."
    }

    if (-not (Get-Variable 'Published' -Scope Local -ErrorAction Ignore)) {
        throw "Test must define 'Published' parameter."
    }

    $Script:testUtilModulePath = Resolve-Path "$PSScriptRoot/TestUtil.psm1"
}

BeforeAll {
    Import-Module -Force $testUtilModulePath

    $Script:past = Get-PastPath -Configuration $Configuration -Published:$Published
    Write-Debug "Using Past Path: $past"
}

AfterAll {
    Remove-Module TestUtil
}
