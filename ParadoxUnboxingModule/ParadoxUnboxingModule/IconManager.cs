using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ParadoxUnboxingModule
{
    public class IconManager : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(this);
            Player.onPlayerCreated += (p) => StartCoroutine(LoadItems());
        }

        public IEnumerator LoadItems()
        {
            yield return new WaitForSeconds(5f);
            var assets = Assets.find(EAssetType.ITEM);

            foreach (Asset asset in assets)
            {
                try
                {
                    var item = new Item(asset.id, true);
                    ItemTool.getIcon(asset.id, item.quality, item.state, (int handle, Texture2D texture) =>
                    {
                        RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                        Graphics.Blit(texture, tmp);
                        RenderTexture previous = RenderTexture.active;
                        RenderTexture.active = tmp;
                        Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                        readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                        readableTexture.Apply();
                        RenderTexture.active = previous;
                        RenderTexture.ReleaseTemporary(tmp);

                        byte[] bytes = readableTexture.EncodeToPNG();
                        AddUnturnedIcon(asset.id, bytes);
                        UnityEngine.Object.Destroy(readableTexture);
                    });
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to load icon for item {asset.id}: {ex.Message}");
                }
            }
        }

        public void AddUnturnedIcon(ushort id, byte[] icon)
        {
            if (File.Exists(Main.Instance.path + $"{id}.png"))
                File.Delete(Main.Instance.path + $"{id}.png");
            File.WriteAllBytes(Main.Instance.path + $"{id}.png", icon);
        }
    }
}
