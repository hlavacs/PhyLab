using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetChooser : MonoBehaviour
{
    public Planet Planet;
    public float RotationSpeed;
    public Planet[] AllPlanets;
    GameManager Manager;

    Coroutine EnablePlanetRoutine = null;

    private void Start() {
        Manager = FindObjectOfType<GameManager>();
    }

    IEnumerator _ChangeToPlanet() {
        yield return Manager.FadeCoroutine(false);

        foreach (var planet in AllPlanets) {
            if (planet.gameObject.activeSelf) {
                planet.gameObject.SetActive(false);
            }
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Planet.gameObject.SetActive(true);
        Planet.EnableWorld();

        yield return Manager.FadeCoroutine(true);
        EnablePlanetRoutine = null;
    }

    public void ChangeToPlanet() {
        if(EnablePlanetRoutine == null) {
            EnablePlanetRoutine = StartCoroutine(_ChangeToPlanet());
        }
    }

    private void Update() {
        transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime, Space.Self);
    }
}
