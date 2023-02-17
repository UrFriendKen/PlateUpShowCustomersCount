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
            RequireSingletonForUpdate<SKitchenParameters>();
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
                KitchenParameters sKitchenParameters = GetSingleton<SKitchenParameters>().Parameters;
                bool isNight = HasSingleton<SIsNightTime>();

                int standardCustomers = 0;
                int rushCustomers = 0;

                if (!_forceTextUpdateToggle)
                {
                    standardCustomers = CustomersCountController.StandardCustomers;
                    rushCustomers = CustomersCountController.RushCustomers;
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
    
}
