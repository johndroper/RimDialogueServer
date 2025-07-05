//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RimDialogue.Access
//{
//  using HarmonyLib;
//  using OpenAI.Extensions;
//  using System.Reflection;
//  using System.Text.Json;
//  using System.Threading;
//  using System.Threading.Tasks;

//  [HarmonyPatch(typeof(ChatEndpoint), nameof(ChatEndpoint.GetCompletionAsync))]
//  [HarmonyPatch(new[] { typeof(ChatRequest), typeof(CancellationToken) })]
//  class ChatEndpoint_GetCompletionAsync_Patch
//  {
//    // Completely replaces the original method
//    static bool Prefix(
//        ChatEndpoint __instance,
//        ChatRequest chatRequest,
//        CancellationToken cancellationToken,
//        ref Task<ChatResponse> __result)
//    {

//      using StringContent payload = JsonSerializer.Serialize(chatRequest, OpenAIClient.JsonSerializationOptions).ToJsonStringContent();
//      using HttpResponseMessage response = await client.Client.PostAsync(GetUrl("/completions"), payload, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
//      return response.Deserialize<ChatResponse>(await response.ReadAsStringAsync(base.EnableDebug, payload, cancellationToken, "GetCompletionAsync").ConfigureAwait(continueOnCapturedContext: false), client);
//    }
//  }

//}
