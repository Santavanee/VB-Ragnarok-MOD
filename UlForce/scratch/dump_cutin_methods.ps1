$cecilPath = "E:\GameH\VenusBlood RAGNAROK International (US)\BepInEx\core\Mono.Cecil.dll"
[System.Reflection.Assembly]::LoadFrom($cecilPath) | Out-Null

$asmPath = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed\Assembly-CSharp.dll"
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($asmPath)

function Dump-Method($typeName, $methodName) {
    Write-Output ("=== " + $typeName + "::" + $methodName + " ===")
    $t = $assembly.MainModule.GetType($typeName)
    if ($t) {
        $m = $t.Methods | Where-Object { $_.Name -eq $methodName }
        if ($m -and $m.HasBody) {
            foreach ($i in $m.Body.Instructions) {
                Write-Output ("  " + $i.Offset + ": " + $i.OpCode + " " + $i.Operand)
            }
        }
    }
}

Dump-Method "InformationCutinControl" "ShowDialog"
Dump-Method "InformationCutinControl" "OnOpen"

