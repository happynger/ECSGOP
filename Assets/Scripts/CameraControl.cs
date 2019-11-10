using UnityEngine;

public class CameraControl : MonoBehaviour
{
	private Vector3 velocity = Vector3.zero;
	private Camera this_Camera;
	private float zoom = 0f;
	private float xv = 0f;
	private float yv = 0f;

	public float scrollSensitivity = 15.0f;
	public float smoothTime = 0.5f;
	public float zoomSpeed = 5f;
	public float zoomMax = 20f;
	public float zoomMin = 2f;

	private void Awake()
	{
		this_Camera = GetComponent<Camera>();
		zoom = this_Camera.orthographicSize;
	}

	private void Update()
	{
		#region SmoothPosition
		xv = Input.GetAxis("Horizontal");
		yv = Input.GetAxis("Vertical");

		Vector3 EndPoint = new Vector3(transform.position.x + xv, transform.position.y + yv, transform.position.z);
		transform.position = Vector3.SmoothDamp(transform.position, EndPoint, ref velocity, smoothTime);
		#endregion

		#region SmoothZoom
		zoom -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
		zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
		#endregion
	}

	private void LateUpdate() => this_Camera.orthographicSize = Mathf.Lerp(this_Camera.orthographicSize, zoom, Time.deltaTime * zoomSpeed);
}
