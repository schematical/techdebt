using UnityEngine;

namespace UI
{
    public class UICoin: MonoBehaviour
    {
        public enum State { Falling, Landed, Leaving}
        protected State state = State.Falling;
        public BoxCollider2D boxCollider2D;
        public Animator animator;
        void Update()
        {
            // transform.position = new Vector3(transform.position.x, transform.position.y - Time.unscaledDeltaTime * 100, transform.position.z);
            if (transform.position.y < 0)
            {
                gameObject.SetActive(false);
            }
        }

        protected void MarkLanded()
        {
            state = State.Landed;
            animator.SetBool("hasLanded", true);
            
        }
        protected void MarkFalling()
        {
            state = State.Falling;
            animator.SetBool("hasLanded", false);
            
        } 
        protected void MarkLeaving()
        {
            state = State.Leaving;
            animator.SetBool("hasLanded", false);
            
        }
        public bool HasLanded()
        {
            return state == State.Landed;
        }
        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (state == State.Leaving)
            {
                return;
            }
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
            Rigidbody2D rigidbody2D =  GetComponent<Rigidbody2D>();
            rigidbody2D.angularVelocity = 0;
            rigidbody2D.rotation = 0;
            rigidbody2D.linearVelocity = Vector2.zero;
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

        public void Explode()
        {
            boxCollider2D.enabled = false;
            int v = 150;
            Rigidbody2D rigidbody2D =  GetComponent<Rigidbody2D>();
            rigidbody2D.linearVelocity = new Vector2(Random.Range(-1 * v, 0), v);
            rigidbody2D.angularVelocity = Random.Range(-1 * 180, 180);
            MarkLeaving();

        }
    }
}