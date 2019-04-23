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
using DG.Tweening;
using System.IO;

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
                nn.gameObject.SetActive(false);
            };
        }
        public RawImage videoImage;
        public RectTransform rectTransform, videoRectTransform,line;
        public VideoPlayer videoPlayer;
        Button closeButton;
        Transform dragButton;
        Text name;
        void S_Init() {
            rectTransform = GetComponent<RectTransform>();
            videoImage = gameObject.transform.Find("video01").GetComponent<RawImage>();
            videoRectTransform = videoImage.GetComponent<RectTransform>();
            line = transform.Find("line").GetComponent<RectTransform>();
            videoPlayer = videoImage.GetComponent<VideoPlayer>();
            closeButton = videoImage.transform.Find("closeButton").GetComponent<Button>();
            dragButton = videoImage.transform.Find("dragButton");
            name= videoImage.transform.Find("name").GetComponent<Text>();
            line.localPosition = Vector3.zero;
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
                   // sizeDelta = source.texture.texelSize;
                    sizeDelta = new Vector2(source.texture.width, source.texture.height);
                    S_SetSize(radius/2);
                }
            };
            closeButton.onClick.AddListener(delegate () {
                card.S_CancelSelect();
              //  S_Close();
            });

            C_UGUI.S_Get(rectTransform).d_Press = delegate (C_UGUI uGUI) {
                S_PressEvent();
            };
            C_UGUI.S_Get(rectTransform).d_Lift = delegate (C_UGUI uGUI) {
                S_LiftEvent();
            };
            C_UGUI.S_Get(dragButton).d_Press = delegate (C_UGUI uGUI) {
                transform.SetSiblingIndex(10000);
                card.rectTransform.SetSiblingIndex(10000);
                isDraging = true;
                fingerPressPosi = C_Tools.mousePosition;
                fingerPressDistance= Vector3.Distance(transform.position, C_Tools.mousePosition);

                C_UIBase.Mono.StartCoroutine(I_DragZoom());
            };
            C_UGUI.S_Get(dragButton).d_Lift = delegate (C_UGUI uGUI) {
                isDraging = false;
                float zoomDistance = Vector3.Distance(transform.position, C_Tools.mousePosition);
                zoomSize= zoomSize * (zoomDistance / fingerPressDistance);
            };
        }


        RectTransform currentSelectCard;
        Vector3 pressPosi, fingerPressPosi;
        public void S_PressEvent() {
            transform.SetSiblingIndex(10000);
            card.rectTransform.SetSiblingIndex(10000);
            currentSelectCard = rectTransform;
            fingerPressPosi = C_Tools.mousePosition;
            pressPosi = currentSelectCard.position;
            C_UIBase.Mono.StartCoroutine(I_DragOffect());
        }
        public void S_LiftEvent() {
            currentSelectCard = null;
        }
        IEnumerator I_DragOffect() {
            if (currentSelectCard == null) {
                yield break;
            }
            Vector3 offect = C_Tools.mousePosition - fingerPressPosi;
            currentSelectCard.position = pressPosi + offect;

            S_RefreshLine();

            yield return new WaitForSeconds(0);
            C_UIBase.Mono.StartCoroutine(I_DragOffect());
        }
        void S_RefreshLine() {
            Vector3 v0 = card.rectTransform.position;
            Vector3 v1 = rectTransform.position;
            Vector3 vv = v0 - v1;
            float angle = Vector3.Angle(vv, Vector3.right);
            if (vv.y < 0) {
                angle = 360 - angle;
            }
            line.eulerAngles = new Vector3(0, 0, angle);
            float distance = Vector3.Distance(v0, v1);
            line.sizeDelta = new Vector2(distance, 35);
        }

        float zoomSize;
        float fingerPressDistance;
        bool isDraging;
        IEnumerator I_DragZoom() {
            if (isDraging == false) {
                yield break;
            }
            float zoomDistance = Vector3.Distance(transform.position, C_Tools.mousePosition);
            S_SetSize((radius / 2)* zoomSize*(zoomDistance/ fingerPressDistance));
            yield return new WaitForSeconds(0);
            C_UIBase.Mono.StartCoroutine(I_DragZoom());
        }




        float radius;
        C_SelectCard.C_Card0 card;
        public void S_Open(C_SelectCard.C_Card0 card, string assetsPath, int assetsType, Vector3 direction, float radius) {
            this.card = card;
            this.radius = radius;
            transform.SetParent(card.rectTransform);
            transform.position = card.rectTransform.position;
            rectTransform.sizeDelta = Vector2.one * radius ;
            name.text = Path.GetFileName(assetsPath);
            zoomSize = 1;
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
                       // sizeDelta = www.texture.texelSize;
                        sizeDelta = new Vector2(www.texture.width, www.texture.height);
                        S_SetSize(radius/2);
                    }
                });
            } else {
                videoPlayer.url = "file:///" + assetsPath;
                videoPlayer.Play();
            }
            transform.localScale = Vector3.one * 0.3f;
            transform.DOScale(Vector3.one, 0.3f);
            line.gameObject.SetActive(false);
            transform.DOMove(transform.position + direction * radius, 0.3f).OnComplete(delegate () {
                line.gameObject.SetActive(true);
                S_RefreshLine();
            });
        }
        Vector2 sizeDelta;
        void S_SetSize(float radius) {
            Vector2 size;
            float thumbnailLenght = radius * 1.6f;
            if (sizeDelta.x >= sizeDelta.y) {
                size = new Vector2(thumbnailLenght, sizeDelta.y * thumbnailLenght / sizeDelta.x);
            } else {
                size = new Vector2(sizeDelta.x * thumbnailLenght / sizeDelta.y, thumbnailLenght);
            }
            //Vector2 size = new Vector2(texture.width, texture.height) * 512 / C_Ttttt.thumbnailLenght;
            videoRectTransform.sizeDelta = size;
            rectTransform.sizeDelta = size + Vector2.one * 40;// Vector2.one* thumbnailLenght;
        }
        public void S_Close() {
            transform.localScale = Vector3.one;
            transform.DOScale(Vector3.one*0.3f, 0.3f);
            line.gameObject.SetActive(false);
            transform.DOMove(card.rectTransform.position, 0.3f).OnComplete(delegate () {
                o_ObjectPool.S_SetToDeathObj(previewOnlyAssetsPath, this);
            });
        }
    }
}
