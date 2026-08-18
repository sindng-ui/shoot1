using UnityEngine;

namespace HappyShoot.View.Cameras
{
    /// <summary>
    /// Smooth 2D camera follow script that tracks the target transform.
    /// </summary>
    public class CameraFollowView : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 5.0f;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        }
    }
}
