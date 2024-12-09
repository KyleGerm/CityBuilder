using UnityEngine;
using Game.Managers;
namespace Game
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private LayerMask placementMask;
        [SerializeField] private float maxZoom, minZoom;
        private Camera _camera;
        private Vector3 lastPos,defaultRotation, returnToPosition, minBounds, maxBounds;
        private float prevZoomValue;
        private bool isFocused;
        private int zOffset = -45;

        private void Start()
        {
            Setup();
            ApplyInitialValues(); 
        }
        /// <summary>
        /// Gets reference to the attatched camera and sets up subscriptions to the InputManager
        /// </summary>
        private void Setup()
        {
            var manager = InputManager.Instance;
            manager.Subscribe(MoveCamera);
            manager.Subscribe(CameraZoom);
            manager.Subscribe(GetMousePosition);
            manager.Subscribe(FocusOnTile);
            _camera = GetComponent<Camera>();
        }
        /// <summary>
        /// Gets initial values and applies them to the necessary fields
        /// </summary>
        private void ApplyInitialValues()
        {
            defaultRotation = _camera.transform.forward;
            prevZoomValue = _camera.fieldOfView;
            returnToPosition = _camera.transform.position;
        }

        /// <summary>
        /// Moves the camera in the given direction
        /// </summary>
        /// <param name="amount"></param>
        private void MoveCamera(Vector3 amount)
        {
            if (isFocused) return;
            transform.position += amount;
            CheckBounds();
        }

        /// <summary>
        /// Zooms the camera within the min and max limit
        /// </summary>
        /// <param name="amount"></param>
        private void CameraZoom(float amount)
        {
            if (amount >= 0 && _camera.fieldOfView >= maxZoom || amount <= 0 && _camera.fieldOfView <= minZoom) return;
            prevZoomValue = _camera.fieldOfView;
            _camera.fieldOfView += amount * Time.deltaTime;
        }

        /// <summary>
        /// Returns the world position of where the mouse is on the screen.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMousePosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = _camera.nearClipPlane;
            Ray ray = _camera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, _camera.farClipPlane, placementMask))
            {
                lastPos = hit.point;
            }
            return lastPos;
        }
        /// <summary>
        /// Focuses or unfocuses the camera based on the value given
        /// </summary>
        /// <param name="isFocused"></param>
        public void FocusOnTile(bool isFocused)
        {
            this.isFocused = isFocused;
            _camera.fieldOfView = isFocused ? minZoom : prevZoomValue;
            if (isFocused)
            {
                returnToPosition = _camera.transform.position;
                transform.position = new Vector3(lastPos.x, transform.position.y, transform.position.z);
                _camera.transform.LookAt(lastPos);
            }
            else
            {
                _camera.transform.position = returnToPosition;
                _camera.transform.forward = defaultRotation;
            }
        }
        /// <summary>
        /// Sets the boundaries which the camera should stay within
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void Setbounds(Vector3 pos, int size)
        {
            minBounds = new Vector3(pos.x, pos.y, pos.z + zOffset);
            maxBounds = new Vector3(pos.x + (size * 10), pos.y, (pos.z + (size * 10)) + (zOffset * 2));
        }

        /// <summary>
        /// Adjusts the camera to be in bounds if it leaves 
        /// </summary>
        private void CheckBounds()
        {
            Vector3 newPos = transform.position;
            if (transform.position.x < minBounds.x) newPos.x = minBounds.x;
            else if (transform.position.x > maxBounds.x) newPos.x = maxBounds.x;

            if (transform.position.z < minBounds.z) newPos.z = minBounds.z;
            else if (transform.position.z > maxBounds.z) newPos.z = maxBounds.z;

            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        }
    }
}