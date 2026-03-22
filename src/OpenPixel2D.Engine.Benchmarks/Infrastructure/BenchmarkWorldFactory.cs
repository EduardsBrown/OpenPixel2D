using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

internal static class BenchmarkWorldFactory
{
    public static World CreateStartedWorld()
    {
        World world = new();
        world.Initialize();
        world.Start();
        return world;
    }

    public static World CreateWorld(LifecycleWorldComposition composition, WorldScale scale)
    {
        World world = new();

        switch (composition)
        {
            case LifecycleWorldComposition.PassiveOnly:
                AddPassiveEntities(world, BenchmarkDimensions.GetEntityCount(scale));
                break;
            case LifecycleWorldComposition.BehaviorOnly:
                AddBehaviorEntities(world, BenchmarkDimensions.GetEntityCount(scale));
                break;
            case LifecycleWorldComposition.UpdateSystemsOnly:
                AddUpdateSystems(world, BenchmarkDimensions.GetSystemCount(scale));
                break;
            case LifecycleWorldComposition.RenderSystemsOnly:
                AddRenderSystems(world, BenchmarkDimensions.GetSystemCount(scale));
                break;
            case LifecycleWorldComposition.Mixed:
                AddMixedWorld(world, scale);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(composition), composition, null);
        }

        return world;
    }

    public static World CreateInitializedWorld(LifecycleWorldComposition composition, WorldScale scale)
    {
        World world = CreateWorld(composition, scale);
        world.Initialize();
        return world;
    }

    public static World CreateStartedWorld(LifecycleWorldComposition composition, WorldScale scale)
    {
        World world = CreateWorld(composition, scale);
        world.Initialize();
        world.Start();
        return world;
    }

    public static Entity CreatePayloadEntity(EntityPayload payload, WorldScale scale)
    {
        return payload switch
        {
            EntityPayload.Empty => new Entity(),
            EntityPayload.PassivePayload => CreateSingleEntityWithPassiveComponents(BenchmarkDimensions.GetEntityCount(scale)),
            EntityPayload.BehaviorPayload => CreateSingleEntityWithBehaviorComponents(BenchmarkDimensions.GetEntityCount(scale)),
            EntityPayload.NestedSubtree => CreateSubtree(SubtreeShape.Mixed, scale),
            _ => throw new ArgumentOutOfRangeException(nameof(payload), payload, null)
        };
    }

    public static Entity CreateSubtree(SubtreeShape shape, WorldScale scale)
    {
        int nodeCount = BenchmarkDimensions.GetEntityCount(scale);

        return shape switch
        {
            SubtreeShape.Deep => CreateDeepSubtree(nodeCount),
            SubtreeShape.Wide => CreateWideSubtree(nodeCount),
            SubtreeShape.Mixed => CreateMixedSubtree(nodeCount),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
        };
    }

    public static void QueuePendingAdds(World world, WorldScale scale)
    {
        world.AddEntity(CreateSubtree(SubtreeShape.Mixed, scale));

        for (int i = 0; i < BenchmarkDimensions.GetMixedSystemCount(scale); i++)
        {
            world.AddSystem(new NoOpUpdateSystem(GetSystemGroup(i)));
            world.AddSystem(new NoOpRenderSystem());
        }
    }

    public static void QueuePendingRemoves(World world, WorldScale scale)
    {
        Entity root = CreateSubtree(SubtreeShape.Mixed, scale);
        world.AddEntity(root);

        List<NoOpUpdateSystem> updateSystems = [];
        List<NoOpRenderSystem> renderSystems = [];

        for (int i = 0; i < BenchmarkDimensions.GetMixedSystemCount(scale); i++)
        {
            NoOpUpdateSystem updateSystem = new(GetSystemGroup(i));
            NoOpRenderSystem renderSystem = new();
            updateSystems.Add(updateSystem);
            renderSystems.Add(renderSystem);
            world.AddSystem(updateSystem);
            world.AddSystem(renderSystem);
        }

        world.Update();
        world.RemoveEntity(root);

        for (int i = 0; i < updateSystems.Count; i++)
        {
            world.RemoveSystem(updateSystems[i]);
            world.RemoveSystem(renderSystems[i]);
        }
    }

    public static void QueuePendingMixedChanges(World world, WorldScale scale)
    {
        Entity rootToRemove = CreateSubtree(SubtreeShape.Mixed, scale);
        world.AddEntity(rootToRemove);

        List<NoOpUpdateSystem> updateSystemsToRemove = [];
        List<NoOpRenderSystem> renderSystemsToRemove = [];

        for (int i = 0; i < BenchmarkDimensions.GetMixedSystemCount(scale); i++)
        {
            NoOpUpdateSystem updateSystem = new(GetSystemGroup(i));
            NoOpRenderSystem renderSystem = new();
            updateSystemsToRemove.Add(updateSystem);
            renderSystemsToRemove.Add(renderSystem);
            world.AddSystem(updateSystem);
            world.AddSystem(renderSystem);
        }

        world.Update();
        world.RemoveEntity(rootToRemove);

        for (int i = 0; i < updateSystemsToRemove.Count; i++)
        {
            world.RemoveSystem(updateSystemsToRemove[i]);
            world.RemoveSystem(renderSystemsToRemove[i]);
        }

        world.AddEntity(CreateSubtree(SubtreeShape.Mixed, scale));

        for (int i = 0; i < BenchmarkDimensions.GetMixedSystemCount(scale); i++)
        {
            world.AddSystem(new NoOpUpdateSystem(GetSystemGroup(i)));
            world.AddSystem(new NoOpRenderSystem());
        }
    }

    public static void ActivateRuntimeRenderSystems(World world, WorldScale scale)
    {
        for (int i = 0; i < BenchmarkDimensions.GetSystemCount(scale); i++)
        {
            world.AddSystem(new NoOpRenderSystem());
        }

        world.Update();
    }

    private static void AddPassiveEntities(World world, int entityCount)
    {
        for (int i = 0; i < entityCount; i++)
        {
            Entity entity = new();
            entity.AddComponent(new NoOpComponent());
            world.AddEntity(entity);
        }
    }

    private static void AddBehaviorEntities(World world, int entityCount)
    {
        for (int i = 0; i < entityCount; i++)
        {
            Entity entity = new();
            entity.AddComponent(new NoOpBehaviorComponent());
            world.AddEntity(entity);
        }
    }

    private static void AddUpdateSystems(World world, int systemCount)
    {
        for (int i = 0; i < systemCount; i++)
        {
            world.AddSystem(new NoOpUpdateSystem(GetSystemGroup(i)));
        }
    }

    private static void AddRenderSystems(World world, int systemCount)
    {
        for (int i = 0; i < systemCount; i++)
        {
            world.AddSystem(new NoOpRenderSystem());
        }
    }

    private static void AddMixedWorld(World world, WorldScale scale)
    {
        world.AddEntity(CreateSubtree(SubtreeShape.Mixed, scale));

        int systemCount = BenchmarkDimensions.GetMixedSystemCount(scale);

        for (int i = 0; i < systemCount; i++)
        {
            world.AddSystem(new NoOpUpdateSystem(GetSystemGroup(i)));
            world.AddSystem(new NoOpRenderSystem());
        }
    }

    private static Entity CreateSingleEntityWithPassiveComponents(int componentCount)
    {
        Entity entity = new();

        for (int i = 0; i < componentCount; i++)
        {
            entity.AddComponent(new NoOpComponent());
        }

        return entity;
    }

    private static Entity CreateSingleEntityWithBehaviorComponents(int componentCount)
    {
        Entity entity = new();

        for (int i = 0; i < componentCount; i++)
        {
            entity.AddComponent(new NoOpBehaviorComponent());
        }

        return entity;
    }

    private static Entity CreateDeepSubtree(int nodeCount)
    {
        Entity root = new();
        ConfigureSubtreeNode(root, 0);

        Entity current = root;

        for (int i = 1; i < nodeCount; i++)
        {
            Entity child = new();
            ConfigureSubtreeNode(child, i);
            current.AddEntity(child);
            current = child;
        }

        return root;
    }

    private static Entity CreateWideSubtree(int nodeCount)
    {
        Entity root = new();
        ConfigureSubtreeNode(root, 0);

        for (int i = 1; i < nodeCount; i++)
        {
            Entity child = new();
            ConfigureSubtreeNode(child, i);
            root.AddEntity(child);
        }

        return root;
    }

    private static Entity CreateMixedSubtree(int nodeCount)
    {
        Entity root = new();
        ConfigureSubtreeNode(root, 0);

        List<Entity> nodes = [root];

        for (int i = 1; i < nodeCount; i++)
        {
            Entity entity = new();
            ConfigureSubtreeNode(entity, i);
            Entity parent = nodes[(i - 1) / 4];
            parent.AddEntity(entity);
            nodes.Add(entity);
        }

        return root;
    }

    private static void ConfigureSubtreeNode(Entity entity, int index)
    {
        if (index % 2 == 0)
        {
            entity.AddComponent(new NoOpComponent());
        }

        if (index % 3 == 0)
        {
            entity.AddComponent(new NoOpBehaviorComponent());
        }
    }

    private static SystemGroup GetSystemGroup(int index) => (index % 3) switch
    {
        0 => SystemGroup.Default,
        1 => SystemGroup.Physics,
        _ => SystemGroup.PostPhysics
    };
}
