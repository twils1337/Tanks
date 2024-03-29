﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarePackageManager : MonoBehaviour
{
    public int m_ActiveCarePackages { get; set; }
    public List<Rigidbody> m_CarePackages;
    public List<GameObject> m_SpawnPoints;
    private int m_MaxCarePackagesActive = 7;
    private float m_SpawnTimer = 0.0f;
    private bool m_FirstSpawn = true;
    private float m_TimeToGoOff = 30.0f;

    // Update is called once per frame
    void Update()
    {
        m_SpawnTimer += Time.deltaTime;
        if (m_FirstSpawn)
        {
            m_FirstSpawn = false;
            for (int i = 0; i < m_MaxCarePackagesActive; ++i)
            {
                SpawnRandomCarePackage();
            }
            m_ActiveCarePackages = m_MaxCarePackagesActive;
        }
        else if (m_ActiveCarePackages < m_MaxCarePackagesActive &&
                m_SpawnTimer >= m_TimeToGoOff)
        {
            SpawnRandomCarePackage();
            m_SpawnTimer = 0.0f;
        }
        List<GameObject> needsFixing = CarePackagesNeedFixing();
        if (needsFixing != null && needsFixing.Count > 0)
        {
            for (int i = 0; i < needsFixing.Count; ++i)
            {
                Transform FixSpawn = RelocationSpawn(needsFixing[i]);
                needsFixing[i].transform.position = FixSpawn.position;
            }
        }
    }

    private Transform RelocationSpawn(GameObject carePackage)
    {
        GameObject[] fixSpawns = GameObject.FindGameObjectsWithTag("FixSpawn");
        Transform relocationSpawn = null;
        if (fixSpawns.Length > 0)
        {
            float minDist = 999f;
            for (int i = 0; i < fixSpawns.Length; ++i)
            {
                float testDist = Vector3.Distance(carePackage.transform.position, fixSpawns[i].transform.position);
                if (minDist > testDist)
                {
                    minDist = testDist;
                    relocationSpawn = fixSpawns[i].transform;
                }
            }
        }
        return relocationSpawn;
    }

    private void SpawnRandomCarePackage()
    {
        var randomSpawn = Random.Range(0, m_SpawnPoints.Count);
        var randomCarePackage = Random.Range(0, m_CarePackages.Count);
        Transform spawnTransform = m_SpawnPoints[randomSpawn].transform;
        Rigidbody carePackage = m_CarePackages[randomCarePackage];
        CarePackage.PackageType CPType = GetCarePackageTypeByID(randomCarePackage);
        CarePackage.SpawnCarePackage(ref carePackage, spawnTransform, CPType, fromManager: true);
        ++m_ActiveCarePackages;
    }

    private CarePackage.PackageType GetCarePackageTypeByID(int carePackageID)
    {
        switch (carePackageID)
        {
            case 1:
                return CarePackage.PackageType.Health;
            case 2:
                return CarePackage.PackageType.ThreeBurst;
            case 3:
                return CarePackage.PackageType.Speed;
            case 4:
                return CarePackage.PackageType.ConeShot;
            case 5:
                return CarePackage.PackageType.BigBullet;
            case 6:
                return CarePackage.PackageType.AlienSwarmBullet;
            default:
                return CarePackage.PackageType.Bullet;
        }
    }

    //Used to gather all the care packages active and get rid of them for resetting the 
    //scene every round
    public void Reset()
    {
        m_ActiveCarePackages = 0;
        GameObject[] AllCarePackages = GameObject.FindGameObjectsWithTag("CarePackage");
        for (int i = 0; i < AllCarePackages.Length; i++)
        {
            Destroy(AllCarePackages[i]);
        }
        m_FirstSpawn = true;
    }

    List<GameObject> CarePackagesNeedFixing()
    {
        float threshold = 1.0f; //height of a care package sitting on a heli-pad (it's the lowest height that a Tank can't reach)
        List<GameObject> objectsToFix = null;
        GameObject[] allCarePackages = GameObject.FindGameObjectsWithTag("CarePackage");
        if (allCarePackages != null && allCarePackages.Length != 0)
        {
            objectsToFix = new List<GameObject>();
            for (int i = 0; i < allCarePackages.Length; i++)
            {
                if (allCarePackages[i].transform.position.y >= threshold)
                {
                    if (allCarePackages[i].GetComponent<Rigidbody>().IsSleeping())
                    {
                        objectsToFix.Add(allCarePackages[i]);
                    }
                }
            }
        }
        return objectsToFix;
    }

    public void DecreaseActiveCarePackages()
    {
        --m_ActiveCarePackages;
    }
}
