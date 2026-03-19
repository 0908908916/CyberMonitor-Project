# 🛡️ AI 資安自動化監控系統 (Security Automation Monitor)

這是一個全端資安實作專案，整合了設備監控、自動化 Ping 測試與 AI 診斷分析。

## 🚀 核心技術

- **前端**: Vue 3 (Vite)
- **後端**: .NET 8 Web API (C#)
- **資料庫**: PostgreSQL
- **部署**: Docker & Docker Compose
- **資安**: 環境變數隔離 (.env)

## 🛠️ 如何在本地執行

1. **複製專案**: `git clone <your-repo-url>`
2. **設定環境變數**: 建立 `.env` 檔案並填入資料庫密碼。
3. **啟動系統**: 執行 `docker-compose up --build -d`
4. **存取網頁**: `http://localhost:5173`

## 🔒 安全聲明

本專案透過 `.gitignore` 嚴格過濾 `.env` 檔案，確保資料庫認證資訊不外洩。
