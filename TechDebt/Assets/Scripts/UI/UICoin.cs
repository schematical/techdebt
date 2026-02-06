using UnityEngine;

namespace UI
{
    public class UICoin: MonoBehaviour
    {
        protected bool hasLanded = false;
        public BoxCollider2D boxCollider2D;
        public Animator animator;
        void Update()
        {
            // transform.position = new Vector3(transform.position.x, transform.position.y - Time.unscaledDeltaTime * 100, transform.position.z);
        }

        protected void MarkLanded()
        {
            hasLanded = true;
            animator.SetBool("hasLanded", hasLanded);
            
        }
        protected void MarkFalling()
        {
            hasLanded = false;
            animator.SetBool("hasLanded", hasLanded);
            
        }
        public bool HasLanded()
        {
            return hasLanded;
        }
        public void OnTriggerEnter2D(Collider2D collision)
        {
          
            UICoin  coin = collision.GetComponent<UICoin>();
            if (coin !=null && coin.HasLanded())
            {
               MarkLanded();
            }
            else if(collision.gameObject.name == "BottomTrigger")
            {
                MarkLanded();
            }
        }

        public void Initialize(bool isFalling = true)
        {
            boxCollider2D.enabled = true;
            if (isFalling)
            {
                MarkFalling();
            }
            else
            {
                MarkLanded();
            }
        }

        public void Spend()
        {
            boxCollider2D.enabled = false;
        }
    }
}