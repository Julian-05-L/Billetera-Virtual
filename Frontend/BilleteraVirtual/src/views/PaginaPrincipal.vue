<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { AnimacionCriptos } from '../composables/useBackgroundAnimation'
import navBar from '@/components/navBar.vue'
import axios from 'axios'

onMounted(() => {
  AnimacionCriptos()
  getSaldoCliente()
})

const saldoCliente = ref(0)
const loadingSaldo = ref(true)

const montoAAgregar = ref<number | null>(null)
const agregarSaldoLoading = ref(false)
const agregarSaldoError = ref('')
const agregarSaldoSuccess = ref('')

const newTransaction = ref({
  cryptoCode: '',
  action: 'purchase',
  cryptoAmount: null,
})

const formLoading = ref(false)
const formError = ref('')
const formSuccess = ref('')

const userId = Number(localStorage.getItem('userId'))

async function getSaldoCliente() {
  loadingSaldo.value = true
  try {
    const response = await axios.get(`http://localhost:5000/cliente/${userId}`)
    saldoCliente.value = response.data.saldoPesos ?? 0
  } catch (err) {
    console.error('Error al cargar el saldo del cliente:', err)
    saldoCliente.value = 0
  } finally {
    loadingSaldo.value = false
  }
}

async function agregarSaldo() {
  if (!montoAAgregar.value || montoAAgregar.value <= 0) {
    agregarSaldoError.value = 'Por favor, ingresa un monto válido.'
    return
  }

  agregarSaldoLoading.value = true
  agregarSaldoError.value = ''
  agregarSaldoSuccess.value = ''

  try {
    await axios.patch(`http://localhost:5000/cliente/${userId}/saldo`, {
      saldoPesos: montoAAgregar.value,
    })

    agregarSaldoSuccess.value = `¡Se agregaron $${(montoAAgregar.value ?? 0).toLocaleString(
      'es-AR',
      {
        minimumFractionDigits: 2,
      },
    )} a tu saldo!`
    montoAAgregar.value = null
    await getSaldoCliente()
  } catch (err: any) {
    agregarSaldoError.value = err.response?.data?.message || 'Error al agregar saldo.'
  } finally {
    agregarSaldoLoading.value = false
  }
}

async function createTransaction() {
  formLoading.value = true
  formError.value = ''
  formSuccess.value = ''

  if (!userId) {
    formError.value = 'Error: Usuario no identificado.'
    formLoading.value = false
    return
  }

  try {
    const payload = {
      cryptoCode: newTransaction.value.cryptoCode,
      action: newTransaction.value.action,
      cryptoAmount: newTransaction.value.cryptoAmount,
      clienteId: userId,
    }

    await axios.post('http://localhost:5000/transaccion', payload)

    formSuccess.value = '¡Transacción creada con éxito!'

    newTransaction.value = {
      cryptoCode: '',
      action: 'purchase',
      cryptoAmount: null,
    }

    await getSaldoCliente()
  } catch (err: any) {
    formError.value = err.response?.data?.message || 'Error al crear la transacción.'
  } finally {
    formLoading.value = false
  }
}
</script>

<template>
  <canvas id="canvasCripto"></canvas>
  <nav-bar />

  <div class="main-page-container">
    <div class="columna-info">
      <h1 class="text-4xl font-bold mb-4">Bienvenido a tu Billetera</h1>
      <p class="text-lg mb-8">
        Aquí puedes gestionar tus activos y realizar nuevas transacciones de forma rápida y segura.
      </p>

      <div class="card-saldo">
        <span class="saldo-label">Saldo Disponible</span>
        <span v-if="loadingSaldo" class="saldo-valor">Cargando...</span>
        <span v-else class="saldo-valor">
          ${{
            (saldoCliente ?? 0).toLocaleString('es-AR', {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2,
            })
          }}
          ARS
        </span>
      </div>

      <div class="card-agregar-saldo">
        <h3 class="form-title-sm">Agregar Saldo a tu Cuenta</h3>
        <form @submit.prevent="agregarSaldo()">
          <div class="form-group">
            <label for="montoAAgregar">Monto en ARS</label>
            <input type="number" step="any" id="montoAAgregar" v-model="montoAAgregar" required />
          </div>

          <div v-if="agregarSaldoError" class="alert-error">{{ agregarSaldoError }}</div>
          <div v-if="agregarSaldoSuccess" class="alert-success">{{ agregarSaldoSuccess }}</div>

          <div class="modal-actions">
            <button type="submit" class="btn-guardar" :disabled="agregarSaldoLoading">
              {{ agregarSaldoLoading ? 'Agregando...' : 'Agregar Saldo' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <div class="columna-form">
      <div class="card-form">
        <h3 class="form-title">Registrar Nueva Transacción</h3>

        <form @submit.prevent="createTransaction()">
          <div class="form-group">
            <label for="cryptoCode">Criptomoneda (ej: BTC, ETH o USDT)</label>
            <input type="text" id="cryptoCode" v-model="newTransaction.cryptoCode" required />
          </div>

          <div class="form-group">
            <label for="action">Acción</label>
            <select id="action" v-model="newTransaction.action" required>
              <option value="purchase">Compra</option>
              <option value="sale">Venta</option>
            </select>
          </div>

          <div class="form-group">
            <label for="cryptoAmount">Monto en Cripto</label>
            <input
              type="number"
              step="any"
              id="cryptoAmount"
              v-model="newTransaction.cryptoAmount"
              required
            />
          </div>

          <div v-if="formError" class="alert-error">{{ formError }}</div>
          <div v-if="formSuccess" class="alert-success">{{ formSuccess }}</div>

          <div class="modal-actions">
            <button type="submit" class="btn-guardar" :disabled="formLoading">
              {{ formLoading ? 'Registrando...' : 'Registrar Transacción' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.main-page-container {
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding: 2rem;
  margin-top: 80px;
  gap: 4rem;
  color: white;
  width: 100%;
  max-width: 1600px;
  margin-left: auto;
  margin-right: auto;
}

.columna-info {
  flex: 1;
  max-width: 600px;
}

.columna-form {
  flex: 1;
  max-width: 500px;
}

.card-saldo {
  background: rgba(79, 209, 197, 0.2);
  border: 1px solid #4fd1c5;
  border-radius: 10px;
  padding: 1.5rem 2rem;
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

.card-agregar-saldo {
  background: rgba(24, 58, 55, 0.6);
  backdrop-filter: blur(5px);
  padding: 1.5rem;
  border-radius: 15px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
  margin-top: 2rem;
}

.card-form {
  background: rgba(24, 58, 55, 0.6);
  backdrop-filter: blur(10px);
  padding: 2rem;
  border-radius: 15px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
}

.form-title {
  font-size: 1.8rem;
  font-weight: 600;
  text-align: center;
  margin-bottom: 1.5rem;
  color: #efefef;
}

.form-title-sm {
  font-size: 1.2rem;
  font-weight: 600;
  text-align: center;
  margin-bottom: 1.5rem;
  color: #efefef;
}

.form-group {
  margin-bottom: 1.25rem;
}
.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #a0aec0;
}
.form-group input,
.form-group select {
  width: 100%;
  padding: 0.75rem 1rem;
  background-color: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  border-radius: 6px;
  font-size: 1rem;
}

.form-group select option {
  background: #183a37;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 2rem;
}
.btn-guardar {
  width: 100%;
  padding: 12px 20px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-weight: 600;
  font-size: 1rem;
  background-color: #4fd1c5;
  color: #1e1e1e;
  transition: background-color 0.3s ease;
}
.btn-guardar:disabled {
  background-color: #4a5568;
  cursor: not-allowed;
}

.alert-error,
.alert-success {
  padding: 0.75rem;
  border-radius: 6px;
  text-align: center;
  margin-top: 1rem;
  border: 1px solid;
}
.alert-error {
  color: #ff4b4b;
  background-color: rgba(255, 75, 75, 0.1);
  border-color: #ff4b4b;
}
.alert-success {
  color: #4fd1c5;
  background-color: rgba(79, 209, 197, 0.1);
  border-color: #4fd1c5;
}
</style>
