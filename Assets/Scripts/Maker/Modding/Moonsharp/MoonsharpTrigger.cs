using UnityEngine;
using System.Collections;

namespace ExternMaker
{
    public class MoonsharpTrigger : MonoBehaviour
    {
        public ExtMoonInstance moon;

        public virtual void OnStart()
        {
            if (moon != null) moon.CallGlobal("trigger.start");
        }

        public virtual void OnEnter(Collider other)
        {
            if (moon != null) moon.CallGlobal("trigger.enter", other.gameObject);
        }

        public virtual void OnUndo()
        {
            if (moon != null) moon.CallGlobal("trigger.undo");
        }

        public virtual void FindObject()
        {
            if (moon != null) moon.CallGlobal("trigger.find");
        }

        public virtual void OnGameStart()
        {
            if (moon != null) moon.CallGlobal("trigger.gameStart");
        }

        public virtual void OnGameStop()
        {
            if (moon != null) moon.CallGlobal("trigger.gameStop");
        }

        int revivePoint;
        CheckpointManager manager;
        void Start()
        {
            manager = FindObjectOfType<CheckpointManager>();
            manager.OnUndo += UndoTrigger;

            if (ExtCore.playState != EditorPlayState.Playing) return;
            GetComponent<MeshRenderer>().enabled = false;
            OnStart();
        }

        void UndoTrigger(int point)
        {
            if (revivePoint == point) OnUndo();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                if (ExtCore.playState != EditorPlayState.Playing) return;

                revivePoint = manager.revivePoint;
                OnEnter(other);
            }
        }

        public static LineMovement GetLine(Collider other)
        {
            return other.GetComponent<LineMovement>();
        }
    }
}