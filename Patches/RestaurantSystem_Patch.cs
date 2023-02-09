using HarmonyLib;
using Kitchen;

namespace KitchenShowCustomersCount.Patches
{
    [HarmonyPatch]
    public static class RestaurantSystem_Patch
    {
        [HarmonyPatch(typeof(RestaurantSystem), "Seed")]
        [HarmonyPostfix]
        public static void Seed_Postfix(ref FixedSeedContext __result, int category_seed, int instance)
        {
            if (Main.IsRunningFakeSeed && category_seed == 1997821)
            {
                __result = new FixedSeedContext(Main.RunSeed, category_seed * 1231231 + instance);
            }
        }
    }
}
