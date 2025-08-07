# ShuitNet.SendEmail

コマンドラインベースのメール送信ツールです。MailKitを使用してSMTP経由でメールを送信し、設定ファイルベースのユーザー管理とテンプレート機能を提供します。

## 機能

- **SMTP経由でのメール送信**: MailKitを使用した安全なメール送信
- **ユーザー設定管理**: ドメイン別のJSON設定ファイルによる管理
- **パスワード暗号化**: 自動パスワード暗号化（Windows: DPAPI、Linux: AES）
- **テンプレート機能**: 変数置換可能なメールテンプレート
- **添付ファイル対応**: ファイル添付機能
- **詳細ログ**: NLogによる包括的なログ機能

## インストール

### 必要環境
- .NET 8.0 Runtime

### ビルドから使用
```bash
git clone https://github.com/your-repo/ShuitNet.SendEmail.git
cd ShuitNet.SendEmail
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained
```

## 使用方法

### 初期設定
メール送信前に、使用するSMTPサーバーの設定を追加してください：

```bash
sendemail add-user user@example.com --smtp-server smtp.gmail.com --smtp-port 587 --smtp-ssl true --smtp-username user@example.com --smtp-password your-password
```

### 基本的なメール送信
```bash
sendemail --from sender@example.com --to recipient@example.com --subject "テストメール" --body "これはテストメールです。"
```

### オプション付きメール送信
```bash
sendemail --from sender@example.com --to recipient@example.com --subject "テストメール" --body "メール本文" --cc cc@example.com --bcc bcc@example.com --attachment ./file.pdf --from-name "送信者名"
```

### テンプレート機能

#### テンプレートの作成
```bash
sendemail template create welcome --subject "{{company}}へようこそ、{{name}}さん" --body "{{name}}さん、{{company}}へのご登録ありがとうございます。"
```

#### テンプレート一覧
```bash
sendemail template list
```

#### テンプレートを使用したメール送信
```bash
sendemail --template welcome --vars "name=田中太郎,company=株式会社サンプル" --from sender@example.com --to recipient@example.com
```

#### テンプレートの削除
```bash
sendemail template delete welcome
```

## 設定ファイル

### ユーザー設定
設定ファイルは以下の場所に保存されます：
- Windows: `%USERPROFILE%\.shuitNet\sendemail\conf.d\{domain}.json`
- Linux: `~/.shuitNet/sendemail/conf.d/{domain}.json`

### テンプレートファイル
テンプレートファイルは以下の場所に保存されます：
- Windows: `%USERPROFILE%\.shuitNet\sendemail\templates\{name}.json`
- Linux: `~/.shuitNet/sendemail/templates/{name}.json`

## コマンドライン オプション

### メール送信オプション
- `--to`: 宛先メールアドレス
- `--from`: 送信元メールアドレス
- `--subject, -s`: 件名
- `--body, -b`: 本文
- `--cc`: CCメールアドレス
- `--bcc`: BCCメールアドレス
- `--attachment`: 添付ファイルパス
- `--from-name`: 送信者名

### SMTP設定オプション（個別指定時）
- `--smtp-server`: SMTPサーバー
- `--smtp-port`: SMTPポート
- `--smtp-username`: SMTP認証ユーザー名
- `--smtp-password`: SMTP認証パスワード
- `--smtp-ssl`: SSL使用フラグ

### テンプレートオプション
- `--template`: 使用するテンプレート名
- `--vars`: テンプレート変数（形式: `key1=value1,key2=value2`）

## サポートされるSMTPプロバイダー

以下のSMTPプロバイダーでテスト済み：
- Gmail (smtp.gmail.com:587)
- Outlook (smtp-mail.outlook.com:587)
- Yahoo Mail (smtp.mail.yahoo.com:587)

## セキュリティ

- パスワードは自動的に暗号化されて保存されます
- 暗号化されたパスワードは同一ユーザー・同一マシンでのみ復号化可能
- 設定ファイルは実行ユーザーごとに独立して管理されます

## トラブルシューティング

### よくある問題

**設定ファイルが見つからないエラー**
```
Error: Configuration file not found. Please add a user first.
```
→ `add-user` コマンドでユーザー設定を追加してください。

**認証エラー**
```
Error: Authentication failed
State: NotAuthenticated
```
→ SMTPユーザー名・パスワードを確認してください。Gmailの場合はアプリパスワードが必要です。

**接続エラー**
```
Error: Connection failed
State: ConnectionError
```
→ SMTPサーバー・ポート・SSL設定を確認してください。

## ライセンス

このプロジェクトはMITライセンスの下で公開されています。

## 貢献

プルリクエストや課題報告を歓迎します。