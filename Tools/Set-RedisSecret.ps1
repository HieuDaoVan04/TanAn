$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$secret = Read-Host 'Nhap mat khau Redis Cloud ca nhan (khong hien thi)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secret)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    if ([string]::IsNullOrWhiteSpace($password)) { throw 'Mat khau khong duoc de trong.' }
    $payload = @{ 'Redis:Password' = $password; 'Redis:Enabled' = 'true' } | ConvertTo-Json -Compress
    foreach ($project in @('src/Service.TanAn/Service.TanAn.API', 'src/Service.UI/Service.UI.CMS.Blazor')) {
        $payload | dotnet user-secrets set --project (Join-Path $workspace $project)
        if ($LASTEXITCODE -ne 0) { throw 'Khong luu duoc User Secrets.' }
    }
    Write-Host 'Da luu Redis cho API va Blazor. Chay lai ung dung trong Development.'
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    $password = $null
    $payload = $null
}
