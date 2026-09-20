$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

function Read-BytesFile($fileName) {
    $path = Join-Path "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\StreamingAssets\data" $fileName
    $fs = [System.IO.File]::OpenRead($path)
    $gz = New-Object System.IO.Compression.GZipStream($fs, [System.IO.Compression.CompressionMode]::Decompress)
    $bf = New-Object System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
    $data = $bf.Deserialize($gz)
    $gz.Dispose(); $fs.Dispose()
    return ,$data
}

$matrix = Read-BytesFile "ResearchMatrixSet.bytes"
$units = Read-BytesFile "UnitDataSet.bytes"

$p8 = $matrix | Where-Object { $_.page -eq 8 }
Write-Output ("Page 8 items: " + $p8.Count)
foreach ($m in $p8) {
    $unitNames = @()
    foreach ($uid in $m.opendata) {
        if ($uid -ge 0 -and $uid -lt $units.Count) {
            $unitNames += ($units[$uid].id + ":" + $units[$uid].name)
        }
    }
    $medalsStr = if ($m.medals) { [string]::Join(",", $m.medals) } else { "" }
    Write-Output ("Index=" + $m.index + " | Row=" + $m.row + " | Open=" + $m.open + " | Function=" + $m.GetFunction() + " | Pay=" + $m.pay + " | Cost=" + $m.cost + " | Medals=[" + $medalsStr + "] | Units=[" + [string]::Join(", ", $unitNames) + "]")
}
