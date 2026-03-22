using OpenPixel2D.Abstractions;
using OpenPixel2D.Engine;
using OpenPixel2D.Engine.Extensions;

namespace OpenPixel2D.Engine.Tests;

public sealed class QueryExtensionTests
{
    [Fact]
    public void HasComponent_ReturnsTrueForAssignableMatches()
    {
        Entity entity = new();
        DerivedQueryComponent component = new();
        entity.AddComponent(component);

        Assert.True(entity.HasComponent<DerivedQueryComponent>());
        Assert.True(entity.HasComponent<BaseQueryComponent>());
    }

    [Fact]
    public void HasComponent_ReturnsFalseWhenNoMatchExists()
    {
        Entity entity = new();
        entity.AddComponent(new AnotherQueryComponent());

        Assert.False(entity.HasComponent<BaseQueryComponent>());
    }

    [Fact]
    public void TryGetComponent_ReturnsFirstMatchingComponentInOrder()
    {
        Entity entity = new();
        DerivedQueryComponent first = new();
        DerivedQueryComponent second = new();
        entity.AddComponent(first);
        entity.AddComponent(second);

        bool found = entity.TryGetComponent<BaseQueryComponent>(out BaseQueryComponent? component);

        Assert.True(found);
        Assert.Same(first, component);
    }

    [Fact]
    public void TryGetComponent_ReturnsFalseAndNullWhenMissing()
    {
        Entity entity = new();

        bool found = entity.TryGetComponent<BaseQueryComponent>(out BaseQueryComponent? component);

        Assert.False(found);
        Assert.Null(component);
    }

    [Fact]
    public void GetComponent_ReturnsFirstMatchingComponent()
    {
        Entity entity = new();
        DerivedQueryComponent first = new();
        DerivedQueryComponent second = new();
        entity.AddComponent(first);
        entity.AddComponent(second);

        BaseQueryComponent? component = entity.GetComponent<BaseQueryComponent>();

        Assert.Same(first, component);
    }

    [Fact]
    public void GetComponent_ReturnsNullWhenMissing()
    {
        Entity entity = new();

        Assert.Null(entity.GetComponent<BaseQueryComponent>());
    }

    [Fact]
    public void GetComponents_ReturnsAllAssignableMatchesInComponentOrder()
    {
        Entity entity = new();
        DerivedQueryComponent first = new();
        AnotherQueryComponent unrelated = new();
        DerivedQueryComponent second = new();
        entity.AddComponent(first);
        entity.AddComponent(unrelated);
        entity.AddComponent(second);

        BaseQueryComponent[] components = [.. entity.GetComponents<BaseQueryComponent>()];

        Assert.Equal(2, components.Length);
        Assert.Same(first, components[0]);
        Assert.Same(second, components[1]);
    }

    [Fact]
    public void GetComponents_ReturnsEmptyWhenMissing()
    {
        Entity entity = new();

        Assert.Empty(entity.GetComponents<BaseQueryComponent>());
    }

    [Fact]
    public void GetEntitiesWithComponentType_RejectsInvalidQueryTypes()
    {
        World world = new();

        Assert.Throws<ArgumentException>(() => world.GetEntitiesWithComponentType(typeof(string)));
        Assert.Throws<ArgumentException>(() => world.GetEntitiesWithComponentType(typeof(IComponent)));
    }

    [Fact]
    public void GetEntitiesWith_ReturnsLiveIndexedViewAfterPreStartActivation()
    {
        World world = new();
        Entity entity = new();
        entity.AddComponent(new BaseQueryComponent());
        IReadOnlyCollection<Entity> query = world.GetEntitiesWithComponentType(typeof(BaseQueryComponent));

        Assert.Empty(query);

        world.AddEntity(entity);

        Assert.Empty(query);

        world.Initialize();

        Assert.Single(query);
        Assert.Contains(entity, query);
    }

    [Fact]
    public void GetEntitiesWith_HidesDetachedEntitiesImmediatelyAndAfterFlush()
    {
        World world = new();
        Entity entity = new();
        entity.AddComponent(new BaseQueryComponent());
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        IReadOnlyCollection<Entity> query = world.GetEntitiesWithComponentType(typeof(BaseQueryComponent));

        Assert.Single(query);
        Assert.Contains(entity, query);

        world.RemoveEntity(entity);

        Assert.Empty(query);

        FlushPendingRemovals(world);

        Assert.Empty(query);
    }

    [Fact]
    public void GetEntitiesWith_RuntimeAdditionsAppearOnlyAfterNextUpdate()
    {
        World world = CreateStartedWorld();
        Entity entity = new();
        entity.AddComponent(new BaseQueryComponent());
        world.AddEntity(entity);

        Assert.Empty(world.GetEntitiesWith<BaseQueryComponent>());

        world.Update();

        Assert.Single(world.GetEntitiesWith<BaseQueryComponent>());
        Assert.Contains(entity, world.GetEntitiesWith<BaseQueryComponent>());
    }

    [Fact]
    public void GetEntitiesWith_RemovedComponentsDisappearImmediatelyAndAfterFlush()
    {
        World world = new();
        Entity entity = new();
        BaseQueryComponent component = new();
        entity.AddComponent(component);
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        IReadOnlyCollection<Entity> query = world.GetEntitiesWithComponentType(typeof(BaseQueryComponent));

        Assert.Single(query);
        entity.RemoveComponent(component);
        Assert.Empty(query);

        FlushPendingRemovals(world);

        Assert.Empty(query);
    }

    [Fact]
    public void GetEntitiesWith_AddThenRemoveBeforeActivation_NeverAppears()
    {
        World world = CreateStartedWorld();
        Entity entity = new();
        entity.AddComponent(new BaseQueryComponent());
        world.AddEntity(entity);
        world.RemoveEntity(entity);

        Assert.Empty(world.GetEntitiesWith<BaseQueryComponent>());

        world.Update();

        Assert.Empty(world.GetEntitiesWith<BaseQueryComponent>());
    }

    [Fact]
    public void GetEntitiesWith_MultipleMatchingComponents_DoesNotDuplicateEntity()
    {
        World world = new();
        Entity entity = new();
        entity.AddComponent(new DerivedQueryComponent());
        entity.AddComponent(new DerivedQueryComponent());
        entity.AddComponent(new SiblingDerivedQueryComponent());
        world.AddEntity(entity);
        FlushPendingAdditions(world);

        Assert.Single(world.GetEntitiesWith<DerivedQueryComponent>());
        Assert.Single(world.GetEntitiesWith<BaseQueryComponent>());
        Assert.Contains(entity, world.GetEntitiesWith<BaseQueryComponent>());
    }

    [Fact]
    public void GetEntitiesWith_SameWorldReparent_PreservesSingleMembership()
    {
        World world = new();
        Entity parent = new();
        Entity newParent = new();
        Entity child = new();
        child.AddComponent(new BaseQueryComponent());
        parent.AddEntity(child);
        world.AddEntity(parent);
        world.AddEntity(newParent);
        FlushPendingAdditions(world);

        newParent.AddEntity(child);

        Assert.Single(world.GetEntitiesWith<BaseQueryComponent>());
        Assert.Contains(child, world.GetEntitiesWith<BaseQueryComponent>());

        FlushPendingRemovals(world);
        FlushPendingAdditions(world);

        Assert.Single(world.GetEntitiesWith<BaseQueryComponent>());
        Assert.Contains(child, world.GetEntitiesWith<BaseQueryComponent>());
    }

    [Fact]
    public void GetEntitiesWith_ComponentMovedWithinSameWorld_TracksNewOwnerWithoutDuplicates()
    {
        World world = new();
        Entity first = new();
        Entity second = new();
        BaseQueryComponent component = new();
        first.AddComponent(component);
        world.AddEntity(first);
        world.AddEntity(second);
        FlushPendingAdditions(world);

        Assert.Single(world.GetEntitiesWith<BaseQueryComponent>());
        Assert.Contains(first, world.GetEntitiesWith<BaseQueryComponent>());

        second.AddComponent(component);

        IReadOnlyCollection<Entity> query = world.GetEntitiesWithComponentType(typeof(BaseQueryComponent));

        Assert.Single(query);
        Assert.DoesNotContain(first, query);
        Assert.Contains(second, query);

        FlushPendingRemovals(world);
        FlushPendingAdditions(world);

        Assert.Single(query);
        Assert.DoesNotContain(first, query);
        Assert.Contains(second, query);
    }

    [Fact]
    public void GetEntitiesWith_CrossWorldMove_RemovesFromOldWorldImmediately_AndActivatesInNewWorldAfterFlush()
    {
        World firstWorld = new();
        World secondWorld = new();
        Entity entity = new();
        entity.AddComponent(new BaseQueryComponent());
        firstWorld.AddEntity(entity);
        FlushPendingAdditions(firstWorld);

        IReadOnlyCollection<Entity> oldWorldQuery = firstWorld.GetEntitiesWithComponentType(typeof(BaseQueryComponent));
        IReadOnlyCollection<Entity> newWorldQuery = secondWorld.GetEntitiesWithComponentType(typeof(BaseQueryComponent));

        Assert.Single(oldWorldQuery);
        Assert.Empty(newWorldQuery);

        secondWorld.AddEntity(entity);

        Assert.Empty(oldWorldQuery);
        Assert.Empty(newWorldQuery);

        FlushPendingAdditions(secondWorld);
        Assert.Empty(newWorldQuery);

        FlushPendingRemovals(firstWorld);
        FlushPendingAdditions(secondWorld);

        Assert.Empty(oldWorldQuery);
        Assert.Single(newWorldQuery);
        Assert.Contains(entity, newWorldQuery);
    }

    [Fact]
    public void GetEntitiesWith_BaseQueriesIncludeDerived_AndExactQueriesStayExact()
    {
        World world = new();
        Entity baseEntity = new();
        Entity derivedEntity = new();
        baseEntity.AddComponent(new BaseQueryComponent());
        derivedEntity.AddComponent(new DerivedQueryComponent());
        world.AddEntity(baseEntity);
        world.AddEntity(derivedEntity);
        FlushPendingAdditions(world);

        IReadOnlyCollection<Entity> baseQuery = world.GetEntitiesWithComponentType(typeof(BaseQueryComponent));
        IReadOnlyCollection<Entity> derivedQuery = world.GetEntitiesWithComponentType(typeof(DerivedQueryComponent));

        Assert.Equal(2, baseQuery.Count);
        Assert.Contains(baseEntity, baseQuery);
        Assert.Contains(derivedEntity, baseQuery);
        Assert.Single(derivedQuery);
        Assert.Contains(derivedEntity, derivedQuery);
        Assert.DoesNotContain(baseEntity, derivedQuery);
    }

    [Fact]
    public void GetEntitiesWith_RepeatedQueriesStayStableAcrossFrames()
    {
        World world = CreateStartedWorld();
        Entity original = new();
        original.AddComponent(new BaseQueryComponent());
        world.AddEntity(original);
        world.Update();

        IReadOnlyCollection<Entity> query = world.GetEntitiesWithComponentType(typeof(BaseQueryComponent));

        Assert.Single(query);
        Assert.Contains(original, query);

        Entity spawned = new();
        spawned.AddComponent(new BaseQueryComponent());
        world.AddEntity(spawned);

        Assert.Single(query);
        Assert.Contains(original, query);

        world.Update();

        Assert.Equal(2, query.Count);
        Assert.Contains(original, query);
        Assert.Contains(spawned, query);

        world.RemoveEntity(original);

        Assert.Single(query);
        Assert.DoesNotContain(original, query);
        Assert.Contains(spawned, query);

        world.Update();

        Assert.Single(query);
        Assert.Contains(spawned, query);
    }

    private static World CreateStartedWorld()
    {
        World world = new();
        world.Initialize();
        world.Start();
        return world;
    }

    private static void FlushPendingAdditions(World world)
    {
        world.FlushPendingAdditions();
    }

    private static void FlushPendingRemovals(World world)
    {
        world.FlushPendingRemovals();
    }

    private class BaseQueryComponent : Component
    {
    }

    private sealed class DerivedQueryComponent : BaseQueryComponent
    {
    }

    private sealed class SiblingDerivedQueryComponent : BaseQueryComponent
    {
    }

    private sealed class AnotherQueryComponent : Component
    {
    }
}
