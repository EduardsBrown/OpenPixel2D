using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderProcessorRegistryTests
{
    [Fact]
    public void Process_RegisteredProcessorsExecuteInRegistrationOrder()
    {
        RenderProcessorRegistry registry = new();
        List<string> calls = [];
        RenderPassRegistry passRegistry = new();
        passRegistry.RegisterDefaultPasses();

        registry.Register(new RecordingProcessor<FirstItem>("first", calls));
        registry.Register(new RecordingProcessor<SecondItem>("second", calls));

        registry.Process(new RenderQueue(), new RenderPipelineContext(new RenderFrame(passRegistry), new RenderQueue()));

        Assert.Equal(["first", "second"], calls);
    }

    private readonly record struct FirstItem(int Id) : IRenderItem;

    private readonly record struct SecondItem(int Id) : IRenderItem;

    private sealed class RecordingProcessor<T> : IRenderItemProcessor<T>
        where T : struct, IRenderItem
    {
        private readonly string _name;
        private readonly List<string> _calls;

        public RecordingProcessor(string name, List<string> calls)
        {
            _name = name;
            _calls = calls;
        }

        public void Process(ReadOnlySpan<T> items, IRenderPipelineContext context)
        {
            _calls.Add(_name);
        }
    }
}
