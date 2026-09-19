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
$pList = Read-BytesFile "PassiveDataSet.bytes"
$passives = @{}
foreach ($p in $pList) { $passives[$p.id] = $p.name }

$ulforcePath = "c:\Users\cs_yo\source\repos\UlForce\UlForce\bin\Debug\UlForce.dll"
$asmMod = [System.Reflection.Assembly]::LoadFrom($ulforcePath)

Write-Output "=== Testing Elisha Template ==="
$elishaDef = $asmMod.GetType("VBRForceLock.ElishaDefinition")
$elishaTmpl = $elishaDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@(,$units))

Write-Output ("Elisha Trick: " + $passives[$elishaTmpl.trick[0].id] + " (" + $elishaTmpl.trick[0].id + ") [" + $elishaTmpl.trick[0].power + "]")
Write-Output ("Elisha Tactics Count: " + $elishaTmpl.tactics.Count)
foreach ($tac in $elishaTmpl.tactics) {
    $vt = $tactics[$tac.id]
    Write-Output ("   " + $tac.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
}

Write-Output "`n=== Testing Miden Template ==="
$midenDef = $asmMod.GetType("VBRForceLock.MidenDefinition")
$midenTmpl = $midenDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@(,$units))

Write-Output ("Miden Trick: " + $passives[$midenTmpl.trick[0].id] + " (" + $midenTmpl.trick[0].id + ") [" + $midenTmpl.trick[0].power + "]")
Write-Output ("Miden Tactics Count: " + $midenTmpl.tactics.Count)
foreach ($tac in $midenTmpl.tactics) {
    $vt = $tactics[$tac.id]
    Write-Output ("   " + $tac.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
}

Write-Output "`n=== Testing Anora Template ==="
$anoraDef = $asmMod.GetType("VBRForceLock.AnoraDefinition")
$anoraTmpl = $anoraDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@(,$units))

Write-Output ("Anora Trick: " + $passives[$anoraTmpl.trick[0].id] + " (" + $anoraTmpl.trick[0].id + ") [" + $anoraTmpl.trick[0].power + "]")
Write-Output ("Anora Tactics Count: " + $anoraTmpl.tactics.Count)
foreach ($tac in $anoraTmpl.tactics) {
    $vt = $tactics[$tac.id]
    Write-Output ("   " + $tac.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
}

Write-Output "`n=== Verification of Vanilla Untouched ==="
$sourceUnits = @('m0671', 'm1091', 'm0698')
foreach ($sid in $sourceUnits) {
    $u = $units | Where-Object { $_.id -eq $sid }
    Write-Output ("Source " + $u.id + " (" + $u.name + "): trick=" + $u.trick.Count + ", tactics=" + $u.tactics.Count)
}

