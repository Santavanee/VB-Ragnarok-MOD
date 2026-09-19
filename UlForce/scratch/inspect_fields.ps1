$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

Write-Output "--- Fields of DataSet.TacticsData ---"
$td = $asmCS.GetType("DataSet.TacticsData")
foreach ($f in $td.GetFields([System.Reflection.BindingFlags]"Public,NonPublic,Instance")) {
    Write-Output ($f.FieldType.Name + " " + $f.Name)
}

Write-Output "--- Fields of DataSet.TacticsDataSet ---"
$tds = $asmCS.GetType("DataSet.TacticsDataSet")
foreach ($f in $tds.GetFields([System.Reflection.BindingFlags]"Public,NonPublic,Instance")) {
    Write-Output ($f.FieldType.Name + " " + $f.Name)
}

