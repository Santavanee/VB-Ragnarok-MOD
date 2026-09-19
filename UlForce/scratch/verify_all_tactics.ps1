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

Write-Output "=== Testing Elisha ==="
$elishaDef = $asmMod.GetType("VBRForceLock.ElishaDefinition")
$elishaTmpl = $elishaDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@(,$units))
$elishaUnit = $elishaDef.GetMethod("CreateUnit", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@($elishaTmpl))

Write-Output ("Elisha Template Trick: " + $passives[$elishaTmpl.trick[0].id] + " (" + $elishaTmpl.trick[0].id + ") [" + $elishaTmpl.trick[0].power + "]")
Write-Output ("Elisha Unit Tactics Count: " + $elishaUnit.tactics.Count)
foreach ($tac in $elishaUnit.tactics) {
    $vt = $tactics[$tac.id]
    Write-Output ("   " + $tac.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
}

Write-Output "`n=== Testing Miden ==="
$midenDef = $asmMod.GetType("VBRForceLock.MidenDefinition")
$midenTmpl = $midenDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@(,$units))
$midenUnit = $midenDef.GetMethod("CreateUnit", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@($midenTmpl))

Write-Output ("Miden Template Trick: " + $passives[$midenTmpl.trick[0].id] + " (" + $midenTmpl.trick[0].id + ") [" + $midenTmpl.trick[0].power + "]")
Write-Output ("Miden Unit Tactics Count: " + $midenUnit.tactics.Count)
foreach ($tac in $midenUnit.tactics) {
    $vt = $tactics[$tac.id]
    Write-Output ("   " + $tac.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
}

Write-Output "`n=== Testing Anora ==="
$anoraDef = $asmMod.GetType("VBRForceLock.AnoraDefinition")
$anoraTmpl = $anoraDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@(,$units))
$anoraUnit = $anoraDef.GetMethod("CreateUnit", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@($anoraTmpl))

Write-Output ("Anora Template Trick: " + $passives[$anoraTmpl.trick[0].id] + " (" + $anoraTmpl.trick[0].id + ") [" + $anoraTmpl.trick[0].power + "]")
Write-Output ("Anora Unit Tactics Count: " + $anoraUnit.tactics.Count)
foreach ($tac in $anoraUnit.tactics) {
    $vt = $tactics[$tac.id]
    Write-Output ("   " + $tac.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
}

Write-Output "`n=== Testing Refresh on Existing Unit (Simulated Save Reload) ==="
# Simulate an existing unit that had empty tactics
$fakeExisting = $elishaDef.GetMethod("CreateUnit", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@($elishaTmpl))
$fakeExisting.tactics.Clear()
$fakeExisting.trick.Clear()
Write-Output ("Before refresh: trick=" + $fakeExisting.trick.Count + ", tactics=" + $fakeExisting.tactics.Count)
$elishaDef.GetMethod("RefreshSupportSkills", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@($fakeExisting))
Write-Output ("After refresh: trick=" + $fakeExisting.trick.Count + ", tactics=" + $fakeExisting.tactics.Count)
if ($fakeExisting.tactics.Count -eq 5 -and $fakeExisting.trick.Count -eq 1) {
    Write-Output "SUCCESS: RefreshSupportSkills correctly repopulates existing unit instance!"
} else {
    Write-Error "FAILURE: tactics count unexpected"
}

