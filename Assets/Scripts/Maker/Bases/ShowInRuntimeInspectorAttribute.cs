using System;

namespace ExternMaker
{
    class ShowInRuntimeInspectorAttribute : Attribute
    {
        public bool modifiable;
        public ShowInRuntimeInspectorAttribute(bool modifiable)
        {
            this.modifiable = modifiable;
        }
    }

    class AllowSavingStateAttribute : Attribute
    {
        public AllowSavingStateAttribute()
        {

        }
    }

    class IgnoreSavingStateAttribute : Attribute
    {
        public IgnoreSavingStateAttribute()
        {

        }
    }

    class SaveAllStateAttribute : Attribute
    {
        public SaveAllStateAttribute()
        {

        }
    }
}
