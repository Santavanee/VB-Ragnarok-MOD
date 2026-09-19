Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile("C:\Users\cs_yo\.gemini\antigravity\brain\9790f50a-68df-41ba-bf8c-2d22c1d3dc05\.user_uploaded\media_1789794679271.png")
Write-Output ("x=181, y=17: " + $bmp.GetPixel(181, 17))
Write-Output ("x=250, y=17: " + $bmp.GetPixel(250, 17))
Write-Output ("x=300, y=17: " + $bmp.GetPixel(300, 17))
Write-Output ("x=330, y=17: " + $bmp.GetPixel(330, 17))
# Find right boundary along y=17:
for ($x = 181; $x -lt 400; $x++) {
    $c = $bmp.GetPixel($x, 17)
    if ($c.R -lt 100) {
        Write-Output ("Right boundary before x=$x : " + $bmp.GetPixel($x-1, 17))
        break
    }
}
for ($y = 17; $y -lt 199; $y++) {
    $c = $bmp.GetPixel(181, $y)
    if ($c.R -lt 100) {
        Write-Output ("Bottom boundary before y=$y : " + $bmp.GetPixel(181, $y-1))
        break
    }
}
$bmp.Dispose()

