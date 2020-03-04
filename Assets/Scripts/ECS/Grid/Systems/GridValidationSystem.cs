using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GridValidationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<GridOccupation, Translation>().WithNone<IsInCache>().ForEach((Entity entity, ref GridOccupation occupation) =>
        {
            if (GridCacheSystem.Instance.GridOccupations.Length != 0)
            {
                bool isInValidPosition = false;

                for (int x = occupation.Start.x; x <= occupation.End.x; x++)
                {
                    if (isInValidPosition)
                        break;

                    for (int y = occupation.Start.y; y <= occupation.End.y; y++)
                    {
                        if (GridCacheSystem.Instance.CheckIndex(x, y) != 0)
                        {
                            isInValidPosition = true;
                            break;
                        }
                    }
                }

                if (isInValidPosition)
                {
                    if (!EntityManager.HasComponent<GridOccupationIsInValidTag>(entity))
                    {
                        EntityManager.RemoveComponent<GridOccupationIsValidTag>(entity);
                        EntityManager.AddComponent<GridOccupationIsInValidTag>(entity);
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
