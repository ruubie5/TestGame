using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Completed
{
    public class DoorScript : MonoBehaviour
    {
        public Vector2 roomOrigin;
        public bool openNew = true;
        public AudioClip openSound;

        void Start()
        {            
            if (openNew == false)
            {
                foreach (var item in FindObjectOfType<BoardManager>().cantplaceObjects.ToList())
                {
                    if (transform.position == item.transform.position && item != null)
                    {
                        if (item.GetComponent<DoorScript>() == null)
                        {
                            FindObjectOfType<BoardManager>().cantplaceObjects.Remove(item);                           
                            Destroy(item);                            
                        }
                    }
                }
            }
        }
    }
}
