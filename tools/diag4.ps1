$root = Split-Path -Parent $PSScriptRoot
$html = [System.IO.File]::ReadAllText((Join-Path $root 'areas-de-atuacao\index.html'))
$ms = [regex]::Matches($html, '<h4>[^<]*</h4>')
"h4 diretos: $($ms.Count)"
foreach ($m in $ms) {
  $s = $m.Value
  $codes = ($s.ToCharArray() | ForEach-Object { [int]$_ }) -join ','
  "  [$s] codes: $codes"
}
# teste de regex
"teste Navega.g.o: " + ([regex]::IsMatch($html, '<h4>(Navega.g.o)</h4>'))
"teste qualquer: " + ([regex]::IsMatch($html, '<h4>[^<]+</h4>'))
