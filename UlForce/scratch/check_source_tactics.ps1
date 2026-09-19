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
$tList = Read-BytesFile "TacticsDataSet.bytes"
$tactics = @{}
foreach ($t in $tList) { $tactics[$t.id] = $t.name }

foreach ($id in @("m1080", "m1091", "m1082", "m1075")) {
    $u = $units | Where-Object id -eq $id | Select-Object -First 1
    if ($u) {
        Write-Output ("Unit: " + $u.name + " (" + $u.id + ")")
        foreach ($tr in $u.trick) {
            Write-Output ("  trick: " + $tr.id + " power=" + $tr.power)
        }
        foreach ($tac in $u.tactics) {
            Write-Output ("  tactic: " + $tac.id + " (" + $tactics[$tac.id] + ") lv=" + $tac.level)
        }
    }
}

