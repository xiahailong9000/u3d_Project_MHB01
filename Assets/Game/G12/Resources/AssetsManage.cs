using System.Collections;
using System.Collections.Generic;
using UI00;
using UnityEngine;
namespace G12 {
    public class AssetsManage {
        public static AssetsManage GetInstance {
            get {
                if (instance == null) {
                    instance = new AssetsManage();
                    instance.Init();
                }
                return AssetsManage.instance;
            }
        }
        static AssetsManage instance;
        public C_SpriteAtlas sprite_oot;
        void Init() {
            sprite_oot = C_SpriteAtlas.GetSpriteAtlas("G12/Sprite/Sprite_oot");
        }
    }
}