$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

Write-Output "--- Types using TacticsData ---"
foreach ($type in $asmCS.GetTypes()) {
    foreach ($m in $type.GetMethods([System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static")) {
        if ($m.ToString() -match "TacticsData") {
            Write-Output ($type.FullName + " :: " + $m.ToString())
        }
    }
}

