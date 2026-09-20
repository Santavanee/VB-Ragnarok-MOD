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
Write-Output ("Total units count: " + $units.Count)

for ($i = 600; $i -lt [Math]::Min($units.Count, 705); $i++) {
    $u = $units[$i]
    Write-Output ("Index $i | ID: " + $u.id + " | Name: " + $u.name + " | Type: " + $u.type + " | Tribe: " + $u.tribe + " | Special: " + $u.special)
}

