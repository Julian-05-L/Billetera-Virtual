<script setup lang="ts">
import { useAuth } from '../composables/useAuth'
import { onMounted } from 'vue'
import { AnimacionCriptos } from '../composables/useBackgroundAnimation'
import navBar from '@/components/navBar.vue'

onMounted(() => {
  AnimacionCriptos()
})
const { logout } = useAuth()

</script>


<template>
  <canvas id="canvasCripto"></canvas>
  <nav-bar />

  <div>
    <h1>Transacciones</h1>
    <p>Aquí puedes ver y gestionar todas tus transacciones de criptomonedas.</p>
  </div>
</template>

