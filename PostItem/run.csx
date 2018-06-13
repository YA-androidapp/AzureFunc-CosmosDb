using System.Net;

/// <summary>
/// Cosmos DBにドキュメントを登録するAPIの関数
/// </summary>
/// <param name="req"></param>
/// <param name="outputDocument">バインドされたAzure Cosmos DB outputのドキュメントパラメーター名</param>
/// <param name="log"></param>
/// <returns>
/// ステータスコード200を返す。例外発生時には400エラーを返す
/// </returns>
public static HttpResponseMessage Run(HttpRequestMessage req, out object outputDocument, TraceWriter log)
{
    string TAG = "PostItem";

    try
    {
        log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));

        // クエリストリングに指定されたISBN文字列を取得する
        string isbn = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "isbn", true) == 0)
        .Value;

        // ISBN(のハイフンを除去した文字列)が数値のみからなることを検査
        if(!string.IsNullOrEmpty(isbn))
        {
            isbn = isbn.Replace("-","");
            long isbnLong = 0;
            if(long.TryParse(isbn, out isbnLong))
            {
                try
                {
                    // ISBNにisbnパラメータの値を設定したドキュメントを用意
                    outputDocument = new {Isbn = isbn};
                    log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
                    return req.CreateResponse(HttpStatusCode.OK);
                }
                catch(Exception e)
                {
                    log.Info("e: " + e.StackTrace);
                }
            }
        }
        outputDocument = null;
        log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
    catch(Exception ex)
    {
        outputDocument = null;
        log.Info(TAG + " " + "ex: " + ex.StackTrace);
        log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
        return req.CreateResponse(HttpStatusCode.BadRequest);
    }
}
# Copyright (c) 2018 YA-androidapp(https://github.com/YA-androidapp) All rights reserved.