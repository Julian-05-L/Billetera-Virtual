import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import axios from 'axios'

const app = createApp(App)

axios.defaults.baseURL = 'http://localhost:5000';

axios.interceptors.request.use(config => {
  const token = localStorage.getItem('userToken');
  if(token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, error => {
  return Promise.reject(error);
});

export default axios;

app.use(createPinia())
app.use(router)

app.mount('#app')
