
## 目次

- [前提条件](#前条件)
- [インストール](#インストール)
- [使い方](#使い方)
- [APIエンドポイント](#apiエンドポイント)
- [開発](#開発)
- [ライセンス](#ライセンス)

## 前提条件

このプロジェクトを実行するために必要な前提条件を以下に示します。

- [.NET Core SDK](https://dotnet.microsoft.com/download) (.NET 7.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

◼︎Dockerを使用しない場合は以下のセットアップが必要
- [yt-dlp](https://github.com/yt-dlp/yt-dlp)
- 下記コマンドでバージョン情報が表示されればOK
  - `yt-dlp --version`
  - ![スクリーンショット 2024-06-08 0 11 36](https://github.com/hirotaka42/tvapp-bff/assets/79750434/bad5e4ee-7886-4972-88a6-fd96feed4dce)

## インストール

プロジェクトのクローンを作成します。

```bash
git clone https://github.com/hirotaka42/tvapp-bff.git
cd tvapp-bff
```

## 使い方
Docker Composeを使用してプロジェクトを起動します。
```
docker-compose up --build
```

Docker Composeを使用してプロジェクトを停止します。
```
docker-compose down
```

APIエンドポイントにアクセスするには、ブラウザまたはAPIクライアント（例: Postman）を使用して以下のURLにアクセスします。
```
http://localhost:8080/エンドポイント
```

## APIエンドポイント
APIの主要なエンドポイントを以下に示します。

|リクエスト|エンドポイント|パラメータ|クエリ|レスポンス|
|---|---|---|---|---|
|POST|`/api/TVapp/session`|-|-|`platformUid`<br>`platformToken`|セッションtoken を発行する|
|GET|`/api/TVapp/ranking/{genre}`|現在使用できるのは以下の6つ <br>`drama`<br>`variety`<br>`anime`<br>`news_documentary`<br>`sports`<br>`other`|-|カテゴリ別のランキング最大30個|
|GET|`/api/TVapp/service/search`|-|`keyword`<br>`platformUid`<br>`platformToken`|検索結果|
|GET|`/api/TVapp/service/callHome`|-|`platformUid`<br>`platformToken`|ホームに表示されている全番組データ|
|GET|`/api/TVapp/service/callEpisode/{episodeId}`|`{episodeId}`|`platformUid`<br>`platformToken`|エピソードIDにヒットする番組情報|
|GET|`/api/TVapp/content/series/{seriesId}`|`{seriesId}`|-|シリーズIDにヒットするシリーズの概要情報|
|GET|`/api/TVapp/streaming/{episodeId}`|`{episodeId}`|-|エピソードIDを元にしたm3u8形式のストリーミングURL|



