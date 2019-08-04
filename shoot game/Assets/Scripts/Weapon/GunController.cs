using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunController : MonoBehaviour
{

    public Transform weaponHold;
    public Gun[] allGunPrefabs;

    private List<Gun> allGuns;
    private Gun equippedGun;
    private float weaponIndex = -1;

    private void Awake()
    {
        allGuns = new List<Gun>();
    }

    public void Start()
    {
        foreach (Gun gunPrefab in allGunPrefabs)
        {
            Gun gun = Instantiate(gunPrefab, weaponHold.position, weaponHold.rotation);
            gun.transform.SetParent(weaponHold);
            gun.gameObject.SetActive(false);
            allGuns.Add(gun);
        }

        SelectGun(0);
    }

    public void SelectGun(int _weaponIndex)
    {
        if (weaponIndex == _weaponIndex)
            return;

        int i = 0;
        foreach (Gun gun in allGuns)
        {
            if (i == _weaponIndex)
            {
                weaponIndex = _weaponIndex;
                equippedGun = gun;
                gun.gameObject.SetActive(true);
            }
            else
            {
                gun.gameObject.SetActive(false);
            }
            i++;
        }
    }

    public void OnTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public float GunHeight
    {
        get
        {
            return weaponHold.position.y;
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if (equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }
}