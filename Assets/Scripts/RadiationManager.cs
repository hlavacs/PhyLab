using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RadiationManager : PhyLabSceneManager {
    public enum Source {
        Alpha,
        Beta,
        Gamma,
        Neutron
    }

    public Source CurrentSource;
    public TextMesh SourceDescription;
    public TextMesh ObstacleDescription;
    public TextMesh DetectionDescription;

    public ParticleSystem SourceSystem;
    public Material ParticleAlpha;
    public Material ParticleBeta;
    public Material ParticleGamma;
    public Material ParticleNeutron;

    public Transform CollisionPlanePaper;
    public Transform CollisionPlaneAluminium;
    public Transform CollisionPlaneLed;

    public Transform CollisionPlaneWaterAlpha;
    public Transform CollisionPlaneWaterBeta;
    public Transform CollisionPlaneWaterGamma;

    public GameObject DetectionAlpha;
    public GameObject DetectionBeta;
    public GameObject DetectionGamma;
    public GameObject DetectionNeutron;

    public GameObject GammaFree;
    public GameObject GammaLed;
    public GameObject GammaWater;

    bool Detected = false;

    public override void InitializeSpecificScene() {
        Manager.Assistant.Speak("Hier sind wir im Radioaktiven Labor! Hier ist eine Maschine mit der du mit radioaktiver Strahlung experimentieren kannst.", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Radiation");
        Manager.Assistant.Speak("Du kannst auf der rechten Seite des Geräts verschiedene Arten von Strahlung auswählen.", 3, true);
        Manager.Assistant.Speak("In der Mitte hast du mehrere Möglichkeiten verschiedene Hindernisse zu setzen.", 3, false);
        Manager.Assistant.Speak("Am Bildschirm auf der linken Seite siehst du ob die eingestellte Strahlung durchkommt.", -1, false);
    }

    public void UpdateObstacles() {
        string obstacleText = "";
        var obstacles = FindObjectsOfType<RadiationObstacle>();
        foreach (var obstacle in obstacles) {
            if(obstacle.IsObstacleActive) {
                if(obstacleText != "") {
                    obstacleText += " + ";
                }
                switch (obstacle.Type) {
                    case RadiationObstacle.ObstacleType.Paper:
                        obstacleText += "Papier";
                        break;
                    case RadiationObstacle.ObstacleType.Aluminium:
                        obstacleText += "Aluminium";
                        break;
                    case RadiationObstacle.ObstacleType.Led:
                        obstacleText += "Blei";
                        break;
                    case RadiationObstacle.ObstacleType.Water:
                        obstacleText += "Wasser";
                        break;
                    default:
                        break;
                }
            }
        }
        if (obstacleText == "") {
            obstacleText += "Kein Hindernis";
        }
        ObstacleDescription.text = obstacleText;
        UpdateSource();
    }

    public void SetSource(Source source) {
        CurrentSource = source;

        switch (CurrentSource) {
            case Source.Alpha:
                SourceDescription.text = "α - Strahler";
                break;
            case Source.Beta:
                SourceDescription.text = "β - Strahler";
                break;
            case Source.Gamma:
                SourceDescription.text = "γ - Strahler";
                break;
            case Source.Neutron:
                SourceDescription.text = "Neutronenstrahler";
                break;
            default:
                break;
        }

        UpdateSource();
    }

    public void PreviousSource() {
        switch (CurrentSource) {
            case Source.Alpha:
                SetSource(Source.Neutron);
                break;
            case Source.Beta:
                SetSource(Source.Alpha);
                break;
            case Source.Gamma:
                SetSource(Source.Beta);
                break;
            case Source.Neutron:
                SetSource(Source.Gamma);
                break;
            default:
                break;
        }
    }

    public void NextSource() {
        switch (CurrentSource) {
            case Source.Alpha:
                SetSource(Source.Beta);
                break;
            case Source.Beta:
                SetSource(Source.Gamma);
                break;
            case Source.Gamma:
                SetSource(Source.Neutron);
                break;
            case Source.Neutron:
                SetSource(Source.Alpha);
                break;
            default:
                break;
        }
    }

    public void UpdateSource() {
        Detected = true;
        var obstacles = FindObjectsOfType<RadiationObstacle>();
        var collision = SourceSystem.collision;
        for (int i = 0; i < 4; i++) {
            collision.SetPlane(i, null);
        }

        var force = SourceSystem.forceOverLifetime;
        force.enabled = false;

        var emission = SourceSystem.emission;
        emission.rateOverTime = 2;

        SourceSystem.gameObject.SetActive(true);
        GammaFree.SetActive(false);
        GammaLed.SetActive(false);
        GammaWater.SetActive(false);

        switch (CurrentSource) {
            case Source.Alpha:
                UpdateAlphaSource(obstacles);
                break;
            case Source.Beta:
                UpdateBetaSource(obstacles);
                break;
            case Source.Gamma:
                UpdateGammaSource(obstacles);
                break;
            case Source.Neutron:
                UpdateNeutronSource(obstacles);
                break;
            default:
                break;
        }

        UpdateDetection();
    }



    void UpdateAlphaSource(RadiationObstacle[] obstacles) {
        float lifetime = 7.0f;
        var collision = SourceSystem.collision;
        foreach (var obstacle in obstacles) {
            if (obstacle.IsObstacleActive) {
                if(obstacle.Type == RadiationObstacle.ObstacleType.Paper) {
                    collision.SetPlane(0, CollisionPlanePaper);
                    Detected = false;
                } else if (obstacle.Type == RadiationObstacle.ObstacleType.Aluminium) {
                    collision.SetPlane(1, CollisionPlaneAluminium);
                    Detected = false;
                } else if (obstacle.Type == RadiationObstacle.ObstacleType.Led) {
                    collision.SetPlane(2, CollisionPlaneLed);
                    Detected = false;
                } else if (obstacle.Type == RadiationObstacle.ObstacleType.Water) {
                    collision.SetPlane(3, CollisionPlaneWaterAlpha);
                    Detected = false;
                }
            }
        }
        var main = SourceSystem.main;
        main.startLifetime = lifetime;
        main.startSize = 0.2f;
        SourceSystem.GetComponent<ParticleSystemRenderer>().material = ParticleAlpha;
    }

    void UpdateBetaSource(RadiationObstacle[] obstacles) {
        float lifetime = 7.0f;
        var collision = SourceSystem.collision;
        foreach (var obstacle in obstacles) {
            if (obstacle.IsObstacleActive) {
                if (obstacle.Type == RadiationObstacle.ObstacleType.Aluminium) {
                    collision.SetPlane(1, CollisionPlaneAluminium);
                    Detected = false;
                } else if (obstacle.Type == RadiationObstacle.ObstacleType.Led) {
                    collision.SetPlane(2, CollisionPlaneLed);
                    Detected = false;
                } else if (obstacle.Type == RadiationObstacle.ObstacleType.Water) {
                    collision.SetPlane(3, CollisionPlaneWaterBeta);
                    Detected = false;
                }
            }
        }
        var main = SourceSystem.main;
        main.startLifetime = lifetime;
        main.startSize = 0.2f;
        SourceSystem.GetComponent<ParticleSystemRenderer>().material = ParticleBeta;
    }

    void UpdateGammaSource(RadiationObstacle[] obstacles) {
        var collision = SourceSystem.collision;
        bool hasWater = false;
        bool hasLed = false;
        foreach (var obstacle in obstacles) {
            if (obstacle.IsObstacleActive) {
                if (obstacle.Type == RadiationObstacle.ObstacleType.Led) {
                    hasLed = true;
                } else if (obstacle.Type == RadiationObstacle.ObstacleType.Water) {
                    hasWater = true;
                }
            }
        }

        if(hasLed) {
            GammaLed.SetActive(true);
            Detected = false;
        } else if (hasWater) {
            Detected = false;
            GammaWater.SetActive(true);
        } else {
            GammaFree.SetActive(true);
        }

        SourceSystem.gameObject.SetActive(false);
    }

    void UpdateNeutronSource(RadiationObstacle[] obstacles) {
        float lifetime = 7.0f;
        var force = SourceSystem.forceOverLifetime;
        foreach (var obstacle in obstacles) {
            if (obstacle.IsObstacleActive) {
                if (obstacle.Type == RadiationObstacle.ObstacleType.Water) {
                    force.enabled = true;
                    lifetime = 14.0f;
                    Detected = false;
                }
            }
        }
        var main = SourceSystem.main;
        main.startLifetime = lifetime;
        main.startSize = 0.2f;
        SourceSystem.GetComponent<ParticleSystemRenderer>().material = ParticleNeutron;
    }


    public void UpdateDetection() {
        DetectionAlpha.SetActive(false);
        DetectionBeta.SetActive(false);
        DetectionGamma.SetActive(false);
        DetectionNeutron.SetActive(false);

        if (Detected) {
            switch (CurrentSource) {
                case Source.Alpha:
                    DetectionDescription.text = "Heliumkern\nerkannt!";
                    DetectionAlpha.SetActive(true);
                    break;
                case Source.Beta:
                    DetectionDescription.text = "Elektron\nerkannt!";
                    DetectionBeta.SetActive(true);
                    break;
                case Source.Gamma:
                    DetectionDescription.text = "Gammastrahlung\nerkannt!";
                    DetectionGamma.SetActive(true);
                    break;
                case Source.Neutron:
                    DetectionDescription.text = "Neutron\nerkannt!";
                    DetectionNeutron.SetActive(true);
                    break;
                default:
                    break;
            }
        } else {
            if(CurrentSource == Source.Neutron) {
                DetectionDescription.text = "Langsames\nNeutron\nerkannt!";
                DetectionNeutron.SetActive(true);
            } else {
                DetectionDescription.text = "Nichts\nerkannt!";
            }
        }
    }
}
