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
foreach ($t in $tList) {
    $tactics[$t.id] = $t.name
}

Write-Output "--- Units with Tactics ---"
foreach ($u in $units) {
    $hasTac = $false
    foreach ($tac in $u.tactics) {
        if ($tac.id -and $tac.id -ne "NULL" -and $tactics.ContainsKey($tac.id)) {
            $hasTac = $true
            break
        }
    }
    if ($hasTac) {
        $tacNames = @()
        foreach ($tac in $u.tactics) {
            if ($tac.id -and $tactics.ContainsKey($tac.id)) {
                $tacNames += ($tactics[$tac.id] + " (" + $tac.id + ")")
            }
        }
        $trickName = if ($u.trick.Count -gt 0 -and $u.trick[0].id) { $u.trick[0].id } else { "none" }
        Write-Output ($u.name + " [" + $u.id + "] trick=" + $trickName + " tactics=" + ($tacNames -join ", "))
    }
}

