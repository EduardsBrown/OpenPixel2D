using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class WorldSystemOwnershipTests
{
    [Fact]
    public void AddSystem_AddsUpdateSystemToWorld()
    {
        World world = new();
        TestUpdateSystem system = new();

        world.AddSystem(system);

        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);
        Assert.Empty(world.RenderSystems);
        Assert.Same(world, system.World);
    }

    [Fact]
    public void RemoveSystem_RemovesUpdateSystemFromWorld()
    {
        World world = new();
        TestUpdateSystem system = new();
        world.AddSystem(system);

        world.RemoveSystem(system);

        Assert.Empty(world.UpdateSystems);
        Assert.Null(system.World);
    }

    [Fact]
    public void AddSystem_AllowsMultipleUpdateSystemsOfTheSameType()
    {
        World world = new();
        TestUpdateSystem first = new();
        TestUpdateSystem second = new();

        world.AddSystem(first);
        world.AddSystem(second);

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

        Assert.Single(world.UpdateSystems);
        Assert.Same(system, world.UpdateSystems[0]);
        Assert.Same(world, system.World);
    }

    [Fact]
    public void AddSystem_MovesUpdateSystemBetweenWorlds()
    {
        World oldWorld = new();
        World newWorld = new();
        TestUpdateSystem system = new();
        oldWorld.AddSystem(system);

        newWorld.AddSystem(system);

        Assert.Empty(oldWorld.UpdateSystems);
        Assert.Single(newWorld.UpdateSystems);
        Assert.Same(system, newWorld.UpdateSystems[0]);
        Assert.Same(newWorld, system.World);
    }

    [Fact]
    public void RemoveSystem_IgnoresUpdateSystemOwnedByAnotherWorld()
    {
        World ownerWorld = new();
        World otherWorld = new();
        TestUpdateSystem system = new();
        ownerWorld.AddSystem(system);

        otherWorld.RemoveSystem(system);

        Assert.Single(ownerWorld.UpdateSystems);
        Assert.Empty(otherWorld.UpdateSystems);
        Assert.Same(ownerWorld, system.World);
    }

    [Fact]
    public void AddSystem_AddsRenderSystemToWorld()
    {
        World world = new();
        TestRenderSystem system = new();

        world.AddSystem(system);

        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);
        Assert.Empty(world.UpdateSystems);
        Assert.Same(world, system.World);
    }

    [Fact]
    public void RemoveSystem_RemovesRenderSystemFromWorld()
    {
        World world = new();
        TestRenderSystem system = new();
        world.AddSystem(system);

        world.RemoveSystem(system);

        Assert.Empty(world.RenderSystems);
        Assert.Null(system.World);
    }

    [Fact]
    public void AddSystem_AllowsMultipleRenderSystemsOfTheSameType()
    {
        World world = new();
        TestRenderSystem first = new();
        TestRenderSystem second = new();

        world.AddSystem(first);
        world.AddSystem(second);

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

        Assert.Single(world.RenderSystems);
        Assert.Same(system, world.RenderSystems[0]);
        Assert.Same(world, system.World);
    }

    [Fact]
    public void AddSystem_MovesRenderSystemBetweenWorlds()
    {
        World oldWorld = new();
        World newWorld = new();
        TestRenderSystem system = new();
        oldWorld.AddSystem(system);

        newWorld.AddSystem(system);

        Assert.Empty(oldWorld.RenderSystems);
        Assert.Single(newWorld.RenderSystems);
        Assert.Same(system, newWorld.RenderSystems[0]);
        Assert.Same(newWorld, system.World);
    }

    [Fact]
    public void RemoveSystem_IgnoresRenderSystemOwnedByAnotherWorld()
    {
        World ownerWorld = new();
        World otherWorld = new();
        TestRenderSystem system = new();
        ownerWorld.AddSystem(system);

        otherWorld.RemoveSystem(system);

        Assert.Single(ownerWorld.RenderSystems);
        Assert.Empty(otherWorld.RenderSystems);
        Assert.Same(ownerWorld, system.World);
    }

    private sealed class TestUpdateSystem : UpdateSystem
    {
    }

    private sealed class TestRenderSystem : RenderSystem
    {
    }
}
