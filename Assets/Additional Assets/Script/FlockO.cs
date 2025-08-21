//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Jobs;
//using UnityEngine.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using System.Linq;


//public class Flock : MonoBehaviour
//{
//	[Header("Spawn Setup")]
//	[SerializeField] private FlockUnit flockUnitPrefab;
//	[SerializeField] private int flockSize;
//	[SerializeField] private Vector3 spawnBounds;

//	[Header("Speed Setup")]
//	[Range(0, 10)]
//	[SerializeField] private float _minSpeed;
//	public float minSpeed { get { return _minSpeed; } }
//	[Range(0, 10)]
//	[SerializeField] private float _maxSpeed;
//	public float maxSpeed { get { return _maxSpeed; } }


//	[Header("Detection Distances")]

//	[Range(0, 20)]
//	[SerializeField] private float _cohesionDistance;
//	public float cohesionDistance { get { return _cohesionDistance; } }

//	[Range(0, 10)]
//	[SerializeField] private float _avoidanceDistance;
//	public float avoidanceDistance { get { return _avoidanceDistance; } }

//	[Range(0, 10)]
//	[SerializeField] private float _aligementDistance;
//	public float aligementDistance { get { return _aligementDistance; } }

//	[Range(0, 10)]
//	[SerializeField] private float _obstacleDistance;
//	public float obstacleDistance { get { return _obstacleDistance; } }

//	[Range(0, 100)]
//	[SerializeField] private float _boundsDistance;
//	public float boundsDistance { get { return _boundsDistance; } }


//	[Header("Behaviour Weights")]

//	[Range(0, 10)]
//	[SerializeField] private float _cohesionWeight;
//	public float cohesionWeight { get { return _cohesionWeight; } }

//	[Range(0, 10)]
//	[SerializeField] private float _avoidanceWeight;
//	public float avoidanceWeight { get { return _avoidanceWeight; } }

//	[Range(0, 10)]
//	[SerializeField] private float _aligementWeight;
//	public float aligementWeight { get { return _aligementWeight; } }

//	[Range(0, 10)]
//	[SerializeField] private float _boundsWeight;
//	public float boundsWeight { get { return _boundsWeight; } }

//	[Range(0, 100)]
//	[SerializeField] private float _obstacleWeight;
//	public float obstacleWeight { get { return _obstacleWeight; } }

//	public FlockUnit[] allUnits { get; set; }


//    Vector3 center = Vector3.zero;
//	Vector3 whaleMouthPosition = Vector3.zero;
//    private float maxDistance = 0f;
//	private int HuntedFish = 0;


//    private float xx;
//	private Vector3 xxx;
//    private float timer = 0f;
//    private Vector3 uu;
//    private float cc;

//	private Transform sharkTransform;

//	private void Start()
//	{
//		GenerateUnits();
//        cc = _boundsDistance;
//		sharkTransform = GameObject.FindGameObjectWithTag("Obstacle")?.transform;
//	}



//	private void Update()
//	{
//		Vector3 sharkPosition = Vector3.zero;
//		if (sharkTransform != null)
//		{
//			sharkPosition = sharkTransform.position;
//		}


	
//        // timer += Time.deltaTime;
//        // xxx = FindObjectOfType<tt>().uplll();
//        // if (timer >= 5f) {
//        // transform.position = new Vector3(xxx.x, xxx.y+3f, xxx.z);
//		// _cohesionWeight += 0.0003f;
//		// _boundsDistance -= 0.003f;
//        // hasMoved = true;
//        // }
//        // uu = FindObjectOfType<NewBehaviourScript>().upa();
//        // xx = (uu - transform.position).sqrMagnitude;

//        // if (xx <= 70)
//        // {
//        //     _boundsDistance = 10;
//        // }else{
//        //     _boundsDistance = cc;
//        // }
//        // Debug.Log(xx);
//		// for (int i = 0; i < allUnits.Length; i++)
//        // {
//        //     center += allUnits[i].transform.position;
//        // }
//        // center /= allUnits.Length;

//        // for (int i = 0; i < allUnits.Length; i++)
//        // {
//        //     float distance = Vector3.Distance(allUnits[i].transform.position, center);
//        //     if(distance > maxDistance){
//        //         maxDistance = distance;
//        //     }
//        // }
//        //Debug.Log( "Swarm radius is " + maxDistance);

//		// whaleMouthPosition = FindObjectOfType<numberOfHuntedFish>().MouthPosition();
//		// for (int i = 0; i < allUnits.Length; i++)
//        // {
//        //     float distance = Vector3.Distance(allUnits[i].transform.position, whaleMouthPosition);
//        //     if(distance < 1f){
//        //         HuntedFish++;
//        //     }
//        // }

//		// Debug.Log(HuntedFish);

//		NativeArray<Vector3> unitForwardDirections = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
//		NativeArray<Vector3> unitCurrentVelocities = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
//		NativeArray<Vector3> unitPositions = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
//		NativeArray<Vector3> cohesionNeighbours = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
//		NativeArray<Vector3> avoidanceNeighbours = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
//		NativeArray<Vector3> aligementNeighbours = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
//		NativeArray<Vector3> aligementNeighboursDirecions = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
//		NativeArray<float> allUnitsSpeeds = new NativeArray<float>(allUnits.Length, Allocator.TempJob);
//		NativeArray<float> neighbourSpeeds = new NativeArray<float>(allUnits.Length, Allocator.TempJob);

//		for (int i = 0; i < allUnits.Length; i++){
//			if (allUnits[i]){
					
//					unitForwardDirections[i] = allUnits[i].myTransform.forward;
//					unitCurrentVelocities[i] = allUnits[i].currentVelocity;
//					unitPositions[i] = allUnits[i].myTransform.position;
//					cohesionNeighbours[i] = Vector3.zero;
//					avoidanceNeighbours[i] = Vector3.zero;
//					aligementNeighbours[i] = Vector3.zero;
//					aligementNeighboursDirecions[i] = Vector3.zero;
//					allUnitsSpeeds[i] = allUnits[i].speed;
//					neighbourSpeeds[i] = 0f;

//		}}

//		MoveJob moveJob = new MoveJob
//		{
//			unitForwardDirections = unitForwardDirections,
//			unitCurrentVelocities = unitCurrentVelocities,
//			unitPositions = unitPositions,
//			cohesionNeighbours = cohesionNeighbours,
//			avoidanceNeighbours = avoidanceNeighbours,
//			aligementNeighbours = aligementNeighbours,
//			aligementNeighboursDirecions = aligementNeighboursDirecions,
//			allUnitsSpeeds = allUnitsSpeeds,
//			neighbourSpeeds = neighbourSpeeds,
//			cohesionDistance = cohesionDistance,
//			avoidanceDistance = avoidanceDistance,
//			aligementDistance = aligementDistance,
//			boundsDistance = boundsDistance,
//			obstacleDistance = obstacleDistance,
//			cohesionWeight = cohesionWeight,
//			avoidanceWeight = avoidanceWeight,
//			aligementWeight = aligementWeight,
//			boundsWeight = boundsWeight,
//			obstacleWeight = obstacleWeight,
//			fovAngle = flockUnitPrefab.FOVAngle,
//			minSpeed = minSpeed,
//			maxSpeed = maxSpeed,
//			smoothDamp = flockUnitPrefab.smoothDamp,
//			flockPosition = transform.position,
//			deltaTime = Time.deltaTime,
//			sharkPosition = sharkPosition,
//			sharkAvoidanceDistance = 15f, // Distance where fish start to avoid shark
//			sharkAvoidanceStrength = 5f  
//		};

//		JobHandle handle = moveJob.Schedule(allUnits.Length, 5);
//		handle.Complete();
//		List<FlockUnit> arrayToList = new List<FlockUnit>(allUnits);
//		arrayToList = allUnits.ToList();
//		int numRemoved = 0;
//		for (int i = 0; i < allUnits.Length; i++)
//		{
//			if (allUnits[i]){
//				if (allUnits[i].tag == "Untagged"){allUnits[i].tag = "Stunned";}
//				if ((allUnits[i].tag == "Stunned")){
//					//
//					int x = UnityEngine.Random.Range (0, 50);
//					if (x == 0){
//					allUnits[i].GetComponent<Rigidbody>().isKinematic = false;
//					allUnits[i].GetComponent<Animator>().SetBool("stunned", false);					
//					allUnits[i].tag = "Fish";


//					}else{

//					allUnits[i].GetComponent<Rigidbody>().isKinematic = true;
//					allUnits[i].GetComponent<Animator>().SetBool("stunned", true);	}				

//					// if distance to nose < amount, delete
//				}else{
//				allUnits[i].myTransform.forward = unitForwardDirections[i];
//				allUnits[i].myTransform.position = unitPositions[i];
//				allUnits[i].currentVelocity = unitCurrentVelocities[i];
//				allUnits[i].speed = allUnitsSpeeds[i];}}
//		}
//		allUnits = arrayToList.ToArray();


//		unitForwardDirections.Dispose();
//		unitCurrentVelocities.Dispose();
//		unitPositions.Dispose();
//		cohesionNeighbours.Dispose();
//		avoidanceNeighbours.Dispose();
//		aligementNeighbours.Dispose();
//		aligementNeighboursDirecions.Dispose();
//		allUnitsSpeeds.Dispose();
//		neighbourSpeeds.Dispose();

//	}

//	private void GenerateUnits()
//	{
//		allUnits = new FlockUnit[flockSize];
//		for (int i = 0; i < flockSize; i++)
//		{
//			var randomVector = UnityEngine.Random.insideUnitSphere;
//			randomVector = new Vector3(randomVector.x * spawnBounds.x, randomVector.y * spawnBounds.y, randomVector.z * spawnBounds.z);
//			var spawnPosition = transform.position + randomVector;
//			var rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
//			allUnits[i] = Instantiate(flockUnitPrefab, spawnPosition, rotation);
//			allUnits[i].tag = "Fish";

//			allUnits[i].AssignFlock(this);
//			allUnits[i].InitializeSpeed(UnityEngine.Random.Range(minSpeed, maxSpeed));
//		}
//	}

//	    public float get_swarm_radius(){
//        for (int i = 0; i < allUnits.Length; i++)
//        {
//            center += allUnits[i].transform.position;
//        }
//        center /= allUnits.Length;

//        for (int i = 0; i < allUnits.Length; i++)
//        {
//            float distance = Vector3.Distance(allUnits[i].transform.position, center);
//            if(distance > maxDistance){
//                maxDistance = distance;
//            }
//        }
//        Debug.Log( "Swarm radius is " + maxDistance);
//        return maxDistance;
//     }
//}

//[BurstCompile]
//public struct MoveJob : IJobParallelFor
//{
//	public NativeArray<Vector3> unitCurrentVelocities;

//	[NativeDisableParallelForRestriction]
//	public NativeArray<Vector3> unitForwardDirections;
//	[NativeDisableParallelForRestriction]
//	public NativeArray<Vector3> unitPositions;
//	[NativeDisableParallelForRestriction]
//	public NativeArray<Vector3> cohesionNeighbours;
//	[NativeDisableParallelForRestriction]
//	public NativeArray<Vector3> avoidanceNeighbours;
//	[NativeDisableParallelForRestriction]
//	public NativeArray<Vector3> aligementNeighbours;
//	[NativeDisableParallelForRestriction]
//	public NativeArray<Vector3> aligementNeighboursDirecions;
//	[NativeDisableParallelForRestriction]
//	public NativeArray<float> allUnitsSpeeds;
//	[NativeDisableParallelForRestriction]
//	public NativeArray<float> neighbourSpeeds;

//	public Vector3 flockPosition;
//	public float cohesionDistance;
//	public float avoidanceDistance;
//	public float aligementDistance;
//	public float boundsDistance;
//	public float obstacleDistance;
//	public float cohesionWeight;
//	public float avoidanceWeight;
//	public float aligementWeight;
//	public float boundsWeight;
//	public float obstacleWeight;
//	public float fovAngle;
//	public float minSpeed;
//	public float maxSpeed;
//	public float smoothDamp;
//	public float deltaTime;
//	public int stunTimer;

//	public Vector3 sharkPosition;
//	public float sharkAvoidanceDistance;
//	public float sharkAvoidanceStrength;



//	public void Execute(int index)
//	{
//		//Find Neighbours
//		int cohesionIndex = 0;
//		int avoidanceIndex = 0;
//		int aligementIndex = 0;
//		for (int i = 0; i < unitPositions.Length; i++)
//		{
//			Vector3 currentUnitPosition = unitPositions[index];
//			Vector3 currentNeighbourPosition = unitPositions[i];
//			Vector3 currentNeighbourDirection = unitForwardDirections[i];
//			if (currentUnitPosition != currentNeighbourPosition)
//			{
//				float currentDistanceToNeighbourSqr = Vector3.SqrMagnitude(currentUnitPosition - currentNeighbourPosition);
//				if (currentDistanceToNeighbourSqr < cohesionDistance * cohesionDistance)
//				{
//					cohesionNeighbours[cohesionIndex] = currentNeighbourPosition;
//					neighbourSpeeds[cohesionIndex] = allUnitsSpeeds[i];
//					cohesionIndex++;
//				}
//				if (currentDistanceToNeighbourSqr < avoidanceDistance * avoidanceDistance)
//				{
//					avoidanceNeighbours[avoidanceIndex] = currentNeighbourPosition;
//					avoidanceIndex++;
//				}
//				if (currentDistanceToNeighbourSqr < aligementDistance * aligementDistance)
//				{
//					aligementNeighbours[aligementIndex] = currentNeighbourPosition;
//					aligementNeighboursDirecions[aligementIndex] = currentNeighbourDirection;
//					aligementIndex++;
//				}
//			}
//		}

//		//Calculate speed
//		float speed = 0f;
//		if (cohesionNeighbours.Length != 0)
//		{
//			for (int i = 0; i < cohesionNeighbours.Length; i++)
//			{
//				speed += neighbourSpeeds[i];
//			}
//			speed /= cohesionNeighbours.Length;

//		}
//		speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

//		//Calculate cohesion
//		Vector3 cohesionVector = Vector3.zero;
//		if (cohesionNeighbours.Length != 0)
//		{
//			int cohesionNeighbourdInFOV = 0;
//			for (int i = 0; i <= cohesionIndex; i++)
//			{
//				if (IsInFov(unitForwardDirections[index], unitPositions[index], cohesionNeighbours[i], fovAngle) && cohesionNeighbours[i] != Vector3.zero)
//				{
//					cohesionNeighbourdInFOV++;
//					cohesionVector += cohesionNeighbours[i];
//				}
//			}
//			cohesionVector /= cohesionNeighbourdInFOV;
//			cohesionVector -= unitPositions[index];
//			cohesionVector = cohesionVector.normalized * cohesionWeight;
//		}

//		//Calculate avoidance
//		Vector3 avoidanceVector = Vector3.zero;
//		if (avoidanceNeighbours.Length != 0)
//		{
//			int avoidanceNeighbourdInFOV = 0;
//			for (int i = 0; i <= avoidanceIndex; i++)
//			{
//				if (IsInFov(unitForwardDirections[index], unitPositions[index], avoidanceNeighbours[i], fovAngle) && avoidanceNeighbours[i] != Vector3.zero)
//				{
//					avoidanceNeighbourdInFOV++;
//					avoidanceVector += (unitPositions[index] - avoidanceNeighbours[i]);
//				}
//			}

//			avoidanceVector /= avoidanceNeighbourdInFOV;
//			avoidanceVector = avoidanceVector.normalized * avoidanceWeight;
//		}

//		//Calculate aligement
//		Vector3 aligementVector = Vector3.zero;
//		if (aligementNeighbours.Length != 0)
//		{
//			int aligementNeighbourdInFOV = 0;
//			for (int i = 0; i <= aligementIndex; i++)
//			{
//				if (IsInFov(unitForwardDirections[index], unitPositions[index], aligementNeighbours[i], fovAngle) && aligementNeighbours[i] != Vector3.zero)
//				{
//					aligementNeighbourdInFOV++;
//					aligementVector += aligementNeighboursDirecions[i].normalized;
//				}
//			}
//			aligementVector /= aligementNeighbourdInFOV;
//			aligementVector = aligementVector.normalized * aligementWeight;
//		}

//		//Calculate bounds
//		Vector3 offsetToCenter = flockPosition - unitPositions[index];
//		bool isNearBound = offsetToCenter.magnitude >= boundsDistance * 0.9f;
//		Vector3 boundsVector = isNearBound ? offsetToCenter.normalized : Vector3.zero;
//		boundsVector *= boundsWeight;


//		//Move Unit
//		Vector3 currentVelocity = unitCurrentVelocities[index];
//		Vector3 moveVector = cohesionVector + avoidanceVector + aligementVector + boundsVector;

//		moveVector = Vector3.SmoothDamp(unitForwardDirections[index], moveVector, ref currentVelocity, smoothDamp, 10000, deltaTime);

//		moveVector = moveVector.normalized * speed;
//		if(moveVector == Vector3.zero)
//		{
//			moveVector = unitForwardDirections[index];
//		}
//		unitPositions[index] = unitPositions[index] + moveVector * deltaTime;
//		unitForwardDirections[index] = moveVector.normalized;
//		allUnitsSpeeds[index] = speed;
//		unitCurrentVelocities[index] = currentVelocity;

//		// Calculate shark avoidance
//		Vector3 sharkAvoidanceVector = Vector3.zero;
//		float distanceToShark = Vector3.Distance(unitPositions[index], sharkPosition);

//		if (distanceToShark < sharkAvoidanceDistance)
//		{
//			sharkAvoidanceVector = (unitPositions[index] - sharkPosition).normalized * sharkAvoidanceStrength;
//}



//	}
//	private bool IsInFov(Vector3 forward, Vector3 unitPosition, Vector3 targetPosition, float angle)
//	{
//		return Vector3.Angle(forward, targetPosition - unitPosition) <= angle;
//	}

//}