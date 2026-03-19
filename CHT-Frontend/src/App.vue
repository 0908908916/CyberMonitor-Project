<template>
  <div class="dashboard-container">
    <div class="content-wrapper">
      <h1 class="title">🛡️ 資安自動化監控系統</h1>

      <div class="action-card">
        <h3>➕ 接入監控設備</h3>
        <div class="input-group">
          <input v-model="newLog.deviceName" placeholder="設備名稱" />
          <input v-model="newLog.ip" placeholder="IP 位址" />
          <input v-model="newLog.status" placeholder="初始狀態" />
          <button class="btn btn-add" @click="createLog">確認新增</button>
        </div>
        <p class="timer-hint">⏱️ 系統設定：每 5 分鐘自動進行全設備連線檢測</p>
      </div>

      <div class="table-wrapper">
        <table class="styled-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>監控設備 (IP)</th>
              <th>連線狀態</th>
              <th class="hide-mobile">系統日誌</th>
              <th>管理操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="log in logs" :key="log.id">
              <td data-label="ID">{{ log.id }}</td>
              <td data-label="設備">
                <strong>{{ log.deviceName }}</strong><br/>
                <small class="ip-text">{{ log.ip }}</small>
              </td>
              <td data-label="狀態">
                <span :class="log.isOnline ? 'status-green' : 'status-red'">
                  {{ log.isOnline === undefined ? '⏳ 待偵測' : (log.isOnline ? '✅ 在線 ('+log.ms+')' : '❌ 斷線') }}
                </span>
              </td>
              <td data-label="日誌" class="hide-mobile">{{ log.message }}</td>
              <td data-label="操作" class="ops-cell">
                <button class="btn btn-ai" @click="analyzeLog(log.id)">🤖 分析</button>
                <button class="btn btn-ping" @click="testPing(log)">⚡ 測試</button>
                <button class="btn btn-del" @click="deleteLog(log.id)">🗑️</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <transition name="slide-up">
        <div v-if="analysisResult" class="ai-report-card">
          <div class="ai-report-header">
            <h3>🤖 AI 資安診斷建議報告</h3>
            <button class="close-btn" @click="analysisResult = ''">✕</button>
          </div>
          <div class="ai-report-body">
            <div class="report-content">
              {{ analysisResult }}
            </div>
          </div>
          <div class="ai-report-footer">
            <small>產出時間：{{ new Date().toLocaleString() }} | 診斷模型：Gemini Security Engine</small>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

const logs = ref([])
const analysisResult = ref('') // AI 分析結果變數
const newLog = ref({ deviceName: '', ip: '', status: '監控中', message: '設備已上線', timestamp: new Date().toISOString() })
let autoPingTimer = null;

// 獲取所有日誌
const fetchLogs = async () => {
  const res = await axios.get('http://localhost:5024/api/log')
  logs.value = res.data
  autoPingAll();
}

// 新增監控設備
const createLog = async () => {
  if(!newLog.value.deviceName || !newLog.value.ip) return alert("請填寫名稱與 IP");
  await axios.post('http://localhost:5024/api/log', newLog.value);
  newLog.value = { deviceName: '', ip: '', status: '監控中', message: '設備已上線', timestamp: new Date().toISOString() };
  fetchLogs();
}

// 單體 Ping 測試
const testPing = async (log) => {
  try {
    const res = await axios.get(`http://localhost:5024/api/log/ping/${log.id}`);
    
    // 1. 更新前端畫面顯示
    log.isOnline = res.data.isAlive;
    log.ms = res.data.responseTime;
    
    // 2. 關鍵：同步更新資料庫中的 Status 與 Message，否則 AI 永遠抓到舊資料
    const updatedStatus = log.isOnline ? "✅ 在線" : "❌ 斷線";
    const updatedMsg = log.isOnline ? `延遲: ${log.ms}ms` : "主機無回應 (Request Timeout)";
    
    await axios.put(`http://localhost:5024/api/log/${log.id}`, {
      ...log,
      status: updatedStatus,
      message: updatedMsg
    });

    log.status = updatedStatus;
    log.message = updatedMsg;

  } catch (error) {
    log.isOnline = false;
    log.status = "❌ 斷線";
    log.message = "網路異常或 IP 格式錯誤";
    // 也要把錯誤狀態存進去
    await axios.put(`http://localhost:5024/api/log/${log.id}`, { ...log });
  }
}

// AI 診斷分析函式
const analyzeLog = async (id) => {
  try {
    const res = await axios.get(`http://localhost:5024/api/log/analyze/${id}`);
    
    // 💡 加這行測試：
    alert("收到後端資料了！內容是：" + JSON.stringify(res.data));

    analysisResult.value = res.data.analysis; 
    console.log("AI 建議內容:", analysisResult.value);
  } catch (error) {
    console.error("分析失敗:", error);
    analysisResult.value = "⚠️ 無法取得分析建議，請檢查後端服務是否正常。";
  }
}

// 定時掃描
const autoPingAll = () => {
  logs.value.forEach(log => testPing(log));
}

// 刪除設備
const deleteLog = async (id) => {
  if (confirm("確定要移除嗎？")) {
    await axios.delete(`http://localhost:5024/api/log/${id}`);
    fetchLogs();
    if(analysisResult.value) analysisResult.value = ''; // 如果正在看該設備分析，一併清除
  }
}

onMounted(() => {
  fetchLogs();
  autoPingTimer = setInterval(autoPingAll, 300000); 
})

onUnmounted(() => {
  if (autoPingTimer) clearInterval(autoPingTimer);
})
</script>

<style scoped>
/* 核心佈局 */
.dashboard-container {
  min-height: 100vh;
  width: 100vw;
  margin: 0;
  padding: 40px;
  box-sizing: border-box;
  background: linear-gradient(135deg, #051937, #004d7a, #008793, #00bf72, #a8eb12);
  background-size: 400% 400%;
  animation: gradient 15s ease infinite;
  color: white;
  display: block; 
}

.content-wrapper {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
}

@keyframes gradient {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

.title { 
  font-size: clamp(2rem, 5vw, 3.5rem);
  text-align: center; 
  margin: 0 0 40px 0; 
  text-shadow: 0 4px 10px rgba(0,0,0,0.3);
}

/* 接入設備卡片 */
.action-card { 
  background: rgba(0, 0, 0, 0.4); 
  padding: 25px; 
  border-radius: 15px; 
  border: 1px solid #00d4ff; 
  margin-bottom: 30px; 
  box-shadow: 0 8px 32px rgba(0,0,0,0.3);
}

.input-group { display: flex; flex-wrap: wrap; gap: 10px; }
.input-group input { 
  flex: 1; min-width: 180px; padding: 12px; 
  font-size: 16px; border-radius: 8px; border: none; 
  background: rgba(255,255,255,0.9);
}

/* 按鈕樣式 */
.btn { padding: 12px 20px; border-radius: 8px; border: none; cursor: pointer; font-weight: bold; transition: 0.3s; }
.btn-ai { background: #6a11cb; color: white; margin-right: 5px; }
.btn-ai:hover { background: #2575fc; transform: translateY(-2px); box-shadow: 0 5px 15px rgba(106,17,203,0.4); }
.btn-ping { background: #f39c12; color: white; margin-right: 5px; }
.btn-del { background: rgba(255, 75, 43, 0.8); color: white; }
.btn-add { background: #00d4ff; color: #051937; width: 100%; }
@media (min-width: 768px) { .btn-add { width: auto; } }

/* 表格樣式 */
.table-wrapper { width: 100%; background: rgba(0,0,0,0.3); border-radius: 15px; overflow: hidden; backdrop-filter: blur(5px); }
.styled-table { width: 100%; border-collapse: collapse; }
.styled-table th { background: rgba(0,212,255,0.2); padding: 15px; text-align: left; font-size: 18px; }
.styled-table td { padding: 15px; border-bottom: 1px solid rgba(255,255,255,0.1); font-size: 16px; }

/* AI 報告區塊樣式 */
.ai-report-card {
  margin-top: 30px;
  background: rgba(26, 26, 46, 0.9);
  border: 1px solid #00d4ff;
  border-left: 6px solid #6a11cb;
  border-radius: 15px;
  padding: 25px;
  box-shadow: 0 15px 35px rgba(0,0,0,0.5);
}

.ai-report-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid rgba(255,255,255,0.1); margin-bottom: 15px; padding-bottom: 10px; }
.ai-report-header h3 { color: #00d4ff; margin: 0; font-size: 22px; }
.report-content { font-size: 18px; line-height: 1.6; color: #e0e0e0; white-space: pre-line; }
.ai-report-footer { margin-top: 15px; color: #888; text-align: right; }
.close-btn { background: none; border: none; color: #666; font-size: 22px; cursor: pointer; }
.close-btn:hover { color: #ff4b2b; }

/* 響應式手機佈局 */
@media (max-width: 768px) {
  .styled-table thead { display: none; }
  .styled-table tr { display: block; margin-bottom: 15px; border: 1px solid rgba(0,212,255,0.3); padding: 10px; border-radius: 10px; }
  .styled-table td { display: flex; justify-content: space-between; align-items: center; border: none; padding: 10px 5px; }
  .styled-table td::before { content: attr(data-label); font-weight: bold; color: #00d4ff; }
  .hide-mobile { display: none; }
  .ops-cell { justify-content: center !important; gap: 5px; }
}

/* 動畫 */
.slide-up-enter-active, .slide-up-leave-active { transition: all 0.5s ease; }
.slide-up-enter-from, .slide-up-leave-to { opacity: 0; transform: translateY(30px); }

.status-green { color: #00ff88; font-weight: bold; }
.status-red { color: #ff4b2b; font-weight: bold; }
.ip-text { color: #00d4ff; }
.timer-hint { margin-top:10px; font-size: 14px; color: #00d4ff; opacity: 0.8; }
</style>