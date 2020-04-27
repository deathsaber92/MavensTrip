using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class ProceduralGenerator : MonoBehaviour
{
    public GameObject[] challengesToSpawn;
    public GameObject jumpCounter;
    public GameObject platformParentObject;
    public Transform startTransform;
    public Transform cameraStartTransform;
    public float explosionFillSpeedIncreaseSensitivity = 0.05f;
    public float explosionFillSpeedIncreaseMaxTime = 1f;
    public float explosionFillSpeedIncreaseMinTime = 0.15f;

    public int spawnGapSize;

    public static ReactiveProperty<bool> isSpawnTime = new ReactiveProperty<bool>(false);
    public static ReactiveProperty<int> activeChallengesSpawned = new ReactiveProperty<int>(0);
    public static ReactiveProperty<int> jumpsTaken = new ReactiveProperty<int>(0);
    public static float _explosionTimeInterval = 2f;
    public static float explosionCircleFillSpeed = 0.5f;
    public static bool isFirstPlatformSpawn = true;

    private static GameObject[] instancesOfChallengesToSpawnArray;
    private static Transform _lastSpawnTransform;
    private Collider[] _nextLevelColliders;
    private static Collider _nextLevelChallengeCollider;
    private static Collider _lastLevelChallengeCollider;
    private static int _randomizer = 0;
    private static int _reRandomizer = 0;
    private static int _randomizerCollectible = 0;
    private static float _lastElementSizeOnX;
    private static float _nextElementSizeOnX;
    private static Vector3 spawnPositionVector;
    private static GameObject _gameObjectToGrab;
    private static GameObject _objectToBePlaced;
    private static GameObject _separatorToBePlaced;
    private static Queue<GameObject> _queue1 = new Queue<GameObject>();
    private static Queue<GameObject> _queue2 = new Queue<GameObject>();
    private static Queue<GameObject> _queueSeparators = new Queue<GameObject>();
    public static GameObject currentPlatform;

    //diamonds
    public int diamondsSpawnNumber;
    public float diamondsMultiplicationFactor;
    public GameObject collectiblePrefab;
    public Transform collectiblesParent;

    private Transform[] _collectibles; //children with tag collectibles in all spaned playtforms
    private GameObject[] _diamondObjectsArray; //add diamonds here on instantiate
    private static Queue<GameObject> queueOfDiamonds;

    private static GameObject _nextDiamondToUse;
    private static GameObject _collectibleToMove;
    private static Transform _transformToMoveTo;
    private static Transform _collectiblesParentLocal;

    //relic charges
    public int relicChargesSpawnNumber;
    public GameObject relicChargeCollectiblePrefab;
    public Transform relicChargesParent;

    private Transform[] _charges;
    private GameObject[] _relicChargesArray;
    private static Queue<GameObject> _queueOfCharges;

    private static GameObject _nextChargeToUse;
    private static GameObject _chargeToMove;
    private static Transform _chargeTransformToMoveTo;
    private static Transform _chargesParentLocal;
    public static float numberOfDiamondsToSpawn;
    public float explosionTimeMax;
    public float explosionTimeMin;   
    public float explosionTimeStep;    

    private void Awake()
    {        
        jumpsTaken.Value = 0;
        explosionCircleFillSpeed = explosionFillSpeedIncreaseMinTime;

        _queue1 = new Queue<GameObject>();
        _queue2 = new Queue<GameObject>();
        _queueSeparators = new Queue<GameObject>();
        _randomizer = 0;
        _reRandomizer = 0;
        _randomizerCollectible = 0;
        _explosionTimeInterval = explosionTimeMax;      
        isFirstPlatformSpawn = true;
    }

    void Start()
    {
        GameManager.set_cameraSpawnTransform = cameraStartTransform;
        FindObjectOfType<DustStormAdvance>().AssignStartDataSets();

        for (int i = 0; i < 15; i++)
        {
            var separator = Instantiate(jumpCounter);
            _queueSeparators.Enqueue(separator);
            separator.SetActive(false);
        }

        _collectiblesParentLocal = collectiblesParent;
        _chargesParentLocal = relicChargesParent;

        InitializeCharges();
        InitializeCollectibles();
       
        _queue1.Clear();
        _queue2.Clear();
        activeChallengesSpawned.Value = 0;
        instancesOfChallengesToSpawnArray = new GameObject[challengesToSpawn.Length];
        _nextLevelColliders = new Collider[challengesToSpawn.Length];
        InstantiateAllChallenges();
        CreateQueueOfUnusedObjects();
        ElementPlaceAndSetActive();
        ElementPlaceAndSetActive();
        ElementPlaceAndSetActive();
        ElementPlaceAndSetActive();
        ElementPlaceAndSetActive();
        ElementPlaceAndSetActive();

        jumpsTaken
            .Where(_ => explosionCircleFillSpeed <= explosionFillSpeedIncreaseMaxTime)
            .Subscribe(_ => IncreaseExplosionSpeed())
            .AddTo(this);

        isSpawnTime
            .Where(isSpawnTime => isSpawnTime == true)
            .Subscribe(_ =>
            {
                ElementPlaceAndSetActive();
                ElementPlaceAndSetActive();
                isSpawnTime.Value = false;
            })
            .AddTo(this);

        jumpsTaken
             .Where(_ => _explosionTimeInterval > explosionTimeMin)
             .Subscribe(_ => { _explosionTimeInterval -= explosionTimeStep;})
             .AddTo(this);
    }

    private void Update()
    {
        ////Debug.Log(activeChallengesSpawned);
        ////Debug.Log(_queue1.Count + " and " + _queue2.Count);
    }

    private void IncreaseExplosionSpeed()
    {
        explosionCircleFillSpeed = explosionCircleFillSpeed + explosionFillSpeedIncreaseSensitivity;
    }

    /// <summary>
    /// Creates the queue of unused objects, ready to be grabbed for spawn
    /// </summary>
    private void CreateQueueOfUnusedObjects()
    {
        for (int i = 0; i < instancesOfChallengesToSpawnArray.Length; i += 2)
        {
            GameObject obj = instancesOfChallengesToSpawnArray[i];
            _randomizer = UnityEngine.Random.Range(0, 2);

            if (!obj.activeSelf)
            {
                AddToRandomQueueAtStart(_randomizer, i);
            }
        }

        for (int i = 1; i < instancesOfChallengesToSpawnArray.Length; i += 2)
        {
            GameObject obj = instancesOfChallengesToSpawnArray[i];
            _randomizer = UnityEngine.Random.Range(0, 2);

            if (!obj.activeSelf)
            {
                AddToRandomQueueAtStart(_randomizer, i);
            }
        }
    }

    /// <summary>
    /// Spawns all the current challenges from the public array
    /// </summary>
    private void InstantiateAllChallenges()
    {
        _collectibles = new Transform[challengesToSpawn.Length];
        _charges = new Transform[challengesToSpawn.Length];
        for (int i = 0; i < challengesToSpawn.Length; i++)
        {
            GameObject instanceOfSpawn = Instantiate(challengesToSpawn[i]);
            Transform transformOfInstance = instanceOfSpawn.transform;
            for (int j = 0; j < instanceOfSpawn.transform.childCount; j++)
            {
                transformOfInstance = instanceOfSpawn.transform.GetChild(j);
                if (transformOfInstance.tag == "collectibles")
                {
                    _collectibles[i] = transformOfInstance;                    
                }
                if (transformOfInstance.tag == "relic_charges")
                {
                    _charges[i] = transformOfInstance;
                }
            }
            instanceOfSpawn.transform.parent = platformParentObject.transform;
            _nextLevelColliders[i] = instanceOfSpawn.transform.GetComponent<Collider>();
            instancesOfChallengesToSpawnArray[i] = instanceOfSpawn;
            instanceOfSpawn.SetActive(false);
            _randomizer = UnityEngine.Random.Range(0.0F, 1.0F) < 0.5F ? 0 : 1;
        }
    }

    /// <summary>
    /// Adds platform to random queue
    /// </summary>
    /// <param name="queueIndex">The queue random index</param>
    /// <param name="spawnIndex">The current instantiate index</param>
    private void AddToRandomQueueAtStart(int queueIndex, int spawnIndex)
    {
        switch (queueIndex)
        {
            case 0:
                _queue1.Enqueue(instancesOfChallengesToSpawnArray[spawnIndex]);
                break;

            case 1:
                _queue2.Enqueue(instancesOfChallengesToSpawnArray[spawnIndex]);
                break;
        }
    }

    /// <summary>
    /// Adds object back to random queue on disable
    /// </summary>
    /// <param name="queueIndex"></param>
    /// <param name="objectToQueue"></param>
    private static void AddToRandomQueueOnDisable(int queueIndex, GameObject objectToQueue)
    {
        switch (queueIndex)
        {
            case 0:
                objectToQueue.SetActive(false);
                _queue1.Enqueue(objectToQueue);
                ////Debug.Log("Enqueued object: " + objectToQueue.name);
                break;

            case 1:
                objectToQueue.SetActive(false);
                _queue2.Enqueue(objectToQueue);
                ////Debug.Log("Enqueued object: " + objectToQueue.name);
                break;
        }
    }

    /// <summary>
    /// Grabs from a random queue and returns the next object to become visible
    /// </summary>
    /// <param name="queueIndex"></param>
    /// <returns></returns>
    private GameObject GrabFromRandomQueue(int queueIndex)
    {
        ////Debug.Log("Queue1 is: " + _queue1.Count);
        ////Debug.Log("Queue2 is: " + _queue1.Count);

        switch (queueIndex)
        {
            case 0:
                if (_queue1.Count != 0)
                {
                    _gameObjectToGrab = _queue1.Dequeue();
                    ////Debug.Log("Dequeued object: " + _gameObjectToGrab.name);
                }
                else
                {
                    goto case 1;
                }
                break;

            case 1:
                if (_queue2.Count != 0)
                {
                    _gameObjectToGrab = _queue2.Dequeue();
                    ////Debug.Log("Dequeued object: " + _gameObjectToGrab.name);
                }
                else
                {
                    goto case 0;
                }                    
                break;
        }

        return _gameObjectToGrab;
    }

    /// <summary>
    /// Returns the next element to become bisible as GameObject
    /// </summary>
    /// <returns></returns>
    private GameObject GetNextVisibleElement()
    {
        _randomizer = UnityEngine.Random.Range(0, 2);
        return GrabFromRandomQueue(_randomizer);
    }

    /// <summary>
    /// Spawn a new platform
    /// </summary>
    private void ElementPlaceAndSetActive()
    {
        ////Debug.Log("Active challenges spawned: " + activeChallengesSpawned.Value);
        numberOfDiamondsToSpawn = (DustStormAdvance.realMoveTowardsSpeed + 1) * DustStormAdvance.realMoveTowardsSpeed * diamondsMultiplicationFactor;

        if (activeChallengesSpawned.Value < 15)
        {
            _objectToBePlaced = GetNextVisibleElement();
            if (_objectToBePlaced == null)
                _objectToBePlaced = GetNextVisibleElement();
            _objectToBePlaced.SetActive(true);   
            ////Debug.Log("Making active objectToBePlaced: " + _objectToBePlaced.name);
            int index = _objectToBePlaced.transform.GetSiblingIndex();
            for (int i = 0; i < numberOfDiamondsToSpawn; i++)
            {
                MoveCollectibleToPositions(index);                
            }

            if (jumpsTaken.Value % 7 == 0 && jumpsTaken.Value >= 7)
            {
                MoveChargesToPosition(index);
                if (jumpsTaken.Value >= 30)
                {
                    MoveChargesToPosition(index);
                }

                if (jumpsTaken.Value >= 50)
                {
                    MoveChargesToPosition(index);
                }
            }

            _nextLevelChallengeCollider = _nextLevelColliders[index];
            _nextElementSizeOnX = _nextLevelChallengeCollider.bounds.size.x;
            ////Debug.Log("_nextElementSizeOnX: " + "of " + nextLevelChallengeCollider.name + ": " + _nextElementSizeOnX);           

            PlaceGameObjectToNextPosition();
            activeChallengesSpawned.Value += 1;

            _lastSpawnTransform = _objectToBePlaced.transform;
            _lastLevelChallengeCollider = _nextLevelChallengeCollider;
            _lastElementSizeOnX = _lastLevelChallengeCollider.bounds.size.x;

            _separatorToBePlaced = _queueSeparators.Dequeue();
            _separatorToBePlaced.SetActive(true);
            _nextElementSizeOnX = _separatorToBePlaced.GetComponent<Collider>().bounds.size.x;

            if (isFirstPlatformSpawn)
            {
                spawnPositionVector = new Vector3(0, 0, 0);
                isFirstPlatformSpawn = false;
            }
            else
            {
                spawnPositionVector = new Vector3(_lastSpawnTransform.position.x + spawnGapSize + _lastElementSizeOnX / 2 + _nextElementSizeOnX / 2, 0, 0);
            }            

            _separatorToBePlaced.SetActive(true);
            _separatorToBePlaced.transform.position = spawnPositionVector;
            _separatorToBePlaced.transform.SetParent(_objectToBePlaced.transform);
        }  
    }

    /// <summary>
    /// Adds gap size from the last spawned position and moved the object to the next position
    /// </summary>
    private void PlaceGameObjectToNextPosition()
    {
        if (_lastSpawnTransform != null)
        {
            spawnPositionVector = new Vector3(_lastSpawnTransform.position.x + spawnGapSize + _lastElementSizeOnX / 2 + _nextElementSizeOnX / 2, 0, 0);
        }
        else
        {
            spawnPositionVector = startTransform.transform.position;
        }

        _objectToBePlaced.transform.position = spawnPositionVector;
        ////Debug.Log("Placed object: " + _objectToBePlaced.name);
    }

    public static void DeactivateChallenge(GameObject deactivatedObject)
    {
        _reRandomizer = UnityEngine.Random.Range(0.0F, 1.0F) < 0.5F ? 0 : 1;

        AddToRandomQueueOnDisable(_reRandomizer, deactivatedObject);
        ////Debug.Log("Deactivated object: " + DestroyCollider.deactivatedObject);
        ////Debug.Log("Active challenges spawned value before decrease: " + activeChallengesSpawned.Value);
        activeChallengesSpawned.Value -= 1;
        ////Debug.Log("Active challenges spawned value after decrease: " + activeChallengesSpawned.Value);
    }

    /// <summary>
    /// Initializing all necesary components for collectible generator
    /// </summary>
    private void InitializeCollectibles()
    {
        _diamondObjectsArray = new GameObject[diamondsSpawnNumber];
        queueOfDiamonds = new Queue<GameObject>();

        for (int i = 0; i < diamondsSpawnNumber; i++)
        {
            _diamondObjectsArray[i] = Instantiate(collectiblePrefab);
            _diamondObjectsArray[i].transform.SetParent(_collectiblesParentLocal);
            queueOfDiamonds.Enqueue(_diamondObjectsArray[i]);
            _diamondObjectsArray[i].SetActive(false);
        }
    }
     
    /// <summary>
    /// Initializing all necesary components for charges generator
    /// </summary>
    private void InitializeCharges()
    {
        _relicChargesArray = new GameObject[relicChargesSpawnNumber];
        _queueOfCharges = new Queue<GameObject>();

        for (int i = 0; i < relicChargesSpawnNumber; i++)
        {
            _relicChargesArray[i] = Instantiate(relicChargeCollectiblePrefab);
            _relicChargesArray[i].transform.SetParent(_chargesParentLocal);
            _queueOfCharges.Enqueue(_relicChargesArray[i]);
            _relicChargesArray[i].SetActive(false);
        }
    }

    private void MoveCollectibleToPositions(int index)
    {
        if (queueOfDiamonds.Count != 0)
        {   
            _collectibleToMove = queueOfDiamonds.Dequeue();
            _collectibleToMove.SetActive(true);
            _randomizerCollectible = UnityEngine.Random.Range(0, _collectibles[index].transform.childCount - 1); // Generate random based on how many possible spawn locations are available
            _transformToMoveTo = _collectibles[index].transform.GetChild(_randomizerCollectible);  // Get the transform you want to move to and parent to
           
            if (_transformToMoveTo.transform.childCount != 0)
            {
                _randomizerCollectible = UnityEngine.Random.Range(0, _collectibles[index].transform.childCount - 1); // Generate random based on how many possible spawn locations are available
                _transformToMoveTo = _collectibles[index].transform.GetChild(_randomizerCollectible);  // Get the transform you want to move to and parent to
            }
           

            if (_transformToMoveTo.transform.childCount == 0)
            {
                _collectibleToMove.transform.position = _transformToMoveTo.position; //Move the collectible there        
                _collectibleToMove.transform.SetParent(_transformToMoveTo);               
            }    
            else
            {
                queueOfDiamonds.Enqueue(_collectibleToMove);
                _collectibleToMove.SetActive(false);
            }
        }
    }

    private void MoveChargesToPosition(int index)
    {
        if (_queueOfCharges.Count != 0)
        {
            _chargeToMove = _queueOfCharges.Dequeue();
            _chargeToMove.SetActive(true);
            _randomizerCollectible = UnityEngine.Random.Range(0, _charges[index].transform.childCount - 1); // Generate random based on how many possible spawn locations are available
            _chargeTransformToMoveTo = _charges[index].transform.GetChild(_randomizerCollectible);  // Get the transform you want to move to and parent to

            if (_chargeTransformToMoveTo.transform.childCount != 0)
            {
                _randomizerCollectible = UnityEngine.Random.Range(0, _charges[index].transform.childCount - 1); // Generate random based on how many possible spawn locations are available
                _chargeTransformToMoveTo = _charges[index].transform.GetChild(_randomizerCollectible);  // Get the transform you want to move to and parent to
            }


            if (_chargeTransformToMoveTo.transform.childCount == 0)
            {
                _chargeToMove.transform.position = _chargeTransformToMoveTo.position; //Move the collectible there        
                _chargeToMove.transform.SetParent(_chargeTransformToMoveTo);
            }
            else
            {
                _queueOfCharges.Enqueue(_collectibleToMove);
                _chargeToMove.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Adds back to queue and disables diamond
    /// </summary>
    /// <param name="diamondToQueue"></param>
    public static void AddDiamondBackToQueue(GameObject diamondToQueue)
    {
        queueOfDiamonds.Enqueue(diamondToQueue);
        diamondToQueue.SetActive(false);
        diamondToQueue.transform.SetParent(_collectiblesParentLocal);
    }

    /// <summary>
    /// Adds the charge back to queue and disables it
    /// </summary>
    /// <param name="chargeToQueue"></param>
    public static void AddChargeBackToQueue(GameObject chargeToQueue)
    {
        _queueOfCharges.Enqueue(chargeToQueue);
        chargeToQueue.SetActive(false);
        chargeToQueue.transform.SetParent(_chargesParentLocal);
    }

    public static void AddJumpCounterbackToQueue(GameObject jumpCounter)
    {
        _queueSeparators.Enqueue(jumpCounter);
        jumpCounter.SetActive(false);
    }
}