# CLAUDE.md - tvapp-bff

> 横断の共通ルールは、このリポジトリを束ねる `ai-steering` 側の文書を正本とする。
> ここにはこのリポジトリ固有の地図と事実を置く。

## 1. これは何か

TVer 系 API へのプロキシと、`yt-dlp` による episodeId からのストリーミング URL 取得を行っていた ASP.NET Core Web API の BFF。

現状は .NET 7 のレガシーコンポーネント。ローカル Git 履歴上の最終コミットは `2024-06-27T21:46:10+09:00`。`tvapp-web` が純 TypeScript のストリーム解決へ移行したため、この BFF は停止中・廃止候補として扱われている。

## 2. 構成

- `Xiao.TVapp.Bff/` - Web API プロジェクト本体。
  - `Program.cs` - CORS、Swagger、EF Core、controller routing。
  - `Controllers/TVappController.cs` - TVer 系 API プロキシ、DB なしの `yt-dlp` ストリーミング URL 取得。
  - `Controllers/StreamingUrlsController.cs` - `StreamingUrls` CRUD、DB キャッシュ付き URL 取得。
  - `Controllers/WeatherForecastController.cs` - ASP.NET テンプレート由来のサンプル API。
  - `Contexts/StreamingUrlsContext.cs` - EF Core DbContext。コンストラクタで `Database.Migrate()` を実行。
  - `Migrations/20240626153019_InitialCreate.cs` - `StreamingUrls` 1 テーブルの初期 migration。
- `Dockerfile.bff` - .NET 7 runtime に `yt-dlp` latest を入れる。
- `docker-compose.yml` - `tvapp-bff`、`adminer`、`mysql`。BFF は `5231:5231`、Adminer は `5230:8080`。
- `docs/STATUS.md` - 停止状態、構成、判断材料の事実整理。

## 3. 技術スタック

- C# / ASP.NET Core Web API
- Target Framework: `net7.0`
- EF Core 7 + Pomelo MySQL provider
- MariaDB 10.5
- Swagger/OpenAPI。Development のみ
- Newtonsoft.Json
- Docker / Docker Compose
- `yt-dlp`

## 4. 現状ステータス

- 停止中のレガシー BFF。
- `tvapp-web` の純 TypeScript ストリーム解決と `Platform-Stream-Loader` が置換先として扱われている。
- テストプロジェクト、テストファイル、CI ワークフローは確認できない。
- README と `docs/STATUS.md` は、廃止/保全の判断ではなく判断材料になる事実を記録する。

## 5. 落とし穴

- Docker の BFF ポートは `5231`。過去 README の `7044` は Docker Compose とは一致しない。`7044` は `launchSettings.json` の HTTPS profile 側。
- `appsettings.json` の DB 接続先は `Server=mysql`。Docker Compose のサービス名を前提にしている。
- `StreamingUrlsContext` 生成時に `Database.Migrate()` が走る。
- ストリーミング URL 取得は DB なし `/api/TVapp/streaming/{episodeId}` と DB キャッシュあり `/api/StreamingUrls/episode/{episodeId}` の 2 系統がある。
- 外部 API レスポンスを JSON として詰め替えず、本文文字列を `Ok(content)` で返す API が複数ある。
- Dockerfile は `yt-dlp` latest をビルド時取得するため、ビルド時点により入るバイナリが変わる。
- CORS は `AllowAnyOrigin` / `AllowAnyHeader` / `AllowAnyMethod`。
- EF Core の `EnableSensitiveDataLogging()` と `EnableDetailedErrors()` が有効。
- `WeatherForecastController` はテンプレート由来のサンプル API と見られる。
