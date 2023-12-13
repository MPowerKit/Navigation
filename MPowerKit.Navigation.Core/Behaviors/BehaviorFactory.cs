using MPowerKit.Navigation.Interfaces;

namespace MPowerKit.Navigation.Behaviors;

public class DelegateBehaviorFactory : IBehaviorFactory
{
    private readonly Action<VisualElement> _applyBehaviors;

    public DelegateBehaviorFactory(Action<VisualElement> applyBehaviors)
    {
        _applyBehaviors = applyBehaviors;
    }

    public void ApplyBehaviors(VisualElement element)
    {
        _applyBehaviors(element);
    }
}

public class DelegateContainerBehaviorFactory : IBehaviorFactory
{
    private readonly Action<IServiceProvider, VisualElement> _applyBehaviors;
    private readonly IServiceProvider _container;

    public DelegateContainerBehaviorFactory(Action<IServiceProvider, VisualElement> applyBehaviors, IServiceProvider container)
    {
        _applyBehaviors = applyBehaviors;
        _container = container;
    }

    public void ApplyBehaviors(VisualElement element)
    {
        _applyBehaviors(_container, element);
    }
}