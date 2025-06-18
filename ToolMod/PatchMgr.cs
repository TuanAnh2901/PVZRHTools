using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ToolModData;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.PatchMgr;

namespace ToolMod
{
    [HarmonyPatch(typeof(AlmanacCardZombie), "OnMouseDown")]
    public static class AlmanacCardZombiePatch
    {
        public static void Postfix(AlmanacCardZombie __instance) => AlmanacZombieType = __instance.theZombieType;
    }

    [HarmonyPatch(typeof(AlmanacPlantCtrl), "GetSeedType")]
    public static class AlmanacPlantCtrlPatch
    {
        public static void Postfix(AlmanacPlantCtrl __instance) => AlmanacSeedType = __instance.plantSelected;
    }

    [HarmonyPatch(typeof(Board), "Awake")]
    public static class BoardPatchA
    {
        public static void Postfix()
        {
            var t = Board.Instance.boardTag;
            originalTravel = t.enableTravelPlant;
            t.isScaredyDream |= PatchMgr.GameModes.ScaredyDream;
            t.isColumn |= PatchMgr.GameModes.ColumnPlanting;
            t.isSeedRain |= PatchMgr.GameModes.SeedRain;
            t.enableAllTravelPlant |= UnlockAllFusions;
            Board.Instance.boardTag = t;
        }
    }

    [HarmonyPatch(typeof(Board), "NewZombieUpdate")]
    public static class BoardPatchB
    {
        public static void Postfix()
        {
            if (NewZombieUpdateCD > 0 && NewZombieUpdateCD < 30 && Board.Instance.newZombieWaveCountDown > NewZombieUpdateCD)
            {
                Board.Instance.newZombieWaveCountDown = NewZombieUpdateCD;
            }
        }
    }

    [HarmonyPatch(typeof(Bucket), "Update")]
    public static class BucketPatch
    {
        public static void Postfix(Bucket __instance)
        {
            if (ItemExistForever) __instance.existTime = 0.1f;
        }
    }

    [HarmonyPatch(typeof(Bullet), "Update")]
    public static class BulletPatchA
    {
        public static void Postfix(Bullet __instance)
        {
            try
            {
                if (BulletDamage[__instance.theBulletType] >= 0 && __instance.Damage != BulletDamage[__instance.theBulletType])
                {
                    __instance.Damage = BulletDamage[__instance.theBulletType];
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(Bullet), "Die")]
    public static class BulletPatchB
    {
        public static bool Prefix(Bullet __instance)
        {
            if (UndeadBullet && !__instance.isZombieBullet)
            {
                __instance.hit = false;
                __instance.penetrationTimes = int.MaxValue;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CardUI))]
    public static class CardUIPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Postfix(CardUI __instance)
        {
            GameObject obj = new("ModifierCardCD");
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
            text.color = new(228f / 256f, 155f / 256f, 38f / 256f);
            obj.transform.SetParent(__instance.transform);
            obj.transform.localScale = new(0.7f, 0.7f, 0.7f);
            obj.transform.localPosition = new(39f, 0, 0);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostUpdate(CardUI __instance)
        {
            var child = __instance.transform.FindChild("ModifierCardCD");
            if (child is null) return;
            if (__instance.isAvailable || !ShowGameInfo)
            {
                child.GameObject().active = false;
            }
            else
            {
                child.GameObject().active = true;
                child.GameObject().GetComponent<TextMeshProUGUI>().text = $"{__instance.CD:N1}/{__instance.fullCD}";
            }
        }
    }

    [HarmonyPatch(typeof(Chomper), "Update")]
    public static class ChomperPatch
    {
        public static void Prefix(Chomper __instance)
        {
            if (ChomperNoCD && __instance.attributeCountdown > 0.05f)
            {
                __instance.attributeCountdown = 0.05f;
            }
        }
    }

    [HarmonyPatch(typeof(ConveyBeltMgr))]
    public static class ConveyBeltMgrPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void PostAwake(ConveyBeltMgr __instance)
        {
            if (ConveyBeltTypes.Count > 0)
            {
                __instance.plants = new();
                foreach (var p in ConveyBeltTypes)
                {
                    __instance.plants.Add((PlantType)p);
                }
            }
        }

        [HarmonyPatch("GetCardPool")]
        [HarmonyPostfix]
        public static void PostGetCardPool(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            if (ConveyBeltTypes.Count > 0)
            {
                Il2CppSystem.Collections.Generic.List<PlantType> list = new();
                foreach (var p in ConveyBeltTypes)
                {
                    list.Add((PlantType)p);
                }
                __result = list;
            }
        }
    }

    [HarmonyPatch(typeof(CreateBullet))]
    [HarmonyPatch("SetBullet")]
    [HarmonyPatch(new System.Type[] { typeof(float), typeof(float), typeof(int), typeof(BulletType), typeof(int), typeof(bool) })]
    public static class SetBullet_IntMovingWay_Patch
    {
        public static void Prefix(ref BulletType theBulletType)
        {
            if (LockBulletType == -1)
            {
                var values = (BulletType[])System.Enum.GetValues(typeof(BulletType));
                theBulletType = values[UnityEngine.Random.Range(0, values.Length)];
            }
            else if (LockBulletType >= 0)
            {
                theBulletType = (BulletType)LockBulletType;
            }
            // Nếu -2 hoặc < -1 thì không thay đổi
        }
    }

    [HarmonyPatch(typeof(CreateBullet))]
    [HarmonyPatch("SetBullet")]
    [HarmonyPatch(new System.Type[] { typeof(float), typeof(float), typeof(int), typeof(BulletType), typeof(BulletMoveWay), typeof(bool) })]
    public static class SetBullet_BulletMoveWay_Patch
    {
        public static void Prefix(ref BulletType theBulletType)
        {
            if (LockBulletType == -1)
            {
                var values = (BulletType[])System.Enum.GetValues(typeof(BulletType));
                theBulletType = values[UnityEngine.Random.Range(0, values.Length)];
            }
            else if (LockBulletType >= 0)
            {
                theBulletType = (BulletType)LockBulletType;
            }
        }
    }

        [HarmonyPatch(typeof(CreatePlant), "SetPlant")]
    public static class CreatePlantPatchC
    {
        public static void Prefix(ref bool isFreeSet) => isFreeSet = FreePlanting || isFreeSet;
    }

    [HarmonyPatch(typeof(DriverZombie), "PositionUpdate")]
    public static class DriverZombiePatch
    {
        public static void Postfix(DriverZombie __instance)
        {
            if (NoIceRoad)
            {
                Board.Instance.iceRoadX[__instance.theZombieRow] = 35f;
            }
        }
    }

    [HarmonyPatch(typeof(DroppedCard), "Update")]
    public static class DroppedCardPatch
    {
        public static void Postfix(DroppedCard __instance)
        {
            if (ItemExistForever) __instance.existTime = 0;
        }
    }

    [HarmonyPatch(typeof(Fertilize), "Update")]
    public static class FertilizePatch
    {
        public static void Postfix(Fertilize __instance)
        {
            if (ItemExistForever) __instance.existTime = 0.1f;
        }
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAppPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostStart()
        {
            GameObject obj = new("Modifier");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.AddComponent<DataProcessor>();
            obj.AddComponent<PatchMgr>();
        }
    }

    [HarmonyPatch(typeof(Glove), "Update")]
    public static class GlovePatchA
    {
        public static void Postfix(Glove __instance)
        {
            __instance.gameObject.transform.GetChild(0).gameObject.SetActive(!GloveNoCD);
            if (GloveFullCD > 0)
            {
                __instance.fullCD = (float)GloveFullCD;
            }
            if (GloveNoCD)
            {
                __instance.CD = __instance.fullCD;
            }
            if (__instance.avaliable || !ShowGameInfo)
            {
                __instance.transform.FindChild("ModifierGloveCD").GameObject().active = false;
            }
            else
            {
                __instance.transform.FindChild("ModifierGloveCD").GameObject().active = true;
                __instance.transform.FindChild("ModifierGloveCD").GameObject().GetComponent<TextMeshProUGUI>().text = $"{__instance.CD:N1}/{__instance.fullCD}";
            }
        }
    }

    [HarmonyPatch(typeof(Glove), "Start")]
    public static class GlovePatchB
    {
        public static void Postfix(Glove __instance)
        {
            GameObject obj = new("ModifierGloveCD");
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
            text.color = new(228f / 256f, 155f / 256f, 38f / 256f);
            obj.transform.SetParent(__instance.GameObject().transform);
            obj.transform.localScale = new(0.4f, 0.4f, 0.4f);
            obj.transform.localPosition = new(27.653f, 0, 0);
        }
    }

    [HarmonyPatch(typeof(GridItem), "SetGridItem")]
    public static class GridItemPatch
    {
        public static bool Prefix(ref GridItemType theType) => (int)theType >= 3 || !NoHole;
    }

    [HarmonyPatch(typeof(HammerMgr), "Update")]
    public static class HammerMgrPatchA
    {
        public static void Postfix(HammerMgr __instance)
        {
            __instance.gameObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(!HammerNoCD);
            if (HammerFullCD > 0)
            {
                __instance.fullCD = (float)HammerFullCD;
            }
            else
            {
                __instance.fullCD = OriginalFullCD;
            }
            if (HammerNoCD)
            {
                __instance.CD = __instance.fullCD;
            }
            if (__instance.avaliable || !ShowGameInfo)
            {
                __instance.transform.FindChild("ModifierHammerCD").GameObject().active = false;
            }
            else
            {
                __instance.transform.FindChild("ModifierHammerCD").GameObject().active = true;
                __instance.transform.FindChild("ModifierHammerCD").GameObject().GetComponent<TextMeshProUGUI>().text = $"{__instance.CD:N1}/{__instance.fullCD}";
            }
        }

        public static float OriginalFullCD { get; set; }
    }

    [HarmonyPatch(typeof(HammerMgr), "Start")]
    public static class HammerMgrPatchB
    {
        public static void Postfix(HammerMgr __instance)
        {
            GameObject obj = new("ModifierHammerCD");
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
            text.color = new(228f / 256f, 155f / 256f, 38f / 256f);
            obj.transform.SetParent(__instance.GameObject().transform);
            obj.transform.localScale = new(2f, 2f, 2f);
            obj.transform.localPosition = new(107, 0, 0);
        }
    }

    [HarmonyPatch(typeof(HyponoEmperor), "Update")]
    public static class HyponoEmperorPatch
    {
        public static void Postfix(HyponoEmperor __instance)
        {
            if (HyponoEmperorNoCD && __instance.summonZombieTime > 2f)
            {
                __instance.summonZombieTime = 2f;
            }
        }
    }

    [HarmonyPatch(typeof(InGameBtn), "OnMouseUpAsButton")]
    public static class InGameBtnPatch
    {
        public static void Postfix(InGameBtn __instance)
        {
            if (__instance.buttonNumber == 3)
            {
                TimeSlow = !TimeSlow;
                TimeStop = false;
                Time.timeScale = TimeSlow ? 0.2f : SyncSpeed;
            }
            if (__instance.buttonNumber == 13)
            {
                BottomEnabled = GameObject.Find("Bottom") is not null;
            }
        }

        public static bool BottomEnabled { get; set; } = false;
    }

    [HarmonyPatch(typeof(InGameText), "ShowText")]
    public static class InGameTextPatch
    {
        public static void Postfix()
        {
            for (int i = 0; i < InGameAdvBuffs.Length; i++)
            {
                if (InGameAdvBuffs[i] != GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades[i])
                {
                    SyncInGameBuffs();
                    return;
                }
            }
            for (int i = 0; i < InGameUltiBuffs.Length; i++)
            {
                if (InGameUltiBuffs[i] != GetBoolArray(GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades)[i])
                {
                    SyncInGameBuffs();
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(InitBoard))]
    public static class InitBoardPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ReadySetPlant")]
        public static void PreReadySetPlant()
        {
            if (CardNoInit)
            {
                if (SeedGroup is not null)
                {
                    for (int i = SeedGroup!.transform.childCount - 1; i >= 0; i--)
                    {
                        var card = SeedGroup.transform.GetChild(i);
                        if (card is null || card.childCount is 0) continue;
                        card.GetChild(0).gameObject.GetComponent<CardUI>().CD = card.GetChild(0).gameObject.GetComponent<CardUI>().fullCD;
                    }
                }
            }
            HammerMgrPatchA.OriginalFullCD = UnityEngine.Object.FindObjectsOfTypeAll(Il2CppType.Of<HammerMgr>())[0].Cast<HammerMgr>().fullCD;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RightMoveCamera")]
        public static void PreRightMoveCamera()
        {
            MelonCoroutines.Start(PostInitBoard());
        }
    }

    [HarmonyPatch(typeof(JackboxZombie), "Update")]
    public static class JackboxZombiePatch
    {
        public static void Postfix(JackboxZombie __instance)
        {
            if (JackboxNotExplode)
            {
                __instance.popCountDown = __instance.originalCountDown;
            }
        }
    }

    [HarmonyPatch(typeof(Plant), "PlantShootUpdate")]
    public static class PlantPatch
    {
        public static void Prefix(Plant __instance)
        {
            var s = __instance.TryCast<Shooter>();
            if (FastShooting && s is not null)
            {
                s.AnimShoot();
            }
        }
    }

    [HarmonyPatch(typeof(PotatoMine), "Update")]
    public static class PotatoMinePatch
    {
        public static void Prefix(PotatoMine __instance)
        {
            if (MineNoCD && __instance.attributeCountdown > 0.05f)
            {
                __instance.attributeCountdown = 0.05f;
            }
        }
    }

    [HarmonyPatch(typeof(Present), "RandomPlant")]
    public static class PresentPatchA
    {
        public static bool Prefix(Present __instance)
        {
            foreach (var plant in __instance.board.plantArray)
            {
                try
                {
                    if (plant.thePlantRow == __instance.thePlantRow && plant.thePlantColumn == __instance.thePlantColumn && plant.thePlantType != __instance.thePlantType)
                    {
                        return true;
                    }
                }
                catch { }
            }

            if (LockPresent >= 0)
            {
                CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow, (PlantType)LockPresent);
                if (CreatePlant.Instance.IsPuff((PlantType)LockPresent))
                {
                    CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow, (PlantType)LockPresent);
                    CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow, (PlantType)LockPresent);
                }

                return false;
            }
            if (SuperPresent)
            {
                __instance.SuperRandomPlant();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Present), "Start")]
    public static class PresentPatchB
    {
        public static void Postfix(Present __instance)
        {
            if (PresentFastOpen && (int)__instance.thePlantType != 245) __instance.AnimEvent();
        }
    }

    [HarmonyPatch(typeof(ProgressMgr), "Awake")]
    public static class ProgressMgrPatchA
    {
        public static void Postfix(ProgressMgr __instance)
        {
            GameObject obj = new("ModifierGameInfo");
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
            text.color = new(0, 1, 1);
            obj.transform.SetParent(__instance.GameObject().transform);
            obj.transform.localScale = new(0.4f, 0.2f, 0.2f);
            obj.transform.localPosition = new(100f, 2.2f, 0);
            obj.GetComponent<RectTransform>().sizeDelta = new(800, 50);
        }
    }

    [HarmonyPatch(typeof(ProgressMgr), "Update")]
    public static class ProgressMgrPatchB
    {
        public static void Postfix(ProgressMgr __instance)
        {
            if (ShowGameInfo)
            {
                __instance.transform.FindChild("ModifierGameInfo").GameObject().active = true;
                __instance.transform.FindChild("ModifierGameInfo").GameObject().GetComponent<TextMeshProUGUI>().text = $"波数: {Board.Instance.theWave}/{Board.Instance.theMaxWave} 刷新CD: {Board.Instance.newZombieWaveCountDown:N1}";
            }
            else
            {
                __instance.transform.FindChild("ModifierGameInfo").GameObject().active = false;
            }
        }
    }

    [HarmonyPatch(typeof(RandomZombie), "SetRandomZombie")]
    public static class RamdomZombiePatch
    {
        public static bool Prefix(RandomZombie __instance, ref GameObject __result)
        {
            if (!UltimateRamdomZombie) return true;
            if (Board.Instance is not null && Board.Instance.isEveStarted) return true;
            int id = UnityEngine.Random.RandomRangeInt(200, 223);
            if (UnityEngine.Random.RandomRangeInt(0, 5) == 1)
            {
                if (!__instance.isMindControlled)
                {
                    __result = CreateZombie.Instance.SetZombie(__instance.theZombieRow, (ZombieType)id, __instance.GameObject().transform.position.x);
                }
                else
                {
                    __result = CreateZombie.Instance.SetZombieWithMindControl(__instance.theZombieRow, (ZombieType)id, __instance.GameObject().transform.position.x);
                }
                return false;
            }
            else { return true; }
        }
    }

    [HarmonyPatch(typeof(Squalour), "LourDie")]
    public static class SqualourPatch
    {
        public static void Postfix() => GameAPP.developerMode = OriginalDevMode;

        public static void Prefix()
        {
            OriginalDevMode = GameAPP.developerMode;
            GameAPP.developerMode |= DevLour;
        }

        public static bool OriginalDevMode { get; set; } = false;
    }

    [HarmonyPatch(typeof(SuperSnowGatling), "Update")]
    public static class SuperSnowGatlingPatchA
    {
        public static void Postfix(SuperSnowGatling __instance)
        {
            if (UltimateSuperGatling)
            { __instance.timer = 0.1f; }
        }
    }

    [HarmonyPatch(typeof(SuperSnowGatling), "Shoot1")]
    public static class SuperSnowGatlingPatchB
    {
        public static void Postfix(SuperSnowGatling __instance)
        {
            if (UltimateSuperGatling) { __instance.AttributeEvent(); }
        }
    }

    [HarmonyPatch(typeof(TravelRefresh), "OnMouseUpAsButton")]
    public static class TravelRefreshPatch
    {
        public static void Postfix(TravelRefresh __instance)
        {
            if (BuffRefreshNoLimit) __instance.refreshTimes = 0;
        }
    }

    [HarmonyPatch(typeof(TravelStore), "RefreshBuff")]
    public static class TravelStorePatch
    {
        public static void Postfix(TravelStore __instance)
        {
            if (BuffRefreshNoLimit) __instance.count = 0;
        }
    }

    /*
    [HarmonyPatch(typeof(CreatePlant), "Lim")]
    public static class CreatePlantPatchA
    {
        public static void Postfix(ref bool __result) => __result = !UnlockAllFusions && __result;
    }

    [HarmonyPatch(typeof(CreatePlant), "LimTravel")]
    public static class CreatePlantPatchB
    {
        public static void Postfix(ref bool __result) => __result = !UnlockAllFusions && __result;
    }
    */

    [HarmonyPatch(typeof(UIMgr), "EnterMainMenu")]
    public static class UIMgrPatch
    {
        public static void Postfix()
        {
            GameObject obj = new("ModifierInfo");
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
            text.color = new(1, 1, 0, 1);
            text.text = "修改器作者为b站@Infinite75\n若存在任何付费/要求三连+关注/私信发链接的情况\n说明你被盗版骗了，请注意隐私和财产安全！！！\n此信息仅在游戏主菜单和修改窗口显示";
            obj.transform.SetParent(GameObject.Find("Leaves").transform);
            obj.transform.localScale = new(0.5f, 0.5f, 0.5f);
            obj.GetComponent<RectTransform>().sizeDelta = new(800, 50);
            obj.transform.localPosition = new(-345.5f, -42.6f, 0);
        }
    }

    [HarmonyPatch(typeof(Zombie), "Start")]
    public static class ZombiePatch
    {
        public static void Postfix(Zombie __instance)
        {
            try
            {
                if (HealthZombies[__instance.theZombieType] >= 0)
                {
                    __instance.theMaxHealth = HealthZombies[__instance.theZombieType];
                    __instance.theHealth = __instance.theMaxHealth;
                }
                if (Health1st[__instance.theFirstArmorType] >= 0 && __instance.theMaxHealth != Health1st[__instance.theFirstArmorType])
                {
                    __instance.theFirstArmorMaxHealth = Health1st[__instance.theFirstArmorType];
                    __instance.theFirstArmorHealth = __instance.theFirstArmorMaxHealth;
                }
                if (Health2nd[__instance.theSecondArmorType] >= 0 && __instance.theMaxHealth != Health2nd[__instance.theSecondArmorType])
                {
                    __instance.theSecondArmorMaxHealth = Health2nd[__instance.theSecondArmorType];
                    __instance.theSecondArmorHealth = __instance.theSecondArmorMaxHealth;
                }
                __instance.UpdateHealthText();
            }
            catch { }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class PatchMgr : MonoBehaviour
    {
        static PatchMgr()
        {
            foreach (var f in Enum.GetValues<Zombie.FirstArmorType>())
            {
                Health1st.Add(f, -1);
            }
            foreach (var s in Enum.GetValues<Zombie.SecondArmorType>())
            {
                Health2nd.Add(s, -1);
            }
        }

        public PatchMgr() : base(ClassInjector.DerivedConstructorPointer<PatchMgr>()) => ClassInjector.DerivedConstructorBody(this);

        public PatchMgr(IntPtr i) : base(i)
        {
        }

        //from Gaoshu
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        //from Gaoshu
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using var memoryStream = new MemoryStream(gZipBuffer);
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gZipStream.CopyTo(resultStream);
            byte[] buffer = resultStream.ToArray();
            return Encoding.UTF8.GetString(buffer);
        }

        public static bool[] GetBoolArray(Il2CppStructArray<int> list) => [.. from i in list select i > 0];

        public static Il2CppStructArray<int> GetIntArray(bool[] array) => new([.. from i in array select (i ? 1 : 0)]);

        public static bool InGame() => Board.Instance is not null && GameAPP.theGameStatus is not GameStatus.OpenOptions or GameStatus.OutGame or GameStatus.Almanac;

        public static IEnumerator PostInitBoard()
        {
            var travelMgr = GameAPP.gameAPP.GetOrAddComponent<TravelMgr>();
            Board.Instance.freeCD = FreeCD;
            yield return null;
            if (!(GameAPP.theBoardType == (LevelType)3 && Board.Instance.theCurrentSurvivalRound != 1))
            {
                yield return null;

                var advs = travelMgr.advancedUpgrades;

                for (int i = 0; i < advs.Count; i++)
                {
                    advs[i] = AdvBuffs[i] || advs[i];
                    yield return null;
                }

                var ultis = travelMgr.ultimateUpgrades;
                for (int i = 0; i < ultis.Count; i++)
                {
                    ultis[i] = (UltiBuffs[i] || ultis[i] is 1) ? 1 : 0;
                    yield return null;
                }

                var deb = travelMgr.debuff;
                for (int i = 0; i < deb.Count; i++)
                {
                    deb[i] = Debuffs[i] || deb[i];
                    yield return null;
                }
            }

            InGameAdvBuffs = new bool[TravelMgr.advancedBuffs.Count];
            InGameUltiBuffs = new bool[TravelMgr.ultimateBuffs.Count];
            InGameDebuffs = new bool[TravelMgr.debuffs.Count];
            yield return null;

            InGameAdvBuffs = travelMgr.advancedUpgrades;
            InGameUltiBuffs = GetBoolArray(travelMgr.ultimateUpgrades);
            InGameDebuffs = travelMgr.debuff;
            yield return null;
            Task.Run(SyncInGameBuffs);

            yield return null;
            if (ZombieSeaLow && SeaTypes.Count > 0)
            {
                int i = 0;
                for (int wave = 0; wave < Board.Instance.theMaxWave; wave++)
                {
                    for (int index = 0; index < 100; index++)
                    {
                        SetZombieList(index, wave, (ZombieType)SeaTypes[i]);
                        if (++i >= SeaTypes.Count) i = 0;
                    }
                }
            }
        }

        //感谢@高数带我飞(Github:https://github.com/LibraHp/)的在出怪表修改上的技术支持
        public static unsafe void SetZombieList(int index, int wave, ZombieType value)
        {
            FieldInfo? fieldInfo = typeof(InitZombieList).GetField("NativeFieldInfoPtr_zombieList", BindingFlags.NonPublic | BindingFlags.Static);

            if (fieldInfo is not null)
            {
                IntPtr nativeFieldInfoPtr = (IntPtr)fieldInfo.GetValue(null)!;
                unsafe
                {
                    Unsafe.SkipInit(out IntPtr intPtr);
                    IL2CPP.il2cpp_field_static_get_value(nativeFieldInfoPtr, &intPtr);
                    if (intPtr == IntPtr.Zero)
                    {
                        return;
                    }
                    ZombieType* arrayData = (ZombieType*)intPtr.ToPointer();
                    arrayData[index * 101 + wave + 9] = value;
                }
            }
        }

        public static void SyncInGameBuffs()
        {
            if (!InGame()) return;
            DataSync.Instance.Value.SendData(new SyncTravelBuff()
            {
                AdvInGame = [.. GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades!],
                UltiInGame = [.. GetBoolArray(GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades!)],
                DebuffsInGame = [.. GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().debuff!]
            });
        }

        public static void UpdateInGameBuffs()
        {
            for (int i = 0; i < GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades.Count; i++)
            {
                GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().advancedUpgrades![i] = InGameAdvBuffs[i];
            }
            for (int i = 0; i < GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades.Count; i++)
            {
                GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().ultimateUpgrades![i] = GetIntArray(InGameUltiBuffs)[i];
            }
            for (int i = 0; i < GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().debuff.Count; i++)
            {
                GameAPP.gameAPP.GetOrAddComponent<TravelMgr>().debuff![i] = InGameDebuffs[i];
            }
        }

        public void Update()
        {
            if (GameAPP.theGameStatus is GameStatus.InGame or GameStatus.InInterlude or GameStatus.Selecting)
            {
                if (Input.GetKeyDown(Core.KeyTimeStop.Value.Value))
                {
                    TimeStop = !TimeStop;
                    TimeSlow = false;
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    TimeStop = false;
                    TimeSlow = !TimeSlow;
                }
                if (Input.GetKeyDown(Core.KeyShowGameInfo.Value.Value))
                {
                    ShowGameInfo = !ShowGameInfo;
                }
                if (!TimeStop && !TimeSlow)
                {
                    Time.timeScale = SyncSpeed;
                }

                if (!TimeStop && TimeSlow)
                {
                    Time.timeScale = 0.2f;
                }
                if (InGameBtnPatch.BottomEnabled || (TimeStop && !TimeSlow))
                {
                    Time.timeScale = 0;
                }
                try
                {
                    try
                    {
                        var slow = GameObject.Find("SlowTrigger").transform;
                        slow.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
                        slow.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
                        if (Input.GetKeyDown(Core.KeyTopMostCardBank.Value.Value))
                        {
                            if (GameAPP.canvas.GetComponent<Canvas>().sortingLayerName == "Default")
                            {
                                GameAPP.canvas.GetComponent<Canvas>().sortingLayerName = "UI";
                            }
                            else
                            {
                                GameAPP.canvas.GetComponent<Canvas>().sortingLayerName = "Default";
                            }
                        }
                    }
                    catch { }

                    if (Input.GetKeyDown(Core.KeyAlmanacCreatePlant.Value.Value) && AlmanacSeedType != -1)
                    {
                        CreatePlant.Instance.SetPlant(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow, (PlantType)AlmanacSeedType);
                    }
                    if (Input.GetKeyDown(Core.KeyAlmanacZombieMindCtrl.Value.Value))
                    {
                        Core.AlmanacZombieMindCtrl.Value.Value = !Core.AlmanacZombieMindCtrl.Value.Value;
                    }
                    if (Input.GetKeyDown(Core.KeyAlmanacCreateZombie.Value.Value) && AlmanacZombieType is not ZombieType.Nothing)
                    {
                        if (Core.AlmanacZombieMindCtrl.Value.Value)
                        {
                            CreateZombie.Instance.SetZombieWithMindControl(Mouse.Instance.theMouseRow, AlmanacZombieType, Mouse.Instance.mouseX);
                        }
                        else
                        {
                            CreateZombie.Instance.SetZombie(Mouse.Instance.theMouseRow, AlmanacZombieType, Mouse.Instance.mouseX);
                        }
                    }
                    if (Input.GetKeyDown(Core.KeyAlmanacCreatePlantVase.Value.Value) && AlmanacSeedType != -1)
                    {
                        GridItem.SetGridItem(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType = (PlantType)AlmanacSeedType;
                    }
                    if (Input.GetKeyDown(Core.KeyAlmanacCreateZombieVase.Value.Value) && AlmanacZombieType is not ZombieType.Nothing)
                    {
                        GridItem.SetGridItem(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType = AlmanacZombieType;
                    }
                    if (Board.Instance is not null)
                    {
                        var t = Board.Instance.boardTag;
                        t.enableTravelPlant = t.enableTravelPlant || UnlockAllFusions;
                        Board.Instance.boardTag = t;
                    }
                    else
                    {
                        return;
                    }
                }
                catch (NullReferenceException e) { MLogger.Error(e); }

                if (LockSun)
                {
                    Board.Instance.theSun = LockSunCount;
                }
                if (LockMoney)
                {
                    Board.Instance.theMoney = LockMoneyCount;
                }
                if (StopSummon)
                {
                    Board.Instance.iceDoomFreezeTime = 1;
                }
                if (ZombieSea)
                {
                    if (++seaTime >= ZombieSeaCD &&
                        Board.Instance.theWave is not 0 && Board.Instance.theWave < Board.Instance.theMaxWave &&
                        GameAPP.theGameStatus == (int)GameStatus.InGame)
                    {
                        foreach (var j in SeaTypes)
                        {
                            if (j < 0) continue;
                            for (int i = 0; i < Board.Instance.rowNum; i++)
                            {
                                CreateZombie.Instance.SetZombie(i, (ZombieType)j);
                            }
                        }
                        seaTime = 0;
                    }
                }
                if (GarlicDay && ++garlicDayTime >= 500 && GameAPP.theGameStatus == (int)GameStatus.InGame)
                {
                    garlicDayTime = 0;
                    _ = FindObjectsOfTypeAll(Il2CppType.Of<Zombie>()).All(b =>
                    {
                        b?.TryCast<Zombie>()?.StartCoroutine_Auto(b?.TryCast<Zombie>()?.DeLayGarliced(0.1f, false, false));
                        return true;
                    });
                }
            }
        }

        public static bool[] AdvBuffs { get; set; } = [];
        public static bool AlmanacCreate { get; set; } = false;
        public static int AlmanacSeedType { get; set; } = -1;
        public static ZombieType AlmanacZombieType { get; set; } = ZombieType.Nothing;
        public static bool BuffRefreshNoLimit { get; set; } = false;
        public static Dictionary<BulletType, int> BulletDamage { get; set; } = [];
        public static bool CardNoInit { get; set; } = false;
        public static bool ChomperNoCD { get; set; } = false;
        public static bool CobCannonNoCD { get; set; } = false;
        public static List<int> ConveyBeltTypes { get; set; } = [];
        public static bool[] Debuffs { get; set; } = [];
        public static bool DevLour { get; set; } = false;
        public static bool FastShooting { get; set; } = false;
        public static bool FreeCD { get; set; } = false;
        public static bool FreePlanting { get; set; } = false;
        public static GameModes GameModes { get; set; }
        public static bool GarlicDay { get; set; } = false;
        public static double GloveFullCD { get; set; } = 0;
        public static bool GloveNoCD { get; set; } = false;
        public static double HammerFullCD { get; set; } = 0;
        public static bool HammerNoCD { get; set; } = false;
        public static bool HardPlant { get; set; } = false;
        public static Dictionary<Zombie.FirstArmorType, int> Health1st { get; set; } = [];
        public static Dictionary<Zombie.SecondArmorType, int> Health2nd { get; set; } = [];
        public static Dictionary<PlantType, int> HealthPlants { get; set; } = [];
        public static Dictionary<ZombieType, int> HealthZombies { get; set; } = [];
        public static bool HyponoEmperorNoCD { get; set; } = false;
        public static int ImpToBeThrown { get; set; } = 37;
        public static bool[] InGameAdvBuffs { get; set; } = [];
        public static bool[] InGameDebuffs { get; set; } = [];
        public static bool[] InGameUltiBuffs { get; set; } = [];
        public static bool ItemExistForever { get; set; } = false;
        public static int JachsonSummonType { get; set; } = 7;
        public static bool JackboxNotExplode { get; set; } = false;
        public static int LockBulletType { get; set; } = -2;
        public static bool LockMoney { get; set; } = false;
        public static int LockMoneyCount { get; set; } = 3000;
        public static int LockPresent { get; set; } = -1;
        public static bool LockSun { get; set; } = false;
        public static int LockSunCount { get; set; } = 500;
        public static bool MineNoCD { get; set; } = false;
        public static MelonLogger.Instance MLogger => Core.Instance.Value.LoggerInstance;
        public static float NewZombieUpdateCD { get; set; } = 30;
        public static bool NoHole { get; set; } = false;
        public static bool NoIceRoad { get; set; } = false;
        public static bool PlantUpgrade { get; set; } = false;
        public static bool PresentFastOpen { get; set; } = false;
        public static List<int> SeaTypes { get; set; } = [];

        public static GameObject? SeedGroup
        {
            get
            {
                try
                {
                    return InGame() ? GameObject.Find("SeedGroup") : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static bool ShowGameInfo { get; set; } = false;
        public static bool StopSummon { get; set; } = false;
        public static bool SuperPresent { get; set; } = false;
        public static float SyncSpeed { get; set; } = -1;
        public static bool TimeSlow { get; set; } = false;
        public static bool TimeStop { get; set; } = false;
        public static bool[] UltiBuffs { get; set; } = [];
        public static bool UltimateRamdomZombie { get; set; } = false;
        public static bool UltimateSuperGatling { get; set; } = false;
        public static bool UndeadBullet { get; set; } = false;
        public static bool UnlockAllFusions { get; set; } = false;
        public static bool ZombieSea { get; set; } = false;
        public static int ZombieSeaCD { get; set; } = 40;
        public static bool ZombieSeaLow { get; set; } = false;
        internal static bool originalTravel;
        private static int garlicDayTime = 0;
        private static int seaTime = 0;
    }
}