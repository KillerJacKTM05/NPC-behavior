using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using SafeZone;

public class BaseNpcBehavior : MonoBehaviour
{
    private bool isAngry = false;
    private bool isNeutral = false;
    private bool isHappy = false;

    [Header("Npc displaying & usage Specializations")]
    public bool isStatic = false;
    public bool staticonMapAtStart = false;
    public string ifStaticSetLeaveAreaTime = "1200";
    [Range(0, 60)] public int taskDurationsMaxLimiter = 15;
    public bool Male = false;
    public bool Female = false;

    [Header("Task")]
    public bool patrol = false;
    public bool standingIdle = false;
    public bool talk = false;
    public bool goToATarget = false;
    public bool talkingWPhone = false;
    public bool sitting = false;

    [SerializeField] private Canvas emojiDisplayer;
    [SerializeField] private Image emojis;
    [SerializeField] private Animator currentAnimatorController;
    [Tooltip("if there is a kid on the npc, fill it. Else, leave it blank")]
    [SerializeField] private Animator kidController;
    [SerializeField] private RuntimeAnimatorController standardController;
    [SerializeField] private RuntimeAnimatorController kidStandardController;
    [SerializeField] private RuntimeAnimatorController haltController;
    [SerializeField] private NavMeshAgent navAgent;

    private bool isKidActive = false;
    private bool onTask = false;
    private bool staticReturning = false;
    private bool dynamicReturning = false;
    private bool halt = false;
    private bool expressionGiven = false;
    private bool angrySequence = false;
    private bool isKicked = false;
    private Coroutine npclife;
    private Transform destination = null;
    private Vector3 beforePos = Vector3.zero;
    private Vector3 currentPos = Vector3.zero;
    private Quaternion destOrientation = new Quaternion();
    private Quaternion rotBeforeExpression = new Quaternion();
    private AudioSource source;

    private int myLeaveHour = 12;
    private int myLeaveMinute = 01;
    private int myUpdatedTaskHour = 12;
    private int myUpdatedTaskMin = 01;
    private int pathfollowindex = 0;
    private float playerDistance = 100f;
    private void Start()
    {
        CheckEmotion();
        InitializeThis();
        StartMission();
    }
    private void Update()
    {
        Vector3 cameraRotation = CameraManager.Instance.GetGameCamera().transform.position;
        emojiDisplayer.transform.LookAt(cameraRotation, Vector3.up);
    }
    private void CheckEmotion()
    {
        int total = GameManager.Instance.GetGameSettings().angryIntensity + GameManager.Instance.GetGameSettings().happyIntensity + GameManager.Instance.GetGameSettings().neutralIntensity;
        int rand = Random.Range(0, total);
        if(rand >= 0 && rand < GameManager.Instance.GetGameSettings().angryIntensity)
        {
            isAngry = true;
        }
        else if(rand >= GameManager.Instance.GetGameSettings().angryIntensity && rand < total - GameManager.Instance.GetGameSettings().happyIntensity)
        {
            isNeutral = true;
        }
        else if(rand >= total - GameManager.Instance.GetGameSettings().happyIntensity && rand < total)
        {
            isHappy = true;
        }
        else
        {
            isAngry = true;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(ray);
    }
    private void ForwardVision()
    {
        RaycastHit hit;
        int playerMask = 1 << 0;
        int npcMask = 1 << 7;
        Vector3 origin = new Vector3(this.transform.position.x, 0.3f, this.transform.position.z);
        if (isHappy)
        {
            if (Physics.Raycast(origin, this.transform.forward, out hit, 1.5f, playerMask) || Physics.Raycast(origin, this.transform.forward, out hit, 1.5f, npcMask))
            {
                //this is for happy npc's player towards attitude
                if (hit.transform.CompareTag("Player"))
                {
                    Debug.Log("Vision seen. Happy");
                    if (!sitting && !standingIdle && !talk)
                    {
                        //break the movement also
                        halt = true;
                        navAgent.enabled = false;
                        currentAnimatorController.runtimeAnimatorController = haltController;
                        if (isKidActive)
                        {
                            kidController.runtimeAnimatorController = haltController;
                        }
                    }
                    else if (sitting)
                    {
                        //just give the expression
                        halt = true;
                    }
                }
                //this is for happy npc's npc towards attitude
                else if (hit.transform.CompareTag("Npc"))
                {
                    if((hit.transform.gameObject.GetComponent<BaseNpcBehavior>().GetMeshAgent().avoidancePriority < GetMeshAgent().avoidancePriority) && (goToATarget || patrol))
                    {
                        halt = true;
                        navAgent.enabled = false;
                        currentAnimatorController.runtimeAnimatorController = haltController;
                        if (isKidActive)
                        {
                            kidController.runtimeAnimatorController = haltController;
                        }
                    }
                }
            }
            else
            {
                if (halt)
                {
                    halt = false;
                    navAgent.enabled = true;
                    currentAnimatorController.runtimeAnimatorController = standardController;
                    if (isKidActive)
                    {
                        kidController.runtimeAnimatorController = kidStandardController;
                    }
                    expressionGiven = false;
                    StartMission();
                }
            }
       }
       else if (isNeutral)
       {
            if (Physics.Raycast(origin, this.transform.forward, out hit, 1f, playerMask))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    Debug.Log("Vision seen. Neutral");
                    if (!sitting && !standingIdle && !talk)
                    {
                        halt = true;
                        navAgent.enabled = false;
                        currentAnimatorController.runtimeAnimatorController = haltController;
                    }
                    else if (sitting)
                    {
                        //just give the expression
                        halt = true;
                    }
                }
            }
            else
            {
                if (halt)
                {
                    halt = false;
                    navAgent.enabled = true;
                    currentAnimatorController.runtimeAnimatorController = standardController;
                    if (isKidActive)
                    {
                        kidController.runtimeAnimatorController = kidStandardController;
                    }
                    expressionGiven = false;
                    StartMission();
                }
            }
       }
       else //angry zone
       {
            if (Physics.Raycast(origin, this.transform.forward, out hit, 2f, playerMask))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    Debug.Log("Vision seen. Angry");
                    halt = true;
                }
            }
            else
            {
                if (halt)
                {
                    halt = false;
                    expressionGiven = false;
                }
            }
       }
    }
    private void InitializeThis()
    {
        source = GetComponent<AudioSource>();
        if(kidController != null)
        {
            isKidActive = true;
        }
        else
        {
            isKidActive = false;
        }

        if (isAngry)
        {
            emojis.sprite = GameManager.Instance.GetGameSettings().angryFace;
            SetAgents("angry");
        }
        else if (isNeutral)
        {
            emojis.sprite = GameManager.Instance.GetGameSettings().neutralFace;
            SetAgents("neutral");
        }
        else if (isHappy)
        {
            emojis.sprite = GameManager.Instance.GetGameSettings().happyFace;
            SetAgents("happy");
        }

        //set positions
        beforePos = transform.position;
        currentPos = transform.position;

        if (isStatic)
        {
            string hour = ifStaticSetLeaveAreaTime.Substring(0, ifStaticSetLeaveAreaTime.Length / 2);
            string minute = ifStaticSetLeaveAreaTime.Substring((ifStaticSetLeaveAreaTime.Length / 2), ifStaticSetLeaveAreaTime.Length / 2);
            myLeaveHour = int.Parse(hour);
            myLeaveMinute = int.Parse(minute);
            onTask = true;
            if (staticonMapAtStart)
            {
                npclife = StartCoroutine(NPCLifeRoutine());
            }
        }
        //start the npc life and set the first mission as going a target.
        //why? because all npcs will be spawned outside of the playable area.
        //we need to get them inside
        //all dynamics will have random visiting time. Some of them can stay on the map all day long. Upgrade: may spawn be weighted.
        else if (!isStatic)
        {
            int rand = Random.Range(1, 24);
            myLeaveHour = GameManager.Instance.GetHour() + rand;
            rand = Random.Range(0, 60);
            myLeaveMinute = GameManager.Instance.GetMinute() + rand;
            if (myLeaveMinute >= 60)
            {
                myLeaveMinute = myLeaveMinute % 60;
                myLeaveHour += 1;
            }
            if (myLeaveHour >= 24)
            {
                myLeaveHour = myLeaveHour % 24;
            }

            goToATarget = true;
            onTask = true;
            Invoke(nameof(func), 0.5f);
        }
    }
    public void InitializeThis(bool staticState, Transform trans, string time)
    {
        isStatic = staticState;
        goToATarget = true;
        onTask = true;
        ifStaticSetLeaveAreaTime = time;
        destination = trans;
        InitializeThis();
        Invoke(nameof(func), 0.5f);
        StartMission();
    }
    public void func()
    {
        npclife = StartCoroutine(NPCLifeRoutine());
    }
    public NavMeshAgent GetMeshAgent()
    {
        return navAgent;
    }
    private void SetAgents(string state)
    {
        if(state == "angry")
        {
            navAgent.avoidancePriority = (int)GameManager.Instance.GetGameSettings().priorityForAngry;
            navAgent.speed = GameManager.Instance.GetGameSettings().npcSpeedForAngry;
        }
        else if(state == "neutral")
        {
            navAgent.avoidancePriority = (int)GameManager.Instance.GetGameSettings().priorityForNeutral;
            navAgent.speed = GameManager.Instance.GetGameSettings().npcSpeedForNeutral;

        }
        else if(state == "happy")
        {
            navAgent.avoidancePriority = (int)GameManager.Instance.GetGameSettings().priorityForHappy;
            navAgent.speed = GameManager.Instance.GetGameSettings().npcSpeedForHappy;
        }
    }
    private void DetermineBehavior()
    {
        int random = Random.Range(0, 5);
        switch (random)
        {
            case 0:
                patrol = true;
                standingIdle = false;
                talk = false;
                goToATarget = false;
                talkingWPhone = false;
                break;
            case 1:
                patrol = false;
                standingIdle = false;
                talk = false;
                goToATarget = true;
                talkingWPhone = false;
                break;
            case 2:
                patrol = false;
                standingIdle = false;
                talk = true;
                goToATarget = false;
                talkingWPhone = false;
                break;
            case 3:
                patrol = false;
                standingIdle = true;
                talk = false;
                goToATarget = false;
                talkingWPhone = false;
                break;
            case 4:
                patrol = false;
                standingIdle = false;
                talk = false;
                goToATarget = false;
                talkingWPhone = true;
                break;
        }
        StartMission();
    }
    public float CalculateAndGetVelocity()
    {
        float diff = (currentPos - beforePos).magnitude;
        //diff = diff / GameManager.Instance.GetGameSettings().realWorldTimeForEachGameHour;
        return diff;
    }
    private void DetermineBehavior(int minInclusive)
    {
        int random = Random.Range(minInclusive, 5);
        switch (random)
        {
            case 0:
                patrol = true;
                standingIdle = false;
                talk = false;
                goToATarget = false;
                talkingWPhone = false;
                break;
            case 1:
                patrol = false;
                standingIdle = false;
                talk = false;
                goToATarget = true;
                talkingWPhone = false;
                break;
            case 2:
                patrol = false;
                standingIdle = false;
                talk = true;
                goToATarget = false;
                talkingWPhone = false;
                break;
            case 3:
                patrol = false;
                standingIdle = true;
                talk = false;
                goToATarget = false;
                talkingWPhone = false;
                break;
            case 4:
                patrol = false;
                standingIdle = false;
                talk = false;
                goToATarget = false;
                talkingWPhone = true;
                break;
        }
        StartMission();
    }
    public void StartMission()
    {
        if (!isStatic)
        {
            if (talk)
            {
                currentAnimatorController.SetBool("isTalking", true);
                currentAnimatorController.SetBool("isNWalking", false);
                currentAnimatorController.SetBool("isWalkingWKid", false);
                currentAnimatorController.SetBool("isSitting", false);
                currentAnimatorController.SetBool("isTalkingWPhone", false);
                if (isKidActive)
                {
                    kidController.SetBool("isNWalking", false);
                    kidController.SetBool("isWalkingWKid", false);
                    kidController.SetBool("isSitting", false);
                }
                Talk();
            }
            else if (talkingWPhone)
            {
                currentAnimatorController.SetBool("isTalking", false);
                currentAnimatorController.SetBool("isNWalking", false);
                currentAnimatorController.SetBool("isWalkingWKid", false);
                currentAnimatorController.SetBool("isSitting", false);
                currentAnimatorController.SetBool("isTalkingWPhone", true);
                TalkPhone();
            }
            else if (patrol || goToATarget)
            {
                if (isKidActive)
                {
                    currentAnimatorController.SetBool("isWalkingWKid", true);
                    currentAnimatorController.SetBool("isNWalking", false);
                    kidController.SetBool("isNWalking", true);
                }
                else
                {
                    currentAnimatorController.SetBool("isNWalking", true);
                    currentAnimatorController.SetBool("isWalkingWKid", false);
                }
                currentAnimatorController.SetBool("isTalking", false);
                currentAnimatorController.SetBool("isSitting", false);
                currentAnimatorController.SetBool("isTalkingWPhone", false);
                if (goToATarget)
                {
                    GoToDefinedTarget();
                }
                else
                {
                    PatrolStarting();
                }
            }
            else if (standingIdle)
            {
                currentAnimatorController.SetBool("isTalking", false);
                currentAnimatorController.SetBool("isNWalking", false);
                currentAnimatorController.SetBool("isWalkingWKid", false);
                currentAnimatorController.SetBool("isSitting", false);
                currentAnimatorController.SetBool("isTalkingWPhone", false);
                if (isKidActive)
                {
                    kidController.SetBool("isNWalking", false);
                    kidController.SetBool("isWalkingWKid", false);
                    kidController.SetBool("isSitting", false);
                }
                Idle();
            }
            onTask = true;
        }
        else
        {
            if (talk)
            {
                currentAnimatorController.SetBool("isTalking", true);
                currentAnimatorController.SetBool("isNWalking", false);
                currentAnimatorController.SetBool("isTalkingWPhone", false);
                currentAnimatorController.SetBool("isSitting", false);
                if (isKidActive)
                {
                    kidController.SetBool("isNWalking", false);
                    kidController.SetBool("isTalking", true);
                    kidController.SetBool("isWalkingWKid", false);
                }
            }
            else if (talkingWPhone)
            {
                currentAnimatorController.SetBool("isTalkingWPhone", true);
                currentAnimatorController.SetBool("isNWalking", false);
                currentAnimatorController.SetBool("isTalking", false);
                currentAnimatorController.SetBool("isSitting", false);
                if (isKidActive)
                {
                    kidController.SetBool("isNWalking", false);
                    kidController.SetBool("isTalking", false);
                    kidController.SetBool("isWalkingWKid", false);
                }
            }
            else if (goToATarget)
            {
                currentAnimatorController.SetBool("isNWalking", true);
                currentAnimatorController.SetBool("isTalking", false);
                currentAnimatorController.SetBool("isTalkingWPhone", false);
                currentAnimatorController.SetBool("isSitting", false);
                if (isKidActive)
                {
                    kidController.SetBool("isNWalking", false);
                    kidController.SetBool("isTalking", false);
                    kidController.SetBool("isWalkingWKid", true);
                }
            }
            else if (sitting)
            {
                //disable the agent
                navAgent.enabled = false;
                currentAnimatorController.SetBool("isSitting", true);
                currentAnimatorController.SetBool("isNWalking", false);
                currentAnimatorController.SetBool("isTalking", false);
                currentAnimatorController.SetBool("isTalkingWPhone", false);
            }
            else if (standingIdle)
            {
                currentAnimatorController.SetBool("isTalking", false);
                currentAnimatorController.SetBool("isNWalking", false);
                currentAnimatorController.SetBool("isSitting", false);
                currentAnimatorController.SetBool("isTalkingWPhone", false);
                if (isKidActive)
                {
                    kidController.SetBool("isNWalking", false);
                    kidController.SetBool("isTalking", false);
                    kidController.SetBool("isWalkingWKid", false);
                }
            }
            currentAnimatorController.SetBool("isWalkingWKid", false);

        }
    }
    private IEnumerator NPCLifeRoutine()
    {
        while (true)
        {
            if (GameManager.Instance.GetGameStarted() && !halt)
            {
                //this line for dynamic npcs, if they are on idle, give them a task
                if (onTask == false && !isStatic && !dynamicReturning)
                {
                    DetermineBehavior();
                }
                else if(onTask == false && !isStatic && dynamicReturning)
                {
                    Termination();
                }
                //static npcs are in the field and stay there for a while until their time has come(staticreturning = true)
                else if (isStatic && !staticReturning)
                {
                    //in the field and its leave time has come.
                    if (GameManager.Instance.GetHour() >= myLeaveHour && GameManager.Instance.GetMinute() >= myLeaveMinute)
                    {
                        onTask = false;
                        //activate agent, else it will not go
                        if (sitting)
                        {
                            navAgent.enabled = true;
                        }
                        staticReturning = true;
                        goToATarget = true;
                        talk = false;
                        sitting = false;
                        talkingWPhone = false;
                        standingIdle = false;
                        StartMission();
                        yield return new WaitForSeconds(0.5f);
                        Vector3 returnPos = new Vector3(destination.position.x, destination.position.y, destination.position.z);
                        navAgent.SetDestination(returnPos);
                    }
                    else if(onTask == false && !goToATarget)
                    {
                        onTask = true;
                        DetermineBehavior(2);
                    }
                    //in the field and its leave time has not come yet. Go to the specified target, then spend some time.
                    else if (onTask == true && goToATarget)
                    {
                        if (IsDestinationReached())
                        {
                            goToATarget = !goToATarget;
                            onTask = false;
                            //turn your face according to specified point orientation
                            StartCoroutine(NPCLookAlignment(1f, destOrientation));
                        }
                    }
                    else if(onTask == true && !goToATarget && (talk || standingIdle || sitting || talkingWPhone))
                    {
                        destination = GameManager.Instance.GetGameSettings().pointHolder.GetSpawnPoint();
                    }
                }
                //static npcs leaving from the game
                else if (onTask == false && isStatic && staticReturning)
                {
                    Termination();
                }

                //***********************************//
                if(onTask && !isStatic)
                {
                    //time to leave
                    if (GameManager.Instance.GetHour() >= myLeaveHour && GameManager.Instance.GetMinute() >= myLeaveMinute)
                    {
                        if (onTask && !dynamicReturning)
                        {
                            dynamicReturning = true;
                            onTask = false;
                            destination = GameManager.Instance.GetGameSettings().pointHolder.GetSpawnPoint();
                            Vector3 returnPos = new Vector3(destination.position.x, destination.position.y, destination.position.z);
                            navAgent.SetDestination(returnPos);
                        }
                    }
                    else
                    {
                        //dynamic npc reached to its target, go on idle
                        if (onTask && goToATarget && !isStatic)
                        {
                            if (IsDestinationReached())
                            {
                                goToATarget = false;
                                onTask = false;
                            }
                        }
                        else if (onTask && patrol && !isStatic)
                        {
                            if (GameManager.Instance.GetHour() >= myUpdatedTaskHour && GameManager.Instance.GetMinute() >= myUpdatedTaskMin)
                            {
                                if (patrol)
                                {
                                    patrol = !patrol;
                                }
                                onTask = false;
                            }
                            else
                            {
                                if (IsDestinationReached())
                                {
                                    if (pathfollowindex >= GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("last"))
                                    {
                                        pathfollowindex = GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("first");
                                    }
                                    else
                                    {
                                        pathfollowindex += 1;
                                    }

                                    SetNewPointForPatrol();
                                }
                                else { }
                            }
                        }
                        else if (onTask && (standingIdle || talkingWPhone || talk) && !isStatic)
                        {
                            if (GameManager.Instance.GetHour() >= myUpdatedTaskHour && GameManager.Instance.GetMinute() >= myUpdatedTaskMin)
                            {
                                if (standingIdle)
                                {
                                    standingIdle = !standingIdle;
                                }
                                else if (talk)
                                {
                                    talk = !talk;
                                }
                                else
                                {
                                    talkingWPhone = !talkingWPhone;
                                }
                                onTask = false;
                            }
                        }
                    }                  
                }
                CheckPlayerDistance();
            }
            else if (GameManager.Instance.GetGameStarted() && !GameManager.Instance.GetIsGameFinished() && halt)
            {
                //this will be active when happy npc sees robot in front of himself/herself
                if (!expressionGiven)
                {
                    expressionGiven = true;
                    if(Male && isHappy)
                    {
                        source.clip = AudioManager.Instance.GetRandomAudioFromEmotionPools(0);
                        source.Play();
                    }
                    else if(Male && isAngry)
                    {
                        source.clip = AudioManager.Instance.GetRandomAudioFromEmotionPools(1);
                        source.Play();
                    }
                    else if(Female && isHappy)
                    {
                        source.clip = AudioManager.Instance.GetRandomAudioFromEmotionPools(2);
                        source.Play();
                    }
                    else if(Female && isAngry)
                    {
                        source.clip = AudioManager.Instance.GetRandomAudioFromEmotionPools(3);
                        source.Play();
                    }
                    else if(Male && isNeutral)
                    {
                        source.clip = AudioManager.Instance.GetRandomAudioFromEmotionPools(4);
                        source.Play();
                    }
                    else
                    {
                        source.clip = AudioManager.Instance.GetRandomAudioFromEmotionPools(5);
                        source.Play();
                    }
                }
            }
            else if(GameManager.Instance.GetIsGameFinished() && !GameManager.Instance.GetGameStarted())
            {
                StopCoroutine(npclife);
            }
            ForwardVision();
            currentPos = transform.position;
            yield return new WaitForSeconds(60f / GameManager.Instance.GetGameSettings().realWorldTimeForEachGameHour);
            beforePos = currentPos;
        }
    }
    private IEnumerator NPCLookAlignment(float time, Quaternion orientation)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        while(elapsedTime < time)
        {
            elapsedTime += 0.05f;
            transform.rotation = Quaternion.Slerp(startRotation, orientation, elapsedTime);
            yield return new WaitForSeconds(0.05f);
        }
    }
    private IEnumerator AngryNpcTimeCounter()
    {
        int counter = 0;
        //first turn our npc.
        Quaternion rotneed = Quaternion.LookRotation((GameManager.Instance.GetPlayer().transform.position - transform.position).normalized);
        StartCoroutine(NPCLookAlignment(1f, rotneed));
        //then count time
        while (angrySequence)
        {
            yield return new WaitForSecondsRealtime(1f);
            counter += 1;
            if(counter == GameManager.Instance.GetGameSettings().angryTimeCountBeforeAnimation)
            {
                currentAnimatorController.SetBool("isPoint", true);
            }
            else if(counter == GameManager.Instance.GetGameSettings().angryTimeCountBeforeKick)
            {
                currentAnimatorController.SetBool("isPoint", false);
                currentAnimatorController.SetBool("isKick", true);
                isKicked = true;
            }
            else if(counter > GameManager.Instance.GetGameSettings().angryTimeCountBeforeKick + 3)
            {
                currentAnimatorController.SetBool("isPoint", false);
                currentAnimatorController.SetBool("isKick", false);
            }
        }
    }
    //check player distance function has identical importance not only npc behavior but also data manager.
    //at the end of the game, we will print the total close contacts to the player.info
    private void CheckPlayerDistance()
    {
        Vector3 distanceVec = this.transform.position - GameManager.Instance.GetPlayer().transform.position;
        playerDistance = distanceVec.magnitude;
        //this is highest detection distance.
        if (playerDistance <= GameManager.Instance.GetGameSettings().npcAngryPlayerDetectionDis)
        {
            ContactInfo newContact = new ContactInfo();
            newContact.contactName = this.gameObject.name;
            //newContact.contactTime = GameManager.Instance.GetHour() + ":" + GameManager.Instance.GetMinute();
            newContact.contactTime = InterfaceManager.Instance.InGameUI.GetHourString();
            newContact.robotData = GameManager.Instance.GetPlayer().transform.position + "," + DataManager.Instance.FloatFlooring(GameManager.Instance.GetPlayer().GetRotationValues().y, 2);
            switch (emojis.sprite.name)
            {
                case "angryface":
                    if(distanceVec.magnitude <= GameManager.Instance.GetGameSettings().npcAngryPlayerDetectionDis)
                    {
                        newContact.contactBehavior = emojis.sprite.name;
                        newContact.contactDistance = distanceVec.magnitude;
                        newContact.contactId = gameObject.GetInstanceID().ToString();
                        newContact.npcYRot = transform.rotation.eulerAngles.y;
                        newContact.npcVel = CalculateAndGetVelocity();
                        newContact.contactNpcPosition = transform.position;
                        if (isKicked)
                        {
                            newContact.robotAbusement = "yes";
                            isKicked = false;
                        }
                        else
                        {
                            newContact.robotAbusement = "no";
                        }
                        DataManager.Instance.UpdateCloseContact(newContact);
                    }
                    break;
                case "neutralface":
                    if (distanceVec.magnitude <= GameManager.Instance.GetGameSettings().npcNeutralPlayerDetectionDis)
                    {
                        newContact.contactBehavior = emojis.sprite.name;
                        newContact.contactDistance = distanceVec.magnitude;
                        newContact.contactId = gameObject.GetInstanceID().ToString();
                        newContact.npcYRot = transform.rotation.eulerAngles.y;
                        newContact.npcVel = CalculateAndGetVelocity();
                        newContact.contactNpcPosition = transform.position;
                        DataManager.Instance.UpdateCloseContact(newContact);
                    }
                    break;
                case "laughingface":
                    if (distanceVec.magnitude <= GameManager.Instance.GetGameSettings().npcHappyPlayerDetectionDis)
                    {
                        newContact.contactBehavior = emojis.sprite.name;
                        newContact.contactDistance = distanceVec.magnitude;
                        newContact.contactId = gameObject.GetInstanceID().ToString();
                        newContact.npcYRot = transform.rotation.eulerAngles.y;
                        newContact.npcVel = CalculateAndGetVelocity();
                        newContact.contactNpcPosition = transform.position;
                        DataManager.Instance.UpdateCloseContact(newContact);
                    }
                    break;
            }
        }

        if(playerDistance <= GameManager.Instance.GetGameSettings().npcHappyPlayerDetectionDis && isAngry && !angrySequence)
        {
            if (standingIdle || talk || talkingWPhone)
            {
                //if our npc is angry we will give specific behavior because the robot's user is careless.
                angrySequence = true;
                //memorize rotation. Bc, we will turn to the robot.
                rotBeforeExpression = this.transform.rotation;
                currentAnimatorController.runtimeAnimatorController = haltController;
                StartCoroutine(AngryNpcTimeCounter());
            }
        }
        else if(playerDistance >= GameManager.Instance.GetGameSettings().npcHappyPlayerDetectionDis && isAngry && angrySequence)
        {
            angrySequence = false;
            //turn back because robot left.
            StartCoroutine(NPCLookAlignment(1f, rotBeforeExpression));
            currentAnimatorController.runtimeAnimatorController = standardController;
            //turn to mission
            StartMission();
        }
       
    }
    //destroy this npc. Remove its existance.
    private void Termination()
    {
        if (IsDestinationReached())
        {
            Debug.Log("I have done my job. Farewell.");
            GameManager.Instance.DeleteMe(this);
        }
    }
    private bool IsDestinationReached()
    {
        Vector3 vec = new Vector3(destination.position.x, destination.position.y, destination.position.z);
        Vector3 diffMag = vec - gameObject.transform.position;
        /*if(diffMag.magnitude <= 0.8f)
        {
            destination = gameObject.transform;
            navAgent.isStopped = true;
            return true;
        }
        else
        {
            return false;
        }*/
        if(navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            if(!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
            {
                destOrientation = destination.transform.rotation;
                destination = gameObject.transform;
                return true;
            }
            /*else if(navAgent.hasPath && diffMag.magnitude <= 0.8f && navAgent.velocity.sqrMagnitude >= 0.5f)
            {
                destination = gameObject.transform;
                return true;
            }*/
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    private void PatrolStarting()
    {
        myUpdatedTaskHour = GameManager.Instance.GetHour() + 1;
        if(myUpdatedTaskHour >= 24)
        {
            myUpdatedTaskHour = myUpdatedTaskHour % 24;
        }
        myUpdatedTaskMin = GameManager.Instance.GetMinute();

        Transform point1 = GameManager.Instance.GetGameSettings().pointHolder.GetPointByIndex(GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("first"));
        Transform point2 = GameManager.Instance.GetGameSettings().pointHolder.GetPointByIndex(GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("last"));
        Vector3 distance1 = point1.position - this.transform.position;
        Vector3 distance2 = point2.position - this.transform.position;
        navAgent.isStopped = false;
        if(distance1.magnitude > distance2.magnitude)
        {
            navAgent.SetDestination(point1.position);
            pathfollowindex = GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("first");
            destination = point1;
        }
        else
        {
            navAgent.SetDestination(point2.position);
            pathfollowindex = GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("last");
            destination = point2;
        }
    }
    private void SetNewPointForPatrol()
    {
        Transform point = GameManager.Instance.GetGameSettings().pointHolder.GetPointByIndex(pathfollowindex);
        navAgent.SetDestination(point.position);
        destination = point;
    }
    private void GoToDefinedTarget()
    {
        //get random destination set player to go to the target. But don't give them spawn point :)
        Transform destinationPoint = transform;
        int rand = Random.Range(1, GameManager.Instance.GetGameSettings().pointHolder.GetPointListLength());
        if(rand == GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("first"))
        {
            //start of a path, default = 1st
            destinationPoint = GameManager.Instance.GetGameSettings().pointHolder.GetPointByIndex(GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("first"));
        }
        else if(rand == GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("last"))
        {
            //end of a path, defaulth = 4th
            destinationPoint = GameManager.Instance.GetGameSettings().pointHolder.GetPointByIndex(GameManager.Instance.GetGameSettings().pointHolder.GetPathByIndex().GetFirstOrLast("last"));
        }
        else
        {
            destinationPoint = GameManager.Instance.GetGameSettings().pointHolder.GetPointByIndex(rand);
        }

        destination = destinationPoint;
        Vector3 destinationVec = new Vector3(destination.position.x, destination.position.y, destination.position.z);
        navAgent.SetDestination(destinationVec);
    }
    public void Talk()
    {
        UpdateTheTaskDuration();
    }
    public void Idle()
    {
        UpdateTheTaskDuration();
    }
    public void TalkPhone()
    {
        UpdateTheTaskDuration();
    }
    private void UpdateTheTaskDuration()
    {
        int rand = Random.Range(0, taskDurationsMaxLimiter);
        myUpdatedTaskMin = GameManager.Instance.GetMinute() + rand;
        if (myUpdatedTaskMin > 59)
        {
            myUpdatedTaskHour = GameManager.Instance.GetHour() + 1;
            myUpdatedTaskMin %= 60;
        }
        else
        {
            myUpdatedTaskHour = GameManager.Instance.GetHour();
        }
    }
}
