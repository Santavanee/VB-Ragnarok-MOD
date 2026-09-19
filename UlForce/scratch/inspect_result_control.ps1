$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

foreach ($typeName in @('BattleSystem.FinalResultControl', 'BattleSystem.FinalResultCode', 'BattleSystem.ResultControl', 'BattleSystem.BattleView')) {
    $t = $asmCS.GetType($typeName)
    if ($t) {
        Write-Output ("=== " + $t.FullName + " ===")
        Write-Output "--- Fields ---"
        foreach ($f in $t.GetFields([System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static")) {
            Write-Output ($f.FieldType.Name + " " + $f.Name)
        }
        Write-Output "--- Methods ---"
        foreach ($m in $t.GetMethods([System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static")) {
            if ($m.DeclaringType -eq $t) {
                Write-Output ($m.ReturnType.Name + " " + $m.Name + "(" + ([string]::Join(", ", ($m.GetParameters() | ForEach-Object { $_.ParameterType.Name + " " + $_.Name }))) + ")")
            }
        }
    }
}

