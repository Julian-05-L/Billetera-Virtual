<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { AnimacionCriptos } from '../composables/useBackgroundAnimation'
import navBar from '@/components/navBar.vue'
import ModificarTransaccionModal from '@/components/ModificarTransaccionModal.vue'
import GraficoPortafolio from '@/components/GraficoPortafolio.vue'
import axios from 'axios'

onMounted(() => {
  AnimacionCriptos()
})

const transacciones = ref<any[]>([])
const loading = ref(true)
const error = ref('')
const portafolio = ref<any>({ distribucion: [], valorTotalARS: 0 })
const loadingPortafolio = ref(true)
const isModifyModalOpen = ref(false)
const selectedTransaction = ref<any | null>(null)
const graficoPortafolioRef = ref<any>(null)

const userId = Number(localStorage.getItem('userId'))

async function GetTransacciones() {
  try {
    const response = await axios.get(`http://localhost:5000/transaccion/cliente/${userId}`)

    if (!response.data || response.data.length === 0) {
      error.value = 'Este usuario no tiene transacciones registradas.'
    } else {
      transacciones.value = response.data
    }
  } catch (err) {
    error.value = 'Error al cargar las transacciones.'
  } finally {
    loading.value = false
  }
}

GetTransacciones()

async function getPortafolio() {
  loadingPortafolio.value = true
  try {
    const response = await axios.get(`http://localhost:5000/portafolio/${userId}`)
    portafolio.value = response.data
  } catch (err) {
    // El error se manejará en el gráfico, aquí solo nos aseguramos de que no bloquee.
    console.error('Error al cargar el portafolio:', err)
    portafolio.value = { distribucion: [], valorTotalARS: 0 }
  } finally {
    loadingPortafolio.value = false
  }
}

async function deleteTransaccion(id: number) {
  // Pedimos confirmación al usuario para evitar borrados accidentales
  if (!confirm('¿Estás seguro de que quieres eliminar esta transacción?')) {
    return
  }

  try {
    await axios.delete(`http://localhost:5000/transaccion/${id}`)
    // Filtramos el array para quitar la transacción eliminada y actualizar la UI
    transacciones.value = transacciones.value.filter((t) => t.id !== id)
    // Recargamos los datos del portafolio para actualizar el saldo y el gráfico
    if (graficoPortafolioRef.value) {
      graficoPortafolioRef.value.cargarPortafolio()
    }
  } catch (err) {
    alert('Error al eliminar la transacción.')
  }
}

function openModifyModal(transaction: any) {
  selectedTransaction.value = transaction
  isModifyModalOpen.value = true
}

async function onTransactionUpdated() {
  isModifyModalOpen.value = false
  await GetTransacciones()
  if (graficoPortafolioRef.value) {
    graficoPortafolioRef.value.cargarPortafolio()
  }
}

onMounted(() => {
  GetTransacciones()
  getPortafolio()
})
</script>

<template>
  <canvas id="canvasCripto"></canvas>
  <nav-bar />

  <div class="transacciones-container">
    <div class="info-columna">
      <h1 class="text-4xl font-bold mb-4">Tu Portafolio</h1>
      <p class="text-lg">
        Visualiza la distribución de tus criptomonedas y el valor total de tu inversión.
      </p>

      <div class="card-transacciones">
        <h2 class="text-2xl font-semibold mb-4 text-center">Transacciones Recientes</h2>

        <div v-if="loading">Cargando transacciones...</div>

        <div v-else-if="error" class="mensaje-sin-datos">
          {{ error }}
        </div>

        <div v-else class="contenedor-tabla">
          <table class="tabla-transacciones">
            <thead>
              <tr>
                <th>Cripto</th>
                <th>Acción</th>
                <th>Monto Cripto</th>
                <th>Monto ARS</th>
                <th>Fecha</th>
                <th></th>
              </tr>
            </thead>

            <tbody class="cuerpo-tabla">
              <tr
                v-for="t in transacciones"
                :key="t.id"
                class="fila-transaccion"
                :class="t.action === 'purchase' ? 'fila-purchase' : 'fila-sale'"
              >
                <td>{{ t.cryptoCode.toUpperCase() }}</td>
                <td :class="t.action === 'purchase' ? 'text-red-400' : 'text-green-400'">
                  {{ t.action }}
                </td>
                <td>{{ t.cryptoAmount }}</td>
                <td>${{ Number(t.money).toFixed(2) }}</td>
                <td>{{ new Date(t.fechaTransaccion).toLocaleDateString() }}</td>
                <td class="acciones-cell">
                  <button @click="openModifyModal(t)" class="btn-accion btn-editar">Editar</button>
                  <button @click="deleteTransaccion(t.id)" class="btn-accion btn-eliminar">
                    Eliminar
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div class="grafico-columna">
      <grafico-portafolio ref="graficoPortafolioRef" />
    </div>

    <!-- Modal para Modificar Transacción -->
    <teleport to="body">
      <modificar-transaccion-modal
        v-if="isModifyModalOpen && selectedTransaction"
        :transaction="selectedTransaction"
        @close="isModifyModalOpen = false"
        @transaction-updated="onTransactionUpdated"
      />
    </teleport>
  </div>
</template>

<style scoped>
.transacciones-container {
  display: flex;
  justify-content: center; /* Centra el bloque de columnas en la página */
  align-items: stretch; /* Hace que las columnas tengan la misma altura */
  padding: 2rem;
  margin-top: 60px; /* Ajustar según la altura de tu navBar */
  gap: 4rem; /* Aumentamos la separación entre columnas */
  color: white;
  width: 100%;
}

.info-columna {
  flex-basis: 65%; /* Le asignamos un 65% del ancho disponible */
  max-width: 1400px; /* Aumentamos el ancho máximo */
  text-align: left;
  display: flex; /* Habilitamos flexbox para esta columna */
  flex-direction: column; /* Apilamos los elementos verticalmente */
}

.grafico-columna {
  flex: 1; /* Esta columna ocupará el espacio restante */
  min-width: 400px; /* Aumentamos el mínimo para que no se comprima demasiado */
  max-width: 450px;
  display: flex; /* Habilitamos flexbox para esta columna */
  flex-direction: column; /* Apilamos los elementos verticalmente */
}
.card-saldo {
  background: rgba(79, 209, 197, 0.2);
  border: 1px solid #4fd1c5;
  border-radius: 10px;
  padding: 1.5rem;
  margin-top: 2rem;
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.saldo-label {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.8);
}
.saldo-valor {
  font-size: 2.5rem;
  font-weight: 700;
}

.card-transacciones {
  background: rgba(24, 58, 55, 0.6);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 1.5rem;
  margin-top: 1.5rem;
  box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
  flex-grow: 1; /* Hacemos que la tarjeta crezca para llenar el espacio */
  display: flex; /* Flexbox para controlar el contenido interno */
  flex-direction: column; /* Apilar título y tabla */
}

.contenedor-tabla {
  overflow-y: auto; /* Habilita el scroll vertical si es necesario */
  max-height: 350px; /* Altura máxima antes de que aparezca el scroll */
  flex-grow: 1; /* Permite que este contenedor crezca */
}

/* Estilo para la barra de scroll */
.contenedor-tabla::-webkit-scrollbar {
  width: 8px;
}
.contenedor-tabla::-webkit-scrollbar-thumb {
  background-color: #4fd1c5;
  border-radius: 4px;
}

.tabla-transacciones {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 0.9rem;
}

.tabla-transacciones th,
.tabla-transacciones td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid rgba(79, 209, 197, 0.2);
}

.tabla-transacciones th {
  color: #4fd1c5; /* Color acento para los encabezados */
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  position: sticky;
  top: 0;
  background: rgb(24, 58, 55);
  z-index: 10;
  white-space: nowrap; /* Evita que el texto del encabezado se divida en varias líneas */
}

.fila-transaccion:hover {
  background-color: rgba(79, 209, 197, 0.1);
}

.fila-transaccion.fila-purchase:hover {
  background-color: rgba(79, 209, 197, 0.1); /* Tono verde */
}

.fila-transaccion.fila-sale:hover {
  background-color: rgba(255, 99, 132, 0.1); /* Tono rojo */
}

.acciones-cell {
  text-align: center;
  white-space: nowrap;
}

.btn-accion {
  border: none;
  padding: 6px 12px;
  border-radius: 5px;
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: 500;
  transition: all 0.2s ease-in-out;
  margin: 0 4px;
}

.btn-editar {
  background-color: rgba(54, 162, 235, 0.7);
  color: white;
}

.btn-eliminar {
  background-color: rgba(255, 99, 132, 0.7);
  color: white;
}

.mensaje-sin-datos {
  text-align: center;
  padding: 2rem;
  background-color: rgba(0, 0, 0, 0.2);
  border-radius: 8px;
}
</style>
