using Kitchen;
using KitchenLib.Utils;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace KitchenShowCustomersCount
{
    internal class CustomersCountController : NightSystem
    {

        private float TimeSinceLastUpdate = 0f;
        internal static int StandardCustomers { get; private set; }
        internal static int RushCustomers { get; private set; }

        private EntityQuery ScheduledCustomers;

        protected override void Initialise()
        {
            base.Initialise();
            ScheduledCustomers = GetEntityQuery(typeof(CScheduledCustomer));
            StandardCustomers = 0;
            RushCustomers = 0;
        }

        protected override void OnUpdate()
        {
            if (TimeSinceLastUpdate > PreferenceUtils.Get<KitchenLib.FloatPreference>(Main.MOD_GUID, Main.UPDATE_DELAY_ID).Value)
            {
                TimeSinceLastUpdate = 0f;
                NativeArray<Entity> entities = ScheduledCustomers.ToEntityArray(Allocator.Temp);
                int standard = 0;
                int rush = 0;
                foreach (Entity entity in entities)
                {
                    if (Require(entity, out CScheduledCustomer data))
                    {
                        if (Has<CExtraScheduledCustomer>(entity))
                        {
                            rush += data.GroupSize;
                        }
                        else
                        {
                            standard += data.GroupSize;
                        }
                    }
                }
                StandardCustomers = standard;
                RushCustomers = rush;

                entities.Dispose();
            }
            else
            {
                TimeSinceLastUpdate += Time.DeltaTime;
            }
        }
    }
}
