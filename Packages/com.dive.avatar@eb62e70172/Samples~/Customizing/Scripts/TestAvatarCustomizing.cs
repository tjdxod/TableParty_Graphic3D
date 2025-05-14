using System;

namespace PlugVR.AvatarCustomizing
{
    /// <summary>
    /// 아바타 커스터마이즈 관리 매니저를 상속받은 샘플 코드
    /// </summary>
    public class TestAvatarCustomizing : AvatarCustomizeBase
    {
        /// <summary>
        /// 특정 부분의 아바타 모델을 변경
        /// </summary>
        /// <param name="appearance">변경할 부분</param>
        /// <param name="index">변경할 모델 인덱스</param>
        /// <param name="isAll">아바타 전체를 변경하는 경우</param>
        /// <param name="isNotUndo">히스토리에 저장하지 않는 경우</param>
        public override void SetItem(Appearance appearance, int index, bool isAll = false, bool isNotUndo = false)
        {
            switch (appearance)
            {
                case Appearance.Hair:
                {
                    if (AvatarData.itemHair == null)
                        return;

                    var localPosition = AvatarData.itemHair.LocalPosition;
                    var localRotation = AvatarData.itemHair.LocalRotation;

                    var hairData = avatarDatabase.GetHairItemData(index);
                    var hairObj = Instantiate(hairData.itemObject, createTarget);

                    if (!isAll)
                    {
                        hairObj.transform.parent = AvatarData.itemFace.transform.GetChild(1).GetChild(0);

                        hairObj.transform.localPosition = localPosition;
                        hairObj.transform.localRotation = localRotation;
                    }
                    else
                    {
                        hairObj.transform.localPosition = localPosition;
                        hairObj.transform.localRotation = localRotation;
                    }

                    var hairItem = hairObj.GetComponent<ItemHair>();
                    hairItem.Initialize(changeAvatarItemArray);

                    Destroy(AvatarData.itemHair.gameObject);

                    AvatarData.itemHair = hairItem;

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.Hair,
                            index = changeAvatarItemArray[0],
                            tick = DateTime.Now.Ticks
                        });
                    }

                    changeAvatarItemArray[0] = index;
                }
                    break;
                case Appearance.Face:
                {
                    if (AvatarData.itemFace == null)
                        return;

                    var localPosition = AvatarData.itemFace.LocalPosition;
                    var localRotation = AvatarData.itemFace.LocalRotation;

                    var faceData = avatarDatabase.GetFaceItemData(index);
                    var faceObj = Instantiate(faceData.itemObject, createTarget);

                    AvatarData.hairOriginPosition = AvatarData.itemHair.LocalPosition;
                    AvatarData.hairOriginRotation = AvatarData.itemHair.LocalRotation;

                    AvatarData.accessoryOriginPosition = AvatarData.itemAccessory.LocalPosition;
                    AvatarData.accessoryOriginRotation = AvatarData.itemAccessory.LocalRotation;

                    AvatarData.itemHair.transform.parent = AvatarData.itemFace.transform.parent;
                    AvatarData.itemAccessory.transform.parent = AvatarData.itemFace.transform.parent;

                    faceObj.transform.localPosition = localPosition;
                    faceObj.transform.localRotation = localRotation;

                    var faceItem = faceObj.GetComponent<ItemFace>();
                    faceItem.Initialize(changeAvatarItemArray);

                    Destroy(AvatarData.itemFace.gameObject);

                    AvatarData.itemFace = faceItem;

                    if (!isAll)
                    {
                        AvatarData.itemHair.SetParent(AvatarData.itemFace);
                        AvatarData.itemAccessory.SetParent(AvatarData.itemFace);

                        var hairItemTr = AvatarData.itemHair.transform;
                        var accessoryItemTransform = AvatarData.itemAccessory.transform;

                        hairItemTr.localPosition = AvatarData.hairOriginPosition;
                        accessoryItemTransform.localPosition = AvatarData.accessoryOriginPosition;
                        hairItemTr.localRotation = AvatarData.hairOriginRotation;
                        accessoryItemTransform.localRotation = AvatarData.accessoryOriginRotation;
                    }

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.Face,
                            index = changeAvatarItemArray[2],
                            tick = DateTime.Now.Ticks
                        });
                    }

                    changeAvatarItemArray[2] = index;
                }
                    break;
                case Appearance.Clothes:
                {
                    if (AvatarData.clothObj == null)
                        return;

                    var localPosition = AvatarData.clothObj.transform.localPosition;
                    var localRotation = AvatarData.clothObj.transform.localRotation;

                    var clothData = avatarDatabase.GetClothData(index);
                    var clothObj = Instantiate(clothData.itemObject, createTarget);

                    clothObj.transform.localPosition = localPosition;
                    clothObj.transform.localRotation = localRotation;

                    Destroy(AvatarData.clothObj);

                    AvatarData.clothObj = clothObj;

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.Clothes,
                            index = changeAvatarItemArray[6],
                            tick = DateTime.Now.Ticks
                        });
                    }

                    changeAvatarItemArray[6] = index;
                }
                    break;
                case Appearance.Accessory:
                {
                    if (AvatarData.accessoryObj == null)
                        return;

                    var localPosition = AvatarData.accessoryObj.transform.localPosition;
                    var localRotation = AvatarData.accessoryObj.transform.localRotation;

                    var addData = avatarDatabase.GetAccessoryData(index);
                    var accObj = Instantiate(addData.itemObject, createTarget);

                    var accItem = accObj.GetComponent<ItemAccessory>();
                    accItem.Initialize(changeAvatarItemArray);

                    if (!isAll)
                    {
                        accObj.transform.parent = AvatarData.itemFace.transform.GetChild(1).GetChild(0);

                        accObj.transform.localPosition = localPosition;
                        accObj.transform.localRotation = localRotation;
                    }
                    else
                    {
                        accObj.transform.localPosition = localPosition;
                        accObj.transform.localRotation = localRotation;
                    }

                    Destroy(AvatarData.accessoryObj);

                    AvatarData.accessoryObj = accObj;
                    AvatarData.itemAccessory = accObj.GetComponent<ItemAccessory>();

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.Accessory,
                            index = changeAvatarItemArray[7],
                            tick = DateTime.Now.Ticks
                        });
                    }

                    changeAvatarItemArray[7] = index;
                }
                    break;
                case Appearance.Skin:
                case Appearance.EyeColor:
                case Appearance.EyebrowsColor:
                case Appearance.HairColor:
                case Appearance.AccessoryColor:
                default:
                    return;
            }
        }

        /// <summary>
        /// 특정 부분의 아바타 색상을 변경
        /// </summary>
        /// <param name="appearance">변경할 부분</param>
        /// <param name="index">변경할 색상 인덱스</param>
        /// <param name="isNotUndo">히스토리에 저장하지 않는 경우</param>
        public override void SetColor(Appearance appearance, int index, bool isNotUndo = false)
        {
            switch (appearance)
            {
                case Appearance.Skin:
                    AvatarData.itemFace.SetSkin(avatarDatabase.GetSkinColor(index));

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.Skin,
                            index = changeAvatarItemArray[3]
                        });
                    }

                    changeAvatarItemArray[3] = index;
                    break;
                case Appearance.EyeColor:
                    AvatarData.itemFace.SetEye(index);

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.EyeColor,
                            index = changeAvatarItemArray[4]
                        });
                    }

                    changeAvatarItemArray[4] = index;
                    break;
                case Appearance.EyebrowsColor:
                    AvatarData.itemFace.SetEyebrow(avatarDatabase.GetEyebrowColor(index));

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.EyebrowsColor,
                            index = changeAvatarItemArray[5]
                        });
                    }

                    changeAvatarItemArray[5] = index;
                    break;
                case Appearance.HairColor:
                    AvatarData.itemHair.SetHair(avatarDatabase.GetHairColor(index));

                    if (!isNotUndo)
                    {
                        itemUndoHistoryStack.Push(new History()
                        {
                            appearance = Appearance.HairColor,
                            index = changeAvatarItemArray[1]
                        });
                    }

                    changeAvatarItemArray[1] = index;
                    break;
                case Appearance.AccessoryColor:
                    var isHat = index >= 6000 && index <= 6999;

                    if (isHat)
                        AvatarData.itemAccessory.SetHat(index);
                    else
                        AvatarData.itemAccessory.SetAcc(avatarDatabase.GetAccColor(index));

                    if (!isNotUndo)
                    {
                        if (isHat)
                        {
                            itemUndoHistoryStack.Push(new History()
                            {
                                appearance = Appearance.AccessoryColor,
                                index = changeAvatarItemArray[1]
                            });
                        }
                        else
                        {
                            itemUndoHistoryStack.Push(new History()
                            {
                                appearance = Appearance.AccessoryColor,
                                index = changeAvatarItemArray[8]
                            });
                        }
                    }

                    if (isHat)
                    {
                        changeAvatarItemArray[1] = index;
                    }
                    else
                    {
                        changeAvatarItemArray[8] = index;
                    }

                    break;
                case Appearance.Hair:
                case Appearance.Face:
                case Appearance.Clothes:
                case Appearance.Accessory:
                default:
                    return;
            }
        }
    }
}