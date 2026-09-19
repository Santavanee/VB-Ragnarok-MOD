Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile("C:\Users\cs_yo\.gemini\antigravity\brain\9790f50a-68df-41ba-bf8c-2d22c1d3dc05\.user_uploaded\media_1789794679271.png")
Write-Output "Scanning for golden box borders..."
# Golden border is around x=190..340, y=20..170
for ($y = 10; $y -lt 40; $y++) {
    for ($x = 180; $x -lt 220; $x++) {
        $c = $bmp.GetPixel($x, $y)
        # Golden orange color: R > 150, G > 90, B < 60
        if ($c.R -gt 160 -and $c.G -gt 100 -and $c.B -lt 50) {
            Write-Output ("Found gold at x=" + $x + ", y=" + $y + ": " + $c)
            break
        }
    }
}
$bmp.Dispose()
