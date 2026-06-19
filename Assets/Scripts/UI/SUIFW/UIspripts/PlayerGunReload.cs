using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGunReload : MonoBehaviour
{
    Transform reloadmove;
    bool reload = false;
    float startpos = -481;
    float arrivepos = -406;
    float reloadtime = 0;
    float reloadpasstime = 0;
    float reloadspeed = 0;
    private void Start()
    {
        reload = false;
        reloadmove = transform.GetChild(0);

        gameObject.SetActive(false);
    }
    void on_event_handler(string name, object udata)
    {
        reloadmove.localPosition = new Vector3(startpos, reloadmove.localPosition.y,0) ;
        reloadtime = (float)udata;
        reloadspeed = (arrivepos - startpos) / reloadtime;
        this.gameObject.SetActive(true);
        reloadpasstime = 0;
        reload=true;
    }

    private void Update()
    {
        if (reload)
        {
            reloadpasstime += Time.deltaTime;
            reloadmove.localPosition = new Vector3(startpos + reloadspeed * reloadpasstime, reloadmove.localPosition.y, 0);
            if (reloadpasstime > reloadtime)
            {
                this.gameObject.SetActive(false);
                reload = false;
            }
        }
    }
}
