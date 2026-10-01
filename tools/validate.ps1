# Validação do site: links locais, títulos, h1, classes CSS
$root = Split-Path -Parent $PSScriptRoot
$css = Get-Content -Raw (Join-Path $root "assets\css\style.css")
$htmlFiles = Get-ChildItem -Path $root -Recurse -Filter *.html | Where-Object { $_.FullName -notmatch '\\tools\\' }

$problems = @()

foreach ($f in $htmlFiles) {
  $html = Get-Content -Raw $f.FullName
  $rel = $f.FullName.Substring($root.Length + 1)

  # title
  if ($html -notmatch '<title>([^<]+)</title>') { $problems += "$rel : SEM TITLE" }

  # h1 count
  $h1 = ([regex]::Matches($html, '<h1[ >]')).Count
  if ($h1 -ne 1) { $problems += "$rel : h1 count = $h1" }

  # lang
  if ($html -notmatch '<html lang="pt-BR">') { $problems += "$rel : sem lang pt-BR" }

  # links/refs locais
  $refs = [regex]::Matches($html, '(?:href|src)="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
  foreach ($r in $refs) {
    if ($r -match '^(https?:|tel:|mailto:|data:|javascript:|#)') { continue }
    $path = ($r -split '#')[0]
    if (-not $path) { continue }
    $resolved = Join-Path $f.DirectoryName ($path -replace '/', '\')
    if (-not (Test-Path $resolved)) { $problems += "$rel : LINK QUEBRADO -> $r" }
  }

  # classes usadas que nao existem no CSS (checagem simples de tokens de classe)
  $classAttrs = [regex]::Matches($html, 'class="([^"]+)"') | ForEach-Object { $_.Groups[1] }
  $used = New-Object System.Collections.Generic.HashSet[string]
  foreach ($a in $classAttrs) { foreach ($c in ($a -split '\s+')) { if ($c) { [void]$used.Add($c) } } }
  foreach ($c in $used) {
    if ($c -match '^(is-visible|is-scrolled|is-open)$') { continue }  # classes de estado via JS
    if ($css -notmatch ('\.' + [regex]::Escape($c) + '[\s\.,:{>#\[]')) {
      $problems += "$rel : classe sem estilo -> .$c"
    }
  }

  # palavras vedadas
  foreach ($bad in @('depoimento', 'avaliações de clientes', 'casos de sucesso', 'resultados obtidos',
                     'causas ganhas', 'garantimos', 'melhor advogado', 'especialidade')) {
    if ($html -match ('(?i)' + [regex]::Escape($bad))) { $problems += "$rel : CONTEUDO VEDADO -> $bad" }
  }
}

"Arquivos HTML: $($htmlFiles.Count)"
$htmlFiles | ForEach-Object { "  " + $_.FullName.Substring($root.Length + 1) }
""
if ($problems.Count -eq 0) { "TODAS AS VERIFICACOES PASSARAM" } else { "PROBLEMAS:"; $problems | ForEach-Object { "  - $_" } }
