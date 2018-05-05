using System;

namespace RocketUI.Input
{
    public delegate bool InputActionPredicate();

    public class InputActionBinding
    {
        private static readonly InputActionPredicate AlwaysTrue = () => true;

        public string InputCommand { get; }
        public InputActionPredicate Predicate { get; }
        public Action Action { get; }


        public InputActionBinding(string command, Action action) : this(command, AlwaysTrue, action)
        {
        }

        public InputActionBinding(string command, InputActionPredicate predicate, Action action)
        {
            InputCommand = command;
            Predicate = predicate;
            Action = action;
        }

    }
}
