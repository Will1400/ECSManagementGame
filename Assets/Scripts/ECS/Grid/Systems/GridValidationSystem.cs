using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GridValidationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<GridOccupation, Translation, BeingPlacedTag>().ForEach((Entity entity, ref GridOccupation occupation) =>
        {
            var occupations = GridCacheSystem.Instance.GridOccupations;
            if (occupations.Length != 0 && !occupations[0].Equals(occupation))
            {
                for (int i = 0; i < occupations.Length; i++)
                {
                    GridOccupation existingOccupation = occupations[i];

                    if ((occupation.Start.x >= existingOccupation.Start.x && occupation.End.x <= existingOccupation.End.x) && occupation.Start.y == existingOccupation.Start.y)
                    {
                        EntityManager.RemoveComponent<GridOccupationIsValidTag>(entity);
                        EntityManager.AddComponent<GridOccupationIsInValidTag>(entity);
                    }
                    else
                    {
                        EntityManager.RemoveComponent<GridOccupationIsInValidTag>(entity);
                        EntityManager.AddComponent<GridOccupationIsValidTag>(entity);
                    }
                }
            }
            else
            {
                if (!EntityManager.HasComponent<GridOccupationIsValidTag>(entity))
                {
                    EntityManager.RemoveComponent<GridOccupationIsInValidTag>(entity);
                    EntityManager.AddComponent<GridOccupationIsValidTag>(entity);
                }
            }
        });
    }
}
