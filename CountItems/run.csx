#r "Newtonsoft.Json"

using System.Net;
using Newtonsoft.Json;
using System.Net.Http.Headers;

/// <summary>
/// Cosmos DBに登録されているされているドキュメントをJSON形式で返すAPIの関数
/// isbnパラメータが指定された場合には該当するアイテムのみ、指定されていない場合には登録されている全てのアイテムをJSONに含める
/// </summary>
/// <param name="req"></param>
/// <param name="inputDocument">バインドされたAzure Cosmos DB inputのドキュメントパラメーター名</param>
/// <param name="log"></param>
/// <returns>
/// JSON形式の応答。例外発生時には400エラーを返す
/// </returns>
public static HttpResponseMessage Run(HttpRequestMessage req, IEnumerable<dynamic> inputDocument, TraceWriter log)
{
    string TAG = "GetItems";

    try
    {
        log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));

        // クエリストリングに指定されたISBN文字列を取得する
        string isbn = req.GetQueryNameValuePairs()
            .FirstOrDefault(q => string.Compare(q.Key, "isbn", true) == 0)
            .Value;

        // ISBN(のハイフンを除去した文字列)が空文字列か、数値のみからなることを検査
        long isbnLong = 0;
        if((string.IsNullOrEmpty(isbn)) || (long.TryParse(isbn, out isbnLong))){
            try
            {
                // ISBNが指定された場合には、Cosmos DBに登録されているデータのうちISBNが一致するものを抽出。それ以外の場合には全件出力
                IEnumerable<dynamic> docs;
                if(string.IsNullOrEmpty(isbn))
                    docs = inputDocument;
                else
                    docs = inputDocument.Where(doc => doc.Isbn == isbn.Replace("-",""));

                // 出力用のJSON形式を生成
                int count = docs.Count();
                Dictionary<string, int> result = new Dictionary<string, int>
                {
                    { "Count", count }
                };
                string responseJSON = JsonConvert.SerializeObject(result);

                // 応答を返す
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(responseJSON);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response.Content.Headers.ContentType.CharSet = "utf-8";
                log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
                return response;
            }
            catch(Exception e)
            {
                log.Info(TAG + " " + "e: " + e.StackTrace);
            }
        }
        log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
    catch(Exception ex)
    {
        log.Info(TAG + " " + "ex: " + ex.StackTrace);
        log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
}
# Copyright (c) 2018 YA-androidapp(https://github.com/YA-androidapp) All rights reserved.