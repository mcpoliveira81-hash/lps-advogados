Add-Type -AssemblyName System.Drawing
$b = New-Object System.Drawing.Bitmap("684165031_18191320006367367_2291330546204645952_n.jpg")
$W = $b.Width; $H = $b.Height
$bg = $b.GetPixel(300, 200)  # may be letter; use corner instead
$bg = $b.GetPixel(1, 1)

function Dist($a, $c) {
  $dr = $a.R - $c.R; $dg = $a.G - $c.G; $db = $a.B - $c.B
  [int][math]::Sqrt($dr*$dr + $dg*$dg + $db*$db)
}

"Rows with >60% content pixels (frame lines):"
for ($y = 0; $y -lt $H; $y++) {
  $n = 0
  for ($x = 0; $x -lt $W; $x++) { if ((Dist $b.GetPixel($x,$y) $bg) -gt 45) { $n++ } }
  if ($n -gt $W * 0.6) { "  row {0}: {1}/{2}" -f $y, $n, $W }
}
"Cols with >60% content pixels:"
for ($x = 0; $x -lt $W; $x++) {
  $n = 0
  for ($y = 0; $y -lt $H; $y++) { if ((Dist $b.GetPixel($x,$y) $bg) -gt 45) { $n++ } }
  if ($n -gt $H * 0.6) { "  col {0}: {1}/{2}" -f $x, $n, $H }
}
"First/last content rows:"
for ($y = 0; $y -lt $H; $y++) {
  $n = 0
  for ($x = 0; $x -lt $W; $x++) { if ((Dist $b.GetPixel($x,$y) $bg) -gt 45) { $n++ } }
  if ($n -gt 0) { "  first content row: {0} (n={1})" -f $y, $n; break }
}
for ($y = $H - 1; $y -ge 0; $y--) {
  $n = 0
  for ($x = 0; $x -lt $W; $x++) { if ((Dist $b.GetPixel($x,$y) $bg) -gt 45) { $n++ } }
  if ($n -gt 0) { "  last content row: {0} (n={1})" -f $y, $n; break }
}
$b.Dispose()
