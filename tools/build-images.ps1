$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

$cs = Get-Content -Raw tools\ImgTool.cs
$params = New-Object System.CodeDom.Compiler.CompilerParameters
$params.CompilerOptions = "/unsafe"
$params.ReferencedAssemblies.Add("System.Drawing.dll") | Out-Null
Add-Type -TypeDefinition $cs -CompilerParameters $params

New-Item -ItemType Directory -Force -Path assets\img | Out-Null

# Retratos individuais (recorte da foto quadrada)
[ImgTool]::CropResize("images.jpg", 8, 68, 216, 270, 440, 550, "assets\img\retrato-1.jpg", 86)   | Out-Null
[ImgTool]::CropResize("images.jpg", 227, 5, 220, 275, 440, 550, "assets\img\retrato-2.jpg", 86)   | Out-Null
# Hero (upscale 2x)
[ImgTool]::CropResize("images.jpg", 0, 0, 447, 447, 894, 894, "assets\img\hero-equipe.jpg", 85)   | Out-Null
# Foto do ambiente (parede com logo)
[ImgTool]::CropResize("681992110_18191320015367367_3427229226656224515_n.jpg", 0, 0, 550, 314, 880, 502, "assets\img\escritorio.jpg", 84) | Out-Null
# Logo com fundo removido (dourado + versão azul)
[ImgTool]::LogoAlpha("684165031_18191320006367367_2291330546204645952_n.jpg", "assets\img\logo-lps-dourado.png", "assets\img\logo-lps-azul.png") | Out-Null
# Favicon
[ImgTool]::Favicon("assets\img\logo-lps-dourado.png", "assets\img\favicon-192.png", 192) | Out-Null
[ImgTool]::Favicon("assets\img\logo-lps-dourado.png", "assets\img\favicon-512.png", 512) | Out-Null
# Open Graph
[ImgTool]::OgImage("assets\img\logo-lps-dourado.png", "assets\img\og-image.png", 1200, 630) | Out-Null

Get-ChildItem assets\img | Select-Object Name, Length | Format-Table -AutoSize
