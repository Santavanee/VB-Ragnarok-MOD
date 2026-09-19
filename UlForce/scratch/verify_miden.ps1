$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$asmUE = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "UnityEngine.dll"))
$asmMod = [System.Reflection.Assembly]::LoadFrom("c:\Users\cs_yo\source\repos\UlForce\UlForce\bin\Debug\UlForce.dll")

$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    if ($name -eq "UnityEngine") { return $asmUE }
    if ($name -eq "UlForce") { return $asmMod }
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

Write-Output "=== 1. Validating Passive Database Skills ==="
$passives = @{}
$pList = Read-BytesFile "PassiveDataSet.bytes"
foreach ($p in $pList) {
    $passives[$p.id] = $p.name
}

$expectedSkills = @{
    "I004" = 0;
    "I015" = 80;
    "I010" = 40;
    "I005" = 15;
    "J013" = 0;
    "E002" = 70;
    "J023" = 100;
    "B013" = 20;
}

$expectedLeaders = @{
    "M010" = 20;
    "H001" = 3;
}

foreach ($id in $expectedSkills.Keys) {
    if (-not $passives.ContainsKey($id)) {
        throw "Missing skill ID: $id"
    }
    Write-Output ("Base Skill: " + $id + " -> " + $passives[$id] + " (Power: " + $expectedSkills[$id] + ")")
}
foreach ($id in $expectedLeaders.Keys) {
    if (-not $passives.ContainsKey($id)) {
        throw "Missing leader ID: $id"
    }
    Write-Output ("Leader Skill: " + $id + " -> " + $passives[$id] + " (Power: " + $expectedLeaders[$id] + ")")
}

Write-Output "`n=== 2. Creating and Testing Miden Template ==="
$masterList = Read-BytesFile "UnitDataSet.bytes"
$midenDef = $asmMod.GetType("VBRForceLock.MidenDefinition")
$create = $midenDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"NonPublic,Static")
$template = $create.Invoke($null, [object[]]@(,$masterList))

Write-Output ("Template ID: " + $template.id)
Write-Output ("Template Name: " + $template.name)
Write-Output ("Template Rank: " + $template.rank)
Write-Output ("Template Cost: " + $template.cost)
Write-Output ("Template Pay: " + $template.pay)
Write-Output ("Template Job: " + $template.job)
Write-Output ("Template Divine: " + $template.divine[0] + "," + $template.divine[1])
Write-Output ("Template Equip Slots: " + $template.equipID[0] + "," + $template.equipID[1])
Write-Output ("Template Basic: HP=" + $template.basic.hp + ", POW=" + $template.basic.pow + ", DEF=" + $template.basic.def + ", SPD=" + $template.basic.spd + ", WIS=" + $template.basic.wis)

$expectedTribe = ("" + [char]0x5973 + [char]0x5668 + [char]0x795E + [char]0x8D85)
$expectedSpecial = ("" + [char]0x5168)

if ($template.id -ne "zzz_custom_twilight_miko_miden") { throw "Incorrect ID" }
if ($template.name -ne "Twilight Miko Miden") { throw "Incorrect Name" }
if ($template.tribe -ne $expectedTribe) { throw "Incorrect Tribe" }
if ($template.special -ne $expectedSpecial) { throw "Incorrect Special" }
if ($template.skillBase.Count -ne 8) { throw "Incorrect Base Skill Count" }
if ($template.leader.Count -ne 2) { throw "Incorrect Leader Skill Count" }

Write-Output "`n=== 3. Mathematical Verification of Level 19 Scaling ==="
# Game Formula: HP = ((level - 1) * 0.25 + 1) * basic.hp
$hp19 = [int]((18 * 0.25 + 1) * $template.basic.hp)
Write-Output ("Calculated HP at Lv 19: $hp19 (Target: 660)")
if ($hp19 -ne 660) { throw "HP mismatch!" }

# Game Formula: SubMagnitudeCalc(base, 3450) = (int)(base + base * sqrt(exp) / 500 + sqrt(exp) / 25)
# C# (int) cast truncates/floors toward zero.
$sqrtExp = [Math]::Sqrt(3450)
function Calc-Stat($baseVal) {
    return [int][Math]::Floor($baseVal + $baseVal * $sqrtExp / 500.0 + $sqrtExp / 25.0)
}

$pow19 = (Calc-Stat $template.basic.pow) + 28
$def19 = (Calc-Stat $template.basic.def) + 36
$spd19 = (Calc-Stat $template.basic.spd) + 12
$wis19 = (Calc-Stat $template.basic.wis) + 20

Write-Output ("Calculated POW at Lv 19 + Estranged Stars (+28): $pow19 (Target: 180)")
Write-Output ("Calculated DEF at Lv 19 + Vermillion Cape (+36): $def19 (Target: 118)")
Write-Output ("Calculated SPD at Lv 19 + Estranged Stars (+12): $spd19 (Target: 102)")
Write-Output ("Calculated WIS at Lv 19 + Items (+20): $wis19 (Target: 39)")

if ($pow19 -ne 180) { throw "POW mismatch!" }
if ($def19 -ne 118) { throw "DEF mismatch!" }
if ($spd19 -ne 102) { throw "SPD mismatch!" }
if ($wis19 -ne 39) { throw "WIS mismatch!" }

Write-Output "`n=== 4. Validating Portrait PNG ==="
$pngPath = "c:\Users\cs_yo\source\repos\UlForce\UlForce\Assets\UlForce_miden.png"
if (-not (Test-Path $pngPath)) { throw "Missing portrait file!" }
Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Image]::FromFile($pngPath)
Write-Output ("Portrait dimensions: " + $img.Width + "x" + $img.Height + " (" + $img.PixelFormat + ")")
$img.Dispose()

Write-Output "`nALL VALIDATION CHECKS PASSED!"
