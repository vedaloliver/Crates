import { createRouter, createWebHistory } from 'vue-router'
import MasterRecordCapture from "./recordCaptureComponents/Master.vue";

const routes = [
  {
    path: '/',
    component: MasterRecordCapture
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router