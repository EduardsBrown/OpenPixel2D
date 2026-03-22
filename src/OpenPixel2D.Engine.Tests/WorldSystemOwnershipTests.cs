using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class WorldSystemOwnershipTests
{
    [Fact]
    public void AddSystem_QueuesUpdateSystemActivationUntilFlush()
    {
        World world = new();
        TestUpdateSystem system = new();

        world.AddSystem(system);

        Assert.Empty(world.UpdateSystems);
        Assert.Same(world, system.World);

        FlushPendingAdditions(world);

        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);
    }

    [Fact]
    public void RemoveSystem_QueuesUpdateSystemRemovalUntilFlush()
    {
        World world = new();
        TestUpdateSystem system = new();
        world.AddSystem(system);
        FlushPendingAdditions(world);

        world.RemoveSystem(system);

        Assert.Null(system.World);
        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);

        FlushPendingRemovals(world);

        Assert.Empty(world.UpdateSystems);
    }

    [Fact]
    public void AddSystem_AllowsMultipleUpdateSystemsOfTheSameType()
    {
        World world = new();
        TestUpdateSystem first = new();
        TestUpdateSystem second = new();

        world.AddSystem(first);
        world.AddSystem(second);

        Assert.Empty(world.UpdateSystems);

        FlushPendingAdditions(world);

        Assert.Equal(2, world.UpdateSystems.Count);
        Assert.Same(first, world.UpdateSystems[0]);
        Assert.Same(second, world.UpdateSystems[1]);
        Assert.Same(world, first.World);
        Assert.Same(world, second.World);
    }

    [Fact]
    public void AddSystem_PreventsDuplicateUpdateSystemRegistration()
    {
        World world = new();
        TestUpdateSystem system = new();

        world.AddSystem(system);
        world.AddSystem(system);

        Assert.Empty(world.UpdateSystems);

        FlushPendingAdditions(world);

        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);
    }

    [Fact]
    public void MoveUpdateSystemBetweenWorlds_BlocksActivationUntilOldWorldRemovalsFlush()
    {
        World oldWorld = new();
        World newWorld = new();
        TestUpdateSystem system = new();
        oldWorld.AddSystem(system);
        FlushPendingAdditions(oldWorld);

        newWorld.AddSystem(system);

        Assert.Same(oldWorld, system.RegisteredWorld);
        Assert.Empty(newWorld.UpdateSystems);
        Assert.Single(oldWorld.UpdateSystems);
        Assert.Same(newWorld, system.World);

        FlushPendingAdditions(newWorld);

        Assert.Empty(newWorld.UpdateSystems);
        Assert.Single(oldWorld.UpdateSystems);

        FlushPendingRemovals(oldWorld);

        Assert.Empty(oldWorld.UpdateSystems);

        FlushPendingAdditions(newWorld);

        Assert.Single(newWorld.UpdateSystems);
        Assert.Same(system, newWorld.UpdateSystems[0]);
    }

    [Fact]
    public void RemoveSystem_IgnoresUpdateSystemOwnedByAnotherWorld()
    {
        World ownerWorld = new();
        World otherWorld = new();
        TestUpdateSystem system = new();
        ownerWorld.AddSystem(system);
        FlushPendingAdditions(ownerWorld);

        otherWorld.RemoveSystem(system);

        Assert.Single(ownerWorld.UpdateSystems);
        Assert.Empty(otherWorld.UpdateSystems);
        Assert.Same(ownerWorld, system.World);
    }

    [Fact]
    public void AddSystem_QueuesRenderSystemActivationUntilFlush()
    {
        World world = new();
        TestRenderSystem system = new();

        world.AddSystem(system);

        Assert.Empty(world.RenderSystems);
        Assert.Same(world, system.World);

        FlushPendingAdditions(world);

        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);
    }

    [Fact]
    public void RemoveSystem_QueuesRenderSystemRemovalUntilFlush()
    {
        World world = new();
        TestRenderSystem system = new();
        world.AddSystem(system);
        FlushPendingAdditions(world);

        world.RemoveSystem(system);

        Assert.Null(system.World);
        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);

        FlushPendingRemovals(world);

        Assert.Empty(world.RenderSystems);
    }

    [Fact]
    public void AddSystem_AllowsMultipleRenderSystemsOfTheSameType()
    {
        World world = new();
        TestRenderSystem first = new();
        TestRenderSystem second = new();

        world.AddSystem(first);
        world.AddSystem(second);

        Assert.Empty(world.RenderSystems);

        FlushPendingAdditions(world);

        Assert.Equal(2, world.RenderSystems.Count);
        Assert.Same(first, world.RenderSystems[0]);
        Assert.Same(second, world.RenderSystems[1]);
        Assert.Same(world, first.World);
        Assert.Same(world, second.World);
    }

    [Fact]
    public void AddSystem_PreventsDuplicateRenderSystemRegistration()
    {
        World world = new();
        TestRenderSystem system = new();

        world.AddSystem(system);
        world.AddSystem(system);

        Assert.Empty(world.RenderSystems);

        FlushPendingAdditions(world);

        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);
    }

    [Fact]
    public void MoveRenderSystemBetweenWorlds_BlocksActivationUntilOldWorldRemovalsFlush()
    {
        World oldWorld = new();
        World newWorld = new();
        TestRenderSystem system = new();
        oldWorld.AddSystem(system);
        FlushPendingAdditions(oldWorld);

        newWorld.AddSystem(system);

        Assert.Empty(newWorld.RenderSystems);
        Assert.Single(oldWorld.RenderSystems);
        Assert.Same(newWorld, system.World);

        FlushPendingAdditions(newWorld);

        Assert.Empty(newWorld.RenderSystems);
        Assert.Single(oldWorld.RenderSystems);

        FlushPendingRemovals(oldWorld);

        Assert.Empty(oldWorld.RenderSystems);

        FlushPendingAdditions(newWorld);

        Assert.Single(newWorld.RenderSystems);
        Assert.Same(system, newWorld.RenderSystems[0]);
    }

    [Fact]
    public void RemoveSystem_IgnoresRenderSystemOwnedByAnotherWorld()
    {
        World ownerWorld = new();
        World otherWorld = new();
        TestRenderSystem system = new();
        ownerWorld.AddSystem(system);
        FlushPendingAdditions(ownerWorld);

        otherWorld.RemoveSystem(system);

        Assert.Single(ownerWorld.RenderSystems);
        Assert.Empty(otherWorld.RenderSystems);
        Assert.Same(ownerWorld, system.World);
    }

    private static void FlushPendingRemovals(World world)
    {
        world.FlushPendingRemovals();
    }

    private static void FlushPendingAdditions(World world)
    {
        world.FlushPendingAdditions();
    }

    private sealed class TestUpdateSystem : UpdateSystem
    {
    }

    private sealed class TestRenderSystem : RenderSystem
    {
    }
}

