Add-Type -AssemblyName System.Drawing
$b = New-Object System.Drawing.Bitmap("assets\img\logo-lps-dourado.png")
$h = $b.Height
$w = $b.Width
$rows = @(0, 1, 2, 3, ($h - 4), ($h - 3), ($h - 2), ($h - 1))
foreach ($y in $rows) {
  $op = 0
  for ($x = 0; $x -lt $w; $x++) { if ($b.GetPixel($x, $y).A -gt 30) { $op++ } }
  "row {0}: opacos={1}/{2}" -f $y, $op, $w
}
# Amostras de canto e centro-fundo
foreach ($pt in @(@(0,0), @(599,0), @(0,220), @(599,220), @(300,5), @(300,215))) {
  $p = $b.GetPixel($pt[0], $pt[1])
  "({0},{1}) A={2} RGB=({3},{4},{5})" -f $pt[0], $pt[1], $p.A, $p.R, $p.G, $p.B
}
$b.Dispose()
