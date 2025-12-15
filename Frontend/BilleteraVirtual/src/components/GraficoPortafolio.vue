<script setup lang="ts">
import { onMounted, ref } from 'vue'
import axios from 'axios'
import { Chart, ArcElement, Tooltip, Legend, PieController } from 'chart.js'

Chart.register(ArcElement, Tooltip, Legend, PieController)

const chartCanvas = ref<HTMLCanvasElement | null>(null)
let mychart: Chart | null = null

const distribucion = ref<any[]>([])
const valorArsTotal = ref<number>(0)
const sinDatos = ref<boolean>(false)

const userId = Number(localStorage.getItem('userId'))

const cargarPortafolio = async () => {
  if (!userId) {
    console.error('Usuario no encontrado')
    return
  }

  try {
    const response = await axios.get(`http://localhost:5000/portafolio/${userId}`)
    const portafolio = response.data

    distribucion.value = portafolio.distribucion ?? []
    valorArsTotal.value = portafolio.valorTotalARS ?? 0

    if (distribucion.value.length === 0) {
      sinDatos.value = true
      return
    } else {
      sinDatos.value = false
      renderChart()
    }
  } catch (error) {
    console.error('Error al cargar el portafolio:', error)
  }
}

const renderChart = () => {
  if (!chartCanvas.value) return

  if (mychart) mychart.destroy()

  const etiquetas = distribucion.value.map((d) => d.cryptoCode.toUpperCase())
  const valores = distribucion.value.map((d) => d.valorARS)

  mychart = new Chart(chartCanvas.value, {
    type: 'pie',
    data: {
      labels: etiquetas,
      datasets: [
        {
          data: valores,
          backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40'],
        },
      ],
    },
    options: {
      responsive: true,
      plugins: {
        legend: {
          position: 'top',
          labels: {
            color: 'white',
            font: { size: 14 },
          },
        },
        tooltip: { enabled: true },
      },
    },
  })
}

onMounted(() => {
  cargarPortafolio()
})

// Exponemos la función para que el componente padre pueda llamarla
defineExpose({
  cargarPortafolio,
})
</script>

<template>
  <div class="card-grafico">
    <h2 class="text-2xl font-bold text-center mb-4">Distribución de Activos</h2>

    <div v-if="sinDatos" class="mensaje-sin-datos">
      <p class="text-lg font-semibold">No hay datos para mostrar</p>
      <p>Aún no has realizado ninguna transacción.</p>
    </div>

    <div v-else>
      <canvas ref="chartCanvas"></canvas>

      <div class="valor-total">
        <h3 class="text-xl font-semibold">
          Valor Total en ARS: ${{
            valorArsTotal.toLocaleString('es-AR', {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2,
            })
          }}
        </h3>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-grafico {
  background: rgba(24, 58, 55, 0.6);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 1.5rem;
  color: white;
  box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
  flex-grow: 1;
}

.mensaje-sin-datos {
  text-align: center;
  padding: 2rem;
  background-color: rgba(0, 0, 0, 0.2);
  border-radius: 8px;
}
.valor-total {
  text-align: center;
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.2);
}
</style>
