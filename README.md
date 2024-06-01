
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

◼︎Dockerを使用しない場合は以下も必須
- [yt-dlp](https://github.com/yt-dlp/yt-dlp)

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
http://localhost:8080/api/エンドポイント
```

## APIエンドポイント
APIの主要なエンドポイントを以下に示します。

### POST /api/TVapp/session
- 説明: セッション時に作成される token を発行する
- パラメータ: なし
- レスポンス: `platformUid`と`platformToken`

### GET /api/TVapp/ranking/{genre}
- 説明: パラメータで受け取ったカテゴリ別のランキングを最大30個、取得する
- パラメータ: 現在使用できるのは以下の6つ
  - `drama`
  - `variety`
  - `anime`
  - `news_documentary`
  - `sports`
  - `other`
- レスポンス: 割愛

### GET /api/TVapp/search
- 説明: フリーワード検索
- パラメータ: 
  - `platformUid`
  - `platformToken`
  - `keyword`
- レスポンス: 割愛



