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
        Assert.Null(system.RegisteredWorld);

        FlushPendingAdditions(world);

        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);
        Assert.Same(world, system.RegisteredWorld);
    }

    [Fact]
    public void RemoveSystem_KeepsUpdateSystemOwnedUntilRemovalFlush()
    {
        World world = new();
        TestUpdateSystem system = new();
        world.AddSystem(system);
        FlushPendingAdditions(world);

        world.RemoveSystem(system);

        Assert.Same(world, system.World);
        Assert.Same(world, system.RegisteredWorld);
        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);

        FlushPendingRemovals(world);

        Assert.Null(system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(world.UpdateSystems);
    }

    [Fact]
    public void RemoveSystem_DetachesPendingUpdateSystemWithoutFlush()
    {
        World world = new();
        TestUpdateSystem system = new();

        world.AddSystem(system);
        world.RemoveSystem(system);

        Assert.Null(system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(world.UpdateSystems);

        FlushPendingAdditions(world);
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
    public void AddSystem_CancelsPendingUpdateSystemRemovalInTheSameWorld()
    {
        World world = new();
        TestUpdateSystem system = new();
        world.AddSystem(system);
        FlushPendingAdditions(world);

        world.RemoveSystem(system);
        world.AddSystem(system);

        Assert.Same(world, system.World);
        Assert.Same(world, system.RegisteredWorld);
        Assert.Single(world.UpdateSystems);

        FlushPendingRemovals(world);
        FlushPendingAdditions(world);

        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);
        Assert.Same(world, system.World);
        Assert.Same(world, system.RegisteredWorld);
    }

    [Fact]
    public void AddSystem_QueuesStructurallyOwnedInactiveUpdateSystem()
    {
        World world = new();
        TestUpdateSystem system = new();
        system.SetWorld(world);

        world.AddSystem(system);

        Assert.Same(world, system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(world.UpdateSystems);

        FlushPendingAdditions(world);

        Assert.Single(world.UpdateSystems);
        Assert.Same(world, system.RegisteredWorld);
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
        Assert.Null(system.RegisteredWorld);
        Assert.Same(newWorld, system.World);

        FlushPendingAdditions(newWorld);

        Assert.Single(newWorld.UpdateSystems);
        Assert.Same(system, newWorld.UpdateSystems[0]);
        Assert.Same(newWorld, system.RegisteredWorld);
    }

    [Fact]
    public void MovePendingUpdateSystemBetweenWorlds_ActivatesOnlyInLatestWorld()
    {
        World firstWorld = new();
        World secondWorld = new();
        TestUpdateSystem system = new();

        firstWorld.AddSystem(system);
        secondWorld.AddSystem(system);

        Assert.Same(secondWorld, system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(firstWorld.UpdateSystems);
        Assert.Empty(secondWorld.UpdateSystems);

        FlushPendingAdditions(firstWorld);
        FlushPendingRemovals(firstWorld);

        Assert.Empty(firstWorld.UpdateSystems);
        Assert.Empty(secondWorld.UpdateSystems);

        FlushPendingAdditions(secondWorld);

        Assert.Empty(firstWorld.UpdateSystems);
        Assert.Single(secondWorld.UpdateSystems);
        Assert.Same(system, secondWorld.UpdateSystems[0]);
        Assert.Same(secondWorld, system.RegisteredWorld);
    }

    [Fact]
    public void MoveUpdateSystemBetweenMultipleWorlds_ActivatesOnlyInFinalWorld()
    {
        World firstWorld = new();
        World secondWorld = new();
        World thirdWorld = new();
        TestUpdateSystem system = new();
        firstWorld.AddSystem(system);
        FlushPendingAdditions(firstWorld);

        secondWorld.AddSystem(system);
        thirdWorld.AddSystem(system);

        Assert.Same(thirdWorld, system.World);
        Assert.Same(firstWorld, system.RegisteredWorld);
        Assert.Single(firstWorld.UpdateSystems);
        Assert.Empty(secondWorld.UpdateSystems);
        Assert.Empty(thirdWorld.UpdateSystems);

        FlushPendingAdditions(secondWorld);
        FlushPendingRemovals(secondWorld);
        FlushPendingAdditions(thirdWorld);

        Assert.Empty(secondWorld.UpdateSystems);
        Assert.Empty(thirdWorld.UpdateSystems);
        Assert.Single(firstWorld.UpdateSystems);

        FlushPendingRemovals(firstWorld);
        FlushPendingAdditions(thirdWorld);

        Assert.Empty(firstWorld.UpdateSystems);
        Assert.Empty(secondWorld.UpdateSystems);
        Assert.Single(thirdWorld.UpdateSystems);
        Assert.Same(thirdWorld, system.World);
        Assert.Same(thirdWorld, system.RegisteredWorld);
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
        Assert.Null(system.RegisteredWorld);

        FlushPendingAdditions(world);

        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);
        Assert.Same(world, system.RegisteredWorld);
    }

    [Fact]
    public void RemoveSystem_KeepsRenderSystemOwnedUntilRemovalFlush()
    {
        World world = new();
        TestRenderSystem system = new();
        world.AddSystem(system);
        FlushPendingAdditions(world);

        world.RemoveSystem(system);

        Assert.Same(world, system.World);
        Assert.Same(world, system.RegisteredWorld);
        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);

        FlushPendingRemovals(world);

        Assert.Null(system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(world.RenderSystems);
    }

    [Fact]
    public void RemoveSystem_DetachesPendingRenderSystemWithoutFlush()
    {
        World world = new();
        TestRenderSystem system = new();

        world.AddSystem(system);
        world.RemoveSystem(system);

        Assert.Null(system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(world.RenderSystems);

        FlushPendingAdditions(world);
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
    public void AddSystem_CancelsPendingRenderSystemRemovalInTheSameWorld()
    {
        World world = new();
        TestRenderSystem system = new();
        world.AddSystem(system);
        FlushPendingAdditions(world);

        world.RemoveSystem(system);
        world.AddSystem(system);

        Assert.Same(world, system.World);
        Assert.Same(world, system.RegisteredWorld);
        Assert.Single(world.RenderSystems);

        FlushPendingRemovals(world);
        FlushPendingAdditions(world);

        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);
        Assert.Same(world, system.World);
        Assert.Same(world, system.RegisteredWorld);
    }

    [Fact]
    public void AddSystem_QueuesStructurallyOwnedInactiveRenderSystem()
    {
        World world = new();
        TestRenderSystem system = new();
        system.SetWorld(world);

        world.AddSystem(system);

        Assert.Same(world, system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(world.RenderSystems);

        FlushPendingAdditions(world);

        Assert.Single(world.RenderSystems);
        Assert.Same(world, system.RegisteredWorld);
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

        Assert.Same(oldWorld, system.RegisteredWorld);
        Assert.Empty(newWorld.RenderSystems);
        Assert.Single(oldWorld.RenderSystems);
        Assert.Same(newWorld, system.World);

        FlushPendingAdditions(newWorld);

        Assert.Empty(newWorld.RenderSystems);
        Assert.Single(oldWorld.RenderSystems);

        FlushPendingRemovals(oldWorld);

        Assert.Empty(oldWorld.RenderSystems);
        Assert.Null(system.RegisteredWorld);
        Assert.Same(newWorld, system.World);

        FlushPendingAdditions(newWorld);

        Assert.Single(newWorld.RenderSystems);
        Assert.Same(system, newWorld.RenderSystems[0]);
        Assert.Same(newWorld, system.RegisteredWorld);
    }

    [Fact]
    public void MovePendingRenderSystemBetweenWorlds_ActivatesOnlyInLatestWorld()
    {
        World firstWorld = new();
        World secondWorld = new();
        TestRenderSystem system = new();

        firstWorld.AddSystem(system);
        secondWorld.AddSystem(system);

        Assert.Same(secondWorld, system.World);
        Assert.Null(system.RegisteredWorld);
        Assert.Empty(firstWorld.RenderSystems);
        Assert.Empty(secondWorld.RenderSystems);

        FlushPendingAdditions(firstWorld);
        FlushPendingRemovals(firstWorld);

        Assert.Empty(firstWorld.RenderSystems);
        Assert.Empty(secondWorld.RenderSystems);

        FlushPendingAdditions(secondWorld);

        Assert.Empty(firstWorld.RenderSystems);
        Assert.Single(secondWorld.RenderSystems);
        Assert.Same(system, secondWorld.RenderSystems[0]);
        Assert.Same(secondWorld, system.RegisteredWorld);
    }

    [Fact]
    public void MoveRenderSystemBetweenMultipleWorlds_ActivatesOnlyInFinalWorld()
    {
        World firstWorld = new();
        World secondWorld = new();
        World thirdWorld = new();
        TestRenderSystem system = new();
        firstWorld.AddSystem(system);
        FlushPendingAdditions(firstWorld);

        secondWorld.AddSystem(system);
        thirdWorld.AddSystem(system);

        Assert.Same(thirdWorld, system.World);
        Assert.Same(firstWorld, system.RegisteredWorld);
        Assert.Single(firstWorld.RenderSystems);
        Assert.Empty(secondWorld.RenderSystems);
        Assert.Empty(thirdWorld.RenderSystems);

        FlushPendingAdditions(secondWorld);
        FlushPendingRemovals(secondWorld);
        FlushPendingAdditions(thirdWorld);

        Assert.Empty(secondWorld.RenderSystems);
        Assert.Empty(thirdWorld.RenderSystems);
        Assert.Single(firstWorld.RenderSystems);

        FlushPendingRemovals(firstWorld);
        FlushPendingAdditions(thirdWorld);

        Assert.Empty(firstWorld.RenderSystems);
        Assert.Empty(secondWorld.RenderSystems);
        Assert.Single(thirdWorld.RenderSystems);
        Assert.Same(thirdWorld, system.World);
        Assert.Same(thirdWorld, system.RegisteredWorld);
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
