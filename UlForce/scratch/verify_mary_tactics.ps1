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

Write-Output "=== Testing Mary Template ==="
$maryDef = $asmMod.GetType("VBRForceLock.MaryDefinition")
$maryTmpl = $maryDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"Static,NonPublic,Public").Invoke($null, [object[]]@(,$units))

Write-Output ("Mary Trick: " + $passives[$maryTmpl.trick[0].id] + " (" + $maryTmpl.trick[0].id + ") [" + $maryTmpl.trick[0].power + "]")
Write-Output ("Mary Tactics Count: " + $maryTmpl.tactics.Count)
foreach ($tac in $maryTmpl.tactics) {
    $vt = $tactics[$tac.id]
    Write-Output ("   " + $tac.id + " | " + $vt.name + " (Lv." + $vt.level + " Cost:" + $vt.cost + "): " + $vt.comment)
}

Write-Output "`n=== Verification of Vanilla Untouched ==="
$source = $units | Where-Object { $_.id -eq 'm0655' }
Write-Output ("Source m0655 (" + $source.name + "): trick=" + $source.trick.Count + ", tactics=" + $source.tactics.Count)

