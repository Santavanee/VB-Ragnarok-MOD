$ErrorActionPreference = 'Stop'
$managed = 'E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/Managed'
$unity = Join-Path $managed 'UnityEngine.dll'
$game = Join-Path $managed 'Assembly-CSharp.dll'
[void][Reflection.Assembly]::LoadFrom($unity)
[void][Reflection.Assembly]::LoadFrom($game)
$source = Get-Content (Join-Path $PSScriptRoot 'CustomUnitRoster.cs') -Raw
$test = @"
public static class RosterRegression {
    static void Check(bool condition, string message) { if (!condition) throw new System.Exception(message); }
    public static void Run() {
        var roster = new System.Collections.Generic.List<DataSet.UnitData>();
        var divisions = new System.Collections.Generic.List<DataSet.DivisionData>();
        var division = new DataSet.DivisionData();
        divisions.Add(division);
        string[] ids = { "native", "zzz_custom_lulu2", "zzz_custom_swimsuit_mary", "zzz_custom_celestial_nanna", "zzz_custom_eclipse_nanna" };
        foreach (string id in ids) {
            var unit = new DataSet.UnitData(); unit.id = id; unit.index = 1500;
            VBRForceLock.CustomUnitRoster.Add(roster, unit);
            Check(object.ReferenceEquals(roster[unit.index], unit), "Wrong selection after append: " + id);
        }
        roster[3].index = 1; roster[4].index = 2;
        roster[3].division = 7; roster[3].location = 2; roster[3].loyalty = 83;
        var equipment = roster[3].equip;
        int templateIndex = roster[3].unitDatas.index;
        division.divs[0] = 1; division.divs[1] = 4; division.divsBack[0] = 4;
        Check(VBRForceLock.CustomUnitRoster.RepairIndices(roster) == 2, "Expected two repairs");
        for (int i = 0; i < roster.Count; i++) Check(roster[roster[i].index].id == ids[i], "Wrong unit selected");
        Check(roster[3].division == 7 && roster[3].location == 2 && roster[3].loyalty == 83 && object.ReferenceEquals(roster[3].equip, equipment) && roster[3].unitDatas.index == templateIndex, "Player or template data changed");
        Check(division.divs[0] == 1 && division.divs[1] == 4, "Existing members reassigned");
        Check(VBRForceLock.CustomUnitRoster.RepairIndices(roster) == 0, "Reload not idempotent");
        VBRForceLock.CustomUnitRoster.RemoveAt(roster, divisions, 1);
        Check(division.divs[0] == -1 && division.divs[1] == 3 && division.divsBack[0] == 3 && roster[division.divs[1]].id == ids[4], "Removal shifted members incorrectly");
        for (int i = 0; i < roster.Count; i++) Check(roster[i].index == i, "Invalid index after removal");
    }
}
"@
Add-Type -TypeDefinition ($source + $test) -ReferencedAssemblies @($unity, $game)
[RosterRegression]::Run()
Write-Output 'PASS: append, Nanna/Lulu/Mary collision repair, preserved player data and slots, idempotent reload, retired-unit removal.'
