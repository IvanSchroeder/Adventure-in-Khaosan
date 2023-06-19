using UnityEngine;

public class ShowOnMobile : MonoBehaviour {
    void Start() {
        gameObject.SetActive(Application.isMobilePlatform);
    }
}
