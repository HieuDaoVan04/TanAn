$ErrorActionPreference = 'Stop'
$workspace = Split-Path -Parent $PSScriptRoot
$pagesRoot = Join-Path $workspace 'src/Service.UI/Service.UI.CMS.Blazor/Components/Pages'
$routes = @{}
Get-ChildItem -LiteralPath $pagesRoot -Filter '*.razor' -Recurse | ForEach-Object {
    $file = $_
    foreach ($match in [regex]::Matches([IO.File]::ReadAllText($file.FullName), '(?m)^@page\s+"([^"]+)"')) {
        $route = $match.Groups[1].Value
        if ($routes.ContainsKey($route)) { throw "Duplicate page route: $route" }
        $routes[$route] = $file.FullName
    }
}

foreach ($route in @('/quan-tri-he-thong/danh-muc/{Catalog}', '/an-sinh/{Nhom}')) {
    if ($routes.ContainsKey($route)) { throw "Generic menu dispatcher remains: $route" }
}
Write-Output "PASS: $($routes.Count) page routes; no duplicate routes or generic menu dispatcher."
