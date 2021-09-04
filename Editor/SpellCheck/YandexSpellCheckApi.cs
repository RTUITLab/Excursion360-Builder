using Packages.Excursion360_Builder.Editor.HTTP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.SpellCheck
{
    internal static class YandexSpellCheckApi
    {
        public static IEnumerator GetResultForRow(string row, Action<RowResponse[]> doneAction)
        {
            var request = HttpHelper.InvokePostRequest(
                "https://speller.yandex.net/services/spellservice.json/checkText",
                "spell check",
                new Dictionary<string, string> { { "text", row } },
                handler =>
                {
                    var responseText = handler.text;
                    var wrappedResponse = $"{{ \"{nameof(ResponseArrayWrapper.responses)}\": {responseText} }}";
                    var parsedWrapper = JsonUtility.FromJson<ResponseArrayWrapper>(wrappedResponse);
                    doneAction(parsedWrapper.responses);
                },
                error => Debug.LogError(error),
                showStatus: false);

            while (request.MoveNext())
            {
                yield return null;
            }
        }
    }
}
