using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;

namespace MPowerKit.Navigation.Behaviors;

public class EventToCommandBehavior : BehaviorBase<BindableObject>
{
    /// <summary>
    /// <see cref="EventInfo"/>
    /// </summary>
    protected EventInfo? _eventInfo;

    /// <summary>
    /// Delegate to Invoke when event is raised
    /// </summary>
    protected Delegate? _handler;

    /// <summary>
    /// Subscribes to the event <see cref="BindableObject"/> object
    /// </summary>
    /// <param name="bindable">Bindable object that is source of event to Attach</param>
    /// <exception cref="ArgumentException">Thrown if no matching event exists on 
    /// <see cref="BindableObject"/></exception>
    protected override void OnAttachedTo(BindableObject bindable)
    {
        base.OnAttachedTo(bindable);

        var type = bindable.GetType();

        _eventInfo = type.GetRuntimeEvent(EventName);

        ArgumentNullException.ThrowIfNull(_eventInfo, nameof(_eventInfo));

        AddEventHandler(_eventInfo, bindable, OnEventRaised);
    }

    /// <summary>
    /// Unsubscribes from the event on <paramref name="bindable"/>
    /// </summary>
    /// <param name="bindable"><see cref="BindableObject"/> that is source of event</param>
    protected override void OnDetachingFrom(BindableObject bindable)
    {
        if (_handler is not null) _eventInfo?.RemoveEventHandler(bindable, _handler);

        _handler = null;
        _eventInfo = null;

        base.OnDetachingFrom(bindable);
    }

    private void AddEventHandler(EventInfo eventInfo, object item, Action<object, EventArgs> action)
    {
        var type = eventInfo.EventHandlerType!;

        var eventParameters = type
            .GetRuntimeMethods()
            .First(static m => m.Name == "Invoke")
            .GetParameters()
            .Select(static p => Expression.Parameter(p.ParameterType))
            .ToArray();

        var actionInvoke = action
            .GetType()
            .GetRuntimeMethods()
            .First(static m => m.Name == "Invoke");

        _handler = Expression.Lambda(
            type,
            Expression.Call(Expression.Constant(action), actionInvoke, eventParameters[0], eventParameters[1]),
            eventParameters)
            .Compile();

        eventInfo.AddEventHandler(item, _handler);
    }

    /// <summary>
    /// Method called when event is raised
    /// </summary>
    /// <param name="sender">Source of that raised the event</param>
    /// <param name="eventArgs">Arguments of the raised event</param>
    protected virtual void OnEventRaised(object sender, EventArgs eventArgs)
    {
        if (Command is null) return;

        var parameter = CommandParameter;

        if (parameter is null && !string.IsNullOrWhiteSpace(EventArgsParameterPath))
        {
            //Walk the ParameterPath for nested properties.
            var propertyPathParts = EventArgsParameterPath.Split('.');
            object? propertyValue = eventArgs;
            foreach (var propertyPathPart in propertyPathParts)
            {
                var propInfo = propertyValue?.GetType().GetRuntimeProperty(propertyPathPart)
                    ?? throw new MissingMemberException($"Unable to find {EventArgsParameterPath}");

                propertyValue = propInfo?.GetValue(propertyValue);

                if (propertyValue is null) break;
            }
            parameter = propertyValue;
        }

        if (parameter is null && eventArgs is not null && eventArgs != EventArgs.Empty && EventArgsConverter is not null)
        {
            parameter = EventArgsConverter.Convert(eventArgs, typeof(object), EventArgsConverterParameter,
                CultureInfo.CurrentUICulture);
        }

        if (Command.CanExecute(parameter)) Command.Execute(parameter);
    }

    #region EventName
    public string EventName
    {
        get { return (string)GetValue(EventNameProperty); }
        set { SetValue(EventNameProperty, value); }
    }

    public static readonly BindableProperty EventNameProperty =
        BindableProperty.Create(
            nameof(EventName),
            typeof(string),
            typeof(EventToCommandBehavior)
            );
    #endregion

    #region Command
    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(EventToCommandBehavior)
            );
    #endregion

    #region CommandParameter
    public object CommandParameter
    {
        get { return (object)GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(EventToCommandBehavior)
            );
    #endregion

    #region EventArgsParameterPath
    public string EventArgsParameterPath
    {
        get { return (string)GetValue(EventArgsParameterPathProperty); }
        set { SetValue(EventArgsParameterPathProperty, value); }
    }

    public static readonly BindableProperty EventArgsParameterPathProperty =
        BindableProperty.Create(
            nameof(EventArgsParameterPath),
            typeof(string),
            typeof(EventToCommandBehavior)
            );
    #endregion

    #region EventArgsConverter
    public IValueConverter EventArgsConverter
    {
        get { return (IValueConverter)GetValue(EventArgsConverterProperty); }
        set { SetValue(EventArgsConverterProperty, value); }
    }

    public static readonly BindableProperty EventArgsConverterProperty =
        BindableProperty.Create(
            nameof(EventArgsConverter),
            typeof(IValueConverter),
            typeof(EventToCommandBehavior)
            );
    #endregion

    #region EventArgsConverterParameter
    public object EventArgsConverterParameter
    {
        get { return (object)GetValue(EventArgsConverterParameterProperty); }
        set { SetValue(EventArgsConverterParameterProperty, value); }
    }

    public static readonly BindableProperty EventArgsConverterParameterProperty =
        BindableProperty.Create(
            nameof(EventArgsConverterParameter),
            typeof(object),
            typeof(EventToCommandBehavior)
            );
    #endregion
}
