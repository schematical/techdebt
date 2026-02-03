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
        public SpriteLibrary headSpriteLibrary;
        public SpriteLibrary bodySpriteLibrary;
        public SpriteLibrary faceSpriteLibrary;
        public SpriteResolver faceSpriteResolver;
        public SpriteRenderer faceSpriteRenderer;
        public string  headSpriteLibraryCategory;

        void Awake()
        {
            Randomize();
        }
        public void Randomize()
        {
            headSpriteLibrary.spriteLibraryAsset = GameManager.Instance.SpriteManager.headSpriteLibraryAsset;
            NPCBipedAssets assets =  GameManager.Instance.SpriteManager.GetRandomNPCBipedAssets();
            headSpriteLibraryCategory = assets.headSpriteLibraryCategory;
            bodySpriteLibrary.spriteLibraryAsset = assets.bodySpriteLibraryAsset;
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
                transform.position.z - 0.0001f
            );
            faceSpriteRenderer.transform.position = new Vector3(
                faceSpriteRenderer.transform.position.x,
                faceSpriteRenderer.transform.position.y,
                transform.position.z - 0.0002f
            );
            bodySpriteRenderer.transform.position = new Vector3(
                bodySpriteRenderer.transform.position.x,
                bodySpriteRenderer.transform.position.y,
                transform.position.z
            );
            headSpriteResolver.SetCategoryAndLabel(
                headSpriteLibraryCategory,
                "Front"
            );
            faceSpriteRenderer.gameObject.SetActive(true);
        }

        protected override void FaceUp()
        {
            headSpriteRenderer.transform.position = new Vector3(
                headSpriteRenderer.transform.position.x,
                headSpriteRenderer.transform.position.y,
                transform.position.z + 0.0001f
            );
            bodySpriteRenderer.transform.position = new Vector3(
                bodySpriteRenderer.transform.position.x,
                bodySpriteRenderer.transform.position.y,
                transform.position.z
            );
            headSpriteResolver.SetCategoryAndLabel(
                headSpriteLibraryCategory,
                "Back"
            );
            faceSpriteRenderer.gameObject.SetActive(false);
        }
    }
}