$cecilPath = "E:\GameH\VenusBlood RAGNAROK International (US)\BepInEx\core\Mono.Cecil.dll"
[System.Reflection.Assembly]::LoadFrom($cecilPath) | Out-Null

$asmPath = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed\Assembly-CSharp.dll"
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($asmPath)

Write-Output "=== Types inheriting from ViewBase ==="
foreach ($module in $assembly.Modules) {
    foreach ($type in $module.Types) {
        if ($type.BaseType -and $type.BaseType.Name -eq "ViewBase") {
            Write-Output ($type.FullName)
        }
    }
}

