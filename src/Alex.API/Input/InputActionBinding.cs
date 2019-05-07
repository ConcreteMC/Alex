using System;

namespace Alex.API.Input
{
    public delegate bool InputActionPredicate();

    public class InputActionBinding
    {
        private static readonly InputActionPredicate AlwaysTrue = () => true;

        public InputCommand InputCommand { get; }
        public InputActionPredicate Predicate { get; }
        public Action Action { get; }


        public InputActionBinding(InputCommand command, Action action) : this(command, AlwaysTrue, action)
        {
        }

        public InputActionBinding(InputCommand command, InputActionPredicate predicate, Action action)
        {
            InputCommand = command;
            Predicate = predicate;
            Action = action;
        }

    }
}
