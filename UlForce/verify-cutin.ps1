
$ErrorActionPreference = "Stop"

$bepInExCore = "E:\GameH\VenusBlood RAGNAROK International (US)\BepInEx\core"
$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$pluginDll = "c:\Users\cs_yo\source\repos\UlForce\UlForce\bin\Debug\UlForce.dll"

# Preload all Managed assemblies
Get-ChildItem -Path $managed -Filter "*.dll" | ForEach-Object {
    try { [System.Reflection.Assembly]::LoadFrom($_.FullName) | Out-Null } catch {}
}
Get-ChildItem -Path $bepInExCore -Filter "*.dll" | ForEach-Object {
    try { [System.Reflection.Assembly]::LoadFrom($_.FullName) | Out-Null } catch {}
}

$mod = [System.Reflection.Assembly]::LoadFrom($pluginDll)
Write-Host "Mod loaded successfully from $pluginDll"

$cup = $mod.GetType("VBRForceLock.CustomUnitPortraits")
$patchTypesField = $cup.GetField("PatchTypes", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static)
$patchTypes = $patchTypesField.GetValue($null)

Write-Host "Checking InfoCut3PortraitPatch and InfoCut2PortraitPatch..."
$cut3Type = $patchTypes | Where-Object { $_.Name -eq "InfoCut3PortraitPatch" }
$cut2Type = $patchTypes | Where-Object { $_.Name -eq "InfoCut2PortraitPatch" }

if (-not $cut3Type) { throw "Missing InfoCut3PortraitPatch!" }
if (-not $cut2Type) { throw "Missing InfoCut2PortraitPatch!" }

$harmony = New-Object HarmonyLib.Harmony("test.cutin.verify")
$harmony.PatchAll($cut3Type)
Write-Host "Successfully patched InfoCut3PortraitPatch!"
$harmony.PatchAll($cut2Type)
Write-Host "Successfully patched InfoCut2PortraitPatch!"

Write-Host "ALL CUTIN PATCH TARGETS VERIFIED!"

