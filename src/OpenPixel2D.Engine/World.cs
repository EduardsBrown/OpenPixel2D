namespace OpenPixel2D.Engine;

public class World
{
    private List<Entity> _entities = [];
    private List<UpdateSystem> _updateSystems = [];
    private List<RenderSystem> _renderSystems = [];
}