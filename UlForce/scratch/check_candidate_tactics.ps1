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
$passives = @{}
foreach ($p in $pList) { $passives[$p.id] = $p.name }
$tactics = @{}
foreach ($t in $tList) { $tactics[$t.id] = $t }

Write-Output "--- Units with Tactical skills ---"
foreach ($u in $units) {
    if ($u.id -match "^m10" -or $u.id -match "^m06") {
        $validTac = @()
        foreach ($tac in $u.tactics) {
            if ($tac.id -and $tactics.ContainsKey($tac.id)) {
                $validTac += $tactics[$tac.id]
            }
        }
        if ($validTac.Count -ge 3) {
            $trName = if ($u.trick.Count -gt 0 -and $passives.ContainsKey($u.trick[0].id)) { $passives[$u.trick[0].id] + " (" + $u.trick[0].id + ") [" + $u.trick[0].power + "]" } else { "none" }
            Write-Output ($u.name + " [" + $u.id + "] tribe=" + $u.tribe + " trick=" + $trName)
            foreach ($vt in $validTac) {
                Write-Output ("    " + $vt.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
            }
        }
    }
}

