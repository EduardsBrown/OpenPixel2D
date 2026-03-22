using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class EntityAttachmentTests
{
    [Fact]
    public void AddEntity_QueuesSubtreeRegistrationUntilFlush()
    {
        World world = new();
        var (player, weapon, gem, playerComponent, weaponBehavior, gemComponent) = CreateSubtree();

        world.AddEntity(player);

        Assert.Single(world.Entities);
        Assert.Same(player, world.Entities[0]);
        Assert.DoesNotContain(weapon, world.Entities);
        Assert.Null(player.Parent);
        Assert.Same(player, weapon.Parent);
        Assert.Same(weapon, gem.Parent);
        Assert.Same(world, player.World);
        Assert.Same(world, weapon.World);
        Assert.Same(world, gem.World);
        Assert.Empty(world.RegisteredComponents);
        Assert.Empty(world.RegisteredBehaviorComponents);

        FlushPendingAdditions(world);

        Assert.Equal(3, world.RegisteredComponents.Count);
        Assert.Single(world.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, world.RegisteredComponents);
        Assert.Contains(weaponBehavior, world.RegisteredComponents);
        Assert.Contains(gemComponent, world.RegisteredComponents);
        Assert.Contains(weaponBehavior, world.RegisteredBehaviorComponents);
    }

    [Fact]
    public void AddChild_QueuesSubtreeRegistrationUntilFlush()
    {
        World world = new();
        Entity player = new();
        TestComponent playerComponent = new();
        player.AddComponent(playerComponent);
        world.AddEntity(player);
        FlushPendingAdditions(world);

        var (weapon, gem, weaponBehavior, gemComponent) = CreateChildSubtree();

        player.AddEntity(weapon);

        Assert.Single(world.Entities);
        Assert.Same(player, world.Entities[0]);
        Assert.Single(player.Children);
        Assert.Same(player, weapon.Parent);
        Assert.Same(weapon, gem.Parent);
        Assert.Same(world, weapon.World);
        Assert.Same(world, gem.World);
        Assert.Single(world.RegisteredComponents);
        Assert.Contains(playerComponent, world.RegisteredComponents);

        FlushPendingAdditions(world);

        Assert.Equal(3, world.RegisteredComponents.Count);
        Assert.Single(world.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, world.RegisteredComponents);
        Assert.Contains(weaponBehavior, world.RegisteredComponents);
        Assert.Contains(gemComponent, world.RegisteredComponents);
    }

    [Fact]
    public void RemoveEntity_QueuesSubtreeRemovalUntilFlush()
    {
        World world = new();
        var (player, _, _, _, weaponBehavior, _) = CreateSubtree();

        world.AddEntity(player);
        FlushPendingAdditions(world);

        world.RemoveEntity(player);

        Assert.Empty(world.Entities);
        Assert.Null(player.World);
        Assert.Equal(3, world.RegisteredComponents.Count);
        Assert.Single(world.RegisteredBehaviorComponents);
        Assert.Contains(weaponBehavior, world.RegisteredBehaviorComponents);

        FlushPendingRemovals(world);

        Assert.Empty(world.RegisteredComponents);
        Assert.Empty(world.RegisteredBehaviorComponents);
    }

    [Fact]
    public void AddComponent_QueuesActivationUntilFlush()
    {
        World world = new();
        Entity entity = new();
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        TestComponent component = new();

        entity.AddComponent(component);

        Assert.Single(entity.Components);
        Assert.Same(entity, component.Parent);
        Assert.Empty(world.RegisteredComponents);

        FlushPendingAdditions(world);

        Assert.Single(world.RegisteredComponents);
        Assert.Contains(component, world.RegisteredComponents);
    }

    [Fact]
    public void AddComponent_RemoveBeforeFlush_CancelsActivation()
    {
        World world = new();
        Entity entity = new();
        TestComponent component = new();
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        entity.AddComponent(component);
        entity.RemoveComponent(component);

        Assert.Empty(entity.Components);
        Assert.Null(component.Parent);
        Assert.Empty(world.RegisteredComponents);

        FlushPendingAdditions(world);
        FlushPendingRemovals(world);

        Assert.Empty(world.RegisteredComponents);
    }

    [Fact]
    public void RemoveComponent_QueuesRemovalUntilFlush()
    {
        World world = new();
        Entity entity = new();
        TestComponent component = new();
        entity.AddComponent(component);
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        entity.RemoveComponent(component);

        Assert.Empty(entity.Components);
        Assert.Null(component.Parent);
        Assert.Single(world.RegisteredComponents);
        Assert.Contains(component, world.RegisteredComponents);

        FlushPendingRemovals(world);

        Assert.Empty(world.RegisteredComponents);
    }

    [Fact]
    public void RemoveComponent_ReAddBeforeRemovalFlush_CancelsRemovalWithoutDuplicates()
    {
        World world = new();
        Entity entity = new();
        TestComponent component = new();
        entity.AddComponent(component);
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        entity.RemoveComponent(component);
        entity.AddComponent(component);

        Assert.Single(entity.Components);
        Assert.Same(entity, component.Parent);
        Assert.Single(world.RegisteredComponents);
        Assert.Contains(component, world.RegisteredComponents);

        FlushPendingRemovals(world);
        FlushPendingAdditions(world);

        Assert.Single(world.RegisteredComponents);
        Assert.Contains(component, world.RegisteredComponents);
    }

    [Fact]
    public void BehaviorComponent_BecomesActiveOnlyAfterComponentFlush()
    {
        World world = new();
        Entity entity = new();
        TestBehaviorComponent behavior = new();
        entity.AddComponent(behavior);
        world.AddEntity(entity);

        Assert.Empty(world.RegisteredComponents);
        Assert.Empty(world.RegisteredBehaviorComponents);

        FlushPendingAdditions(world);

        Assert.Single(world.RegisteredComponents);
        Assert.Single(world.RegisteredBehaviorComponents);
        Assert.Contains(behavior, world.RegisteredComponents);
        Assert.Contains(behavior, world.RegisteredBehaviorComponents);
    }

    [Fact]
    public void BehaviorComponent_RemainsConsistentDuringQueuedRemoval()
    {
        World world = new();
        Entity entity = new();
        TestBehaviorComponent behavior = new();
        entity.AddComponent(behavior);
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        entity.RemoveComponent(behavior);

        Assert.Single(world.RegisteredComponents);
        Assert.Single(world.RegisteredBehaviorComponents);

        FlushPendingRemovals(world);

        Assert.Empty(world.RegisteredComponents);
        Assert.Empty(world.RegisteredBehaviorComponents);
    }

    [Fact]
    public void SameWorldMove_DoesNotChangeActiveRegistrations()
    {
        World world = new();
        Entity player = new();
        Entity chest = new();
        Entity weapon = new();
        TestComponent weaponComponent = new();

        weapon.AddComponent(weaponComponent);
        player.AddEntity(weapon);
        world.AddEntity(player);
        world.AddEntity(chest);
        FlushPendingAdditions(world);

        int registeredBefore = world.RegisteredComponents.Count;

        chest.AddEntity(weapon);

        Assert.Equal(2, world.Entities.Count);
        Assert.Empty(player.Children);
        Assert.Single(chest.Children);
        Assert.Same(chest, weapon.Parent);
        Assert.Same(world, weapon.World);
        Assert.Equal(registeredBefore, world.RegisteredComponents.Count);

        FlushPendingRemovals(world);
        FlushPendingAdditions(world);

        Assert.Equal(registeredBefore, world.RegisteredComponents.Count);
        Assert.Contains(weaponComponent, world.RegisteredComponents);
    }

    [Fact]
    public void CrossWorldMove_BlocksActivationUntilOldWorldRemovalsFlush()
    {
        World firstWorld = new();
        World secondWorld = new();
        var (player, weapon, gem, playerComponent, weaponBehavior, gemComponent) = CreateSubtree();

        firstWorld.AddEntity(player);
        FlushPendingAdditions(firstWorld);

        secondWorld.AddEntity(player);

        Assert.Empty(firstWorld.Entities);
        Assert.Single(secondWorld.Entities);
        Assert.Same(secondWorld, player.World);
        Assert.Same(secondWorld, weapon.World);
        Assert.Same(secondWorld, gem.World);
        Assert.Equal(3, firstWorld.RegisteredComponents.Count);
        Assert.Single(firstWorld.RegisteredBehaviorComponents);
        Assert.Empty(secondWorld.RegisteredComponents);
        Assert.Empty(secondWorld.RegisteredBehaviorComponents);

        FlushPendingAdditions(secondWorld);

        Assert.Empty(secondWorld.RegisteredComponents);
        Assert.Empty(secondWorld.RegisteredBehaviorComponents);

        FlushPendingRemovals(firstWorld);

        Assert.Empty(firstWorld.RegisteredComponents);
        Assert.Empty(firstWorld.RegisteredBehaviorComponents);

        FlushPendingAdditions(secondWorld);

        Assert.Equal(3, secondWorld.RegisteredComponents.Count);
        Assert.Single(secondWorld.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, secondWorld.RegisteredComponents);
        Assert.Contains(weaponBehavior, secondWorld.RegisteredComponents);
        Assert.Contains(gemComponent, secondWorld.RegisteredComponents);
    }

    [Fact]
    public void MovePendingSubtreeBetweenWorlds_ActivatesOnlyInLatestWorld()
    {
        World firstWorld = new();
        World secondWorld = new();
        var (player, _, _, playerComponent, weaponBehavior, gemComponent) = CreateSubtree();

        firstWorld.AddEntity(player);
        secondWorld.AddEntity(player);

        Assert.Empty(firstWorld.Entities);
        Assert.Single(secondWorld.Entities);
        Assert.Same(secondWorld, player.World);
        Assert.Empty(firstWorld.RegisteredComponents);
        Assert.Empty(secondWorld.RegisteredComponents);

        FlushPendingAdditions(firstWorld);
        FlushPendingRemovals(firstWorld);

        Assert.Empty(firstWorld.RegisteredComponents);
        Assert.Empty(secondWorld.RegisteredComponents);

        FlushPendingAdditions(secondWorld);

        Assert.Empty(firstWorld.RegisteredComponents);
        Assert.Equal(3, secondWorld.RegisteredComponents.Count);
        Assert.Single(secondWorld.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, secondWorld.RegisteredComponents);
        Assert.Contains(weaponBehavior, secondWorld.RegisteredComponents);
        Assert.Contains(gemComponent, secondWorld.RegisteredComponents);
    }

    [Fact]
    public void MoveSubtreeBetweenMultipleWorlds_ActivatesComponentsOnlyInFinalWorld()
    {
        World firstWorld = new();
        World secondWorld = new();
        World thirdWorld = new();
        var (player, _, _, playerComponent, weaponBehavior, gemComponent) = CreateSubtree();

        firstWorld.AddEntity(player);
        FlushPendingAdditions(firstWorld);

        secondWorld.AddEntity(player);
        thirdWorld.AddEntity(player);

        Assert.Same(thirdWorld, player.World);
        Assert.Equal(3, firstWorld.RegisteredComponents.Count);
        Assert.Empty(secondWorld.RegisteredComponents);
        Assert.Empty(thirdWorld.RegisteredComponents);

        FlushPendingAdditions(secondWorld);
        FlushPendingRemovals(secondWorld);
        FlushPendingAdditions(thirdWorld);

        Assert.Equal(3, firstWorld.RegisteredComponents.Count);
        Assert.Empty(secondWorld.RegisteredComponents);
        Assert.Empty(thirdWorld.RegisteredComponents);

        FlushPendingRemovals(firstWorld);
        FlushPendingAdditions(thirdWorld);

        Assert.Empty(firstWorld.RegisteredComponents);
        Assert.Empty(secondWorld.RegisteredComponents);
        Assert.Equal(3, thirdWorld.RegisteredComponents.Count);
        Assert.Single(thirdWorld.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, thirdWorld.RegisteredComponents);
        Assert.Contains(weaponBehavior, thirdWorld.RegisteredComponents);
        Assert.Contains(gemComponent, thirdWorld.RegisteredComponents);
    }

    [Fact]
    public void AddChild_IgnoresSelfAttachment()
    {
        Entity entity = new();

        entity.AddEntity(entity);

        Assert.Empty(entity.Children);
        Assert.Null(entity.Parent);
        Assert.Null(entity.World);
    }

    [Fact]
    public void AddChild_IgnoresDescendantAttachment()
    {
        Entity player = new();
        Entity weapon = new();
        Entity gem = new();

        weapon.AddEntity(gem);
        player.AddEntity(weapon);
        gem.AddEntity(player);

        Assert.Null(player.Parent);
        Assert.Same(player, weapon.Parent);
        Assert.Same(weapon, gem.Parent);
        Assert.Empty(gem.Children);
    }

    [Fact]
    public void AddEntity_RepairsChildCollectionWhenParentPointerAlreadyMatches()
    {
        Entity parent = new();
        Entity child = new();
        child.SetParent(parent);

        parent.AddEntity(child);

        Assert.Single(parent.Children);
        Assert.Same(child, parent.Children[0]);
        Assert.Same(parent, child.Parent);
    }

    [Fact]
    public void AddComponent_RepairsComponentCollectionWhenParentPointerAlreadyMatches()
    {
        Entity entity = new();
        TestComponent component = new();
        component.SetParent(entity);

        entity.AddComponent(component);

        Assert.Single(entity.Components);
        Assert.Same(component, entity.Components[0]);
        Assert.Same(entity, component.Parent);
    }

    [Fact]
    public void AddChildDirect_IgnoresDuplicateEntries()
    {
        Entity parent = new();
        Entity child = new();

        parent.AddChildDirect(child);
        parent.AddChildDirect(child);

        Assert.Single(parent.Children);
        Assert.Same(child, parent.Children[0]);
        Assert.Same(parent, child.Parent);
    }

    [Fact]
    public void AddComponentDirect_IgnoresDuplicateEntries()
    {
        Entity entity = new();
        TestComponent component = new();

        entity.AddComponent(component);
        entity.AddComponent(component);

        Assert.Single(entity.Components);
        Assert.Same(component, entity.Components[0]);
        Assert.Same(entity, component.Parent);
    }

    [Fact]
    public void RemoveOperations_IgnoreEntitiesTheyDoNotOwn()
    {
        World world = new();
        Entity player = new();
        Entity weapon = new();
        TestComponent weaponComponent = new();

        weapon.AddComponent(weaponComponent);
        player.AddEntity(weapon);
        world.AddEntity(player);
        FlushPendingAdditions(world);

        Entity otherParent = new();

        otherParent.RemoveEntity(weapon);
        world.RemoveEntity(weapon);

        Assert.Single(world.Entities);
        Assert.Same(player, world.Entities[0]);
        Assert.Single(player.Children);
        Assert.Same(player, weapon.Parent);
        Assert.Same(world, weapon.World);
        Assert.Single(world.RegisteredComponents);
        Assert.Contains(weaponComponent, world.RegisteredComponents);
    }

    [Fact]
    public void AddComponent_AllowsMultipleComponentsOfTheSameType()
    {
        World world = new();
        Entity entity = new();
        TestComponent first = new();
        TestComponent second = new();

        entity.AddComponent(first);
        entity.AddComponent(second);
        world.AddEntity(entity);

        Assert.Equal(2, entity.Components.Count);
        Assert.Empty(world.RegisteredComponents);

        FlushPendingAdditions(world);

        Assert.Equal(2, world.RegisteredComponents.Count);
        Assert.Same(first, entity.Components[0]);
        Assert.Same(second, entity.Components[1]);
        Assert.Contains(first, world.RegisteredComponents);
        Assert.Contains(second, world.RegisteredComponents);
    }

    [Fact]
    public void AddComponent_PreventsDuplicateRegistrationForTheSameInstance()
    {
        World world = new();
        Entity entity = new();
        TestComponent component = new();
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        entity.AddComponent(component);
        entity.AddComponent(component);

        Assert.Single(entity.Components);
        Assert.Empty(world.RegisteredComponents);

        FlushPendingAdditions(world);

        Assert.Single(world.RegisteredComponents);
        Assert.Same(component, entity.Components[0]);
        Assert.Contains(component, world.RegisteredComponents);
    }

    private static void FlushPendingRemovals(World world)
    {
        world.FlushPendingRemovals();
    }

    private static void FlushPendingAdditions(World world)
    {
        world.FlushPendingAdditions();
    }

    private static (Entity Player, Entity Weapon, Entity Gem, TestComponent PlayerComponent, TestBehaviorComponent WeaponBehavior, TestComponent GemComponent) CreateSubtree()
    {
        Entity player = new();
        TestComponent playerComponent = new();
        player.AddComponent(playerComponent);

        var (weapon, gem, weaponBehavior, gemComponent) = CreateChildSubtree();
        player.AddEntity(weapon);

        return (player, weapon, gem, playerComponent, weaponBehavior, gemComponent);
    }

    private static (Entity Weapon, Entity Gem, TestBehaviorComponent WeaponBehavior, TestComponent GemComponent) CreateChildSubtree()
    {
        Entity weapon = new();
        Entity gem = new();
        TestBehaviorComponent weaponBehavior = new();
        TestComponent gemComponent = new();

        gem.AddComponent(gemComponent);
        weapon.AddComponent(weaponBehavior);
        weapon.AddEntity(gem);

        return (weapon, gem, weaponBehavior, gemComponent);
    }

    private sealed class TestComponent : Component
    {
    }

    private sealed class TestBehaviorComponent : BehaviorComponent
    {
    }
}
