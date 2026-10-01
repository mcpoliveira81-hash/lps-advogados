Add-Type -AssemblyName System.Drawing

function Show($label, $path, $points) {
  $b = New-Object System.Drawing.Bitmap($path)
  "== $label =="
  "format: $($b.PixelFormat)"
  foreach ($pt in $points) {
    $p = $b.GetPixel($pt[0], $pt[1])
    "  ({0},{1}) -> A={2} R={3} G={4} B={5}" -f $pt[0], $pt[1], $p.A, $p.R, $p.G, $p.B
  }
  $b.Dispose()
}

Show "PNG saida" "assets\img\logo-lps-dourado.png" @(@(300,5), @(10,10), @(300,40), @(50,60), @(300,150), @(120,120))
Show "JPG original" "684165031_18191320006367367_2291330546204645952_n.jpg" @(@(1,1), @(300,1), @(300,6), @(300,12), @(300,335), @(5,170), @(300,341))
