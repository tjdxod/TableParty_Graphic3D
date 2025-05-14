#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Oculus.Avatar2;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

namespace Dive.Avatar.Meta
{
    /// <summary>
    /// 메타 아바타 플랫폼 초기화 상태
    /// </summary>
    public static class PXRMetaAvatarPlatformInit
    {
        #region Private Fields

        private const string logScope = "MetaAvatarPlatformInit";

        private static PXRMetaFederateSetting federateSetting;

        #endregion

        #region Public Properties

        public static MetaAvatarPlatformInitStatus status { get; private set; } = MetaAvatarPlatformInitStatus.NotStarted;

        #endregion

        public static void InitializeMetaAvatarPlatform()
        {
            if (status == MetaAvatarPlatformInitStatus.Succeeded)
            {
                OvrAvatarLog.LogWarning("Meta avatar platform is already initialized.", logScope);
                return;
            }

            try
            {
                status = MetaAvatarPlatformInitStatus.Initializing;
                Core.AsyncInitialize().OnComplete(InitializeComplete);

                void InitializeComplete(Message<PlatformInitialize> msg)
                {
                    if (msg.Data.Result != PlatformInitializeResult.Success)
                    {
                        status = MetaAvatarPlatformInitStatus.Failed;
                        OvrAvatarLog.LogError("Failed to initialize Meta avatar platform", logScope);
                    }
                    else
                    {
                        Entitlements.IsUserEntitledToApplication().OnComplete(CheckEntitlement);
                    }
                }

                void CheckEntitlement(Message msg)
                {
                    if (msg.IsError == false)
                    {
                        Users.GetAccessToken().OnComplete(GetAccessTokenComplete);
                    }
                    else
                    {
                        status = MetaAvatarPlatformInitStatus.Failed;
                        var e = msg.GetError();
                        OvrAvatarLog.LogError($"Failed entitlement check: {e.Code} - {e.Message}", logScope);
                    }
                }

                void GetAccessTokenComplete(Message<string> msg)
                {
                    if (String.IsNullOrEmpty(msg.Data))
                    {
                        string output = "Token is null or empty.";
                        if (msg.IsError)
                        {
                            var e = msg.GetError();
                            output = $"{e.Code} - {e.Message}";
                        }

                        status = MetaAvatarPlatformInitStatus.Failed;
                        OvrAvatarLog.LogError($"Failed to retrieve access token: {output}", logScope);
                    }
                    else
                    {
                        OvrAvatarLog.LogDebug($"Successfully retrieved access token.", logScope);
                        OvrAvatarEntitlement.SetAccessToken(msg.Data);
                        status = MetaAvatarPlatformInitStatus.Succeeded;
                    }
                }
            }
            catch (Exception e)
            {
                status = MetaAvatarPlatformInitStatus.Failed;
                OvrAvatarLog.LogError($"{e.Message}\n{e.StackTrace}", logScope);
            }
        }

        public static void InitializeMetaAvatarStandAlonePlatform(string uniqueId, string nickname)
        {
            if (status == MetaAvatarPlatformInitStatus.Succeeded)
            {
                OvrAvatarLog.LogWarning("Meta avatar platform is already initialized.", logScope);
                return;
            }

            if (federateSetting == null)
            {
                federateSetting = Resources.Load<PXRMetaFederateSetting>("PXRMetaFederateSetting");

                if (federateSetting == null)
                    throw new Exception("PXRMetaFederateSetting is not found in Resources folder.");
            }

            try
            {
                status = MetaAvatarPlatformInitStatus.Initializing;

                Core.AsyncInitialize(federateSetting.FederateAppId).OnComplete(InitializeComplete);

                async void InitializeComplete(Message<PlatformInitialize> msg)
                {
                    if (msg.Data.Result != PlatformInitializeResult.Success)
                    {
                        status = MetaAvatarPlatformInitStatus.Failed;
                        OvrAvatarLog.LogError("Failed to initialize Meta avatar platform", logScope);
                    }
                    else
                    {
                        var tokenCallback = await FederateGeneratedUserAccessToken(uniqueId, nickname);
                        var accessToken = string.Empty;

                        if (!tokenCallback.IsSuccess)
                        {
                            var createCallback = await FederateCreateUser(uniqueId, nickname);

                            if (!createCallback.IsSuccess)
                            {
                                status = MetaAvatarPlatformInitStatus.Failed;
                                OvrAvatarLog.LogError(createCallback.Message, logScope);
                                return;
                            }

                            var reTokenCallback = await FederateGeneratedUserAccessToken(uniqueId, nickname);

                            if (!reTokenCallback.IsSuccess)
                            {
                                status = MetaAvatarPlatformInitStatus.Failed;
                                OvrAvatarLog.LogError(reTokenCallback.Message, logScope);
                                return;
                            }

                            accessToken = reTokenCallback.Response.access_token;
                        }
                        else
                        {
                            accessToken = tokenCallback.Response.access_token;
                        }

                        OvrAvatarEntitlement.SetAccessToken(accessToken);
                        status = MetaAvatarPlatformInitStatus.Succeeded;

                        OvrAvatarLog.LogDebug($"Successfully retrieved access token.", logScope);
                    }
                }
            }
            catch (Exception e)
            {
                status = MetaAvatarPlatformInitStatus.Failed;
                OvrAvatarLog.LogError($"{e.Message}\n{e.StackTrace}", logScope);
            }
        }

        public static async UniTask<DeleteFederatedUserCallback> FederateDeleteUser(string uniqueId)
        {
            if (federateSetting == null)
            {
                federateSetting = Resources.Load<PXRMetaFederateSetting>("PXRMetaFederateSetting");

                if (federateSetting == null)
                    throw new Exception("PXRMetaFederateSetting is not found in Resources folder.");
            }

            var appId = federateSetting.FederateAppId;
            var accessAppToken = federateSetting.FederateAppAccessToken;

            var url = $"https://graph.oculus.com/{appId}/federated_user/?persistent_id={uniqueId}";

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", $"Bearer {accessAppToken}");
            request.Method = "DELETE";
            request.ContentLength = 0;

            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    var msg = $"{nameof(FederateDeleteUser)} : Response stream is null";
                    Debug.LogError(msg);
                    return DeleteFederatedUserCallback.GetError(msg);
                }

                var streamReader = new StreamReader(responseStream);
                var responseText = await streamReader.ReadToEndAsync();
                var json = JsonConvert.DeserializeObject<DeleteFederatedUserResponse>(responseText);

                var callback = new DeleteFederatedUserCallback
                {
                    IsSuccess = true,
                    Response = json,
                    Message = "Success"
                };

                return callback;
            }
            catch (Exception e)
            {
                var msg = $"{nameof(FederateDeleteUser)} : {e.Message}";
                Debug.LogError(msg);
                return DeleteFederatedUserCallback.GetError(msg);
            }
        }

        public static async UniTask<UpdateFederatedUserCallback> FederateUpdateUser(string uniqueId, string changeDisplayName)
        {
            if (federateSetting == null)
            {
                federateSetting = Resources.Load<PXRMetaFederateSetting>("PXRMetaFederateSetting");

                if (federateSetting == null)
                    throw new Exception("PXRMetaFederateSetting is not found in Resources folder.");
            }

            var appId = federateSetting.FederateAppId;
            var accessAppToken = federateSetting.FederateAppAccessToken;

            var url = $"https://graph.oculus.com/{appId}/federated_user/{uniqueId}/update/";
            url += $"?display_name={changeDisplayName}";

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", $"Bearer {accessAppToken}");
            request.Method = "POST";
            request.ContentLength = 0;

            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    var msg = $"{nameof(FederateUpdateUser)} : Response stream is null";
                    Debug.LogError(msg);
                    return UpdateFederatedUserCallback.GetError(msg);
                }

                var streamReader = new StreamReader(responseStream);
                var responseText = await streamReader.ReadToEndAsync();
                var json = JsonConvert.DeserializeObject<UpdateFederatedUserResponse>(responseText);

                var callback = new UpdateFederatedUserCallback
                {
                    IsSuccess = true,
                    Response = json,
                    Message = "Success"
                };

                Debug.Log($"{nameof(FederateUpdateUser)} : {responseText}");

                return callback;
            }
            catch (Exception e)
            {
                var msg = $"{nameof(FederateUpdateUser)} : {e.Message}";
                Debug.LogError(msg);
                return UpdateFederatedUserCallback.GetError($"{nameof(FederateUpdateUser)} : {e.Message}");
            }
        }

        public static async UniTask<CreateFederatedUserCallback> FederateCreateUser(string uniqueId, string nickname)
        {
            if (federateSetting == null)
            {
                federateSetting = Resources.Load<PXRMetaFederateSetting>("PXRMetaFederateSetting");

                if (federateSetting == null)
                    throw new Exception("PXRMetaFederateSetting is not found in Resources folder.");
            }

            var appId = federateSetting.FederateAppId;
            var accessAppToken = federateSetting.FederateAppAccessToken;

            var url = $"https://graph.oculus.com/{appId}/federated_user_create";
            url += $"/?persistent_id={uniqueId}&display_name={nickname}";

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", $"Bearer {accessAppToken}");
            request.Method = "POST";
            request.ContentLength = 0;

            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    var msg = $"{nameof(FederateCreateUser)} : Response stream is null";
                    Debug.LogError(msg);
                    return CreateFederatedUserCallback.GetError(msg);
                }

                var streamReader = new StreamReader(responseStream);
                var responseText = await streamReader.ReadToEndAsync();
                var json = JsonConvert.DeserializeObject<CreateFederatedUserResponse>(responseText);

                var callback = new CreateFederatedUserCallback
                {
                    IsSuccess = true,
                    Response = json,
                    Message = "Success"
                };

                return callback;
            }
            catch (Exception e)
            {
                var msg = $"{nameof(FederateCreateUser)} : {e.Message}";
                Debug.LogError(msg);
                return CreateFederatedUserCallback.GetError(msg);
            }
        }

        public static async UniTask<GenerateFederatedUserAccessTokenCallback> FederateGeneratedUserAccessToken(string uniqueId, string nickname)
        {
            if (federateSetting == null)
            {
                federateSetting = Resources.Load<PXRMetaFederateSetting>("PXRMetaFederateSetting");

                if (federateSetting == null)
                    throw new Exception("PXRMetaFederateSetting is not found in Resources folder.");
            }

            var appId = federateSetting.FederateAppId;
            var accessAppToken = federateSetting.FederateAppAccessToken;

            var url = $"https://graph.oculus.com/{appId}/federated_user_gen_access_token/";
            url += $"?persistent_id={uniqueId}";

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", $"Bearer {accessAppToken}");
            request.Method = "POST";
            request.ContentLength = 0;

            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    var msg = $"{nameof(FederateGeneratedUserAccessToken)} : Response stream is null";
                    Debug.LogError(msg);
                    return GenerateFederatedUserAccessTokenCallback.GetError(msg);
                }

                var streamReader = new StreamReader(responseStream);
                var responseText = await streamReader.ReadToEndAsync();
                var json = JsonConvert.DeserializeObject<GenerateFederatedUserAccessTokenResponse>(responseText);

                var callback = new GenerateFederatedUserAccessTokenCallback
                {
                    IsSuccess = true,
                    Response = json,
                    Message = "Success"
                };

                return callback;
            }
            catch (Exception e)
            {
                var msg = $"{nameof(FederateGeneratedUserAccessToken)} : {e.Message}";
                Debug.LogError(msg);
                return GenerateFederatedUserAccessTokenCallback.GetError(msg);
            }
        }

        public static async UniTask<ReadFederatedUserCallback> FederateReadUser(string uniqueId, string nickname)
        {
            if (federateSetting == null)
            {
                federateSetting = Resources.Load<PXRMetaFederateSetting>("PXRMetaFederateSetting");

                if (federateSetting == null)
                    throw new Exception("PXRMetaFederateSetting is not found in Resources folder.");
            }

            var accessUserTokenCallback = await FederateGeneratedUserAccessToken(uniqueId, nickname);

            if (!accessUserTokenCallback.IsSuccess)
            {
                Debug.LogError(accessUserTokenCallback.Message);
                return ReadFederatedUserCallback.GetError(accessUserTokenCallback.Message);
            }

            var appId = federateSetting.FederateAppId;
            var userAccessToken = accessUserTokenCallback.Response.access_token;
            var accessAppToken = federateSetting.FederateAppAccessToken;

            var url = $"https://graph.oculus.com/{appId}/federated_user/";
            url += $"?persistent_id={uniqueId}";

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", $"Bearer {accessAppToken}");
            request.Headers.Add("UserAccessToken", userAccessToken);
            request.Method = "GET";
            request.ContentLength = 0;

            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    var msg = $"{nameof(FederateReadUser)} : Response stream is null";
                    Debug.LogError(msg);
                    return ReadFederatedUserCallback.GetError(msg);
                }

                var streamReader = new StreamReader(responseStream);
                var responseText = await streamReader.ReadToEndAsync();
                var json = JsonConvert.DeserializeObject<ReadFederatedUserResponse>(responseText);

                var callback = new ReadFederatedUserCallback
                {
                    IsSuccess = true,
                    Response = json,
                    Message = "Success"
                };

                return callback;
            }
            catch (Exception e)
            {
                var msg = $"{nameof(FederateReadUser)} : {e.Message}";
                Debug.LogError(msg);
                return ReadFederatedUserCallback.GetError(msg);
            }
        }
    }

    #region Create

    public class CreateFederatedUserCallback
    {
        public CreateFederatedUserResponse Response;
        public bool IsSuccess;
        public string Message;

        public static CreateFederatedUserCallback GetError(string errorMessage)
        {
            var callback = new CreateFederatedUserCallback
            {
                Response = null,
                IsSuccess = false,
                Message = errorMessage
            };

            return callback;
        }
    }

    public class CreateFederatedUserResponse
    {
        public long id;
        public string persistent_id;
        public string unique_name;
        public string display_name;
    }

    #endregion

    #region Delete

    public class DeleteFederatedUserCallback
    {
        public DeleteFederatedUserResponse Response;
        public bool IsSuccess;
        public string Message;

        public static DeleteFederatedUserCallback GetError(string errorMessage)
        {
            var callback = new DeleteFederatedUserCallback
            {
                Response = null,
                IsSuccess = false,
                Message = errorMessage
            };

            return callback;
        }
    }

    public class DeleteFederatedUserResponse
    {
        public bool success;
    }

    #endregion

    #region Generated

    public class GenerateFederatedUserAccessTokenCallback
    {
        public GenerateFederatedUserAccessTokenResponse Response;
        public bool IsSuccess;
        public string Message;

        public static GenerateFederatedUserAccessTokenCallback GetError(string errorMessage)
        {
            var callback = new GenerateFederatedUserAccessTokenCallback
            {
                Response = null,
                IsSuccess = false,
                Message = errorMessage
            };

            return callback;
        }
    }

    public class GenerateFederatedUserAccessTokenResponse
    {
        public string access_token;
    }

    #endregion

    #region Read

    public class ReadFederatedUserCallback
    {
        public ReadFederatedUserResponse Response;
        public bool IsSuccess;
        public string Message;

        public static ReadFederatedUserCallback GetError(string errorMessage)
        {
            var callback = new ReadFederatedUserCallback
            {
                Response = null,
                IsSuccess = false,
                Message = errorMessage
            };

            return callback;
        }
    }

    public class ReadFederatedUserResponse
    {
        public long id;
        public string persistent_id;
        public string unique_name;
        public string display_name;
    }

    #endregion

    #region ReadAll

    public class ReadFederatedAllUserCallback
    {
        public ReadFederatedAllUserResponse Response;
        public bool IsSuccess;
        public string Message;

        public static ReadFederatedAllUserCallback GetError(string errorMessage)
        {
            var callback = new ReadFederatedAllUserCallback
            {
                Response = null,
                IsSuccess = false,
                Message = errorMessage
            };

            return callback;
        }
    }

    public class ReadFederatedAllUserResponse
    {
        public ReadFederatedAllUserResponseData[] data;
        public ReadFederatedAllUserResponsePaging paging;
    }

    public class ReadFederatedAllUserResponseData
    {
        public long id;
        public string persistent_id;
        public string unique_name;
        public string display_name;
    }

    public class ReadFederatedAllUserResponsePaging
    {
        public string previous;
        public string next;
    }

    #endregion

    #region Update

    public class UpdateFederatedUserCallback
    {
        public UpdateFederatedUserResponse Response;
        public bool IsSuccess;
        public string Message;

        public static UpdateFederatedUserCallback GetError(string errorMessage)
        {
            var callback = new UpdateFederatedUserCallback
            {
                Response = null,
                IsSuccess = false,
                Message = errorMessage
            };

            return callback;
        }
    }

    public class UpdateFederatedUserResponse
    {
        public long id;
        public string persistent_id;
        public string unique_name;
        public string display_name;
    }

    #endregion
}

#endif