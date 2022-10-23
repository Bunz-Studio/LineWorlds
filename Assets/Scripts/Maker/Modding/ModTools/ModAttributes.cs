using System;

namespace LineWorldsMod
{
    public class HideFromInspectorAttribute : Attribute
    {
        public HideFromInspectorAttribute()
        {
        }
    }

    public class ShowInCustomInspector : Attribute
    {
        public ShowInCustomInspector()
        {

        }
    }
}
