using UnityEngine;

namespace __Scripts
{
    public class CamFollowDray : MonoBehaviour
    {
        public static bool IsTransitioning = false;

        [Header("Set in Inspector")]
        public InRoom drayInRm;
        public float transTime = 0.5f;

        private Vector3 p0, p1;

        private InRoom _inRm;
        private float _transStart;

        private void Awake()
        {
            _inRm = GetComponent<InRoom>(); 
        }

        private void Update()
        {
            if (IsTransitioning)
            {
                float u = (Time.time - _transStart) / transTime;
                if(u>= 1)
                {
                    u = 1;
                    IsTransitioning = false;
                }
                transform.position = (1 - u) * p0 + u * p1;
            }
            else
            {
                if(drayInRm.RoomNum != _inRm.RoomNum)
                {
                    TransitionTo(drayInRm.RoomNum);
                }
            }
        }

        void TransitionTo(Vector2 rm)
        {
            p0 = transform.position;
            _inRm.RoomNum = rm;
            p1 = transform.position + (Vector3.back * 10);
            transform.position = p0;

            _transStart = Time.time;
            IsTransitioning = true;
        }
    }
}
