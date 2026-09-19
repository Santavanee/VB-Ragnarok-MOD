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

Write-Output "=== 1. Validating Elisha Skills ==="
$passives = @{}
$pList = Read-BytesFile "PassiveDataSet.bytes"
foreach ($p in $pList) {
    $passives[$p.id] = $p.name
}

$expectedSkills = @{
    "L011" = 40; # Undead Boost
    "M011" = 20; # Command Undead
    "L019" = 20; # Night Boost
    "O002" = 75; # Strat Support
    "J013" = 0;  # Target Miss
    "H014" = 0;  # Multi-Ailment
    "B012" = 20; # Poison Field
    "F003" = 20; # Division Heal
}

$expectedLeaders = @{
    "M011" = 50; # Command Undead
    "J007" = 0;  # Surround Null
}

foreach ($id in $expectedSkills.Keys) {
    if (-not $passives.ContainsKey($id)) { throw "Missing skill: $id" }
    Write-Output ("Base Skill: " + $id + " -> " + $passives[$id] + " (Power: " + $expectedSkills[$id] + ")")
}
foreach ($id in $expectedLeaders.Keys) {
    if (-not $passives.ContainsKey($id)) { throw "Missing leader: $id" }
    Write-Output ("Leader Skill: " + $id + " -> " + $passives[$id] + " (Power: " + $expectedLeaders[$id] + ")")
}

Write-Output "`n=== 2. Creating and Testing Elisha Template ==="
$masterList = Read-BytesFile "UnitDataSet.bytes"
$elishaDef = $asmMod.GetType("VBRForceLock.ElishaDefinition")
$create = $elishaDef.GetMethod("CreateTemplate", [System.Reflection.BindingFlags]"NonPublic,Static")
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

$expectedTribe = ("" + [char]0x5973 + [char]0x795E + [char]0x6B7B + [char]0x591C + [char]0x8D85)
$expectedSpecial = ("" + [char]0x795E + [char]0x9B54 + [char]0x6B7B)

if ($template.id -ne "zzz_custom_abyss_miko_elisha") { throw "Incorrect ID" }
if ($template.name -ne "Abyss Miko Elisha") { throw "Incorrect Name" }
if ($template.tribe -ne $expectedTribe) { throw "Incorrect Tribe" }
if ($template.special -ne $expectedSpecial) { throw "Incorrect Special" }
if ($template.skillBase.Count -ne 8) { throw "Incorrect Base Skill Count" }
if ($template.leader.Count -ne 2) { throw "Incorrect Leader Skill Count" }

Write-Output "`n=== 3. HP Scaling at Lv 300 ==="
$hp300 = [int][Math]::Floor(((300 - 1) * 0.25 + 1) * $template.basic.hp)
Write-Output ("HP at Lv 300: $hp300 (Target in screenshot: 6202)")

Write-Output "`n=== 4. Validating Portrait PNG ==="
$pngPath = "c:\Users\cs_yo\source\repos\UlForce\UlForce\Assets\UlForce_elisha.png"
if (-not (Test-Path $pngPath)) { throw "Missing portrait file!" }
Add-Type -AssemblyName System.Drawing
$img = [System.Drawing.Image]::FromFile($pngPath)
Write-Output ("Portrait dimensions: " + $img.Width + "x" + $img.Height + " (" + $img.PixelFormat + ")")
$img.Dispose()

Write-Output "`nALL ELISHA VALIDATION CHECKS PASSED!"

