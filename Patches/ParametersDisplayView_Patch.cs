using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;
using TMPro;
using UnityEngine;

namespace KitchenShowCustomersCount.Patches
{
    [HarmonyPatch]
    public static class ParametersDisplayView_Patch
    {
        public static string DisplayedText = "";
        public static string ExpectedGroupsText = "Expected Groups";
        public static string ExpectedCustomersText = "Expected Customers";

        public static bool IsApproximated = false;

        [HarmonyPatch(typeof(ParametersDisplayView), nameof(ParametersDisplayView.UpdateData))]
        [HarmonyPostfix]
        public static void ParametersDisplayView_Postfix(ParametersDisplayView __instance, ParametersDisplayView.ViewData view_data, ref TextMeshPro ___CustomersPerHour)
        {
            Transform groups = __instance.transform.Find("Groups");
            if (groups != null)
            {
                Transform customersLabel = groups.Find("Customers (1)");
                if (customersLabel != null)
                {
                    if (customersLabel.TryGetComponent(out TextMeshPro tmp))
                    {
                        if (PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value == 1)
                        {
                            tmp.text = ExpectedCustomersText;
                            if (view_data.ExtraGroups <= 0)
                            {
                                ___CustomersPerHour.text = $"{Mathf.Round(view_data.ExpectedGroupCount)}";
                            }
                            else
                            {
                                ___CustomersPerHour.text = $"{Mathf.Round(view_data.ExpectedGroupCount)} + {view_data.ExtraGroups}";
                            }

                            if (!Preferences.Get<bool>(Pref.SeedsAffectEverything))
                            {
                                ___CustomersPerHour.text = "~" + ___CustomersPerHour.text;
                            }
                        }
                        else
                        {
                            tmp.text = ExpectedGroupsText;
                        }
                        DisplayedText = tmp.text;
                        IsApproximated = ___CustomersPerHour.text.StartsWith("~");
                    }
                }
            }
        }
    }
}