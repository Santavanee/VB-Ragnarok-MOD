$cecilPath = "E:\GameH\VenusBlood RAGNAROK International (US)\BepInEx\core\Mono.Cecil.dll"
[System.Reflection.Assembly]::LoadFrom($cecilPath) | Out-Null

$asmPath = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed\Assembly-CSharp.dll"
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($asmPath)

Write-Output "=== Finding all accesses to UnitDataSet.get_image1 or UnitData.get_image1 ==="
foreach ($module in $assembly.Modules) {
    foreach ($type in $module.Types) {
        foreach ($method in $type.Methods) {
            if ($method.HasBody) {
                foreach ($instr in $method.Body.Instructions) {
                    if ($instr.Operand -is [Mono.Cecil.MethodReference]) {
                        $mRef = [Mono.Cecil.MethodReference]$instr.Operand
                        if ($mRef.Name -eq "get_image1") {
                            # Check next few instructions to see what index is being accessed!
                            $next = $instr.Next
                            $idx = "unknown"
                            if ($next -and $next.OpCode.Name -match "ldc\.i4") {
                                $idx = $next.OpCode.Name
                            }
                            Write-Output ($type.FullName + "::" + $method.Name + " -> " + $mRef.FullName + " (next: " + $next.OpCode.Name + " " + $next.Operand + ")")
                        }
                    }
                }
            }
        }
    }
}

