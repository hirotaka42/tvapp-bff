# 基本イメージとして公式の .NET SDK イメージを使用
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# 作業ディレクトリを設定
WORKDIR /app

# ソリューションファイルをコピーして復元
COPY tvapp-bff.sln ./
COPY Xiao.TVapp.Bff/*.csproj ./Xiao.TVapp.Bff/
RUN dotnet restore

# 残りのソースコードをコピーしてビルド
COPY . ./
RUN dotnet publish -c Release -o out

# 実行環境として公式の .NET ランタイム イメージを使用
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# 作業ディレクトリを設定
WORKDIR /app

# ビルド成果物をコピー
COPY --from=build /app/out ./

# yt-dlp のスタンドアロンバイナリをダウンロードしてインストール
RUN apt-get update && apt-get install -y wget && \
    wget https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux -O /usr/local/bin/yt-dlp && \
    chmod a+rx /usr/local/bin/yt-dlp && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# アプリケーションを実行
ENTRYPOINT ["dotnet", "Xiao.TVapp.Bff.dll"]