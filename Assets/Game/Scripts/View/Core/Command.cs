using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Game.Scripts.View.Core
{
    public class Command
    {
        public IViewableProperty<bool> IsEnabled { get; }
        public IViewableProperty<bool> IsInteractable { get; }
        public ISource<Unit> Execute => _execute;
        
        private readonly Signal<Unit> _execute = new();

        public Command(bool isEnabled = true, bool isInteractable = true)
        {
            IsEnabled = new ViewableProperty<bool>(isEnabled);
            IsInteractable = new ViewableProperty<bool>(isInteractable);
        }

        public void ExecuteCommand()
        {
            if (IsEnabled.Value && IsInteractable.Value)
            {
                _execute.Fire(Unit.Instance);
                return;
            }

            throw new System.InvalidOperationException();
        }
    }

    public class Command<T>
    {
        private readonly Signal<T> _execute = new Signal<T>();
        public IViewableProperty<bool> IsEnabled { get; }
        public IViewableProperty<bool> IsInteractable { get; }
        public ISource<T> Execute => _execute;

        public Command(bool isEnabled = true, bool isInteractable = true)
        {
            IsEnabled = new ViewableProperty<bool>(isEnabled);
            IsInteractable = new ViewableProperty<bool>(isInteractable);
        }

        public void ExecuteCommand(T value)
        {
            if (IsEnabled.Value && IsInteractable.Value)
            {
                _execute.Fire(value);
                return;
            }

            throw new System.InvalidOperationException();
        }
    }
}
