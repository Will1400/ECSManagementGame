using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

[UpdateBefore(typeof(RemoveCitizenFromWorkSystem))]
public class RemoveWorkPlaceSystem : ComponentSystem
{
    EntityQuery workPlaces;
    EntityQuery citizens;

    protected override void OnCreate()
    {
        workPlaces = Entities.WithAll<RemoveWorkPlaceTag>()
                             .ToEntityQuery();
        citizens = Entities.WithAll<CitizenWork>().ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(workPlaces).ForEach((Entity entity) =>
        {
            int remainingWorkers = 0;
            Entities.With(citizens).ForEach((Entity citizen, ref CitizenWork citizenWork) =>
            {
                if (citizenWork.WorkPlaceEntity == entity)
                {
                    EntityManager.AddComponent<RemoveFromWorkTag>(citizen);
                    remainingWorkers++;
                }
            });

            if (remainingWorkers == 0)
            {
                EntityManager.DestroyEntity(entity);
            }
        });
    }
}
