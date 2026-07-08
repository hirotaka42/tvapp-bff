# tvapp-bff STATUS

## 現状

`tvapp-bff` は .NET 7 の ASP.NET Core Web API で作られた BFF/API サービスです。ローカル Git 履歴上の最終コミットは `2024-06-27T21:46:10+09:00` の `fix: 消えていたCORSポリシーを再設定` です。

現状、`tvapp-web` 側が純 TypeScript のストリーム解決へ移行したため、この BFF は停止中のレガシーコンポーネントとして扱われています。廃止候補として参照される状態ですが、この文書では廃止/保全の判断は行わず、判断材料になる事実のみを列挙します。

## 構成

- 単一プロジェクト: `Xiao.TVapp.Bff`
- Target Framework: `net7.0`
- Web API: ASP.NET Core Web API
- ORM/DB: EF Core 7 + Pomelo MySQL provider + MariaDB
- DB スキーマ: `StreamingUrls` 1 テーブル
- Migration: `Xiao.TVapp.Bff/Migrations/20240626153019_InitialCreate.cs`
- Swagger: `Development` 環境のみ有効
- CORS: `AllowAnyOrigin`, `AllowAnyHeader`, `AllowAnyMethod`
- Docker Compose: `tvapp-bff`, `adminer`, `mysql`
- Docker 公開ポート: BFF `5231:5231`、Adminer `5230:8080`
- Dockerfile: `yt-dlp` を GitHub Releases の `latest` からビルド時取得。バージョン固定なし
- テスト/CI: リポジトリ内にテストプロジェクト、テストファイル、CI ワークフローは確認できない

## ストリーミング URL 取得の 2 系統

実装上、用途が重なるストリーミング URL 取得 API が 2 系統あります。

|系統|エンドポイント|DB 利用|挙動|
|---|---|---|---|
|DB なし|`GET /api/TVapp/streaming/{episodeId}`|なし|毎回 `yt-dlp --get-url https://tver.jp/episodes/{episodeId}` を実行し、結果文字列を返す|
|DB キャッシュあり|`GET /api/StreamingUrls/episode/{episodeId}`|あり|`StreamingUrls` を検索し、存在しない場合または `UpdatedAt` から 2 日以上経過した場合に `yt-dlp` で取得する|

`TVappController` には、DB キャッシュ付きストリーミング取得の旧実装らしきコメントアウトも残っています。

## DB

`InitialCreate` migration が作る DB は `StreamingUrls` 1 テーブルです。

|列|型の概要|補足|
|---|---|---|
|`Id`|int|主キー、identity|
|`EpisodeId`|varchar(255)|index あり|
|`CreatedAt`|datetime(6)|作成日時|
|`UpdatedAt`|datetime(6)|更新日時。キャッシュ更新判定に使用|
|`StreamingUrl`|longtext|`yt-dlp` で取得した URL|
|`IsDeleted`|tinyint(1)|モデル既定値は `false`|

`StreamingUrlsContext` のコンストラクタで `Database.Migrate()` が呼ばれるため、Context 生成時に migration 適用が走る構成です。

## 外部依存

コード上で確認できる外部依存は次の通りです。

- `https://platform-api.tver.jp/v2/api/platform_users/browser/create`
- `https://service-api.tver.jp/api/v1/callEpisodeRankingDetail/{genre}`
- `https://platform-api.tver.jp/service/api/v1/callKeywordSearch`
- `https://platform-api.tver.jp/service/api/v1/callHome`
- `https://platform-api.tver.jp/service/api/v1/callEpisode/{episodeId}`
- `https://statics.tver.jp/content/episode/{episodeId}.json`
- `https://statics.tver.jp/content/series/{seriesId}.json`
- `yt-dlp --get-url https://tver.jp/episodes/{episodeId}`

## 置換先

`tvapp-web` では、ストリーム解決が純 TypeScript 実装へ移行されています。関連する実装・文書では `Platform-Stream-Loader` が置換先として扱われています。このリポジトリには、移行後の純 TypeScript 実装は含まれていません。

## 廃止/保全の判断材料

判断材料として確認できる事実は次の通りです。ここでは判断自体は行いません。

- 最新コミットは 2024-06-27。
- Target Framework は `net7.0`。
- `tvapp-web` 側が純 TypeScript のストリーム解決へ移行したため、この BFF は停止中のコンポーネントとして扱われている。
- ストリーミング URL 取得は `yt-dlp` 実行に依存している。
- Dockerfile は `yt-dlp` の `latest` バイナリをビルド時に取得しており、固定バージョンではない。
- DB キャッシュ付き取得と DB なし取得の 2 系統が併存している。
- DB は `StreamingUrls` 1 テーブルのみ。
- CORS は全 Origin/Header/Method 許可。
- Swagger は Development 環境のみ有効。
- `StreamingUrlsContext` 生成時に `Database.Migrate()` が実行される。
- EF Core は `EnableSensitiveDataLogging()` と `EnableDetailedErrors()` が有効。
- テストプロジェクト、テストファイル、CI ワークフローは確認できない。
- README の過去記述には Docker URL として `7044` があったが、現在の Docker Compose は `5231` を公開している。
