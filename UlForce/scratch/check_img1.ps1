Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Image]::FromFile("C:\Users\cs_yo\.gemini\antigravity\brain\9790f50a-68df-41ba-bf8c-2d22c1d3dc05\.user_uploaded\media_1789794679271.png")
Write-Output ("Width=" + $img.Width + " Height=" + $img.Height)
$img.Dispose()

