$managed = "E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\Managed"
$asmCS = [System.Reflection.Assembly]::LoadFrom((Join-Path $managed "Assembly-CSharp.dll"))
$resolver = {
    param($sender, $e)
    $name = ([System.Reflection.AssemblyName]$e.Name).Name
    if ($name -eq "Assembly-CSharp") { return $asmCS }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)

$ex = $asmCS.GetType("ExValue")
$divImg = $ex.GetField("divineImage").GetValue($null)
for ($i=0; $i -lt $divImg.Length; $i++) {
    Write-Output ("Divine " + $i + " -> " + $divImg[$i])
}

