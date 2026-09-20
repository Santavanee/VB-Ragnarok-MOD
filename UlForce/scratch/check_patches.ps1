[System.Reflection.Assembly]::LoadFrom("bin/Debug/Mono.Cecil.dll") | Out-Null
$assemblyDef = [Mono.Cecil.AssemblyDefinition]::ReadAssembly("E:\GameH\VenusBlood RAGNAROK International (US)\BepInEx\plugins\UlForce.dll")
$type = $assemblyDef.MainModule.Types | Where-Object { $_.Name -eq "EquipmentIconPlugin" }
foreach ($nested in $type.NestedTypes) {
    Write-Host ("NESTED: " + $nested.Name)
    foreach ($m in $nested.Methods) {
        $pList = @()
        foreach ($p in $m.Parameters) {
            $pList += ($p.ParameterType.Name + " " + $p.Name)
        }
        Write-Host ("  " + $m.Name + "(" + ($pList -join ", ") + ")")
    }
}

