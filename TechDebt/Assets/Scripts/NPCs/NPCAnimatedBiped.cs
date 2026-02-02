using DefaultNamespace;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace NPCs
{
    public class NPCAnimatedBiped: NPCBase
    {
        public string headSprite = "Head1";
        public SpriteRenderer headSpriteRenderer;
        public SpriteRenderer bodySpriteRenderer; 
        public SpriteResolver headSpriteResolver;
        public SpriteLibrary bodySpriteLibrary;
        public SpriteLibrary faceSpriteLibrary;
        public SpriteResolver faceSpriteResolver;
        public SpriteRenderer faceSpriteRenderer;
        public HeadSpriteCollection  headSpriteCollection;

        void Awake()
        {
            Randomize();
        }
        public void Randomize()
        {
            headSpriteCollection = GameManager.Instance.SpriteManager.GetHeadSpriteCollection();
            // bodySpriteLibrary.spriteLibraryAsset = GameManager.Instance.SpriteManager.GetRandomBodySpriteLibraryAsset();
        }
        protected override void FaceLeft()
        {
            headSpriteRenderer.flipX = !flipMoventSprite; // Moving left
            bodySpriteRenderer.flipX = !flipMoventSprite; // Moving left
            faceSpriteRenderer.flipX = !flipMoventSprite; // Moving left
        }
        protected override void FaceRight() {
            headSpriteRenderer.flipX = flipMoventSprite; // Moving right
            bodySpriteRenderer.flipX = flipMoventSprite; // Moving right
            faceSpriteRenderer.flipX = flipMoventSprite; // Moving right
        }

        protected override void FaceDown()
        {
      
            headSpriteRenderer.transform.position = new Vector3(
                headSpriteRenderer.transform.position.x,
                headSpriteRenderer.transform.position.y,
                -0.1f
            );
            // headSpriteResolver.SetCategoryAndLabel(headSprite, "Front");
            headSpriteRenderer.sprite = headSpriteCollection.headFront;
            faceSpriteRenderer.gameObject.SetActive(true);
        }

        protected override void FaceUp()
        {
            headSpriteRenderer.transform.position = new Vector3(
                headSpriteRenderer.transform.position.x,
                headSpriteRenderer.transform.position.y,
                0.1f
            );
            // headSpriteResolver.SetCategoryAndLabel(headSprite, "Back");
            headSpriteRenderer.sprite = headSpriteCollection.headBack;
            faceSpriteRenderer.gameObject.SetActive(false);
        }
    }
}