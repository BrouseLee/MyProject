using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlockUnit : MonoBehaviour
{
    void OnTriggerEnter(Collider CollisionObject)
    {
        if (CollisionObject.tag == "ThresherShark")
        {
            Vector3 targetDirection = transform.position - CollisionObject.transform.position;
            transform.rotation = Quaternion.LookRotation(targetDirection);
            transform.position = Vector3.MoveTowards(transform.position, CollisionObject.transform.position, -1 * 2 * Time.deltaTime);
        }
    }
    [SerializeField] private float _FOVAngle;
    public float FOVAngle { get { return _FOVAngle; } }

    [SerializeField] private float _smoothDamp;
    public float smoothDamp { get { return _smoothDamp; } }

    private Vector3 _currentVelocity;
    public Vector3 currentVelocity { get; set; }

    private Flock assignedFlock;
    public float speed { get; set; }

    public Transform myTransform { get; set; }

    private void Awake()
    {
        myTransform = transform;
    }



    public void AssignFlock(Flock flock)
    {
        assignedFlock = flock;
    }

    public void InitializeSpeed(float speed)
    {
        this.speed = speed;
    }

}