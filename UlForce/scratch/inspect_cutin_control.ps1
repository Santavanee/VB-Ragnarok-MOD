$cecilPath = "E:\GameH\VenusBlood RAGNAROK International (US)\BepInEx\core\Mono.Cecil.dll"
[System.Reflection.Assembly]::LoadFrom($cecilPath) | Out-Null

$asmPath = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed\Assembly-CSharp.dll"
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($asmPath)

$t = $assembly.MainModule.GetType("InformationCutinControl")
if ($t) {
    Write-Output ("=== InformationCutinControl ===")
    Write-Output "--- Fields ---"
    foreach ($f in $t.Fields) {
        Write-Output ($f.FieldType.Name + " " + $f.Name)
    }
    Write-Output "--- Methods ---"
    foreach ($m in $t.Methods) {
        Write-Output ($m.ReturnType.Name + " " + $m.Name)
    }
}

