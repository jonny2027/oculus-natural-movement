﻿using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class OVRNaturalMovementController : MonoBehaviour
{
    OVRCameraRig rig;

    Quaternion lastRotation;
    Vector3 lastPosition;
	
	void Start()
	{
        rig = GetComponentInChildren<OVRCameraRig>();

        float[] a = new float[] {Ovr.Hmd.OVR_DEFAULT_NECK_TO_EYE_VERTICAL, Ovr.Hmd.OVR_DEFAULT_NECK_TO_EYE_HORIZONTAL};
        a = OVRManager.capiHmd.GetFloatArray(Ovr.Hmd.OVR_KEY_NECK_TO_EYE_DISTANCE, a);
        neckToEye = new Vector3(0, a[0], a[1]);

        playerEyeHeight = GetPlayerEyeHeight();
        rig.transform.localPosition = Vector3.up * playerEyeHeight;
	}

	public static float GetPlayerEyeHeight()
	{
		return OVRManager.capiHmd.GetFloat(Ovr.Hmd.OVR_KEY_PLAYER_HEIGHT, Ovr.Hmd.OVR_DEFAULT_PLAYER_HEIGHT);
	}

	Vector3 neckToEye;
	float playerEyeHeight;

	float velocity;

	void LateUpdate()
	{
		UpdateNaturalMovement(); 
		UpdateKeyboardMovement();
		UpdateCollider();
	}

	void UpdateKeyboardMovement()
	{
#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) transform.position += transform.rotation * Vector3.forward * Time.deltaTime * 10;
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) transform.position += transform.rotation * Vector3.left * Time.deltaTime * 10;
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) transform.position += transform.rotation * Vector3.right * Time.deltaTime * 10;
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) transform.position += transform.rotation * Vector3.back * Time.deltaTime * 10;
		transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
#endif
	}

	public void ResetRotation()
	{
		transform.rotation = Quaternion.Euler(Vector3.up * 90);
	}

	void UpdateNaturalMovement()
	{
		// compensate the vertical movement of the head made by nodding 
		var rotPositionChange = rig.centerEyeAnchor.localRotation * neckToEye - lastRotation * neckToEye;
		var rotCompensation = Mathf.Abs(rotPositionChange.y);
		
		var yChange = rig.centerEyeAnchor.localPosition.y - lastPosition.y;
		
		var desiredVelocity = (Mathf.Abs(yChange) - rotCompensation) * 5 / Time.deltaTime;	
		velocity = Mathf.Lerp(velocity, desiredVelocity, Time.deltaTime * 5);
		
		var lookDirection = rig.centerEyeAnchor.localRotation * Vector3.forward;
		var moveDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
		var move = moveDirection * velocity * Time.deltaTime;

		transform.position += transform.rotation * move;
		
		lastPosition = rig.centerEyeAnchor.localPosition;
        lastRotation = rig.centerEyeAnchor.localRotation;
	}

	void UpdateCollider()
	{
		var cc = ((CapsuleCollider)collider);
		var center = cc.center;

		// if no obstacles on the way move collider together with camera
		if (Mathf.Abs(rigidbody.velocity.x) < 0.001f && Mathf.Abs(rigidbody.velocity.z) < 0.001f)
			center = rig.centerEyeAnchor.localPosition;

        var height = rig.centerEyeAnchor.localPosition.y + playerEyeHeight;
		center.y = height / 2;
		cc.center = Vector3.Lerp(cc.center, center, Time.deltaTime);
		cc.height = cc.center.y * 2;
	} 
}
