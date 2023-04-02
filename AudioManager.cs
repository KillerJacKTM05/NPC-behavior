using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SafeZone
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

        }

        public List<AudioClip> TaskCompletionPool = new List<AudioClip>();
        public List<AudioClip> MaleHappyPool = new List<AudioClip>();
        public List<AudioClip> MaleAngryPool = new List<AudioClip>();
        public List<AudioClip> FemaleHappyPool = new List<AudioClip>();
        public List<AudioClip> FemaleAngryPool = new List<AudioClip>();
        public List<AudioClip> MaleNeutralPool = new List<AudioClip>();
        public List<AudioClip> FemaleNeutralPool = new List<AudioClip>();
        private AudioSource audioSource;
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void PlayRandomClipFromTaskPool()
        {
            int rand = Random.Range(0, TaskCompletionPool.Count);
            audioSource.clip = TaskCompletionPool[rand];
            audioSource.Play();
        }
        public AudioClip GetRandomAudioFromEmotionPools(int poolQueue)
        {
            int rand = 0;
            AudioClip returnClip = null;
            switch (poolQueue)
            {
                case 0:
                    rand = Random.Range(0, MaleHappyPool.Count);
                    returnClip = MaleHappyPool[rand];
                    break;
                case 1:
                    rand = Random.Range(0, MaleAngryPool.Count);
                    returnClip = MaleAngryPool[rand];
                    break;
                case 2:
                    rand = Random.Range(0, FemaleHappyPool.Count);
                    returnClip = FemaleHappyPool[rand];
                    break;
                case 3:
                    rand = Random.Range(0, MaleAngryPool.Count);
                    returnClip = MaleAngryPool[rand];
                    break;
                case 4:
                    rand = Random.Range(0, MaleNeutralPool.Count);
                    returnClip = MaleNeutralPool[rand];
                    break;
                case 5:
                    rand = Random.Range(0, FemaleNeutralPool.Count);
                    returnClip = FemaleNeutralPool[rand];
                    break;
            }
            return returnClip;
        }
    }
}

