
using UnityEngine;

public class PrivacyPolicyButton : MonoBehaviour
{
    // Replace this with your actual Privacy Policy URL
    [SerializeField] private string privacyPolicyURL =
        "https://thelostdraftprivacyypolicy.my.canva.site/";

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(privacyPolicyURL);
    }
}
