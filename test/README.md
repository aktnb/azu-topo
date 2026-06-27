# Test Function App

azu-topo の可視化動作確認用サンプル Azure Functions アプリです。

## トポロジー

```
HTTP POST /orders
  └─ ProcessOrderFunction  ──[output binding]──▶  orders キュー
                                                       │
                                              [trigger binding]
                                                       │
                                                 HandleOrderFunction  ──[client send]──▶  notifications キュー
                                                                                                  │
HTTP POST /notifications                                                              [timer, client receive]
  └─ ManualSendFunction  ──[client send]──▶  notifications キュー ────────────────▶  ManualReceiveFunction
```

| 関数 | トリガー | 接続方式 | 役割 |
|---|---|---|---|
| ProcessOrderFunction | HTTP POST `/orders` | ServiceBus output binding | orders キューへメッセージ送信 |
| HandleOrderFunction | ServiceBusTrigger `orders` | ServiceBus trigger binding + client send | orders を受信して notifications へ転送 |
| ManualSendFunction | HTTP POST `/notifications` | ServiceBusClient | notifications キューへメッセージ送信 |
| ManualReceiveFunction | Timer (毎分) | ServiceBusClient | notifications キューからメッセージを pull 受信 |

## 前提条件

- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) がインストール済み
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local) がインストール済み
- .NET 10 SDK がインストール済み
- デプロイ先の Azure サブスクリプションが利用可能な状態

## デプロイ手順

### 1. Azure へログイン

```bash
az login
az account set --subscription <サブスクリプション ID>
```

### 2. Resource Group の作成

```bash
az group create --name <リソースグループ名> --location japaneast
```

### 3. インフラのデプロイ

`infra/parameters/dev.bicepparam` の `appSuffix` を必要に応じて変更してください（リソース名の末尾に付くサフィックスです）。

```bash
az deployment group create \
  --resource-group <リソースグループ名> \
  --template-file test/infra/main.bicep \
  --parameters test/infra/parameters/dev.bicepparam
```

作成されるリソース:

| リソース | 名前 |
|---|---|
| Service Bus Namespace | `my-servicebus-<appSuffix>` |
| Service Bus Queue | `orders` |
| Service Bus Queue | `notifications` |
| Storage Account | `stororder-processor-<appSuffix>` の先頭24文字 |
| App Service Plan (Consumption) | `order-processor-<appSuffix>-plan` |
| Function App | `order-processor-<appSuffix>` |

### 4. Function App のビルドと発行

`test/FunctionApp/` ディレクトリで実行します。

```bash
cd test/FunctionApp

func azure functionapp publish order-processor-<appSuffix> \
  --dotnet-isolated \
  --force
```

> `order-processor-<appSuffix>` は手順 3 で作成した Function App 名に合わせてください。

## ローカル開発

### 1. Service Bus の接続文字列を取得

```bash
az servicebus namespace authorization-rule keys list \
  --resource-group <リソースグループ名> \
  --namespace-name my-servicebus-<appSuffix> \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

### 2. local.settings.json を編集

`test/FunctionApp/local.settings.json` の `ServiceBusConnectionString` に取得した接続文字列を設定します。

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnectionString": "<取得した接続文字列>"
  }
}
```

### 3. ローカル起動

```bash
cd test/FunctionApp
func start
```

### 4. 動作確認

```bash
# ProcessOrder → HandleOrder → ManualReceive の経路をトリガー
curl -X POST http://localhost:7071/api/orders \
  -H "Content-Type: application/json" \
  -d '{"item": "sample"}'

# ManualSend → ManualReceive の経路をトリガー
curl -X POST http://localhost:7071/api/notifications \
  -H "Content-Type: application/json" \
  -d '{"message": "hello"}'
```

`ManualReceiveFunction` は毎分起動して `notifications` キューを pull 受信します。ログで受信を確認できます。
