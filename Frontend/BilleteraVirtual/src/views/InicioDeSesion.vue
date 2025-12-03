<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { AnimacionCriptos } from '../composables/useBackgroundAnimation';
import axios from 'axios'

onMounted(() => {
  AnimacionCriptos()
});

const email = ref('')
const password = ref('')
const error = ref<string | null>(null)
const isLoading = ref(false)
const activeTab = ref('login')

const router = useRouter()

async function login() {
  error.value = null
  isLoading.value = true

  const loginPayload = {
    email: email.value,
    password: password.value,
  }

  try {
    const response = await axios.post('/cliente/login', loginPayload)

    const { token, userId, userName } = response.data

    localStorage.setItem('userToken', token)
    localStorage.setItem('userId', userId)
    localStorage.setItem('userName', userName)

    router.push({ name: 'home' })
  } catch (err: any) {
    if (err.response) {
      const errorMessage = err.response.data.message
      error.value = errorMessage || `Error al inicar sesión`
    } else {
      error.value = 'Verifica que el backend esté activo.'
    }
  } finally {
    isLoading.value = false
  }
}

async function register() {
  const nuevoCliente = {
    nombre: (document.getElementById('register-name') as HTMLInputElement).value,
    email: (document.getElementById('register-email') as HTMLInputElement).value,
    password: (document.getElementById('register-password') as HTMLInputElement).value,
  }

  try {
    await axios.post('/cliente', nuevoCliente)
    alert('Usuario creado con éxito. Ahora puedes iniciar sesión.')
    activeTab.value = 'login'
  } catch (err: any) {
    if (err.response) {
      const errorMessage = err.response.data.message
      alert(errorMessage || `Error del servidor: ${err.response.status}.`)
    } else {
      alert('Verifica que el backend esté activo.')
    }
  }
}


</script>

<template>
  <canvas id="canvasCripto"></canvas>
  <div class="main-container">
    <div class="welcome-column">
      <div class="welcome-content">
        <h1 class="welcome-title">Billetera Virtual</h1>
        <p class="welcome-subtitle">Tu portal al mundo de las criptomonedas</p>
      </div>
    </div>

    <div class="login-layout">
      <div class="form-column">
        <div class="auth-card">
          <div class="auth-tabs-header">
            <ul class="auth-tabs">
              <li class="auth-tab-item">
                <a
                  class="auth-tab-link"
                  :class="{ active: activeTab === 'login' }"
                  @click="activeTab = 'login'"
                  href="#"
                  >Iniciar Sesión</a
                >
              </li>
              <li class="auth-tab-item">
                <a
                  class="auth-tab-link"
                  :class="{ active: activeTab === 'register' }"
                  @click="activeTab = 'register'"
                  href="#"
                  >Registrarse</a
                >
              </li>
            </ul>
          </div>
          <div class="auth-card-body">
            <!-- Pestaña de Iniciar Sesión -->
            <div v-if="activeTab === 'login'">
              <h5 class="auth-card-title">Bienvenido de vuelta</h5>
              <!-- El formulario ahora llama a la función login -->
              <form @submit.prevent="login()">
                <div class="form-group">
                  <label for="login-email" class="form-label">Email</label>
                  <input
                    type="email"
                    class="form-control"
                    id="login-email"
                    v-model="email"
                    placeholder="tu@email.com"
                    required
                  />
                </div>
                <div class="form-group">
                  <label for="login-password" class="form-label">Contraseña</label>
                  <input
                    type="password"
                    class="form-control"
                    v-model="password"
                    id="login-password"
                    placeholder="Tu contraseña"
                    required
                  />
                </div>
                <div v-if="error" class="alert-error">
                  {{ error }}
                </div>
                <div class="form-actions">
                  <button type="submit" class="btn btn-ingresar" :disabled="isLoading">
                    <span v-if="isLoading">Ingresando...</span>
                    <span v-else>Ingresar</span>
                  </button>
                </div>
              </form>
            </div>
            <!-- Pestaña de Registrarse -->
            <div v-if="activeTab === 'register'">
              <h5 class="auth-card-title">Crea tu cuenta</h5>
              <form @submit.prevent="register()">
                <div class="form-group">
                  <label for="register-name" class="form-label">Nombre</label>
                  <input
                    type="text"
                    class="form-control"
                    id="register-name"
                    placeholder="Tu nombre"
                    required
                  />
                </div>
                <div class="form-group">
                  <label for="register-email" class="form-label">Email</label>
                  <input
                    type="email"
                    class="form-control"
                    id="register-email"
                    placeholder="Tu email"
                    required
                  />
                </div>
                <div class="form-group">
                  <label for="register-password" class="form-label">Contraseña</label>
                  <input
                    type="password"
                    class="form-control"
                    id="register-password"
                    placeholder="Crea una contraseña"
                    required
                  />
                </div>
                <div class="form-actions">
                  <button type="submit" class="btn btn-registrar">Crear Usuario</button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style>
body {
  background-color: #04151f;
  color: #efefef;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.main-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  justify-content: center;
  align-items: center;
  padding: 1rem;
  position: relative;
  z-index: 1;
  width: 100%;
  margin: 0;
}

.login-layout {
  display: flex;
  flex-grow: 1; /* Ocupa el espacio restante */
  width: 100%;
  justify-content: center;
  align-items: center;
  padding-top: 1rem; /* Añade padding vertical para que no se pegue al título */
}

.welcome-column {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  flex-shrink: 0; /* Evita que el título se encoja */
}

.welcome-content {
  text-align: center;
  color: white;
}

.welcome-title {
  font-size: 4.5rem;
  font-weight: 700;
  text-shadow: 0px 4px 15px rgba(0, 0, 0, 0.5);
}

.welcome-subtitle {
  font-size: 1.5rem;
  margin-top: 0.5rem;
}

.form-panel {
  /* En pantallas pequeñas, el panel del formulario necesita un fondo para que el texto sea legible sobre la animación */
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  width: 100%;
}

@media (min-width: 992px) {
  .form-panel {
    background-color: transparent;
  }
}

.auth-card {
  width: 100%;
  max-width: 500px;
  background: rgba(24, 58, 55, 0.6); /* #183a37 con transparencia */
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  color: white;
  box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37); /* Sombra más pronunciada */
}

.auth-tabs-header {
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.auth-tabs {
  display: flex;
  list-style: none;
  padding-left: 0;
  margin-bottom: 0;
}

.auth-tab-item {
  flex: 1;
  text-align: center;
}

.auth-tab-link {
  display: block;
  padding: 0.5rem 1rem;
  text-decoration: none;
  color: rgba(255, 255, 255, 0.7);
  font-weight: bold;
}

.auth-tab-link.active {
  color: white;
  background-color: transparent;
  border-bottom: 1.5px solid #4fd1c5; /* Un color acento brillante */
}

.auth-card-body {
  padding: 1rem 1.5rem;
}
@media (min-width: 992px) {
  .auth-card-body {
    padding: 2rem 3rem;
  }
}

.auth-card-title {
  text-align: center;
  margin-bottom: 1.25rem;
  font-size: 1.8rem;
  font-weight: 600;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-actions {
  text-align: center;
  margin-top: 1.5rem;
}

.form-label {
  display: block;
  text-align: left;
  font-size: 1.1rem;
}

.form-control {
  background-color: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  font-size: 1.2rem;
  padding: 0.75rem 1rem;
}

.form-control:focus {
  background-color: rgba(255, 255, 255, 0.2);
  border-color: #4fd1c5;
  box-shadow: 0 0 0 0.2rem rgba(79, 209, 197, 0.25);
  color: white;
}

.form-control::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.btn-ingresar,
.btn-registrar {
  background-color: #4fd1c5;
  border: none;
  color: #04151f;
  font-weight: bold;
  padding: 12px;
  font-size: 1.2rem;
  transition: all 0.3s ease;
}

.btn-ingresar:hover,
.btn-registrar:hover {
  background-color: #38b2ac;
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(79, 209, 197, 0.2);
}

/* Estilo para el mensaje de error */
.alert-error {
  text-align: center;
  background-color: rgba(220, 53, 69, 0.2);
  color: #f8d7da;
  border: 1px solid rgba(220, 53, 69, 0.5);
  padding: 0.75rem 1.25rem;
  margin-bottom: 1rem;
  border-radius: 0.25rem;
  width: 98%; /* Hacemos que ocupe el mismo ancho que los inputs */
}
#canvasCripto {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 0;
  pointer-events: none;
}
</style>
