using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BattleSystem;
using BepInEx;
using DataSet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VBRForceLock
{
    // Nambahin unit custom yang gak ada di data game asli, lalu langsung
    // dimasukkan ke roster player (bukan lewat flag recruit cerita).
    [BepInPlugin("vbr.force.customunit", "VBR Custom Unit", "1.2.0")]
    public class CustomUnitPlugin : BaseUnityPlugin
    {
        private const string LuluId = "zzz_custom_lulu2";
        private const string LuluName = "Faceless Reaper Lulu";
        private const string LuluPortraitKey = "zzz_custom_lulu_portrait";

        // Marker buat buang node Research bekas percobaan lama (sebelum fitur
        // recruit-lewat-Research ditinggalkan). Aman dibiarkan, no-op kalau
        // gak ketemu.
        private const string LeftoverResearchNodeMarker = "Unit custom dari seri VBR lain.";

        private static Sprite _luluSprite;
        private static BepInEx.Logging.ManualLogSource _log;

        // Dilacak lewat reference object userData-nya sendiri, bukan flag bool
        // biasa — soalnya GameDatas.Instance.userData bisa udah non-null duluan
        // di title screen (data kosongan) SEBELUM save asli di-load/continue,
        // dan objeknya keganti pas itu terjadi. Flag bool sekali-jalan bakal
        // kepake di data kosongan itu dan gak pernah jalan ulang buat save asli.
        private userDataSet _lastProcessedUserData;

        void Awake()
        {
            _log = Logger;
            Logger.LogInfo("VBR Custom Unit loaded (Lulu otomatis masuk roster)");
            LoadLuluSprite();
            // Tiap PatchAll dibungkus try/catch sendiri-sendiri — patch generic
            // method (Load<T>) pernah gagal apply dan itu bikin SISA Awake() ini
            // ikut berhenti total (Update() gak pernah jalan lagi sesudahnya).
            Harmony harmony = new Harmony("vbr.force.customunit");
            SafePatch(harmony, typeof(LuluPortraitPatch));
            SafePatch(harmony, typeof(LuluUnitInfoPortraitPatch));
            SafePatch(harmony, typeof(LuluBlockPortraitPatch));
            SafePatch(harmony, typeof(LuluUnitBlockPortraitPatch));
            SafePatch(harmony, typeof(LuluStatusPortraitPatch));
            SafePatch(harmony, typeof(LuluLoadSpritePatch));
        }

        private void SafePatch(Harmony harmony, Type patchType)
        {
            try
            {
                harmony.PatchAll(patchType);
            }
            catch (Exception e)
            {
                Logger.LogWarning("[CustomUnit] Gagal patch " + patchType.Name + ": " + e);
            }
        }

        void Update()
        {
            userDataSet userData = GameDatas.Instance != null ? GameDatas.Instance.userData : null;
            if (userData == null || userData == _lastProcessedUserData)
            {
                return;
            }
            _lastProcessedUserData = userData;

            RemoveLeftoverResearchNode(userData);
            AddLulu(userData);
        }

        // Buang node riset custom yang sempat ditambahin versi mod sebelumnya
        // (identifikasi lewat comment marker-nya). Gak nyentuh node lain sama
        // sekali, gak nge-reset level/open siapa pun.
        private void RemoveLeftoverResearchNode(userDataSet userData)
        {
            for (int i = userData.ResearchMatrixData.Count - 1; i >= 0; i--)
            {
                if (userData.ResearchMatrixData[i].comment == LeftoverResearchNodeMarker)
                {
                    userData.ResearchMatrixData.RemoveAt(i);
                    Logger.LogInfo("[CustomUnit] Node Research bekas mod sebelumnya dibuang.");
                }
            }
        }

        private void LoadLuluSprite()
        {
            try
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(dir, "UlForce_lulu.png");
                if (!File.Exists(path))
                {
                    Logger.LogWarning("[CustomUnit] File portrait tidak ketemu: " + path);
                    return;
                }
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(bytes);
                _luluSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                Logger.LogWarning("[CustomUnit] Gagal load portrait: " + e.Message);
            }
        }

        // ---------- Unit custom: Lulu, langsung aktif di roster ----------

        private void AddLulu(userDataSet userData)
        {
            UnitData existing = userData.UnitData.player.Find(u => u.id == LuluId);
            if (existing != null)
            {
                existing.barrack = 0;
                existing.division = -1;
                existing.image1[0] = LuluPortraitKey;
                // image1[1] SENGAJA gak ditimpa — dipakai sistem tag battle HUD
                // (BattleSystem.MakeCode) buat cari aset langsung, bukan lewat
                // method yang kita patch. Diisi kunci palsu bikin blank putih.
                existing.image1[4] = LuluPortraitKey;
                // Unit yang udah ada dibikin dari template versi lama (dipersist di save) —
                // timpa ulang semua field kosmetik & skill template-nya tiap load biar
                // perubahan kode selalu ke-refresh, gak nyangkut ke versi lama di save.
                ApplyLuluSkills(existing.unitDatas);
                existing.SetBaseSkill(existing.unitDatas, -1);
                ApplyLuluCosmetics(existing.unitDatas);
                Logger.LogInfo("[CustomUnit] '" + LuluName + "' sudah ada di roster, dipastikan aktif.");
                return;
            }

            UnitDataSet template = CreateLuluTemplate(userData.UnitDataSet);
            userData.UnitDataSet.Add(template);

            UnitData unit = new UnitData(template);
            unit.division = -1;
            unit.barrack = 0;
            unit.SetExp(1);
            unit.status.pow = 199;
            unit.status.def = 93;
            unit.status.spd = 172;
            unit.status.wis = 42;

            userData.UnitData.player.Add(unit);
            Logger.LogInfo("[CustomUnit] '" + LuluName + "' ditambahkan ke roster player.");
        }

        // Field kosmetik teks/ikon yang gak nyangkut ke sistem lain (race/slay icon,
        // flavor comment, baris "Com" per-loyalty) — dipisah biar bisa dipanggil ulang
        // buat refresh unit yang udah kepersist di save (lihat AddLulu, branch existing).
        private void ApplyLuluCosmetics(UnitDataSet template)
        {
            // Race icon row (UnitInfoHandler._tribes, lookup via "race_icon_48x5_"+mode
            // atlas, 1 char = 1 icon). Karakternya dicocokkan dari unit game asli "Luna
            // Fairy" (m0573) yang punya race persis: Female, Demon, Fly, Nature, Night, Ice.
            template.tribe = "女魔飛樹夜氷"; // Female, Demon, Fly, Nature, Night, Ice
            // Slay icon row (UnitInfoHandler._specials, field UnitDataSet.special) — races
            // yang DIUNTUNGKAN diserang unit ini, dipakai sama atlas & mekanisme kayak tribe.
            // Sama-sama disamain dari "Luna Fairy" (m0573): Fire, Ice, Lightning.
            template.special = "炎氷雷"; // Fire, Ice, Lightning
            // Flavor text singkat di bawah HP bar kartu detail (UnitInfoHandler._unitCom,
            // field UnitDataSet.comment) — sama kayak "I dance, bathed in the moonlight!"
            // punya Luna Fairy.
            template.comment = "Death doesn't need a face to find you.";
            // Baris "CHOP!CHOP!" (UnitInfoHandler._com) BUKAN dari comment — dari
            // UnitDataSet.script, dipilih pakai TextResource.iMode(tier, 5) berdasarkan
            // loyalty unit (tier 0-4 = loyalty <150/<350/<550/<750/>=750). List 15 elemen
            // (5 tier x 3 locale jp/us/cn); locale US mulai index 5. Timpa index 5-9 biar
            // gak nyisa punya goblin ("CHOP!CHOP!" dst) di tier mana pun.
            template.script[5] = "...Don't get in my way.";
            template.script[6] = "Hm. You're still standing. Noted.";
            template.script[7] = "Fine. I'll cut a path for you today.";
            template.script[8] = "Stay close. My scythe won't mistake you for prey.";
            template.script[9] = "Even a faceless reaper... can call this home.";
        }

        // Sama kayak goblin: clone template yang sudah ada biar list internal valid,
        // lalu timpa stat/nama/skill-nya. Stat pow/def/spd/wis final di-set langsung
        // di instance UnitData (lihat AddLulu), bukan lewat basic di sini, karena
        // game punya rumus growth sendiri yang gak gampang ditiru persis.
        // basic.hp tetap dipakai karena HP dihitung ulang tiap saat dari basic.hp+level.
        private UnitDataSet CreateLuluTemplate(List<UnitDataSet> masterList)
        {
            UnitDataSet baseTemplate = masterList[0];
            UnitDataSet custom = (UnitDataSet)baseTemplate.Clone();

            custom.index = GetNextUnitIndex(masterList);
            custom.id = LuluId;
            custom.name = LuluName;
            custom.rank = 17;
            custom.cost = 7;
            custom.pay = 2;
            custom.open = HDDataSetDef.MAXOPEN;
            ApplyLuluCosmetics(custom);
            // basic.hp dikalibrasi biar HP di level 106 ~= 2861 (rumus game: ((lv-1)*0.25+1) * basic.hp)
            custom.basic.Set(35, 16, 30, 7, 109);
            // image1[4] = icon kecil (grid Research/battle, lewat LuluPortraitPatch).
            // image1[0] = portrait battle-viewer (lewat LuluLoadSpritePatch).
            // image1[1] SENGAJA gak ditimpa — dipakai sistem tag battle HUD
            // (BattleSystem.MakeCode) buat cari aset langsung, bukan lewat
            // method yang kita patch. Diisi kunci palsu bikin blank putih.
            // Kotak Division Info-nya sendiri tetap kebenerin lewat
            // LuluUnitBlockPortraitPatch (override langsung, gak gantung ke
            // image1[1]). Portrait besar & icon grid army ditangani terpisah
            // lewat LuluUnitInfoPortraitPatch/LuluBlockPortraitPatch.
            custom.image1[0] = LuluPortraitKey;
            custom.image1[4] = LuluPortraitKey;

            ApplyLuluSkills(custom);

            return custom;
        }

        // Skill NYATA Lulu (id ketemu lewat PassiveDataSet — lihat MODDING_NOTES.md),
        // power/level-nya disamain ke angka yang tampil di kartu skill Lulu aslinya.
        private void ApplyLuluSkills(UnitDataSet template)
        {
            SetSkillSlot(template.skillBase, 0, "L014", 40); // Lightning Boost
            SetSkillSlot(template.skillBase, 1, "B011", 20); // Lightning Field
            SetSkillSlot(template.skillBase, 2, "I005", 20); // Flank Attack
            SetSkillSlot(template.skillBase, 3, "I003", 0);  // Cross Attack
            SetSkillSlot(template.skillBase, 4, "I010", 50); // Dimension Slash
            SetSkillSlot(template.skillBase, 5, "I011", 50); // Lethal Critical
            SetSkillSlot(template.skillBase, 6, "I015", 50); // Max-Power Attack (Full Power Attack)
            SetSkillSlot(template.skillBase, 7, "J002", 0);  // Range Null
            SetSkillSlot(template.leader, 0, "I010", 25);    // Dimension Slash (tier leader)
            SetSkillSlot(template.leader, 1, "L030", 50);    // Squad Boost (tier leader)
            SetSkillSlot(template.trick, 0, "L030", 15);     // Squad Boost (tier trick)
        }

        // Bikin SkillData baru langsung dari id (name dikosongin/NULL biar
        // resolve nama & efeknya lewat id, bukan lewat name — lihat catatan di
        // MODDING_NOTES.md soal kenapa .name gak bisa dipakai buat rename).
        private void SetSkillSlot(List<SkillData> targetList, int targetIdx, string skillId, int power)
        {
            if (targetIdx >= targetList.Count)
            {
                return;
            }
            SkillData sk = new SkillData();
            sk.id = skillId;
            sk.name = HDDataSetDef.NULL;
            sk.power = power;
            targetList[targetIdx] = sk;
        }

        // Manual loop, sengaja hindari System.Linq (Max) — versi System.Core
        // yang dipakai compile-time beda dengan yang dibundle Mono runtime
        // game ini, dan itu bisa bikin assembly gagal di-load di dalam game.
        private int GetNextUnitIndex(List<UnitDataSet> masterList)
        {
            int max = 0;
            for (int i = 0; i < masterList.Count; i++)
            {
                if (masterList[i].index > max)
                {
                    max = masterList[i].index;
                }
            }
            return max + 1;
        }

        // Ganti sprite portrait unit dari lookup atlas bawaan game jadi gambar
        // custom (di-load dari UlForce_lulu.png) kalau key-nya cocok. Dipakai
        // grid Research/battle icon lewat image1[4].
        [HarmonyPatch(typeof(BinaryLoad), nameof(BinaryLoad.LoadSpriteMultiple))]
        private static class LuluPortraitPatch
        {
            static bool Prefix(string fp, string fn, ref Sprite __result)
            {
                if (fn == LuluPortraitKey && _luluSprite != null)
                {
                    __result = _luluSprite;
                    return false;
                }
                return true;
            }
        }

        // Portrait besar di kartu detail unit ("Assigned Units" dkk) — method
        // konsumennya (BUKAN BinaryLoad.Load<T> generik yang bahaya di-patch)
        // yang ditimpa di sini, jadi lebih aman.
        [HarmonyPatch(typeof(UnitInfoHandler), "DrawData", new Type[] { typeof(UnitData), typeof(UnitData[]) })]
        private static class LuluUnitInfoPortraitPatch
        {
            private static bool _everCalled;

            static void Postfix(UnitInfoHandler __instance, UnitData u)
            {
                if (!_everCalled)
                {
                    _everCalled = true;
                    _log.LogInfo("[diag] UnitInfoHandler.DrawData KEPANGGIL (unit=" + (u != null ? u.unique : "null") + ")");
                }
                if (u == null || u.id != LuluId)
                {
                    return;
                }
                _log.LogInfo("[diag] UnitInfoHandler.DrawData KEPANGGIL BUAT LULU");
                if (_luluSprite == null)
                {
                    return;
                }
                Image img = Traverse.Create(__instance).Field("_unitImage").GetValue<Image>();
                if (img != null)
                {
                    ForceApplySprite(img);
                }
            }
        }

        // Icon kecil di grid roster/army — sama, timpa method konsumennya.
        [HarmonyPatch(typeof(SingleUnitBlockHandler), "DataDaraw")]
        private static class LuluBlockPortraitPatch
        {
            private static bool _everCalled;

            static void Postfix(SingleUnitBlockHandler __instance)
            {
                if (!_everCalled)
                {
                    _everCalled = true;
                    _log.LogInfo("[diag] SingleUnitBlockHandler.DataDaraw KEPANGGIL (unit=" + (__instance.data != null ? __instance.data.unique : "null") + ")");
                }
                if (__instance.data == null || __instance.data.id != LuluId)
                {
                    return;
                }
                _log.LogInfo("[diag] SingleUnitBlockHandler.DataDaraw KEPANGGIL BUAT LULU");
                if (_luluSprite == null)
                {
                    return;
                }
                Image img = Traverse.Create(__instance).Field("_Image").GetValue<Image>();
                if (img != null)
                {
                    ForceApplySprite(img);
                }
            }
        }

        // Kotak status battle HUD — beda lagi sistemnya: BattleSystem.MakeCode
        // (encode/decode string tag), diproses StatusControl.SetStatus() yang
        // manggil Resources.Load<Sprite>(make.image) — Unity generik langsung,
        // BUKAN lewat BinaryLoad sama sekali. Patch method-nya (bukan
        // Resources.Load<T> yang generik & bahaya), identifikasi Lulu lewat
        // make.name (diisi dari unitData.unique pas TagMake()).
        // PENTING: sistem tag ini split di SPASI, jadi "Faceless Reaper Lulu"
        // kepotong jadi cuma "Faceless" pas nyampe sini (unit lain yang namanya
        // 1 kata, kayak "Jack-o-Lantern", aman). Makanya bandinginnya ke kata
        // pertama doang, bukan LuluName penuh.
        [HarmonyPatch(typeof(StatusControl), "SetStatus")]
        private static class LuluStatusPortraitPatch
        {
            private const string LuluNameFirstWord = "Faceless";

            static void Postfix(StatusControl __instance)
            {
                MakeCode make = Traverse.Create(__instance).Field("make").GetValue<MakeCode>();
                if (make == null || make.name != LuluNameFirstWord)
                {
                    return;
                }
                if (_luluSprite == null || __instance.image == null)
                {
                    return;
                }
                Image img = __instance.image.GetComponent<Image>();
                if (img != null)
                {
                    ForceApplySprite(img);
                }
            }
        }

        // Kotak Division Info (dengan tombol eject/X) — UnitBlockHandler (BEDA
        // dari SingleUnitBlockHandler di atas!) manggil Load<T> pakai image1[1],
        // bukan image1[2]/[4]. Method konsumennya di-patch, bukan Load<T>-nya.
        [HarmonyPatch(typeof(UnitBlockHandler), "DataDaraw")]
        private static class LuluUnitBlockPortraitPatch
        {
            static void Postfix(UnitBlockHandler __instance)
            {
                if (__instance.data == null || __instance.data.id != LuluId)
                {
                    return;
                }
                _log.LogInfo("[diag] UnitBlockHandler.DataDaraw KEPANGGIL BUAT LULU");
                if (_luluSprite == null)
                {
                    return;
                }
                Image img = Traverse.Create(__instance).Field("_Background").GetValue<Image>();
                if (img != null)
                {
                    ForceApplySprite(img);
                }
            }
        }

        // Portrait di viewer battle (BattleSystem.UnitViewerControl dkk) manggil
        // BinaryLoad.LoadSprite("unit/" + image1[0], ...) — bukan LoadSpriteMultiple
        // ataupun Load<T>. Patch versi PALING DALAM (5 parameter) biar semua
        // overload (1/4/5 parameter) yang funnel ke situ ikut ke-cover.
        [HarmonyPatch(typeof(BinaryLoad), nameof(BinaryLoad.LoadSprite), new Type[] { typeof(string), typeof(float), typeof(float), typeof(BinaryLoad.LoadType), typeof(float) })]
        private static class LuluLoadSpritePatch
        {
            static bool Prefix(string path, ref Sprite __result)
            {
                if (path != null && path.EndsWith(LuluPortraitKey) && _luluSprite != null)
                {
                    __result = _luluSprite;
                    return false;
                }
                return true;
            }
        }

        // Timpa .sprite doang. SEMPAT juga nimpa .overrideSprite, tapi itu
        // ternyata bikin bug: nunjuk ke icon Lulu bikin SEMUA portrait unit
        // ikut ketimpa jadi Lulu — kemungkinan overrideSprite dipakai juga sama
        // sistem hover/selection game ini secara internal (nilainya di-cache
        // terus "dipulihkan" ke elemen lain secara salah). .sprite doang aman
        // dan udah cukup buat semua titik yang udah kebukti jalan.
        private static void ForceApplySprite(Image img)
        {
            // Simpen ukuran kotak SEBELUM ganti sprite — kalau ada kode yang
            // manggil SetNativeSize() abis assign sprite, ukurannya bakal
            // ke-resize ngikutin ukuran asli PNG kita (183x195, jauh lebih
            // kecil dari portrait asli), makanya keliatan ngerecil di pojok.
            // Dipaksa balik lagi ke ukuran semula abis itu.
            RectTransform rt = img.rectTransform;
            Vector2 sizeBefore = rt.sizeDelta;

            img.sprite = _luluSprite;
            img.SetAllDirty();

            rt.sizeDelta = sizeBefore;

            CheckAnimatorInterference(img);
        }

        // Diagnostik: telusuri GameObject Image ini + sampai beberapa parent
        // ke atas, cari komponen Animator/Animation yang mungkin nge-drive
        // ulang sprite-nya tiap frame (nutupin override manual kita).
        private static readonly HashSet<int> _loggedAnimatorCheck = new HashSet<int>();
        private static void CheckAnimatorInterference(Image img)
        {
            int id = img.GetInstanceID();
            if (!_loggedAnimatorCheck.Add(id))
            {
                return;
            }
            Transform t = img.transform;
            int depth = 0;
            while (t != null && depth < 6)
            {
                Animator anim = t.GetComponent<Animator>();
                Animation legacyAnim = t.GetComponent<Animation>();
                if (anim != null)
                {
                    RuntimeAnimatorController ctrl = anim.runtimeAnimatorController;
                    _log.LogInfo("[diag] Animator KETEMU di '" + t.name + "' (depth " + depth + "), controller=" +
                        (ctrl != null ? ctrl.name : "null") + ", enabled=" + anim.enabled);
                }
                if (legacyAnim != null)
                {
                    _log.LogInfo("[diag] Animation (legacy) KETEMU di '" + t.name + "' (depth " + depth + "), enabled=" + legacyAnim.enabled + ", clip=" + (legacyAnim.clip != null ? legacyAnim.clip.name : "null"));
                }
                t = t.parent;
                depth++;
            }
        }
    }
}
