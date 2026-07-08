# tvapp-bff

`tvapp-bff` は ASP.NET Core Web API で作られた BFF/API サービスです。TVer 系 API へのプロキシ、`yt-dlp` によるストリーミング URL 取得、MariaDB への URL キャッシュを持ちます。

## ステータス

このリポジトリは .NET 7 のレガシー BFF です。ローカル Git 履歴上の最終コミットは `2024-06-27T21:46:10+09:00` の `fix: 消えていたCORSポリシーを再設定` です。

現状、`tvapp-web` 側が純 TypeScript のストリーム解決へ移行したため、この BFF は停止中のコンポーネントとして扱われています。廃止候補として参照される状態ですが、この README では廃止/保全の判断は行わず、実装と運用状態の事実を整理します。判断材料は [docs/STATUS.md](docs/STATUS.md) も参照してください。

## 構成

- プロジェクト: `Xiao.TVapp.Bff`
- Target Framework: `net7.0`
- Web API: ASP.NET Core Web API
- DB/ORM: EF Core 7 + Pomelo MySQL provider + MariaDB
- DB テーブル: `StreamingUrls` 1 テーブル
- API ドキュメント: Swagger/OpenAPI。`Development` 環境のみ有効
- CORS: `AllowAnyOrigin` / `AllowAnyHeader` / `AllowAnyMethod`
- Docker: `Dockerfile.bff` が .NET 7 SDK/runtime イメージを使用
- `yt-dlp`: Docker ビルド時に GitHub Releases の `latest` バイナリを取得。バージョン固定なし

主要ファイル:

- `Xiao.TVapp.Bff/Program.cs`: DI、CORS、Swagger、EF Core、HTTP pipeline
- `Xiao.TVapp.Bff/Controllers/TVappController.cs`: TVer 系 API プロキシ、DB なしの `yt-dlp` ストリーミング URL 取得
- `Xiao.TVapp.Bff/Controllers/StreamingUrlsController.cs`: `StreamingUrls` CRUD、DB キャッシュ付きストリーミング URL 取得
- `Xiao.TVapp.Bff/Controllers/WeatherForecastController.cs`: ASP.NET テンプレート由来のサンプル API
- `Xiao.TVapp.Bff/Contexts/StreamingUrlsContext.cs`: EF Core DbContext。コンストラクタで `Database.Migrate()` を実行
- `Xiao.TVapp.Bff/Migrations/20240626153019_InitialCreate.cs`: 既存の初期 migration

## 前提条件

- .NET 7 SDK
- Docker
- Docker Compose
- Docker を使わずにストリーミング URL 取得 API を動かす場合は `yt-dlp`

`yt-dlp` は次のコマンドで確認できます。

```bash
yt-dlp --version
```

## Docker Compose で起動

```bash
docker-compose up --build
```

`docker-compose.yml` の公開ポートは次の通りです。

- BFF: `http://localhost:5231`
- Swagger UI: `http://localhost:5231/swagger/index.html`
- Adminer: `http://localhost:5230`

README の過去記述には Docker の URL として `http://localhost:7044` がありましたが、現在の `docker-compose.yml` は `5231:5231` を公開し、`ASPNETCORE_URLS=http://+:5231` を設定しています。`7044` は `launchSettings.json` の `https` profile にある HTTPS ポートです。

停止:

```bash
docker-compose down
```

## CLI で起動

HTTP profile を明示して起動します。

```bash
dotnet run --project Xiao.TVapp.Bff --launch-profile http
```

アクセス先:

- BFF: `http://localhost:5231`
- Swagger UI: `http://localhost:5231/swagger/index.html`

HTTPS profile を使う場合は `launchSettings.json` により `https://localhost:7044` と `http://localhost:5231` が使われます。

```bash
dotnet run --project Xiao.TVapp.Bff --launch-profile https
```

注意点:

- `appsettings.json` の既定接続文字列は `Server=mysql;Port=3306;Database=xiaobff;User=xiaobff;Password=xiaobff;...` です。これは Docker Compose の `mysql` サービス名を前提にしています。
- CLI でホスト OS から直接 DB 接続する場合は、`mysql` というホスト名が解決できる構成にするか、環境変数などで接続文字列を上書きする必要があります。
- `StreamingUrlsContext` のコンストラクタで `Database.Migrate()` が実行されるため、Context 生成時に migration 適用が走ります。

## EF Core migration

既存 migration として `20240626153019_InitialCreate` があります。新規セットアップ時に同名の migration を再作成する必要はありません。

DB に反映する場合:

```bash
dotnet tool install --global dotnet-ef --version 7.0.20
dotnet ef database update --project Xiao.TVapp.Bff
```

実装上は `StreamingUrlsContext` 生成時にも `Database.Migrate()` が呼ばれます。

`InitialCreate` が作るテーブル:

|テーブル|列|補足|
|---|---|---|
|`StreamingUrls`|`Id`, `EpisodeId`, `CreatedAt`, `UpdatedAt`, `StreamingUrl`, `IsDeleted`|`EpisodeId` に index|

## API エンドポイント

### TVappController

ベースパスは `/api/TVapp` です。外部 API から取得した本文を JSON オブジェクトへ詰め替えず、文字列 `content` を `Ok(content)` で返すエンドポイントがあります。

|メソッド|エンドポイント|必須パラメータ|成功時レスポンス|主なエラー|
|---|---|---|---|---|
|POST|`/api/TVapp/session`|なし|`SessionToken` オブジェクト。`platformUid`, `platformToken`|外部 API が失敗した場合、その HTTP ステータスで `"Failed to create sessionToken"`|
|GET|`/api/TVapp/ranking/{genre}`|path: `genre`|ランキング取得結果の本文文字列を `Ok(content)` で返す|`400 "Genre is required"`、`400 "Invalid genre"`、外部 API 失敗時は外部 API の HTTP ステータス|
|GET|`/api/TVapp/service/search`|query: `keyword`, `platformUid`, `platformToken`|検索結果の本文文字列を `Ok(content)` で返す|`400 "Keyword, platformUid and platformToken are required"`、外部 API 失敗時は外部 API の HTTP ステータス|
|GET|`/api/TVapp/service/callHome`|query: `platformUid`, `platformToken`|ホーム取得結果の本文文字列を `Ok(content)` で返す|`400 "platformUid and platformToken are required"`、外部 API 失敗時は外部 API の HTTP ステータス|
|GET|`/api/TVapp/service/callEpisode/{episodeId}`|path: `episodeId`; query: `platformUid`, `platformToken`|エピソード取得結果の本文文字列を `Ok(content)` で返す|`400 "episodeId, platformUid and platformToken are required"`、外部 API 失敗時は外部 API の HTTP ステータス|
|GET|`/api/TVapp/content/episode/{episodeId}`|path: `episodeId`|静的 episode JSON の本文文字列を `Ok(content)` で返す|`400 "episodeId are required"`、外部 API 失敗時は外部 API の HTTP ステータス|
|GET|`/api/TVapp/content/series/{seriesId}`|path: `seriesId`|静的 series JSON の本文文字列を `Ok(content)` で返す|`400 "seriesId are required"`、外部 API 失敗時は外部 API の HTTP ステータス|
|GET|`/api/TVapp/streaming/{episodeId}`|path: `episodeId`|`yt-dlp --get-url https://tver.jp/episodes/{episodeId}` の結果文字列を `Ok(result.Trim())` で返す。DB は使わない|`400 "Episode ID is required"`、`yt-dlp` 終了コード非 0 で `500 "Failed to retrieve streaming URL"`、例外時 `500 "Internal server error"`|

`genre` は次の enum 値です。大文字小文字は `Enum.TryParse(..., true, ...)` により区別されません。

- `drama`
- `variety`
- `anime`
- `news_documentary`
- `sports`
- `other`

### StreamingUrlsController

ベースパスは `/api/StreamingUrls` です。`/episode/{episodeId}` は DB キャッシュ付きのストリーミング URL 取得です。DB に同じ `EpisodeId` があり、`UpdatedAt` から 2 日未満の場合は DB の `StreamingUrl` を返します。2 日以上経過している場合は `yt-dlp` で再取得して DB を更新します。

|メソッド|エンドポイント|必須パラメータ/本文|成功時レスポンス|主なエラー|
|---|---|---|---|---|
|GET|`/api/StreamingUrls`|なし|`StreamingUrls[]`|DbSet が null の場合 `404`|
|GET|`/api/StreamingUrls/{id}`|path: `id`|`StreamingUrls` オブジェクト|DbSet が null、または該当なしで `404`|
|GET|`/api/StreamingUrls/episode/{episodeId}`|path: `episodeId`|ストリーミング URL 文字列を `Ok(...)` で返す|`400 "Episode ID is required"`、新規取得時の例外で `500 "Internal server error"`。既存レコード更新時の `yt-dlp` 例外はメソッド内で捕捉されない|
|PUT|`/api/StreamingUrls/{id}`|path: `id`; body: `StreamingUrls`|`204 No Content`|path の `id` と body の `Id` 不一致で `400`。更新対象なしの concurrency 例外で `404`|
|POST|`/api/StreamingUrls`|body: `StreamingUrls`|`201 Created` と作成した `StreamingUrls`|DbSet が null の場合 `Problem(...)`|
|DELETE|`/api/StreamingUrls/{id}`|path: `id`|`204 No Content`|DbSet が null、または該当なしで `404`|

`StreamingUrls` の形:

```json
{
  "id": 1,
  "episodeId": "example",
  "createdAt": "2024-06-26T15:30:19Z",
  "updatedAt": "2024-06-26T15:30:19Z",
  "streamingUrl": "https://...",
  "isDeleted": false
}
```

### WeatherForecastController

ASP.NET Web API テンプレート由来のサンプル API です。

|メソッド|エンドポイント|必須パラメータ|成功時レスポンス|主なエラー|
|---|---|---|---|---|
|GET|`/WeatherForecast`|なし|5 件のランダムな天気データ配列。`date`, `temperatureC`, `temperatureF`, `summary`|明示的なエラー処理なし|

## テスト/CI

このリポジトリ内には、現時点でテストプロジェクト、テストファイル、CI ワークフローは確認できません。

## 置換先

現在の `tvapp-web` では、ストリーム解決が純 TypeScript 実装へ移行されています。関連する実装・文書では `Platform-Stream-Loader` が置換先として扱われています。この BFF 側には、その移行後の実装は含まれていません。
