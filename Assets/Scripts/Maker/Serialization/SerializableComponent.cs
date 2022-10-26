using UnityEngine;
using System.Collections;
using ExternMaker.Serialization;

namespace ExternMaker
{
    public class SerializableComponent : MonoBehaviour
    {
        public Serializables.SerializedUniversalComponent serializer;
        public void Initialize(System.Type type)
        {
            serializer.actualType = type.FullName;
        }
    }
}
