using UnityEngine;

namespace UI
{
    public class UICoin: MonoBehaviour
    {
        public enum State { Falling, Landed, Leaving}
        protected State state = State.Falling;
        public BoxCollider2D boxCollider2D;
        public Animator animator;
        protected float contactImuneCount = -100;
        protected UICoin coinAbove;
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
            Debug.Log("MarkLanded");
            if (state == State.Landed)
            {
                return;
            }
            state = State.Landed;
            animator.SetBool("hasLanded", true);
            
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            if (coinAbove != null)
            {
                coinAbove.MarkLanded();
            }
            
        }
        protected void MarkFalling()
        {
            state = State.Falling;
            animator.SetBool("hasLanded", false);
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            /*if (coinAbove != null)
            {
                coinAbove.Nudge();
            }*/
            
        } 
        protected void MarkLeaving()
        {
            state = State.Leaving;
            animator.SetBool("hasLanded", false);
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            /*if (coinAbove != null)
            {
                coinAbove.Nudge();
            }*/
            
        }

        public void Nudge()
        {
            if (state == State.Falling)
            {
                return;
            }
            // state = State.Falling;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            
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

           
            UICoin coin = collision.GetComponent<UICoin>();
            if (coin !=null && coin.HasLanded())
            { 
               coin.SetCoinAbove(this);
               MarkLanded();
            }
            else if(collision.gameObject.name == "BottomTrigger")
            {
           
             
                MarkLanded();
            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            SetCoinAbove(null);
        }

        public void SetCoinAbove(UICoin coin)
        {
            Debug.Log("SetCoinAbove");
            coinAbove = coin;
        }
        public void Initialize(bool isFalling = true)
        {
       
            Rigidbody2D rigidbody2D =  GetComponent<Rigidbody2D>();
            rigidbody2D.angularVelocity = 0;
            rigidbody2D.rotation = 0;
            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            boxCollider2D.enabled = true;
            coinAbove = null;
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
            int v = 5;
            Rigidbody2D rigidbody2D =  GetComponent<Rigidbody2D>();
            rigidbody2D.linearVelocity = new Vector2(Random.Range(-1 * v, 0), v);
            rigidbody2D.angularVelocity = Random.Range(-1 * 180, 180);
            MarkLeaving();

        }
    }
}