using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC_Game;
using CC_Util;
using UI00;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace G12 {
    /// <summary>
    /// 第三层卡片
    /// </summary>
    public class C_PreviewCard : MonoBehaviour {

        public static C_ObjectPool<C_PreviewCard> o_ObjectPool = new C_ObjectPool<C_PreviewCard>();
        public static C_Int4 previewOnlyAssetsPath = new C_Int4(-12, 14, 2, 0);
        public static void S_InitObjectPool() {
            o_ObjectPool.d_ObjCreateEvent = delegate (C_Int4 onlyAssetsPath) {
                GameObject backBox = C_128_AssetsCore.GetInstance.S_GetModel(onlyAssetsPath, 10);
                C_PreviewCard selectCard = backBox.AddComponent<C_PreviewCard>();
                selectCard.S_Init();
                selectCard.rectTransform.SetParent(o_ObjectPool.ObjectPoolFather);
                return selectCard;
            };
            o_ObjectPool.d_ObjRebornEvent = delegate (C_PreviewCard nn) {
                nn.gameObject.SetActive(true);
            };
            o_ObjectPool.d_ObjDeathEvent = delegate (C_PreviewCard nn) {
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
               // card.S_CancelSelect();
                S_Close();
            });

            C_UGUI.S_Get(rectTransform).d_Press = delegate (C_UGUI uGUI) {
                S_PressEvent(rectTransform);
            };
            C_UGUI.S_Get(rectTransform).d_Lift = delegate (C_UGUI uGUI) {
                S_LiftEvent();
            };
        }


        static RectTransform currentSelectCard;
        static Vector3 pressPosi, fingerPressPosi;
        public void S_PressEvent(RectTransform rectTransforms) {
            currentSelectCard = rectTransforms;
            fingerPressPosi = Input.mousePosition;
            pressPosi = currentSelectCard.position;
            C_UIBase.Mono.StartCoroutine(I_DragUpdate());
        }
        public void S_LiftEvent() {
            currentSelectCard = null;
        }
        IEnumerator I_DragUpdate() {
            if (currentSelectCard == null) {
                yield break;
            }
            Vector3 offect = Input.mousePosition - fingerPressPosi;
            currentSelectCard.position = pressPosi + offect;
            yield return new WaitForSeconds(0);
            C_UIBase.Mono.StartCoroutine(I_DragUpdate());
        }




        float radius;
        C_SelectCard.C_Card0 card;
        public void S_Open(C_SelectCard.C_Card0 card, string assetsPath, int assetsType, float radius) {
            this.card = card;
            this.radius = radius;
            transform.position = card.rectTransform.position;
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
            o_ObjectPool.S_SetToDeathObj(previewOnlyAssetsPath, this);
        }
    }
}
