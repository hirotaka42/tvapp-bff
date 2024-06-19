
## 目次

- [目次](#目次)
- [前提条件](#前提条件)
- [インストール](#インストール)
- [使い方 (Docker-Compose)](#使い方-docker-compose)
  - [起動](#起動)
  - [停止](#停止)
- [使い方 (CLI)](#使い方-cli)
  - [(初回起動時に実施)DBのマイグレーション](#初回起動時に実施dbのマイグレーション)
  - [起動](#起動-1)
- [APIエンドポイント一覧](#apiエンドポイント一覧)

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

## 使い方 (Docker-Compose)
### 起動
Docker Composeを使用してプロジェクトを起動します。
```
docker-compose up --build
```
APIエンドポイントにアクセスするには、ブラウザまたはAPIクライアント（例: Postman）を使用して以下のURLにアクセスします。

DockerではHTTPSではなく、HTTPでの接続になります

・http://localhost:7044/swagger/index.html  
・http://localhost:7044/エンドポイント


### 停止
Docker Composeを使用してプロジェクトを停止します。
```
docker-compose down
```

## 使い方 (CLI)
 .NET 7 SDK がインストールされている事
 https://dotnet.microsoft.com/ja-jp/download/dotnet


### (初回起動時に実施)DBのマイグレーション

マイグレーションツールのインストール
```
dotnet tool install --global dotnet-ef --version 7.0.20
dotnet ef --version
```

マイグレーションの実施
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```


### 起動
```
dotnet run
```
APIエンドポイントにアクセスするには、ブラウザまたはAPIクライアント（例: Postman）を使用して以下のURLにアクセスします。

CLIではHTTPSではなく、HTTPでの接続になります

・http://localhost:ランダムなポート番号/swagger/index.html  
・http://localhost:ランダムなポート番号/エンドポイント



## APIエンドポイント一覧
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



