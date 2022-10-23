using UnityEngine;
using System.Collections;

namespace LineWorldsMod
{
    public class ModTrigger : MonoBehaviour
    {
        public string targetTrigger = "Player";

        public virtual void OnStart()
        {

        }

        public virtual void OnEnter(Collider other)
        {

        }

        public virtual void OnUndo()
        {

        }

        public virtual void FindObject()
        {

        }

        public virtual void OnGameStart()
        {

        }

        public virtual void OnGameStop()
        {

        }

        int revivePoint;
        CheckpointManager manager;
        void Start()
        {
            manager = FindObjectOfType<CheckpointManager>();
            manager.OnUndo += UndoTrigger;

            if (ExternMaker.ExtCore.playState != ExternMaker.EditorPlayState.Playing) return;
            GetComponent<MeshRenderer>().enabled = false;
            OnStart();
        }

        void UndoTrigger(int point)
        {
            if (revivePoint == point) OnUndo();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == targetTrigger)
            {
                if (ExternMaker.ExtCore.playState != ExternMaker.EditorPlayState.Playing) return;

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