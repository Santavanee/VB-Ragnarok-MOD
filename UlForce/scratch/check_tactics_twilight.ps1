$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
$trType = $asmCS.GetType("CoreSystem.TextResource")
$modeField = $trType.GetField("_mode", [System.Reflection.BindingFlags]"NonPublic,Static")
$modeField.SetValue($null, "us")

function Read-BytesFile($fileName) {
    $path = Join-Path "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\StreamingAssets\data" $fileName
    $fs = [System.IO.File]::OpenRead($path)
    $gz = New-Object System.IO.Compression.GZipStream($fs, [System.IO.Compression.CompressionMode]::Decompress)
    $bf = New-Object System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
    $data = $bf.Deserialize($gz)
    $gz.Dispose(); $fs.Dispose()
    return ,$data
}

$tList = Read-BytesFile "TacticsDataSet.bytes"
foreach ($t in $tList) {
    if ($t.name -match "Miden|Twilight" -or $t.comment -match "Miden|Twilight") {
        Write-Output ($t.id + " | " + $t.name + " (Lv." + $t.level + " Cost:" + $t.cost + "): " + $t.comment)
    }
}

