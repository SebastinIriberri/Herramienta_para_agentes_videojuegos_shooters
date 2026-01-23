using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] PlayerShooter shooter;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] string format = "{0}/{1}";

    void Awake()
    {
        if (ammoText == null) ammoText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (ammoText == null) return;

        if (shooter == null)
        {
            ammoText.text = "--/--";
            return;
        }

        if (!shooter.useAmmo)
        {
            ammoText.text = "?";
            return;
        }

        ammoText.text = string.Format(format, shooter.CurrentAmmo, shooter.clipSize);
    }
}
