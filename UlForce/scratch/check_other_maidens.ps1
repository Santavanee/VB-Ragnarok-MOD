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

$units = Read-BytesFile "UnitDataSet.bytes"
$tList = Read-BytesFile "TacticsDataSet.bytes"
$tactics = @{}
foreach ($t in $tList) { $tactics[$t.id] = $t }

foreach ($id in @('m0159', 'm0237', 'm0460', 'm0600', 'm0635', 'm0652')) {
    $u = $units | Where-Object { $_.id -eq $id }
    if ($u) {
        Write-Output ("Found: " + $u.name + " [" + $u.id + "]")
        foreach ($tac in $u.tactics) {
            $vt = $tactics[$tac.id]
            if ($vt) {
                Write-Output ("    " + $vt.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
            }
        }
    }
}

