<script setup lang="ts">
import { useAuth } from '../composables/useAuth'
import { onMounted } from 'vue'
import { AnimacionCriptos } from '../composables/useBackgroundAnimation'

onMounted(() => {
  AnimacionCriptos()
})

const { logout } = useAuth()

</script>

<template>
  <canvas id="canvasCripto"></canvas>
  <header class="navbar">
    <div class="left">
      <h2>Billetera</h2>
    </div>

    <nav class="right">
      <router-link to="/" class="nav-btn">Home</router-link>
      <router-link to="/movimientos" class="nav-btn">Movimientos</router-link>
      <button class="logout-btn" @click="logout">Cerrar Sesión</button>
    </nav>
  </header>
</template>

<style>
.navbar {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 60px;
  background: rgba(24, 58, 55, 0.5);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  color: #efefef;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 30px;
  z-index: 1000;
  border-bottom: 1px solid rgba(79, 209, 197, 0.2);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.25);
}

.navbar .left h2 {
  font-weight: 600;
  color: #efefef;
  margin: 0;
  text-shadow: 0 1px 3px rgba(0, 0, 0, 0.5);
}

.nav-btn {
  margin-right: 20px;
  color: #efefef;
  text-decoration: none;
  font-weight: 500;
  padding: 8px 12px;
  border-radius: 6px;
  transition:
    background-color 0.3s ease,
    color 0.3s ease;
}

.nav-btn:hover,
.nav-btn.router-link-exact-active {
  background-color: rgba(79, 209, 197, 0.15);
  color: #4fd1c5;
}

.logout-btn {
  background: transparent;
  border: 1px solid #ff4b4b;
  color: #ff4b4b;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition:
    background-color 0.3s ease,
    color 0.3s ease;
}

.logout-btn:hover {
  background: #ff4b4b;
  color: #1e1e1e;
  box-shadow: 0 0 10px rgba(255, 75, 75, 0.5);
}
</style>
