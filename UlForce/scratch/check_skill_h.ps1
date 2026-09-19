$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

Write-Output "--- Checking Skill_H types ---"
foreach ($t in $asmCS.GetTypes()) {
    if ($t.FullName -match "SkillConfig.Skill_H") {
        $inst = [System.Activator]::CreateInstance($t)
        $info = $t.GetMethod("setInfo").Invoke($inst, $null)
        Write-Output ($t.Name + " -> " + $inst.name + " (" + $inst.comment + ")")
    }
}

