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
$pList = Read-BytesFile "PassiveDataSet.bytes"
$passives = @{}
foreach ($p in $pList) { $passives[$p.id] = $p.name }

foreach ($u in @('m1091', 'm1079', 'm1080', 'm1084', 'm1074', 'm1087', 'm1069', 'm1028')) {
    $unit = $units | Where-Object { $_.id -eq $u }
    if ($unit) {
        $tricks = @()
        foreach ($tr in $unit.trick) {
            $pName = if ($passives.ContainsKey($tr.id)) { $passives[$tr.id] } else { $tr.id }
            $tricks += ($pName + " (" + $tr.id + ") [" + $tr.power + "]")
        }
        Write-Output ($unit.name + " [" + $unit.id + "]: " + ($tricks -join ", "))
    }
}

