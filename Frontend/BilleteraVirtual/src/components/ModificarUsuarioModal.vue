<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'

const emit = defineEmits(['close', 'userUpdated'])

const nombre = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(true)
const error = ref('')

const userId = Number(localStorage.getItem('userId'))

async function fetchUserData() {
  if (!userId) {
    error.value = 'No se pudo encontrar el ID del usuario.'
    loading.value = false
    return
  }
  try {
    const response = await axios.get(`http://localhost:5000/cliente/${userId}`)
    nombre.value = response.data.nombre
    email.value = response.data.email
  } catch (err) {
    error.value = 'Error al cargar los datos del usuario.'
  } finally {
    loading.value = false
  }
}

async function updateUser() {
  if (!confirm('¿Estás seguro de que quieres guardar los cambios?')) {
    return
  }

  loading.value = true
  error.value = ''

  if (password.value !== confirmPassword.value) {
    error.value = 'Las contraseñas no coinciden.'
    loading.value = false
    return
  }

  try {
    const payload: { nombre: string; email: string; password?: string } = {
      nombre: nombre.value,
      email: email.value,
    }

    if (password.value) {
      payload.password = password.value
    }

    const response = await axios.put(`http://localhost:5000/cliente/${userId}`, payload)

    localStorage.setItem('userName', response.data.nombre)

    // Emitimos eventos para notificar al padre
    emit('userUpdated')
    emit('close')
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Error al actualizar el usuario.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchUserData()
})
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal-content">
      <h3 class="modal-title">Modificar Perfil</h3>

      <div v-if="loading" class="text-center">Cargando...</div>

      <form v-else @submit.prevent="updateUser">
        <div class="form-group">
          <label for="nombre">Nombre</label>
          <input type="text" id="nombre" v-model="nombre" required />
        </div>
        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" v-model="email" required />
        </div>
        <div class="form-group">
          <label for="password">Nueva Contraseña</label>
          <input
            type="password"
            id="password"
            v-model="password"
            placeholder="Ingrese la nueva contraseña"
          />
        </div>
        <div class="form-group">
          <label for="confirmPassword">Confirmar Contraseña</label>
          <input
            type="password"
            id="confirmPassword"
            v-model="confirmPassword"
            placeholder="Repetir nueva contraseña"
            :required="!!password"
          />
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

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #a0aec0;
}

.form-group input {
  width: 100%;
  padding: 0.75rem 1rem;
  background-color: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  border-radius: 6px;
  font-size: 1rem;
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
  margin-top: 1rem;
}
</style>
