$ErrorActionPreference = 'Stop'
$game = 'E:/GameH/VenusBlood RAGNAROK International (US)'
$managed = Join-Path $game 'VBRI_Data/Managed'
$unity = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'UnityEngine.dll'))
$asm = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
[void][Reflection.Assembly]::LoadFrom((Join-Path $game 'BepInEx/core/0Harmony.dll'))
[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $eventArgs)
    $name = ([Reflection.AssemblyName]$eventArgs.Name).Name
    if ($name -eq 'Assembly-CSharp') { return $asm }
    if ($name -eq 'UnityEngine') { return $unity }
    return $null
})
$asm.GetType('CoreSystem.TextResource').GetField('_mode', [Reflection.BindingFlags]'NonPublic,Static').SetValue($null, 'us')
function Read-Database($name) {
    $file = [IO.File]::OpenRead((Join-Path $game "VBRI_Data/StreamingAssets/data/$name.bytes"))
    $gzip = New-Object IO.Compression.GZipStream($file, [IO.Compression.CompressionMode]::Decompress)
    try { return ,(New-Object Runtime.Serialization.Formatters.Binary.BinaryFormatter).Deserialize($gzip) }
    finally { $gzip.Dispose(); $file.Dispose() }
}
$units = Read-Database 'UnitDataSet'
$passives = Read-Database 'PassiveDataSet'
$tacticDatabase = Read-Database 'TacticsDataSet'
$mod = [Reflection.Assembly]::LoadFrom((Join-Path $PSScriptRoot 'bin/Debug/UlForce.dll'))
$definition = $mod.GetType('VBRForceLock.NannaDefinition')
$flags = [Reflection.BindingFlags]'NonPublic,Static'
$create = $definition.GetMethod('CreateTemplate', $flags)
$originalId = $units[0].id
$originalName = $units[0].name
$originalSkill = $units[0].skillBase[0].id
foreach ($dark in @($false, $true)) {
    $template = $create.Invoke($null, @($units, $dark))
    if ($template.type -ne ([string][char]0x82f1 + [char]0x970a)) { throw 'Expected HeroicSpirit type' }
    $sourceId = if ($dark) { 'm1028' } else { 'm1069' }
    $source = $units | Where-Object id -eq $sourceId | Select-Object -First 1
    if ($template.trick.Count -ne 1 -or $template.tactics.Count -ne 5) { throw 'Missing assist/tactics' }
    if ($template.trick[0].id -ne $source.trick[0].id -or $template.trick[0].power -ne $source.trick[0].power -or [object]::ReferenceEquals($template.trick[0], $source.trick[0])) { throw 'Invalid assist copy' }
    foreach ($tactic in $template.tactics) {
        $matchTactic = @($tacticDatabase | Where-Object id -eq $tactic.id)
        if ($matchTactic.Count -ne 1) { throw "Unresolved tactic $($tactic.id)" }
        Write-Output "$($template.name) tactic: $($matchTactic[0].name)"
    }
    for ($i = 0; $i -lt 5; $i++) {
        if ($template.tactics[$i].id -ne $source.tactics[$i].id -or [object]::ReferenceEquals($template.tactics[$i], $source.tactics[$i])) { throw 'Invalid tactical copy' }
    }
    if ($template.skillBase.Count -ne 8 -or $template.leader.Count -ne 2) { throw 'Invalid skill slot counts' }
    if ($template.image1[1] -ne $units[0].image1[1]) { throw 'Native battle resource changed' }
    $match = $definition.GetMethod('MatchesBattleImage', $flags)
    $firstName = $template.name.Split(' ')[0]
    if (-not $match.Invoke($null, @($firstName, $template.image1[1]))) { throw 'Custom HUD match failed' }
    $native = $units | Where-Object { $_.id -eq 'm1069' } | Select-Object -First 1
    if ($match.Invoke($null, @($firstName, $native.image1[1]))) { throw 'Native Nanna HUD collision' }
    foreach ($skill in @($template.skillBase) + @($template.leader)) {
        $found = @($passives | Where-Object { $_.id -eq $skill.id })
        if ($found.Count -ne 1 -or $skill.name -ne [HDDataSetDef]::NULL) { throw "Invalid skill $($skill.id)" }
        Write-Output "$($template.name): $($found[0].name) [$($skill.power)]"
    }
    $units.Add($template)
}
if ($units[0].id -ne $originalId -or $units[0].name -ne $originalName -or $units[0].skillBase[0].id -ne $originalSkill) { throw 'Vanilla template mutated' }
if ($units[$units.Count - 2].name -ne 'Celestial Nanna' -or $units[$units.Count - 1].name -ne 'Eclipse Nanna') { throw 'Names shared between variants' }
Write-Output 'PASS: Both templates, localized skill IDs, slot counts, sentinel names, native battle resource, vanilla isolation.'
$units | Where-Object { $_.name -match 'Nanna' } | Select-Object id,name,category,job,equipID | Format-Table -AutoSize
$items = Read-Database 'ItemDataSet'
$items | Where-Object { $_.name -match "Hero's Great Sword|Assassin Cloak" } | Format-List id,name,type

