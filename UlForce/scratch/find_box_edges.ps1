Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile("C:\Users\cs_yo\.gemini\antigravity\brain\9790f50a-68df-41ba-bf8c-2d22c1d3dc05\.user_uploaded\media_1789794679271.png")
# Left border is x=181. Let's find top y:
for ($y = 10; $y -lt 30; $y++) {
    $c = $bmp.GetPixel(250, $y)
    if ($c.R -gt 160 -and $c.G -gt 100 -and $c.B -lt 50) {
        Write-Output ("Top gold at y=" + $y + ": " + $c)
        break
    }
}
# Find bottom y:
for ($y = 190; $y -gt 140; $y--) {
    $c = $bmp.GetPixel(250, $y)
    if ($c.R -gt 160 -and $c.G -gt 100 -and $c.B -lt 50) {
        Write-Output ("Bottom gold at y=" + $y + ": " + $c)
        break
    }
}
# Find right x:
for ($x = 350; $x -gt 280; $x--) {
    $c = $bmp.GetPixel($x, 100)
    if ($c.R -gt 160 -and $c.G -gt 100 -and $c.B -lt 50) {
        Write-Output ("Right gold at x=" + $x + ": " + $c)
        break
    }
}
$bmp.Dispose()

