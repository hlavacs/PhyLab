using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PhotoSphereEntry {
    public Texture2D Photo;
    public string Text;
    public AudioClip VoiceOver;
    public float Duration;
}

public class PhotoSphere : MonoBehaviour {
    GameManager Manager;

    public AudioSource BackgroundMusic;
    public AudioSource VoiceOver;
    public PhotoSphereEntry[] Entries;
    public GameObject PhotoEntryPrefab;

    List<GameObject> ShownEntries = new List<GameObject>();
    Coroutine CurrentDisplayRoutine = null;
    int CurrentEntryIndex = 0;

    Vector3 PlayerStartPosition;

    float MaxPhotoDistance = 15.0f;
    float MinPhotoDistance = 3.0f;
    float PhotoSpacing = 0.1f;

    float PauseTime = 2.0f;

    private void Start() {
        Manager = FindObjectOfType<GameManager>();
    }

    IEnumerator _PlaySphereEntry(PhotoSphereEntry entry, int spacingLayer) {
        VoiceOver.Stop();
        if(entry.VoiceOver) {
            VoiceOver.clip = entry.VoiceOver;
            VoiceOver.Play();
        }

        GameObject newPhotoEntry = Instantiate(PhotoEntryPrefab, transform);
        ShownEntries.Add(newPhotoEntry);
        Vector3 startPos = Camera.main.transform.forward * MaxPhotoDistance;
        Vector3 endPos = Camera.main.transform.forward * (MinPhotoDistance - PhotoSpacing * spacingLayer);
        newPhotoEntry.transform.localPosition = startPos;
        newPhotoEntry.transform.LookAt(newPhotoEntry.transform.position + Camera.main.transform.forward);
        PhotoSphereEntryPrefab prefab = newPhotoEntry.GetComponent<PhotoSphereEntryPrefab>();
        
        prefab.PhotoRenderer.material.mainTexture = entry.Photo;
        prefab.Text.text = entry.Text;
        prefab.PhotoRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        prefab.Text.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        float time = 0;
        while(time < entry.Duration) {
            float alpha = time / entry.Duration;
            float positionAlpha = Mathf.Clamp01(alpha * 2.0f);
            float colorAlpha = Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01(alpha * 4.0f));
            prefab.PhotoRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, colorAlpha);
            prefab.Text.color = new Color(0.0f, 0.0f, 0.0f, colorAlpha);


            newPhotoEntry.transform.localPosition = Vector3.Lerp(startPos, endPos, positionAlpha);
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        time = 0;
        while (time < PauseTime) {
            float alpha = 1.0f - time / PauseTime;

            prefab.PhotoRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            prefab.Text.color = new Color(0.0f, 0.0f, 0.0f, alpha);

            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }
        ShownEntries.Remove(newPhotoEntry);
        Destroy(newPhotoEntry);

        Skip();
    }

    IEnumerator _Enter() {
        yield return Manager.FadeCoroutine(false);
        Manager.Player.transform.position = transform.position;
        BackgroundMusic.Play();
        yield return Manager.FadeCoroutine(true);
        yield return new WaitForSeconds(PauseTime);
        CurrentDisplayRoutine = StartCoroutine(_PlaySphereEntry(Entries[0], 0));
    }

    public void Enter() {
        CurrentEntryIndex = 0;
        PlayerStartPosition = Manager.Player.transform.position;
        StartCoroutine(_Enter());
    }

    public void Skip() {
        if(CurrentDisplayRoutine != null) {
            StopCoroutine(CurrentDisplayRoutine);
        }
        ++CurrentEntryIndex;
        if (CurrentEntryIndex < Entries.Length) {
            CurrentDisplayRoutine = StartCoroutine(_PlaySphereEntry(Entries[CurrentEntryIndex], CurrentEntryIndex));
        } else {
            Leave();
        }
    }

    IEnumerator _Leave() {
        yield return Manager.FadeCoroutine(false);
        Manager.Player.transform.position = PlayerStartPosition;
        BackgroundMusic.Stop();
        foreach (var entry in ShownEntries) {
            Destroy(entry);
        }
        ShownEntries.Clear();
        yield return Manager.FadeCoroutine(true);
    }

    public void Leave() {
        if (CurrentDisplayRoutine != null) {
            StopCoroutine(CurrentDisplayRoutine);
        }
        StartCoroutine(_Leave());
    }
}
