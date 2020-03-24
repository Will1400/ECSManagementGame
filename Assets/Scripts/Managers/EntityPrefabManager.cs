using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine.Rendering;
using System.Linq;
using Unity.Mathematics;
using System;
using System.Runtime.Serialization;

public class EntityPrefabManager : MonoBehaviour
{
    public IComponentData test;

    public static EntityPrefabManager Instance;

    public Dictionary<string, Entity> Prefabs = new Dictionary<string, Entity>();

    EntityManager EntityManager;

    readonly ComponentType[] renderComponents = new ComponentType[]
    {
        ComponentType.ReadOnly<Translation>(),
        ComponentType.ReadOnly<Rotation>(),
        ComponentType.ReadOnly<Scale>(),
        ComponentType.ReadOnly<RenderMesh>(),
        ComponentType.ReadOnly<RenderBounds>(),
        ComponentType.ReadOnly<WorldRenderBounds>(),
        ComponentType.ReadOnly<LocalToWorld>()
    };

    Dictionary<string, ComponentType> componentTypeNames = new Dictionary<string, ComponentType>() { 
        // Rendering
        { "Translation", ComponentType.ReadOnly<Translation>() },
        { "Rotation", ComponentType.ReadOnly<Rotation>() },
        { "Scale", ComponentType.ReadOnly<Scale>() },
        { "RenderMesh", ComponentType.ReadOnly<RenderMesh>() },
        { "RenderBounds", ComponentType.ReadOnly<RenderBounds>() },
        { "WorldRenderBounds", ComponentType.ReadOnly<WorldRenderBounds>() },
        { "LocalToWorld", ComponentType.ReadOnly<LocalToWorld>() },

        // Custom
        { "GridOccupation", ComponentType.ReadOnly<GridOccupation>() },
        { "NavMeshObstacle", ComponentType.ReadOnly<NavMeshObstacle>() },


    };

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
    }

    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CreatePrefabsFixed();
        CreatePrefabs();
    }

    public Entity SpawnEntityPrefab(string name)
    {
        if (Prefabs.TryGetValue(name, out Entity prefab))
        {
            Entity entity = EntityManager.Instantiate(prefab);
            EntityManager.RemoveComponent<Disabled>(entity);
            return entity;
        }

        return Entity.Null;
    }

    void CreatePrefabsFixed()
    {
        // TallHouse
        var entity = EntityManager.CreateEntity(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Rotation>(),
            ComponentType.ReadOnly<Scale>(),
            ComponentType.ReadOnly<RenderMesh>(),
            ComponentType.ReadOnly<RenderBounds>(),
            ComponentType.ReadOnly<WorldRenderBounds>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.ReadOnly<GridOccupation>()
            );
        EntityManager.SetComponentData(entity, new Translation { Value = new float3(0, 1.38f, 0) });
        EntityManager.SetComponentData(entity, new Rotation { Value = new quaternion(-0.7071068f, 0, 0, 0.7071f) });
        EntityManager.SetComponentData(entity, new Scale { Value = 100 });
        EntityManager.SetSharedComponentData(entity, new RenderMesh
        {
            receiveShadows = true,
            castShadows = ShadowCastingMode.On,
            material = Resources.Load<Material>("Models/MedievalHouse/MedievalHouse"),
            mesh = Resources.Load<Mesh>("Models/MedievalHouse/medieval")
        });
        EntityManager.AddComponentData(entity, new RenderBounds { Value = Resources.Load<Mesh>("Models/MedievalHouse/medieval").bounds.ToAABB() });

        EntityManager.AddComponent<Disabled>(entity);
        Prefabs.Add("TallHouse", entity);

        // Inn
        entity = EntityManager.CreateEntity(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Rotation>(),
            ComponentType.ReadOnly<Scale>(),
            ComponentType.ReadOnly<RenderMesh>(),
            ComponentType.ReadOnly<RenderBounds>(),
            ComponentType.ReadOnly<WorldRenderBounds>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.ReadOnly<GridOccupation>()
            );
        EntityManager.SetComponentData(entity, new Translation { Value = new float3(0, 7.97f, 0) });
        EntityManager.SetComponentData(entity, new Rotation { Value = new quaternion(0, 1, 0, 0) });
        EntityManager.SetComponentData(entity, new Scale { Value = 1 });
        EntityManager.SetSharedComponentData(entity, new RenderMesh
        {
            receiveShadows = true,
            castShadows = ShadowCastingMode.On,
            material = Resources.Load<Material>("Models/Inn/InnMaterial"),
            mesh = Resources.Load<Mesh>("Models/MedievalHouse/housemedieval")
        });
        EntityManager.AddComponentData(entity, new RenderBounds { Value = Resources.Load<Mesh>("Models/MedievalHouse/housemedieval").bounds.ToAABB() });

        EntityManager.AddComponent<Disabled>(entity);
        Prefabs.Add("Inn", entity);

        // Mine
        entity = EntityManager.CreateEntity(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Rotation>(),
            ComponentType.ReadOnly<Scale>(),
            ComponentType.ReadOnly<RenderMesh>(),
            ComponentType.ReadOnly<RenderBounds>(),
            ComponentType.ReadOnly<WorldRenderBounds>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.ReadOnly<GridOccupation>()
            );
        EntityManager.SetComponentData(entity, new Translation { Value = new float3(0, 2.17f, 0) });
        EntityManager.SetComponentData(entity, new Rotation { Value = new quaternion(0, 0.7071068f, 0, 0.7071068f) });
        EntityManager.SetComponentData(entity, new Scale { Value = 2 });
        EntityManager.SetSharedComponentData(entity, new RenderMesh
        {
            receiveShadows = true,
            castShadows = ShadowCastingMode.On,
            material = Resources.Load<Material>("Models/LowPoly Environment Pack/FBX/Materials/Gray.4"),
            mesh = Resources.Load<Mesh>("Models/LowPoly Environment Pack/FBX/Rock_6")
        });
        EntityManager.AddComponentData(entity, new RenderBounds { Value = Resources.Load<Mesh>("Models/LowPoly Environment Pack/FBX/Rock_6").bounds.ToAABB() });

        EntityManager.AddComponent<Disabled>(entity);
        Prefabs.Add("Mine", entity);
    }

    void CreatePrefabs()
    {
        var prefabInfos = new List<EntityPrefabInfo>
        {
            new EntityPrefabInfo
            {
                Name = "TallHouse",
                ShouldRender = true,
                AddDefaultComponents = true,
                RenderInfo = new RenderInfo
                {
                    ReceiveShadows = true,
                    ShadowCastingMode = ShadowCastingMode.On,
                    Material = Resources.Load<Material>("Models/MedievalHouse/MedievalHouse"),
                    Mesh = Resources.Load<Mesh>("Models/MedievalHouse/medieval")
                },
                TransformInfo = new TransformInfo
                {
                    Position = new float3(0, 1.38f, 0),
                    Rotation = quaternion.Euler(-90, 0, 0),
                    Scale = 100
                },
                CustomComponents = new IComponentData[] { new GridOccupation { }, new NavMeshObstacle { Area = 1 } }
            },

            new EntityPrefabInfo
            {
                Name = "Inn",
                ShouldRender = true,
                AddDefaultComponents = true,
                RenderInfo = new RenderInfo
                {
                    ReceiveShadows = true,
                    ShadowCastingMode = ShadowCastingMode.On,
                    Material = Resources.Load<Material>("Models/Inn/InnMaterial"),
                    Mesh = Resources.Load<Mesh>("Models/Inn/housemedieval")
                },
                TransformInfo = new TransformInfo
                {
                    Position = new float3(0, 7.97f, 0),
                    Rotation = quaternion.Euler(0, -180, 0),
                    Scale = 1
                },
                CustomComponents = new IComponentData[] { new GridOccupation { }, new NavMeshObstacle { Area = 1 } }
            }
        };


        // Convert PrefabInfos to disabled entities
        foreach (var prefabInfo in prefabInfos)
        {
            List<ComponentType> componentTypes = new List<ComponentType>();

            componentTypes.Add(ComponentType.ReadOnly<Disabled>());

            if (prefabInfo.ShouldRender)
            {
                componentTypes.AddRange(renderComponents);
            }

            var entity = EntityManager.CreateEntity(componentTypes.ToArray());

            foreach (var component in prefabInfo.CustomComponents)
            {
                EntityManager.SetComponentData(entity, component);
            }

            if (prefabInfo.ShouldRender)
            {
                EntityManager.AddComponentData(entity, new Scale { Value = prefabInfo.TransformInfo.Scale });
                EntityManager.AddComponentData(entity, new Translation { Value = prefabInfo.TransformInfo.Position });
                EntityManager.AddComponentData(entity, new Rotation { Value = prefabInfo.TransformInfo.Rotation });
                EntityManager.AddComponentData(entity, new RenderBounds { Value = prefabInfo.RenderInfo.Mesh.bounds.ToAABB() });
                EntityManager.AddSharedComponentData(entity, new RenderMesh { mesh = prefabInfo.RenderInfo.Mesh, material = prefabInfo.RenderInfo.Material, castShadows = prefabInfo.RenderInfo.ShadowCastingMode, receiveShadows = prefabInfo.RenderInfo.ReceiveShadows });
            }

            Prefabs.Add(prefabInfo.Name, entity);
        }
    }



    struct EntityPrefabInfo
    {
        public string Name;

        public IComponentData[] CustomComponents;

        public bool AddDefaultComponents;

        public bool ShouldRender;
        public RenderInfo RenderInfo;
        public TransformInfo TransformInfo;
    }

    struct RenderInfo
    {
        public Mesh Mesh;
        public Material Material;
        public ShadowCastingMode ShadowCastingMode;
        public bool ReceiveShadows;
    }

    struct TransformInfo
    {
        public float3 Position;
        public quaternion Rotation;
        public float Scale;
    }
}
