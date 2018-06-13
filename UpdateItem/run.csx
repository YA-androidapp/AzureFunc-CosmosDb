#r "Microsoft.Azure.Documents.Client"
#r "Newtonsoft.Json"

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

// Document Clientクラスを使用してCosmos DBへアクセスする
private static string endpointUrl = "https://example.documents.azure.com:443/";
private static string dbName = "TablesDB";
private static string collectionName = "CollectionBook";
private static string authorizationKey = "ygIBcBQi1mcicWewJJcsc5YcwSHHg22yJBNpKJBd9ERjO2DcZPmEoMxrRyzoopZ4HYPAZekCEtmchda1BVnLeA==";
private static DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey, new ConnectionPolicy() {
    RequestTimeout = new TimeSpan(0, 0, 30),
    RetryOptions = new RetryOptions() {
        MaxRetryAttemptsOnThrottledRequests = 3,
        MaxRetryWaitTimeInSeconds = 60
    }
});

// ドキュメントアイテムを表現するクラス
public class Book
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    [JsonProperty(PropertyName = "isbn")]
    public string Isbn { get; set; }

    public string SelfLink { get; set; }
}

/// <summary>
/// Cosmos DBから指定されたISBNを持つドキュメントを更新するAPIの関数
/// </summary>
/// <param name="req"></param>
/// <param name="log"></param>
/// <returns>
/// ステータスコード200を返す。例外発生時には400エラーを返す
/// </returns>
public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    string TAG = "RemoveItem";

    try
    {
        log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));

        // クエリストリングに指定されたISBN文字列を取得する
        string oldIsbn = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "oldisbn", true) == 0)
        .Value;
        string newIsbn = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "newisbn", true) == 0)
        .Value;

        // ISBN(のハイフンを除去した文字列)が数値のみからなることを検査
        if((!string.IsNullOrEmpty(oldIsbn)) && (!string.IsNullOrEmpty(newIsbn)))
        {
            oldIsbn = oldIsbn.Replace("-","");
            newIsbn = newIsbn.Replace("-","");
            long oldIsbnLong = 0;
            long newIsbnLong = 0;
            if((long.TryParse(oldIsbn, out oldIsbnLong)) && (long.TryParse(newIsbn, out newIsbnLong)))
            {
                try
                {
                    // Cosmos DBに接続し、ドキュメントの配列を用意
                    var collectionUri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);
                    var documents = client.CreateDocumentQuery<Book>(collectionUri).AsEnumerable();
                    foreach (var d in documents)
                        if(d.Isbn == oldIsbn) // 指定されたISBNを持つ場合には更新
                        {
                            // 更新後のドキュメントを示すオブジェクトを作成
                            var b = new Book()
                            {
                                Id = d.Id, // UpsertDocumentAsyncなので、このIdが異なると上書きではなく挿入になる
                                Isbn = newIsbn,
                                SelfLink = d.SelfLink
                            };
                            client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(dbName, collectionName), b);
                        }

                    log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
                    return req.CreateResponse(HttpStatusCode.OK);
                }
                catch(Exception e)
                {
                    log.Info("e: " + e.StackTrace);
                }
            }
        }
    }
    catch(Exception ex)
    {
        log.Info(TAG + " " + "ex: " + ex.StackTrace);
    }
    log.Info(TAG + " " + DateTime.Now.ToString("yyyyMMddHHmmss"));
    return req.CreateResponse(HttpStatusCode.BadRequest);
}
# Copyright (c) 2018 YA-androidapp(https://github.com/YA-androidapp) All rights reserved.