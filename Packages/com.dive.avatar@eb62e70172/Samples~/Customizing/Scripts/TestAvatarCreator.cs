using UnityEngine;
using System.Collections;

namespace PlugVR.AvatarCustomizing
{
    /// <summary>
    /// 아바타 생성기를 상속받은 샘플 코드
    /// </summary>
    public class TestAvatarCreator : AvatarCreator
    {
        /// <summary>
        /// 코루틴 실행
        /// </summary>
        private void Start()
        {
            StartCoroutine(DelayCreateAvatar());
        }

        /// <summary>
        /// 3초 뒤 랜덤한 아이템 배열의 아바타가 생성되는 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayCreateAvatar()
        {
            yield return new WaitForSeconds(3.0f);

            var randomAvatar = AvatarDatabase.GetRandomAvatarArray();

            var avatarData = CreateAvatar(transform, randomAvatar);
            AvatarCustomizeBase.InitAvatar(avatarData, randomAvatar, transform);
        }

        /// <summary>
        /// 아바타 생성
        /// </summary>
        /// <param name="target">생성 위치</param>
        /// <param name="itemArray">적용 아바타 배열</param>
        /// <param name="actorNumber">포톤 네트워크 ActorNumber</param>
        /// <param name="isIgnore">본인 아바타인 경우 true, 그렇지 않은 경우 false</param>
        /// <returns>생성된 아바타 데이터</returns>
        public override AvatarData CreateAvatar(Transform target, int[] itemArray, int actorNumber, bool isIgnore = false)
        {
            Debug.LogError($"현재 Photon 네트워크 사용중이 아닙니다.");
            return new AvatarData();
        }

        /// <summary>
        /// 아바타 생성
        /// </summary>
        /// <param name="target">생성 위치</param>
        /// <param name="itemArray">적용 아바타 배열</param>
        /// <param name="isIgnore">본인 아바타인 경우 true, 그렇지 않은 경우 false</param>
        /// <returns>생성된 아바타 데이터</returns>
        public override AvatarData CreateAvatar(Transform target, int[] itemArray, bool isIgnore = false)
        {
            var face = AvatarDatabase.GetFaceItemData(itemArray[(int) Appearance.Face]);
            var faceObj = Instantiate(face.itemObject, target);
            var faceItem = faceObj.GetComponent<ItemFace>();
            var neck = faceObj.transform.GetChild(1).GetChild(0);

            faceItem.Initialize(itemArray);

            var hair = AvatarDatabase.GetHairItemData(itemArray[(int) Appearance.Hair]);
            var hairObj = Instantiate(hair.itemObject, target);
            hairObj.transform.parent = neck;
            var hairItem = hairObj.GetComponent<ItemHair>();

            hairItem.Initialize(itemArray);


            var cloth = AvatarDatabase.GetClothData(itemArray[(int) Appearance.Clothes]);
            var clothObj = Instantiate(cloth.itemObject, target);

            CustomItemData acc = null;
            GameObject accObj = null;

            if (AvatarDatabase.GetItemDictionary(Appearance.Accessory).Count > 0)
            {
                acc = AvatarDatabase.GetAccessoryData(itemArray[(int) Appearance.Accessory]);
                accObj = Instantiate(acc.itemObject, target);
                accObj.transform.parent = neck;

                var accItem = accObj.GetComponent<ItemAccessory>();
                accItem.Initialize(itemArray);
            }

            var playerAvatarData = new AvatarData()
            {
                itemHair = hairItem,
                itemFace = faceItem,
                clothObj = clothObj,
                accessoryObj = accObj,
                itemAccessory = accObj.GetComponent<ItemAccessory>(),
                neckTransform = neck
            };

            return playerAvatarData;
        }
    }
}