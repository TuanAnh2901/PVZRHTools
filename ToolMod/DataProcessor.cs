using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using System.Text.Json;
using System.Text.Json.Nodes;
using ToolModData;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.PatchMgr;
using static ToolModData.Modifier;

namespace ToolMod
{
    [RegisterTypeInIl2Cpp]
    public class DataProcessor : MonoBehaviour
    {
        public DataProcessor() : base(ClassInjector.DerivedConstructorPointer<DataProcessor>()) => ClassInjector.DerivedConstructorBody(this);

        public DataProcessor(IntPtr i) : base(i)
        {
        }

        public static void AddData(string d) => Data = d;

        public static void ProcessData<T>(T data) where T : ISyncData
        {
            if (data is ValueProperties v)
            {
                if (v.PlantsHealth is not null)
                {
                    HealthPlants[(PlantType)v.PlantsHealth.Value.Key] = v.PlantsHealth.Value.Value;
                    if (InGame())
                    {
                        foreach (var pl in Board.Instance.plantArray)
                        {
                            if (HealthPlants[pl.thePlantType] >= 0)
                            {
                                pl.thePlantMaxHealth = HealthPlants[pl.thePlantType];
                                if (pl.thePlantHealth > pl.thePlantMaxHealth) pl.thePlantHealth = pl.thePlantHealth;
                                pl.UpdateText();
                            }
                        }
                    }
                }
                if (v.ZombiesHealth is not null)
                {
                    HealthZombies[(ZombieType)v.ZombiesHealth.Value.Key] = v.ZombiesHealth.Value.Value;
                    if (InGame())
                    {
                        foreach (var z in Board.Instance.zombieArray)
                        {
                            if (HealthZombies[z.theZombieType] >= 0)
                            {
                                z.theMaxHealth = HealthZombies[z.theZombieType];
                                if (z.theHealth > z.theMaxHealth) z.theHealth = z.theMaxHealth;
                            }
                            z.UpdateHealthText();
                        }
                    }
                }
                if (v.FirstArmorsHealth is not null)
                {
                    Health1st[(Zombie.FirstArmorType)v.FirstArmorsHealth.Value.Key] = v.FirstArmorsHealth.Value.Value;
                    if (InGame())
                    {
                        foreach (var z in Board.Instance.zombieArray)
                        {
                            if (Health1st[z.theFirstArmorType] >= 0)
                            {
                                z.theFirstArmorMaxHealth = Health1st[z.theFirstArmorType];
                                if (z.theFirstArmorHealth > z.theFirstArmorMaxHealth) z.theFirstArmorHealth = z.theFirstArmorMaxHealth;
                            }
                            z.UpdateHealthText();
                        }
                    }
                }
                if (v.SecondArmorsHealth is not null)
                {
                    Health2nd[(Zombie.SecondArmorType)v.SecondArmorsHealth.Value.Key] = v.SecondArmorsHealth.Value.Value;
                    if (InGame())
                    {
                        foreach (var z in Board.Instance.zombieArray)
                        {
                            if (Health2nd[z.theSecondArmorType] >= 0)
                            {
                                z.theSecondArmorMaxHealth = Health2nd[z.theSecondArmorType];
                                if (z.theSecondArmorHealth > z.theSecondArmorMaxHealth) z.theSecondArmorHealth = z.theSecondArmorMaxHealth;
                            }
                            z.UpdateHealthText();
                        }
                    }
                }
                if (v.BulletsDamage is not null)
                {
                    BulletDamage[(BulletType)v.BulletsDamage.Value.Key] = v.BulletsDamage.Value.Value;
                }
                if (v.LockBulletType is not null)
                {
                    LockBulletType = (int)v.LockBulletType;
                    MelonLogger.Msg($"Lock Bullet Type：{LockBulletType}");
                }
                return;
            }
            if (data is BasicProperties p)
            {
                if (p.DeveloperMode is not null) GameAPP.developerMode = (bool)p.DeveloperMode;
                if (p.GameSpeed is not null) SyncSpeed = (float)p.GameSpeed;
                if (p.GloveNoCD is not null) GloveNoCD = (bool)p.GloveNoCD;
                if (p.HammerNoCD is not null) HammerNoCD = (bool)p.HammerNoCD;
                if (p.PlantingNoCD is not null && Board.Instance is not null)
                {
                    FreeCD = (bool)p.PlantingNoCD;
                    Board.Instance.freeCD = FreeCD;
                }
                if (p.FreePlanting is not null) FreePlanting = (bool)p.FreePlanting;
                if (p.UnlockAllFusions is not null)
                {
                    UnlockAllFusions = (bool)p.UnlockAllFusions;
                    if (InGame())
                    {
                        var t = Board.Instance!.boardTag;
                        t.enableAllTravelPlant = UnlockAllFusions || originalTravel;
                        Board.Instance.boardTag = t;
                    }
                }
                if (p.SuperPresent is not null) SuperPresent = (bool)p.SuperPresent;
                if (p.UltimateRamdomZombie is not null) UltimateRamdomZombie = (bool)p.UltimateRamdomZombie;
                if (p.PresentFastOpen is not null) PresentFastOpen = (bool)p.PresentFastOpen;
                if (p.LockPresent is not null) LockPresent = (int)p.LockPresent;
                if (p.FastShooting is not null) FastShooting = (bool)p.FastShooting;
                if (p.HardPlant is not null) HardPlant = (bool)p.HardPlant;
                if (p.NoHole is not null) NoHole = (bool)p.NoHole;
                if (p.HyponoEmperorNoCD is not null) HyponoEmperorNoCD = (bool)p.HyponoEmperorNoCD;
                if (p.MineNoCD is not null) MineNoCD = (bool)p.MineNoCD;
                if (p.ChomperNoCD is not null) ChomperNoCD = (bool)p.ChomperNoCD;
                if (p.CobCannonNoCD is not null) CobCannonNoCD = (bool)p.CobCannonNoCD;
                if (p.NoIceRoad is not null) NoIceRoad = (bool)p.NoIceRoad;
                if (p.ItemExistForever is not null) ItemExistForever = (bool)p.ItemExistForever;
                if (p.CardNoInit is not null) CardNoInit = (bool)p.CardNoInit;
                if (p.JackboxNotExplode is not null) JackboxNotExplode = (bool)p.JackboxNotExplode;
                if (p.UndeadBullet is not null) UndeadBullet = (bool)p.UndeadBullet;
                if (p.GarlicDay is not null) GarlicDay = (bool)p.GarlicDay;
                if (p.DevLour is not null) DevLour = (bool)p.DevLour;
                if (p.HammerFullCD is not null) HammerFullCD = (double)p.HammerFullCD;
                if (p.GloveFullCD is not null) GloveFullCD = (double)p.GloveFullCD;
                if (p.NewZombieUpdateCD is not null) NewZombieUpdateCD = (float)p.NewZombieUpdateCD;
                if (p.UltimateSuperGatling is not null) UltimateSuperGatling = (bool)p.UltimateSuperGatling;
                if (p.PlantUpgrade is not null) PlantUpgrade = (bool)p.PlantUpgrade;
                return;
            }
            if (data is InGameHotkeys h)
            {
                for (int index = 0; index < h.KeyCodes.Count; index++)
                {
                    Core.KeyBindings.Value[index].Value = (KeyCode)h.KeyCodes[index];
                }
                return;
            }
            if (data is SyncTravelBuff s)
            {
                if (s.AdvTravelBuff is not null)
                {
                    AdvBuffs = [.. s.AdvTravelBuff];
                }
                if (s.UltiTravelBuff is not null)
                {
                    PatchMgr.UltiBuffs = [.. s.UltiTravelBuff];
                }
                if (s.Debuffs is not null)
                {
                    Debuffs = [.. s.Debuffs];
                }
                if (InGame())
                {
                    if (s.AdvInGame is not null)
                    {
                        InGameAdvBuffs = [.. s.AdvInGame];
                    }
                    if (s.UltiInGame is not null)
                    {
                        InGameUltiBuffs = [.. s.UltiInGame];
                    }
                    if (s.DebuffsInGame is not null)
                    {
                        InGameDebuffs = [.. s.DebuffsInGame];
                    }
                    UpdateInGameBuffs();
                }
                return;
            }
            if (data is InGameActions iga)
            {
                if (iga.ZombieSeaEnabled is not null
                && iga.ZombieSeaCD is not null
                && iga.ZombieSeaTypes is not null
                && iga.ZombieSeaLowEnabled is not null)
                {
                    ZombieSea = (bool)iga.ZombieSeaEnabled;
                    ZombieSeaCD = (int)iga.ZombieSeaCD;
                    SeaTypes = iga.ZombieSeaTypes;
                    ZombieSeaLow = (bool)iga.ZombieSeaLowEnabled;
                }
                if (iga.LockSun is not null
                && iga.CurrentSun is not null)
                {
                    LockSun = (bool)iga.LockSun;
                    LockSunCount = (int)iga.CurrentSun;
                }
                if (iga.LockMoney is not null
                  && iga.CurrentMoney is not null)
                {
                    LockMoney = (bool)iga.LockMoney;
                    LockMoneyCount = (int)iga.CurrentMoney;
                }
                if (iga.NoFail is not null)
                {
                    EnableAll<GameLose>(!(bool)iga.NoFail);
                }
                if (iga.BuffRefreshNoLimit is not null)
                {
                    BuffRefreshNoLimit = (bool)iga.BuffRefreshNoLimit;
                }
                if (iga.StopSummon is not null)
                {
                    StopSummon = (bool)iga.StopSummon;
                }
                if (iga.ConveyBeltTypes is not null)
                {
                    ConveyBeltTypes = iga.ConveyBeltTypes;
                }
                if (iga.AbyssCheat is not null)
                {
                    GameAPP.gameAPP.GetComponent<AbyssManager>().money = 99999999;
                    GameAPP.gameAPP.GetComponent<AbyssManager>().refreshCount = 99999999;
                    GameAPP.gameAPP.GetComponent<AbyssManager>().maxPlantCount = 99999999;
                }
                if (iga.LoadCustomPlantData is not null)
                {
                    if (!Utils.LoadPlantData())
                    {
                        MLogger.Error("Failed to reload custom plant data.");
                    }
                }
                if (!InGame()) return;

                if (iga.Row is not null && iga.Column is not null && iga.PlantType is not null && iga.Times is not null)
                {
                    int id = (int)iga.PlantType;
                    int r = (int)iga.Row;
                    int c = (int)iga.Column;
                    if (iga.Times > 50) iga.Times = 50;
                    try
                    {
                        for (int n = 0; n < iga.Times; n++)
                        {
                            if (r * r + c * c == 0)
                            {
                                for (int i = 0; i < Board.Instance!.rowNum; i++)
                                {
                                    for (int j = 0; j < Board.Instance.columnNum; j++)
                                    {
                                        CreatePlant.Instance.SetPlant(j, i, (PlantType)id);
                                    }
                                }
                                continue;
                            }
                            ;
                            if (r == 0 && c != 0)
                            {
                                for (int j = 0; j < Board.Instance!.columnNum; j++)
                                {
                                    CreatePlant.Instance.SetPlant(c - 1, j, (PlantType)id);
                                }
                                continue;
                            }
                            if (c == 0 && r != 0)
                            {
                                for (int j = 0; j < Board.Instance!.columnNum; j++)
                                {
                                    CreatePlant.Instance.SetPlant(j, r - 1, (PlantType)id);
                                }
                                continue;
                            }
                            if (c > 0 && r > 0 && c <= Board.Instance!.columnNum && r <= Board.Instance.rowNum)
                            {
                                CreatePlant.Instance.SetPlant(c - 1, r - 1, (PlantType)id);
                            }
                            continue;
                        }
                    }
                    catch
                    {
                        if (id is 300)
                        {
                            InGameText.Instance.ShowText("不支持此操作，用豌豆代替", 5);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                if (iga.Row is not null
                  && iga.Column is not null
                  && iga.ZombieType is not null
                  && iga.SummonMindControlledZombies is not null
                  && iga.Times is not null)
                {
                    if (iga.Times > 50) iga.Times = 50;
                    for (int n = 0; n < iga.Times; n++)
                    {
                        int id = (int)iga.ZombieType;
                        int r = (int)iga.Row;
                        int c = (int)iga.Column;
                        if (r * r + c * c == 0)
                        {
                            for (int i = 0; i < Board.Instance.rowNum; i++)
                            {
                                for (int j = 0; j < Board.Instance.columnNum; j++)
                                {
                                    if (!(bool)iga.SummonMindControlledZombies)
                                    {
                                        CreateZombie.Instance.SetZombie(i, (ZombieType)id, -5f + (j) * 1.37f);
                                    }
                                    else
                                    {
                                        CreateZombie.Instance.SetZombieWithMindControl(i, (ZombieType)id, -5f + (j) * 1.37f);
                                    }
                                }
                            }
                            continue;
                        }
                        ;
                        if (r == 0 && c != 0)
                        {
                            for (int j = 0; j < Board.Instance.rowNum; j++)
                            {
                                if (!(bool)iga.SummonMindControlledZombies)
                                {
                                    CreateZombie.Instance.SetZombie(j, (ZombieType)id, -5f + (c - 1) * 1.37f);
                                }
                                else
                                {
                                    CreateZombie.Instance.SetZombieWithMindControl(j, (ZombieType)id, -5f + (c - 1) * 1.37f);
                                }
                            }
                            continue;
                        }
                        if (c == 0 && r != 0)
                        {
                            for (int j = 0; j < Board.Instance.columnNum; j++)
                            {
                                if (!(bool)iga.SummonMindControlledZombies)
                                {
                                    CreateZombie.Instance.SetZombie(r - 1, (ZombieType)id, -5f + (j) * 1.37f);
                                }
                                else
                                {
                                    CreateZombie.Instance.SetZombieWithMindControl(r - 1, (ZombieType)id, -5f + (j) * 1.37f);
                                }
                            }
                            continue;
                        }
                        if (c > 0 && r > 0 && c <= Board.Instance.columnNum + 1 && r <= Board.Instance.rowNum)
                        {
                            if (!(bool)iga.SummonMindControlledZombies)
                            {
                                CreateZombie.Instance.SetZombie(r - 1, (ZombieType)id, -5f + (c - 1) * 1.37f);
                            }
                            else
                            {
                                CreateZombie.Instance.SetZombieWithMindControl(r - 1, (ZombieType)id, -5f + (c - 1) * 1.37f);
                            }
                            continue;
                        }
                    }
                }
                if (iga.Row is not null && iga.Column is not null && iga.PlantType is not null && iga.PlantVase is not null)
                {
                    int id = (int)iga.PlantType;
                    int r = (int)iga.Row;
                    int c = (int)iga.Column;
                    if (r * r + c * c == 0)
                    {
                        for (int i = 0; i < Board.Instance!.rowNum; i++)
                        {
                            for (int j = 0; j < Board.Instance.columnNum; j++)
                            {
                                GridItem.SetGridItem(j, i, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType = (PlantType)id;
                            }
                        }
                    }

                    if (r == 0 && c != 0)
                    {
                        for (int j = 0; j < Board.Instance!.columnNum; j++)
                        {
                            GridItem.SetGridItem(c - 1, j, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType = (PlantType)id;
                        }
                    }
                    if (c == 0 && r != 0)
                    {
                        for (int j = 0; j < Board.Instance!.columnNum; j++)
                        {
                            GridItem.SetGridItem(j, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType = (PlantType)id;
                        }
                    }
                    if (c > 0 && r > 0 && c <= Board.Instance!.columnNum && r <= Board.Instance.rowNum)
                    {
                        GridItem.SetGridItem(c - 1, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType = (PlantType)id;
                    }
                }
                if (iga.Row is not null && iga.Column is not null && iga.ZombieType is not null && iga.ZombieVase is not null)
                {
                    int id = (int)iga.ZombieType;
                    int r = (int)iga.Row;
                    int c = (int)iga.Column;
                    if (r * r + c * c == 0)
                    {
                        for (int i = 0; i < Board.Instance!.rowNum; i++)
                        {
                            for (int j = 0; j < Board.Instance.columnNum; j++)
                            {
                                GridItem.SetGridItem(j, i, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType = (ZombieType)id;
                            }
                        }
                    }
                        ;
                    if (r == 0 && c != 0)
                    {
                        for (int j = 0; j < Board.Instance!.columnNum; j++)
                        {
                            GridItem.SetGridItem(c - 1, j, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType = (ZombieType)id;
                        }
                    }
                    if (c == 0 && r != 0)
                    {
                        for (int j = 0; j < Board.Instance!.columnNum; j++)
                        {
                            GridItem.SetGridItem(j, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType = (ZombieType)id;
                        }
                    }
                    if (c > 0 && r > 0 && c <= Board.Instance!.columnNum && r <= Board.Instance.rowNum)
                    {
                        GridItem.SetGridItem(c - 1, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType = (ZombieType)id;
                    }
                }

                if (iga.ItemType is not null)
                {
                    Instantiate(Items[(int)iga.ItemType]).transform.SetParent(GameAPP.board.transform);
                }
                if (iga.CreatePassiveMateorite is not null)
                {
                    Board.Instance.CreatePassiveMateorite();
                }
                if (iga.CreateActiveMateorite is not null)
                {
                    Board.Instance.CreateActiveMateorite();
                }
                if (iga.CreateUltimateMateorite is not null)
                {
                    Board.Instance.CreateUltimateMateorite();
                }
                if (iga.CurrentSun is not null)
                {
                    Board.Instance.theSun = (int)iga.CurrentSun;
                }
                if (iga.CurrentMoney is not null)
                {
                    Board.Instance.theMoney = (int)iga.CurrentMoney;
                }

                if (iga.ClearAllPlants is not null)
                {
                    for (int i = Board.Instance.plantArray.Count - 1; i >= 0; i--)
                    {
                        Board.Instance.plantArray[i]?.Die();
                    }
                    Board.Instance.plantArray.Clear();
                }
                if (iga.ClearAllZombies is not null)
                {
                    Il2CppReferenceArray<UnityEngine.Object> zombies = FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
                    for (int i = zombies.Count - 1; i >= 0; i--)
                    {
                        try { ((Zombie)zombies[i])?.Die(); } catch { }
                    }
                    for (int j = Board.Instance.zombieArray.Count; j >= 0; j--)
                    {
                        try { (Board.Instance.zombieArray[j])?.Die(); } catch { }
                    }
                    Board.Instance.zombieArray.Clear();
                }
                if (iga.ClearAllHoles is not null)
                {
                    for (int i = Board.Instance.griditemArray.Count - 1; i >= 0; i--)
                    {
                        Destroy(Board.Instance.griditemArray[i].GameObject());
                    }
                    Board.Instance.griditemArray.Clear();
                }
                if (iga.MindControlAllZombies is not null)
                {
                    Il2CppReferenceArray<UnityEngine.Object> zombies = FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
                    for (int i = zombies.Count - 1; i >= 0; i--)
                    {
                        try { ((Zombie)zombies[i])?.SetMindControl(); } catch { }
                    }
                    for (int j = Board.Instance.zombieArray.Count; j >= 0; j--)
                    {
                        try { (Board.Instance.zombieArray[j])?.SetMindControl(); } catch { }
                    }
                }
                if (iga.ChangeLevelName is not null)
                {
                    var uimgr = InGameUI.Instance;
                    if (uimgr is not null)
                    {
                        uimgr.ChangeString(new([
                        uimgr.LevelName1.GetComponent<TextMeshProUGUI>(),
                        uimgr.LevelName2.GetComponent<TextMeshProUGUI>(),
                        uimgr.LevelName3.GetComponent<TextMeshProUGUI>(),
                        uimgr.LevelName1.transform.GetChild(0).GameObject().GetComponent<TextMeshProUGUI>(),
                        uimgr.LevelName2.transform.GetChild(0).GameObject().GetComponent<TextMeshProUGUI>(),
                        uimgr.LevelName3.transform.GetChild(0).GameObject().GetComponent<TextMeshProUGUI>(),
                        ]), iga.ChangeLevelName);
                    }
                }
                if (iga.ShowText is not null)
                {
                    try
                    {
                        InGameText.Instance.ShowText(iga.ShowText, 5);
                    }
                    catch { }
                }
                if (iga.SetZombieIdle is not null)
                {
                    foreach (var z in Board.Instance.zombieArray)
                    {
                        z?.anim.Play("idle");
                    }
                }
                if (iga.ClearAllIceRoads is not null)
                {
                    for (int i = 0; i < Board.Instance.iceRoadFadeTime.Count; i++)
                    {
                        Board.Instance.iceRoadFadeTime[i] = 0f;
                    }
                }

                if (iga.NextWave is not null)
                {
                    Board.Instance.newZombieWaveCountDown = 0;
                }
                //感谢@高数带我飞(Github:https://github.com/LibraHp/)的植物阵容码导出和解码代码
                //现在此修改器和高数带我飞的修改器植物阵容码可以互通了
                if (iga.WriteField is not null
                    && iga.ClearOnWritingField is not null)
                {
                    /*try
                    {
                        var plants = JsonSerializer.Deserialize<List<PlantInfo>>(iga.WriteField);
                        if (plants is not null)
                        {
                            if ((bool)iga.ClearOnWritingField)
                            {
                                for (int i = Board.Instance.plantArray.Count - 1; i >= 0; i--)
                                {
                                    Board.Instance.plantArray[i]?.Die();
                                }
                                Board.Instance.plantArray.Clear();
                            }

                            foreach (var plant in plants)
                            {
                                var pl = CreatePlant.Instance.SetPlant(plant.Column, plant.Row, (PlantType)plant.ID);
                                if (pl is null) continue;
                                if (pl.GetComponent<Plant>().isLily) pl.GetComponent<Plant>().theLilyType = (PlantType)plant.LilyType;
                            }
                        }
                    }
                    catch (JsonException) { MLogger.Error("布阵代码存在错误！"); }
                    catch (NotSupportedException) { MLogger.Error("布阵代码存在错误！"); }*/
                    if ((bool)iga.ClearOnWritingField)
                    {
                        for (int i = Board.Instance.plantArray.Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                Board.Instance.plantArray[i]?.Die();
                            }
                            catch { }
                        }
                        Board.Instance.plantArray.Clear();
                    }
                    //from Gaoshu
                    string lineupCode = DecompressString(iga.WriteField);
                    string[] plantEntries = lineupCode.Split(';');
                    foreach (string entry in plantEntries)
                    {
                        string[] plantData = entry.Split(',');
                        if (plantData.Length == 3)
                        {
                            if (int.TryParse(plantData[0], out int column) &&
                                int.TryParse(plantData[1], out int row) &&
                                int.TryParse(plantData[2], out int plantType))
                            {
                                CreatePlant.Instance.SetPlant(column, row, (PlantType)plantType, null, default, false, true);
                            }
                        }
                    }
                }
                if (iga.ReadField is not null)
                {
                    /*List<PlantInfo> bases = [];
                    List<PlantInfo> plants = [];
                    foreach (var plant in Board.Instance.plantArray)
                    {
                        if (plant is null) continue;
                        if (plant.plantTag.potPlant || plant.isLily)
                        {
                            bases.Add(new()
                            {
                                ID = (int)plant.thePlantType,
                                Row = plant.thePlantRow,
                                Column = plant.thePlantColumn,
                                LilyType = (int)plant.theLilyType
                            });
                            continue;
                        }
                        plants.Add(new()
                        {
                            ID = (int)plant.thePlantType,
                            Row = plant.thePlantRow,
                            Column = plant.thePlantColumn,
                            LilyType = (int)plant.theLilyType
                        });
                    }
                    bases.AddRange(plants);
                    DataSync.Instance.Value.SendData(new InGameActions()
                    {
                        WriteField = JsonSerializer.Serialize(bases)
                    });*/

                    //from Gaoshu
                    List<string> lineupData = [];
                    foreach (Plant plant in Board.Instance.plantArray)
                    {
                        // 格式为 "行,列,类型"
                        if (plant == null) continue;
                        string plantData = $"{plant.thePlantColumn},{plant.thePlantRow},{(int)plant.thePlantType}";
                        lineupData.Add(plantData);
                    }
                    string lineupCode = string.Join(";", lineupData);
                    DataSync.Instance.Value.SendData(new InGameActions()
                    {
                        WriteField = CompressString(lineupCode)
                    });
                }
                if (iga.ReadVases is not null)
                {
                    List<VaseInfo> vases = [];
                    foreach (var vase in Board.Instance.griditemArray)
                    {
                        if (vase is null || vase.theItemType is not (GridItemType)4 or (GridItemType)5 or (GridItemType)6) continue;
                        vases.Add(new()
                        {
                            Row = vase.theItemRow,
                            Col = vase.theItemColumn,
                            PlantType = (int)vase.Cast<ScaryPot>().thePlantType,
                            ZombieType = (int)vase.Cast<ScaryPot>().theZombieType,
                        });
                    }
                    DataSync.Instance.Value.SendData(new InGameActions()
                    {
                        WriteVases = JsonSerializer.Serialize(vases)
                    });
                }
                if (iga.WriteVases is not null && iga.ClearOnWritingVases is not null)
                {
                    try
                    {
                        var fieldVases = JsonSerializer.Deserialize<List<VaseInfo>>(iga.WriteVases);
                        if (fieldVases is not null)
                        {
                            if ((bool)iga.ClearOnWritingVases)
                            {
                                for (int i = Board.Instance.griditemArray.Count - 1; i >= 0; i--)
                                {
                                    if (Board.Instance.griditemArray[i] is not null && Board.Instance.griditemArray[i].theItemType is (GridItemType)4 or (GridItemType)5 or (GridItemType)6)
                                    {
                                        Board.Instance.griditemArray[i].gameObject.active = false;
                                        UnityEngine.Object.Destroy(Board.Instance.griditemArray[i]);
                                    }
                                }
                            }
                            foreach (var vase in fieldVases)
                            {
                                var g = GridItem.SetGridItem(vase.Col, vase.Row, GridItemType.ScaryPot);
                                g.Cast<ScaryPot>().thePlantType = (PlantType)vase.PlantType;
                                g.Cast<ScaryPot>().theZombieType = (ZombieType)vase.ZombieType;
                            }
                        }
                    }
                    //catch (JsonException) { MLogger.Error("布阵代码存在错误！"); }
                    catch (NotSupportedException) { MLogger.Error("布阵代码存在错误！"); }
                }

                if (iga.Card is not null
                  && iga.PlantType is not null)
                {
                    Lawnf.SetDroppedCard(new(0f, 0f), (PlantType)iga.PlantType).GameObject().transform.SetParent(InGameUI.Instance.transform);
                }
                if (iga.ReadZombies is not null)
                {
                    List<ZombieInfo> zombies = [];
                    foreach (Zombie zombie in Board.Instance.zombieArray!)
                    {
                        if (zombie is not null && zombie.gameObject is not null && !zombie.isMindControlled)
                        {
                            zombies.Add(new()
                            {
                                ID = (int)zombie.theZombieType,
                                X = zombie.gameObject.transform.position.x,
                                Row = zombie.theZombieRow
                            });
                        }
                    }
                    DataSync.Instance.Value.SendData(new InGameActions()
                    {
                        WriteZombies = JsonSerializer.Serialize(zombies)
                    });
                }
                if (iga.WriteZombies is not null
                    && iga.ClearOnWritingZombies is not null)
                {
                    try
                    {
                        var fieldZombies = JsonSerializer.Deserialize<List<ZombieInfo>>(iga.WriteZombies);
                        if (fieldZombies is not null)
                        {
                            if ((bool)iga.ClearOnWritingZombies)
                            {
                                Il2CppReferenceArray<UnityEngine.Object> zombies = FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
                                for (int i = zombies.Count - 1; i >= 0; i--)
                                {
                                    try { ((Zombie)zombies[i])?.Die(); } catch { }
                                }
                                Board.Instance.zombieArray!.Clear();
                            }
                            foreach (var z in fieldZombies)
                            {
                                CreateZombie.Instance.SetZombie(z.Row, (ZombieType)z.ID, z.X);
                            }
                        }
                    }
                    catch (JsonException) { MLogger.Error("布阵代码存在错误！"); }
                    catch (NotSupportedException) { MLogger.Error("布阵代码存在错误！"); }
                }
                if (iga.StartMower is not null)
                {
                    foreach (var i in Board.Instance.mowerArray)
                    {
                        try
                        {
                            i!.StartMove();
                        }
                        catch { }
                    }
                }
                if (iga.CreateMower is not null)
                {
                    GameAPP.board.GetComponent<InitBoard>().InitMower();
                }
                if (iga.SetAward is not null)
                {
                    Lawnf.SetAward(Board.Instance, Vector2.zero);
                }
            }

            if (data is GameModes ga)
            {
                PatchMgr.GameModes = ga;

                if (InGame())
                {
                    var t = Board.Instance.boardTag;
                    t.isScaredyDream = PatchMgr.GameModes.ScaredyDream;
                    t.isColumn = PatchMgr.GameModes.ColumnPlanting;
                    t.isSeedRain = PatchMgr.GameModes.SeedRain;
                    Board.Instance.boardTag = t;
                }
                return;
            }
        }

        public static void ProcessData(string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data) || string.IsNullOrEmpty(data)) return;
                if (Dev) MLogger.Msg(data);
                JsonObject json = JsonNode.Parse(data)!.AsObject();
                switch ((int)json["ID"]!)
                {
                    case 1:
                        {
                            ProcessData(JsonSerializer.Deserialize<ValueProperties>(json));
                            break;
                        }
                    case 2:
                        {
                            ProcessData(JsonSerializer.Deserialize<BasicProperties>(json));
                            break;
                        }
                    case 3:
                        {
                            ProcessData(JsonSerializer.Deserialize<InGameHotkeys>(json));
                            break;
                        }
                    case 4:
                        {
                            ProcessData(JsonSerializer.Deserialize<SyncTravelBuff>(json));
                            break;
                        }
                    case 6:
                        {
                            ProcessData(JsonSerializer.Deserialize<InGameActions>(json));
                            break;
                        }
                    case 7:
                        {
                            ProcessData(JsonSerializer.Deserialize<GameModes>(json));
                            break;
                        }
                    case 15:
                        {
                            SyncAll all = JsonSerializer.Deserialize<SyncAll>(json);
                            ProcessData((SyncTravelBuff)all.TravelBuffs!);
                            ProcessData((InGameActions)all.InGameActions!);
                            ProcessData((BasicProperties)all.BasicProperties!);
                            ProcessData((ValueProperties)all.ValueProperties!);
                            ProcessData((GameModes)all.GameModes!);
                            break;
                        }
                    case 16:
                        {
                            Application.Quit();
                            break;
                        }

                    default:
                        break;
                }
            }
            catch (JsonException)
            {
                Core.Instance.Value.LoggerInstance.Error("操作失败，可能是点太快或设置词条时出错，取消后重设置一下吧。");
            }
            catch (Exception ex)
            {
                Core.Instance.Value.LoggerInstance.Error(ex.ToString() + ex.StackTrace);
            }
        }

        public void Awake() => Task.Run(() =>
        {
            Thread.Sleep(3000);
            DataSync.Instance.Value.SendData(new SyncAll());
            DataSync.Instance.Value.SendData(new InGameHotkeys() { KeyCodes = [.. from i in Core.KeyBindings.Value select (int)i.Value] });
        });

        public void Update()
        {
            lock (Data)
            {
                if (!string.IsNullOrEmpty(Data))
                {
                    ProcessData(Data);
                }
                Data = "";
            }
        }

        protected static void EnableAll<T>(bool enabled) where T : Component => _ = FindObjectsOfTypeAll(Il2CppType.Of<T>()).All((c) =>
        {
            var v = ((c?.Cast<T>())?.gameObject.GetComponent<BoxCollider2D>());
            var v2 = ((c?.Cast<T>())?.gameObject.GetComponent<PolygonCollider2D>());
            if (v is not null) v.enabled = enabled;
            if (v2 is not null) v2.enabled = enabled;
            return true;
        });

        public static string Data { get; set; } = "";

        public static List<GameObject> Items =>
        [
            Resources.Load<GameObject>("Items/Fertilize/Ferilize"),
            Resources.Load<GameObject>("Items/Bucket"),
            Resources.Load<GameObject>("Items/Helmet"),
            Resources.Load<GameObject>("Items/Jackbox"),
            Resources.Load<GameObject>("Items/Pickaxe"),
            Resources.Load<GameObject>("Items/Machine"),
            Resources.Load<GameObject>("Items/SuperMachine"),
            Resources.Load<GameObject>("Items/SproutPotPrize/SproutPotPrize"),
                                    Resources.Load<GameObject>("Items/PortalHeart"),

        ];
    }
}