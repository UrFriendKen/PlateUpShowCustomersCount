using Kitchen;
using Kitchen.Modules;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Utils;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System;
using KitchenMods;
using KitchenData;

// Namespace should have "Kitchen" in the beginning
namespace KitchenShowCustomersCount
{
    public class Main : BaseMod
    {
        // guid must be unique and is recommended to be in reverse domain name notation
        // mod name that is displayed to the player and listed in the mods menu
        // mod version must follow semver e.g. "1.2.3"
        public const string MOD_GUID = "IcedMilo.PlateUp.ShowCustomersCount";
        public const string MOD_NAME = "Show Customers Count";
        public const string MOD_VERSION = "0.1.1";
        public const string MOD_AUTHOR = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.1";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.1" current and all future
        // e.g. ">=1.1.1 <=1.2.3" for all from/until

        public static bool IsRunningFakeSeed = false;
        public static Kitchen.Seed RunSeed = default(Seed);
        public const string SHOW_CUSTOMERS_COUNT_ID = "showCustomersCount";
        public const int SHOW_CUSTOMERS_COUNT_INITIAL = 1;
        public static KitchenLib.IntPreference ShowCustomersCountPreference;
        public static int ShowCustomersCountValue = 0;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnPostActivate(Mod mod)
        {
            base.OnPostActivate(mod);
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            ShowCustomersCountPreference = PreferenceUtils.Register<KitchenLib.IntPreference>(MOD_GUID, SHOW_CUSTOMERS_COUNT_ID, "Show Customers Count");
            PreferenceUtils.Get<KitchenLib.IntPreference>(MOD_GUID, SHOW_CUSTOMERS_COUNT_ID).Value = SHOW_CUSTOMERS_COUNT_INITIAL;
            PreferenceUtils.Load();
            LogInfo($"Registered preference {MOD_GUID}:{SHOW_CUSTOMERS_COUNT_ID}");
            ShowCustomersCountValue = PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value;
            SetupPreferences();
        }

        protected override void OnInitialise()
        {
            base.OnInitialise();

            try
            {
                World.GetExistingSystem<ParametersDisplayView.UpdateView>().Enabled = false;
            }
            catch (NullReferenceException)
            {
                Main.LogInfo("Could not disable system Kitchen.ParametersDisplayView.UpdateView!");
            }
        }

        private void SetupPreferences()
        {
            //Setting Up For Pause Menu
            Events.PreferenceMenu_PauseMenu_CreateSubmenusEvent += (s, args) =>
            {
                args.Menus.Add(typeof(ShowCustomersCountPreferences<PauseMenuAction>), new ShowCustomersCountPreferences<PauseMenuAction>(args.Container, args.Module_list));
            };
            ModsPreferencesMenu<PauseMenuAction>.RegisterMenu(MOD_NAME, typeof(ShowCustomersCountPreferences<PauseMenuAction>), typeof(PauseMenuAction));

            Events.PreferencesSaveEvent += (s, args) =>
            {
                ShowCustomersCountValue = PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value;
            };
        }

        protected override void OnUpdate()
        {
        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }

    public class ShowCustomersCountPreferences<T> : KLMenu<T>
    {
        private Option<int> Option;

        public ShowCustomersCountPreferences(Transform container, ModuleList module_list) : base(container, module_list)
        {
        }

        public override void Setup(int player_id)
        {
            this.Option = new Option<int>(
                new List<int>() { 0, 1 },
                PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value,
                new List<string>() { "Expected Groups", "Expected Customers" });

            AddLabel("Customers Display Type");
            Add<int>(this.Option).OnChanged += delegate (object _, int f)
            {
                PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value = f;
                PreferenceUtils.Save();
            };

            New<SpacerElement>();
            New<SpacerElement>();

            AddButton(base.Localisation["MENU_BACK_SETTINGS"], delegate
            {
                RequestPreviousMenu();
            });
        }
    }

}
