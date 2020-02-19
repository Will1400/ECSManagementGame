using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class PlayerInputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new PlayerInputJob
        {
            leftClick = Input.GetMouseButton(0),
            rightClick = Input.GetMouseButton(1)
        };

        return job.Schedule(this, inputDeps);
    }

    struct PlayerInputJob : IJobForEach<PlayerInputComponent>
    {
        public bool leftClick;
        public bool rightClick;

        public void Execute(ref PlayerInputComponent input)
        {
            input.LeftClick = leftClick;
            input.RightClick = rightClick;
        }
    }
}
