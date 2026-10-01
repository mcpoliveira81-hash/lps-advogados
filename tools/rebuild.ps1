powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\build-images.ps1"
if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) { exit $LASTEXITCODE }

Add-Type -AssemblyName System.Drawing
$b = New-Object System.Drawing.Bitmap("assets\img\logo-lps-dourado.png")
"WPSA saida: $($b.Width)x$($b.Height) format=$($b.PixelFormat)"
# Linhas superiores/inferiores devem ser 100% transparentes (sem resquício do quadro)
foreach ($y in @(0, 1, 2, 3, $b.Height - 4, $b.Height - 3, $b.Height - 2, $b.Height - 1)) {
  $op = 0
  for ($x = 0; $x -lt $b.Width; $x++) { if ($b.GetPixel($x, $y).A -gt 30) { $op++ } }
  "  row {0}: opacos={1}/{2}" -f $y, $op, $b.Width
}
# Total de pixels opacos
$tot = 0
for ($y = 0; $y -lt $b.Height; $y++) { for ($x = 0; $x -lt $b.Width; $x++) { if ($b.GetPixel($x, $y).A -gt 30) { $tot++ } } }
"  opacos total: {0}/{1} ({2:N1}%)" -f $tot, ($b.Width * $b.Height), (100 * $tot / ($b.Width * $b.Height))
$b.Dispose()
