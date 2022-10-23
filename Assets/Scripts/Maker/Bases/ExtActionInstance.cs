using System;
using CommandUndoRedo;

namespace ExternMaker
{
    public class ExtActionInstance : ICommand
    {
        public object[] objects;

        public Action action;
        public Action undo;

        public void Execute() { if (action != null) action.Invoke(); }
        public void UnExecute() { if (undo != null) undo.Invoke(); }

        public void AddToManager()
        {
            UndoRedoManager.Insert(this);
        }
    }
}
