using Kitchen;
using Unity.Entities;
using Unity.Collections;
using KitchenData;
using KitchenLib.Utils;
using KitchenShowCustomersCount.Patches;

namespace KitchenShowCustomersCount
{
    public class UpdateCustomersAndGroupCountView : ParametersDisplayView.UpdateView
    {
        EntityQuery Players;
        EntityQuery Views;

        private bool _forceTextUpdateToggle = true;

        protected override void Initialise()
        {
            base.Initialise();
            Players = GetEntityQuery(typeof(CPlayer));
            Views = GetEntityQuery(new QueryHelper()
                .All(typeof(CLinkedView),
                     typeof(CParametersDisplay)));
        }

        protected override void OnUpdate()
        {
            bool showCustomers = PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value == 1;

            string correctText = showCustomers ? ParametersDisplayView_Patch.ExpectedCustomersText : ParametersDisplayView_Patch.ExpectedGroupsText;
            bool isApproximation = showCustomers && !Preferences.Get<bool>(Pref.SeedsAffectEverything);

            if (correctText == ParametersDisplayView_Patch.DisplayedText && isApproximation == ParametersDisplayView_Patch.IsApproximated)
            {
                _forceTextUpdateToggle = false;
            }
            else
            {
                _forceTextUpdateToggle = !_forceTextUpdateToggle;
            }

            if (PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value == 1 && Has<SIsNightTime>())
            {
                CreateCustomerSchedule existingSystem = base.World.GetExistingSystem<CreateCustomerSchedule>();
                int playerCount = Players.CalculateEntityCount();

                KitchenParameters sKitchenParameters = GetSingleton<SKitchenParameters>().Parameters;
                int day = GetSingleton<SDay>().Day;
                bool isNight = HasSingleton<SIsNightTime>();

                int standardCustomers = 0;
                int rushCustomers = 0;

                if (!_forceTextUpdateToggle)
                {
                    if (!Preferences.Get<bool>(Pref.SeedsAffectEverything))
                    {
                        Main.IsRunningFakeSeed = true;
                        if (Require<SFixedSeed>(out var comp) && comp.Seed.IsSet)
                        {
                            Main.RunSeed = comp.Seed;
                        }
                    }
                    CreateCustomerSchedule.GetCustomersForDay(existingSystem, sKitchenParameters, playerCount, day + 1, delegate (int size, float time, bool is_special)
                    {
                        if (!(time > 1f))
                        {
                            if (is_special)
                            {
                                rushCustomers += size;
                            }
                            else
                            {
                                standardCustomers += size;
                            }
                        }
                    });
                    Main.IsRunningFakeSeed = false;
                }

                Entity entity = GetEntity<SGlobalStatusList>();
                if (!base.EntityManager.HasComponent<CDecorationScore>(entity))
                {
                    base.EntityManager.AddBuffer<CDecorationScore>(entity);
                }
                DynamicBuffer<CDecorationScore> buffer = GetBuffer<CDecorationScore>(entity);
                DecorationValues decoValues = default(DecorationValues);
                for (int i = 0; i < buffer.Length; i++)
                {
                    CDecorationScore cDecorationScore = buffer[i];
                    decoValues[cDecorationScore] = cDecorationScore.Value;
                }

                NativeArray<Entity> linkedViews = Views.ToEntityArray(Allocator.Temp);

                foreach (Entity e in linkedViews)
                {
                    if (Require<CLinkedView>(e, out CLinkedView linked_view))
                        SendUpdate(linked_view, new ParametersDisplayView.ViewData
                        {
                            IsNight = isNight,
                            ExpectedGroupCount = standardCustomers,
                            MinimumGroupSize = sKitchenParameters.MinimumGroupSize,
                            MaximumGroupSize = sKitchenParameters.MaximumGroupSize,
                            Decoration = decoValues,
                            ExtraGroups = rushCustomers
                        });
                }
            }
            else
            {
                base.OnUpdate();
            }
        }
    }

    //public class NewParameterDisplayView : ParametersDisplayView
    //{
    //    static FieldInfo TMP_CustomersPerHour_Info;
    //    static FieldInfo TMP_GroupSize_Info;
    //    static FieldInfo BonusSetModule_Info;

    //    public class UpdateCustomersCountView : ParametersDisplayView.UpdateView
    //    {
    //        EntityQuery Players;
    //        EntityQuery Views;

    //        protected override void Initialise()
    //        {
    //            base.Initialise();
    //            Players = GetEntityQuery(typeof(CPlayer));
    //            Views = GetEntityQuery(new QueryHelper()
    //                .All(typeof(CLinkedView),
    //                     typeof(CParametersDisplay)));
    //        }

    //        protected override void OnUpdate()
    //        {
    //            if (PreferenceUtils.Get<KitchenLib.IntPreference>(Main.MOD_GUID, Main.SHOW_CUSTOMERS_COUNT_ID).Value == 1)
    //            {
    //                CreateCustomerSchedule existingSystem = base.World.GetExistingSystem<CreateCustomerSchedule>();
    //                int playerCount = Players.CalculateEntityCount();

    //                KitchenParameters sKitchenParameters = GetSingleton<SKitchenParameters>().Parameters;
    //                int day = GetSingleton<SDay>().Day;
    //                bool isNight = HasSingleton<SIsNightTime>();

    //                int standardCustomers = 0;
    //                int rushCustomers = 0;
    //                string txt = "";
    //                string txt2 = "";
    //                if (!Preferences.Get<bool>(Pref.SeedsAffectEverything))
    //                {
    //                    Main.IsRunningFakeSeed = true;
    //                    if (Require<SFixedSeed>(out var comp) && comp.Seed.IsSet)
    //                    {
    //                        Main.RunSeed = comp.Seed;
    //                    }
    //                }
    //                CreateCustomerSchedule.GetCustomersForDay(existingSystem, sKitchenParameters, playerCount, day + 1, delegate (int size, float time, bool is_special)
    //                {
    //                    if (!(time > 1f))
    //                    {
    //                        if (is_special)
    //                        {
    //                            txt2 += $"{size},";
    //                            rushCustomers += size;
    //                        }
    //                        else
    //                        {
    //                            txt += $"{size},";
    //                            standardCustomers += size;
    //                        }
    //                    }
    //                });
    //                Main.IsRunningFakeSeed = false;

    //                Entity entity = GetEntity<SGlobalStatusList>();
    //                if (!base.EntityManager.HasComponent<CDecorationScore>(entity))
    //                {
    //                    base.EntityManager.AddBuffer<CDecorationScore>(entity);
    //                }
    //                DynamicBuffer<CDecorationScore> buffer = GetBuffer<CDecorationScore>(entity);
    //                DecorationValues decoValues = default(DecorationValues);
    //                for (int i = 0; i < buffer.Length; i++)
    //                {
    //                    CDecorationScore cDecorationScore = buffer[i];
    //                    decoValues[cDecorationScore] = cDecorationScore.Value;
    //                }

    //                NativeArray<Entity> linkedViews = Views.ToEntityArray(Allocator.Temp);

    //                foreach (Entity e in linkedViews)
    //                {
    //                    if (Require<CLinkedView>(e, out CLinkedView linked_view))
    //                        SendUpdate(linked_view, new ParametersDisplayView.ViewData
    //                        {
    //                            IsNight = isNight,
    //                            ExpectedGroupCount = standardCustomers,
    //                            MinimumGroupSize = sKitchenParameters.MinimumGroupSize,
    //                            MaximumGroupSize = sKitchenParameters.MaximumGroupSize,
    //                            Decoration = decoValues,
    //                            ExtraGroups = rushCustomers
    //                        });
    //                }
    //            }
    //            else
    //            {
    //                base.OnUpdate();
    //            }
    //        }
    //    }
        
    //    public override void Initialise()
    //    {
    //        base.Initialise();
    //        TMP_CustomersPerHour_Info = typeof(ParametersDisplayView).GetField("CustomersPerHour", BindingFlags.Instance | BindingFlags.NonPublic);
    //        TMP_GroupSize_Info = typeof(ParametersDisplayView).GetField("GroupSize", BindingFlags.Instance | BindingFlags.NonPublic);
    //        BonusSetModule_Info = typeof(ParametersDisplayView).GetField("BonusSetModule", BindingFlags.Instance | BindingFlags.NonPublic);
    //    }

    //    protected override void UpdateData(ViewData view_data)
    //    {
    //        base.gameObject.SetActive(view_data.IsNight);
    //        TextMeshPro CustomersPerHour = (TextMeshPro)TMP_CustomersPerHour_Info.GetValue(this);
    //        TextMeshPro GroupSize = (TextMeshPro)TMP_GroupSize_Info.GetValue(this);
    //        DecorationBonusSetElement BonusSetModule = (DecorationBonusSetElement)BonusSetModule_Info.GetValue(this);
    //        if (view_data.ExtraGroups <= 0)
    //        {
    //            CustomersPerHour.text = $"~{Mathf.Round(view_data.ExpectedGroupCount)}";
    //        }
    //        else
    //        {
    //            CustomersPerHour.text = $"~{Mathf.Round(view_data.ExpectedGroupCount)} + {view_data.ExtraGroups}";
    //        }
    //        GroupSize.text = $"{view_data.MinimumGroupSize} - {view_data.MaximumGroupSize}";
    //        BonusSetModule.Set(view_data.Decoration);
    //        //base.UpdateData(view_data);
    //    }
        
    //}
    
}
