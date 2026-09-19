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
$pList = Read-BytesFile "PassiveDataSet.bytes"
$tactics = @{}
foreach ($t in $tList) { $tactics[$t.id] = $t }
$passives = @{}
foreach ($p in $pList) { $passives[$p.id] = $p.name }

$u = $units | Where-Object { $_.id -eq 'm0698' }
if ($u) {
    Write-Output ("Found: " + $u.name + " [" + $u.id + "]")
    Write-Output ("Tribe: " + $u.tribe + " Rank: " + $u.rank + " Cost: " + $u.cost)
    $tricks = @()
    foreach ($tr in $u.trick) {
        $pName = if ($passives.ContainsKey($tr.id)) { $passives[$tr.id] } else { $tr.id }
        $tricks += ($pName + " (" + $tr.id + ") [" + $tr.power + "]")
    }
    Write-Output ("Trick: " + ($tricks -join ", "))
    Write-Output ("Tactics count: " + $u.tactics.Count)
    foreach ($tac in $u.tactics) {
        $vt = $tactics[$tac.id]
        if ($vt) {
            Write-Output ("    " + $vt.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
        } else {
            Write-Output ("    " + $tac.id)
        }
    }
}

