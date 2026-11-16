using UnityEngine;

namespace Robbie111.AtlasImage.Samples
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private AtlasImage atlasImage;

        public void ChangeSpriteName(string spriteName)
        {
            if (!atlasImage)
            {
                return;
            }

            atlasImage.spriteName = spriteName;
        }
    }
}
