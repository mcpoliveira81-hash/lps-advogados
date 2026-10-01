# Promove h4 do rodapé para h3 (correção de heading-order / acessibilidade)
# Nota: script mantido em ASCII; acentos são capturados via regex ($1).
$root = Split-Path -Parent $PSScriptRoot
$files = Get-ChildItem -Path $root -Recurse -Filter *.html | Where-Object { $_.FullName -notmatch '\\tools\\' }
$utf8 = New-Object System.Text.UTF8Encoding($false)

$changed = 0
foreach ($f in $files) {
  $orig = [System.IO.File]::ReadAllText($f.FullName)
  $t = $orig
  $t = [regex]::Replace($t, '<h4>(Navega..o)</h4>', '<h3>$1</h3>')
  $t = [regex]::Replace($t, '<h4>(Contato)</h4>', '<h3>$1</h3>')
  $t = [regex]::Replace($t, '<h4>(Redes)</h4>', '<h3>$1</h3>')
  if ($t -ne $orig) {
    [System.IO.File]::WriteAllText($f.FullName, $t, $utf8)
    $changed++
    "corrigido: " + $f.FullName.Substring($root.Length + 1)
  }
}
"arquivos alterados: $changed"

# Verificação de ordem de headings em todos os arquivos
$problems = @()
foreach ($f in $files) {
  $html = [System.IO.File]::ReadAllText($f.FullName)
  $levels = [regex]::Matches($html, '<h([1-6])[ >]') | ForEach-Object { [int]$_.Groups[1].Value }
  $prev = 0
  foreach ($lv in $levels) {
    if ($prev -gt 0 -and $lv -gt $prev + 1) {
      $problems += ($f.FullName.Substring($root.Length + 1) + " : h$prev -> h$lv")
    }
    $prev = $lv
  }
}
""
if ($problems.Count -eq 0) { "ORDEM DE HEADINGS: OK em todos os arquivos" }
else { "PROBLEMAS:"; $problems | ForEach-Object { "  - $_" } }
