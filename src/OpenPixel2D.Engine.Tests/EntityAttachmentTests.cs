using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class EntityAttachmentTests
{
    [Fact]
    public void AddEntity_AttachesSubtreeAsRootAndRegistersComponents()
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
        Assert.Equal(3, world.RegisteredComponents.Count);
        Assert.Single(world.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, world.RegisteredComponents);
        Assert.Contains(weaponBehavior, world.RegisteredComponents);
        Assert.Contains(gemComponent, world.RegisteredComponents);
        Assert.Contains(weaponBehavior, world.RegisteredBehaviorComponents);
    }

    [Fact]
    public void AddChild_AttachesSubtreeToParentsWorldAndKeepsWorldRootsFlat()
    {
        World world = new();
        Entity player = new();
        TestComponent playerComponent = new();
        player.AddComponent(playerComponent);
        world.AddEntity(player);

        var (weapon, gem, weaponBehavior, gemComponent) = CreateChildSubtree();

        player.AddEntity(weapon);

        Assert.Single(world.Entities);
        Assert.Same(player, world.Entities[0]);
        Assert.Single(player.Children);
        Assert.Same(player, weapon.Parent);
        Assert.Same(weapon, gem.Parent);
        Assert.Same(world, weapon.World);
        Assert.Same(world, gem.World);
        Assert.Equal(3, world.RegisteredComponents.Count);
        Assert.Single(world.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, world.RegisteredComponents);
        Assert.Contains(weaponBehavior, world.RegisteredComponents);
        Assert.Contains(gemComponent, world.RegisteredComponents);
    }

    [Fact]
    public void AddChild_MovesWorldRootUnderParentWithoutDuplicatingRegistration()
    {
        World world = new();
        Entity player = new();
        TestComponent playerComponent = new();
        player.AddComponent(playerComponent);

        Entity weapon = new();
        TestBehaviorComponent weaponBehavior = new();
        weapon.AddComponent(weaponBehavior);

        world.AddEntity(player);
        world.AddEntity(weapon);

        int registeredBefore = world.RegisteredComponents.Count;
        int behaviorsBefore = world.RegisteredBehaviorComponents.Count;

        player.AddEntity(weapon);

        Assert.Single(world.Entities);
        Assert.Same(player, world.Entities[0]);
        Assert.Single(player.Children);
        Assert.Same(player, weapon.Parent);
        Assert.Same(world, weapon.World);
        Assert.Equal(registeredBefore, world.RegisteredComponents.Count);
        Assert.Equal(behaviorsBefore, world.RegisteredBehaviorComponents.Count);
        Assert.Contains(playerComponent, world.RegisteredComponents);
        Assert.Contains(weaponBehavior, world.RegisteredComponents);
    }

    [Fact]
    public void AddEntity_MovesChildToWorldRootWithoutDuplicatingRegistration()
    {
        World world = new();
        Entity player = new();
        Entity weapon = new();
        TestBehaviorComponent weaponBehavior = new();

        weapon.AddComponent(weaponBehavior);
        player.AddEntity(weapon);
        world.AddEntity(player);

        int registeredBefore = world.RegisteredComponents.Count;
        int behaviorsBefore = world.RegisteredBehaviorComponents.Count;

        world.AddEntity(weapon);

        Assert.Equal(2, world.Entities.Count);
        Assert.Empty(player.Children);
        Assert.Null(weapon.Parent);
        Assert.Same(world, weapon.World);
        Assert.Equal(registeredBefore, world.RegisteredComponents.Count);
        Assert.Equal(behaviorsBefore, world.RegisteredBehaviorComponents.Count);
        Assert.Contains(weaponBehavior, world.RegisteredBehaviorComponents);
    }

    [Fact]
    public void AddChild_MovesEntityBetweenParentsInSameWorldWithoutDuplicatingRegistration()
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

        int registeredBefore = world.RegisteredComponents.Count;

        chest.AddEntity(weapon);

        Assert.Equal(2, world.Entities.Count);
        Assert.Empty(player.Children);
        Assert.Single(chest.Children);
        Assert.Same(chest, weapon.Parent);
        Assert.Same(world, weapon.World);
        Assert.Equal(registeredBefore, world.RegisteredComponents.Count);
        Assert.Contains(weaponComponent, world.RegisteredComponents);
    }

    [Fact]
    public void AddEntity_MovesSubtreeBetweenWorldsAndTransfersRegistration()
    {
        World firstWorld = new();
        World secondWorld = new();
        var (player, weapon, gem, playerComponent, weaponBehavior, gemComponent) = CreateSubtree();

        firstWorld.AddEntity(player);
        secondWorld.AddEntity(player);

        Assert.Empty(firstWorld.Entities);
        Assert.Empty(firstWorld.RegisteredComponents);
        Assert.Empty(firstWorld.RegisteredBehaviorComponents);
        Assert.Single(secondWorld.Entities);
        Assert.Same(player, secondWorld.Entities[0]);
        Assert.Same(secondWorld, player.World);
        Assert.Same(secondWorld, weapon.World);
        Assert.Same(secondWorld, gem.World);
        Assert.Equal(3, secondWorld.RegisteredComponents.Count);
        Assert.Single(secondWorld.RegisteredBehaviorComponents);
        Assert.Contains(playerComponent, secondWorld.RegisteredComponents);
        Assert.Contains(weaponBehavior, secondWorld.RegisteredComponents);
        Assert.Contains(gemComponent, secondWorld.RegisteredComponents);
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
    public void AddEntity_ReattachesSubtreeWithoutDuplicatingRegistration()
    {
        World world = new();
        var (player, _, _, _, weaponBehavior, _) = CreateSubtree();

        world.AddEntity(player);
        world.RemoveEntity(player);
        world.AddEntity(player);

        Assert.Single(world.Entities);
        Assert.Equal(3, world.RegisteredComponents.Count);
        Assert.Single(world.RegisteredBehaviorComponents);
        Assert.Contains(weaponBehavior, world.RegisteredBehaviorComponents);
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
        Assert.Same(first, entity.Components[0]);
        Assert.Same(second, entity.Components[1]);
        Assert.Same(entity, first.Parent);
        Assert.Same(entity, second.Parent);
        Assert.Equal(2, world.RegisteredComponents.Count);
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

        entity.AddComponent(component);
        entity.AddComponent(component);

        Assert.Single(entity.Components);
        Assert.Single(world.RegisteredComponents);
        Assert.Same(component, entity.Components[0]);
        Assert.Contains(component, world.RegisteredComponents);
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


