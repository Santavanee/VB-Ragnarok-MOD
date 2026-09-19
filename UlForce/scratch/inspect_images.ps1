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

$units = Read-BytesFile "UnitDataSet.bytes"
foreach ($id in @('m0671', 'm1091', 'm0698', 'm1069', 'm1028', 'm0001', 'm0086')) {
    $u = $units | Where-Object { $_.id -eq $id }
    if ($u) {
        Write-Output ("Unit " + $u.id + " (" + $u.name + "):")
        for ($i = 0; $i -lt $u.image1.Count; $i++) {
            Write-Output ("  image1[$i] = " + $u.image1[$i])
        }
        for ($i = 0; $i -lt $u.image2.Count; $i++) {
            Write-Output ("  image2[$i] = " + $u.image2[$i])
        }
    }
}

