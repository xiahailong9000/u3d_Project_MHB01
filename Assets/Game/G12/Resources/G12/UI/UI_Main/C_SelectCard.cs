using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC_Game;
using UI00;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace G12 {
    public class C_SelectCard : MonoBehaviour {
        public static C_ObjectPool<C_SelectCard> o_ObjectPool = new C_ObjectPool<C_SelectCard>();
        public static C_Int4 backOnlyAssetsPath = new C_Int4(-12, 14, 1, 0);
        public static void S_InitObjectPool() {
            o_ObjectPool.d_ObjCreateEvent = delegate (C_Int4 onlyAssetsPath) {
                GameObject backBox = C_128_AssetsCore.GetInstance.S_GetModel(onlyAssetsPath, 10);
                C_SelectCard selectCard = backBox.AddComponent<C_SelectCard>();
                selectCard.S_Init();
                return selectCard;
            };
            o_ObjectPool.d_ObjRebornEvent = delegate (C_SelectCard nn) {
                nn.gameObject.SetActive(true);
            };
            o_ObjectPool.d_ObjDeathEvent = delegate (C_SelectCard nn) {
                nn.transform.SetParent(null);
                nn.gameObject.SetActive(false);
            };
        }
        public RawImage videoImage;
        public RectTransform rectTransform, boxTransform, videoRectTransform;
        public VideoPlayer videoPlayer;
        Button closeButton;
        void S_Init() {
            rectTransform = GetComponent<RectTransform>();
            boxTransform = transform.Find("box").GetComponent<RectTransform>();
            videoImage = gameObject.transform.Find("video01").GetComponent<RawImage>();
            videoRectTransform = videoImage.GetComponent<RectTransform>();
            videoPlayer = videoImage.GetComponent<VideoPlayer>();
            closeButton = transform.Find("closeButton").GetComponent<Button>();
            videoPlayer.playOnAwake = false;
            videoPlayer.source = VideoSource.Url;
            //  videoPlayer.url = "file:///" + assetsPath;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.sendFrameReadyEvents = true;
            bool isStartframe = true;
            videoPlayer.frameReady += delegate (VideoPlayer source, long frameIdx) {
                videoImage.texture = source.texture;
                if (isStartframe) {
                    isStartframe = false;
                    S_SetSize(videoRectTransform, source.texture, radius);
                }
            };
            closeButton.onClick.AddListener(delegate () {
                card.S_CancelSelect();
                //S_Close();
            });
        }
        float radius;
        C_Crad card;
        public void S_Open(C_Crad card, string assetsPath, int assetsType, float radius) {
            this.card = card;
            this.radius = radius;
            transform.SetParent(card.rectTransform);
            transform.localPosition = Vector3.zero;
            // transform.localScale = Vector3.one * radius / 200;
            rectTransform.sizeDelta = Vector2.one * radius * 2;
            boxTransform.sizeDelta = Vector2.one * (radius * 2f + 60);
            videoImage.texture = null;
            if (assetsType < 10) {
                C_AssetsLoad.S_WWW(C_UIBase.Mono, "file:///" + assetsPath, delegate (WWW www) {
                    if (www == null) {
                        Debug.LogErrorFormat("读取图片错误__{0}__", assetsPath);
                        videoImage.texture = null;
                    } else {
                        Texture2D tt = www.texture;
                        //  tt.format = TextureFormat.ARGB32;
                        videoImage.texture = tt;
                        S_SetSize(videoRectTransform, www.texture, radius);
                    }
                });
            } else {
                videoPlayer.url = "file:///" + assetsPath;
                videoPlayer.Play();
            }
        }
        void S_SetSize(RectTransform rectTransform0, Texture texture, float radius) {
            Vector2 size;
            float thumbnailLenght = radius * 1.6f;
            if (texture.width >= texture.height) {
                size = new Vector2(thumbnailLenght, texture.height * thumbnailLenght / texture.width);
            } else {
                size = new Vector2(texture.width * thumbnailLenght / texture.height, thumbnailLenght);
            }
            //Vector2 size = new Vector2(texture.width, texture.height) * 512 / C_Ttttt.thumbnailLenght;
            rectTransform0.sizeDelta = size;
        }
        public void S_Close() {
            o_ObjectPool.S_SetToDeathObj(backOnlyAssetsPath, this);
        }
    }
}
