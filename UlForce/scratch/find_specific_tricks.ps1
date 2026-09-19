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

foreach ($u in $units) {
    if ($u.trick.Count -gt 0) {
        foreach ($tr in $u.trick) {
            if ($tr.id -in @('T002','T006','T009','T010','T011','T033','T053')) {
                $pName = if ($passives.ContainsKey($tr.id)) { $passives[$tr.id] } else { $tr.id }
                Write-Output ($u.id + " " + $u.name + ": " + $pName + " (" + $tr.id + ") [" + $tr.power + "]")
            }
        }
    }
}

