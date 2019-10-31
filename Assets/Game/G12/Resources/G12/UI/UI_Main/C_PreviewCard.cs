
using CC_Game;
using CC_Util;
using UI00;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using System.IO;
using UnityEngine.EventSystems;

namespace G12 {
    /// <summary>
    /// 第三层卡片
    /// </summary>
    public class C_PreviewCard : MonoBehaviour {

        public static C_ObjectPool<C_Int4,C_PreviewCard> o_ObjectPool = new C_ObjectPool<C_Int4,C_PreviewCard>();
        public static C_Int4 previewOnlyAssetsPath = C_Int4.GetInt4(-12, 14, 2, 0);
        public static void S_InitObjectPool() {
            o_ObjectPool.d_ObjCreateEvent = delegate (C_Int4 onlyAssetsPath) {
                GameObject backBox = C_128_AssetsCore.GetInstance.S_GetModel(onlyAssetsPath, 10);
                C_PreviewCard selectCard = backBox.AddComponent<C_PreviewCard>();
                selectCard.S_Init();
                selectCard.rectTransform.SetParent(o_ObjectPool.ObjectPoolFather);
                return selectCard;
            };
            //o_ObjectPool.d_ObjRebornEvent = delegate (C_PreviewCard nn) {
            //    nn.gameObject.SetActive(true);
            //};
            o_ObjectPool.d_ObjDeathEvent = delegate (C_PreviewCard nn) {
                //Debug.LogErrorFormat("C_PreviewCard.SetActive(false).________");
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
                Debug.Log("C_PreviewCard__点击按下_________________"+ assetsPath);
                S_PressEvent(uGUI.o_PointerEventData);
            };
            C_UGUI.S_Get(rectTransform).d_Lift = delegate (C_UGUI uGUI) {
                S_LiftEvent(uGUI.o_PointerEventData);
            };
            C_UGUI.S_Get(rectTransform).d_DragEvent = delegate (C_UGUI uGUI) {
                S_DragEvent(uGUI.o_PointerEventData);
            };



            C_UGUI.S_Get(dragButton).d_Press = delegate (C_UGUI uGUI) {
                Debug.Log("C_PreviewCard_拖动按下_________________"+ assetsPath);
                transform.SetSiblingIndex(10000);
                card.rectTransform.SetSiblingIndex(10000);
                isDraging = true;
                fingerPressPosi = uGUI.o_PointerEventData.position.S_ToVector3();
                fingerPressDistance= Vector3.Distance(transform.position, uGUI.o_PointerEventData.position.S_ToVector3());

                //C_UIBase.Mono.StartCoroutine(I_DragZoom());
            };
            C_UGUI.S_Get(dragButton).d_Lift = delegate (C_UGUI uGUI) {
                isDraging = false;
                float zoomDistance = Vector3.Distance(transform.position, uGUI.o_PointerEventData.position.S_ToVector3());
                zoomSize= zoomSize * (zoomDistance / fingerPressDistance);
            };
            C_UGUI.S_Get(dragButton).d_DragEvent = delegate (C_UGUI uGUI) {
                float zoomDistance = Vector3.Distance(transform.position, uGUI.o_PointerEventData.position.S_ToVector3());
                S_SetSize((radius / 2) * zoomSize * (zoomDistance / fingerPressDistance));
            };
        }


        RectTransform currentSelectCard;
        Vector3 pressPosi, fingerPressPosi;
        public void S_PressEvent(PointerEventData pointerEventData) {
            transform.SetSiblingIndex(10000);
            card.rectTransform.SetSiblingIndex(10000);
            currentSelectCard = rectTransform;
            fingerPressPosi = pointerEventData.position.S_ToVector3();
            pressPosi = currentSelectCard.position;
           // C_UIBase.Mono.StartCoroutine(I_DragOffect());
        }
        public void S_LiftEvent(PointerEventData pointerEventData) {
            currentSelectCard = null;
        }
        public void S_DragEvent(PointerEventData pointerEventData) {
            Vector3 offect = pointerEventData.position.S_ToVector3() - fingerPressPosi;
            currentSelectCard.position = pressPosi + offect;

            S_RefreshLine();
        }
        //IEnumerator 
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




        string assetsPath;
        float radius;
        C_SelectCard.C_Card0 card;
        public void S_Open(C_SelectCard.C_Card0 card, string assetsPath, int assetsType, Vector3 direction, float radius) {
            this.card = card;
            this.radius = radius;
            transform.SetParent(card.rectTransform);
            transform.position = card.rectTransform.position;
            rectTransform.sizeDelta = Vector2.one * radius ;
            name.text = Path.GetFileName(assetsPath);
            this.assetsPath = assetsPath;
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
            //Debug.LogErrorFormat("C_PreviewCard.SetActive(false).________");
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
            //Debug.LogErrorFormat("C_PreviewCard.SetActive(false)._______3_");
            line.gameObject.SetActive(false);
            transform.DOMove(card.rectTransform.position, 0.3f).OnComplete(delegate () {
                o_ObjectPool.S_SetToDeathObj(previewOnlyAssetsPath, this);
            });
        }
    }
}
