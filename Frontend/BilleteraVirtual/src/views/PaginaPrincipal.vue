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
    <h1>Bienvenido a tu Billetera Virtual</h1>
    <p>Gestiona tus transacciones de criptomonedas de manera segura y eficiente.</p>
  </div>
</template>

<style>
</style>
