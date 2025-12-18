<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'

const props = defineProps({
  transaccion: {
    type: Object,
    required: true,
  },
})

const emit = defineEmits(['close', 'transaction-updated'])

const cryptoCode = ref('')
const action = ref('purchase')
const cryptoAmount = ref(0)
const money = ref(0)
const fechaTransaccion = ref('')

const loading = ref(false) // Ya no cargamos datos, así que empieza en false
const error = ref('')

function populateForm() {
  const data = props.transaccion
  cryptoCode.value = data.cryptoCode
  action.value = data.action
  cryptoAmount.value = data.cryptoAmount
  money.value = data.money
  fechaTransaccion.value = new Date(data.fechaTransaccion).toISOString().split('T')[0]
}

async function modificarTransaccion() {
  if (!confirm('¿Estás seguro de que quieres guardar los cambios en esta transacción?')) {
    return
  }

  loading.value = true
  error.value = ''

  try {
    const payload = {
      id: props.transaccion.id,
      cryptoCode: cryptoCode.value,
      action: action.value,
      cryptoAmount: Number(cryptoAmount.value),
      money: Number(money.value),
      fechaTransaccion: `${fechaTransaccion.value}T12:00:00.000Z`,
      clienteId: props.transaccion.clienteId,
    }
    await axios.put(`http://localhost:5000/transaccion/${props.transaccion.id}`, payload)

    emit('transaction-updated')
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Error al actualizar la transacción.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  populateForm()
})
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content">
      <h3 class="modal-title">Modificar Transacción</h3>

      <div v-if="loading" class="text-center">Cargando datos...</div>

      <form v-else @submit.prevent="modificarTransaccion()">
        <div class="form-columns">
          <div class="form-column">
            <div class="form-group">
              <label for="cryptoCode">Criptomoneda (código)</label>
              <input type="text" id="cryptoCode" v-model="cryptoCode" required />
            </div>

            <div class="form-group">
              <label for="action">Acción</label>
              <select id="action" v-model="action" required>
                <option value="purchase">Compra</option>
                <option value="sale">Venta</option>
              </select>
            </div>

            <div class="form-group">
              <label for="cryptoAmount">Monto en Cripto</label>
              <input type="number" step="any" id="cryptoAmount" v-model="cryptoAmount" required />
            </div>
          </div>

          <div class="form-column">
            <div class="form-group">
              <label for="money">Monto en ARS</label>
              <input type="number" step="any" id="money" v-model="money" required disabled />
            </div>

            <div class="form-group">
              <label for="fechaTransaccion">Fecha</label>
              <input type="date" id="fechaTransaccion" v-model="fechaTransaccion" required />
            </div>
          </div>
        </div>

        <div v-if="error" class="alert-error">{{ error }}</div>

        <div class="modal-actions">
          <button type="button" class="btn-cancelar" @click="emit('close')">Cancelar</button>
          <button type="submit" class="btn-guardar" :disabled="loading">
            {{ loading ? 'Guardando...' : 'Guardar Cambios' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
}

.modal-content {
  background: #183a37;
  padding: 2rem;
  border-radius: 15px;
  border: 1px solid rgba(79, 209, 197, 0.2);
  width: 90%;
  max-width: 500px;
  box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
}

.modal-title {
  font-size: 1.8rem;
  font-weight: 600;
  text-align: center;
  margin-bottom: 1.5rem;
  color: #efefef;
}

.form-columns {
  display: flex;
  gap: 1.5rem;
}

.form-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
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
  gap: 1rem;
  margin-top: 2rem;
}

.btn-cancelar,
.btn-guardar {
  padding: 10px 20px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-weight: 500;
}

.btn-cancelar {
  background-color: #4a5568;
  color: white;
}

.btn-guardar {
  background-color: #4fd1c5;
  color: #1e1e1e;
}

.alert-error {
  color: #ff4b4b;
  background-color: rgba(255, 75, 75, 0.1);
  border: 1px solid #ff4b4b;
  padding: 0.75rem;
  border-radius: 6px;
  text-align: center;
  margin: 1.5rem 0;
}
</style>
