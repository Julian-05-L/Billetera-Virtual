<script setup lang="ts">
import { useAuth } from '../composables/useAuth'
import { ref, onMounted } from 'vue'
import ModificarUsuarioModal from './ModificarUsuarioModal.vue'

const { logout } = useAuth()
const userName = ref<string | null>(null)
const isDropdownOpen = ref(false)
const isModifyModalOpen = ref(false)

onMounted(() => {
  userName.value = localStorage.getItem('userName')
})

const toggleDropdown = () => {
  isDropdownOpen.value = !isDropdownOpen.value
}

const openModifyModal = () => {
  isDropdownOpen.value = false // Cerramos el dropdown
  isModifyModalOpen.value = true
}

const onUserUpdated = () => {
  alert('Datos actualizados correctamente. Se cerrará la sesión para que inicies de nuevo.')
  // Cerramos la sesión para forzar un nuevo login con las credenciales actualizadas
  logout()
}
</script>

<template>
  <header class="navbar">
    <div class="left">
      <router-link to="/" class="logo-link">
        <h2>Billetera</h2>
      </router-link>
    </div>

    <nav class="right">
      <router-link to="/" class="nav-btn">Pagina Principal</router-link>
      <router-link to="/transacciones" class="nav-btn">Transacciones</router-link>

      <!-- Menú de Perfil de Usuario -->
      <div class="profile-menu">
        <button @click="toggleDropdown" class="profile-button">
          <span>{{ userName ?? 'Usuario' }}</span>
        </button>

        <div v-if="isDropdownOpen" class="dropdown-menu">
          <a @click="openModifyModal" href="#" class="dropdown-item">Modificar Usuario</a>
          <a @click="logout" href="#" class="dropdown-item logout">Cerrar Sesión</a>
        </div>
      </div>
    </nav>
  </header>

  <!-- El Modal -->
  <teleport to="body">
    <modificar-usuario-modal
      v-if="isModifyModalOpen"
      @close="isModifyModalOpen = false"
      @user-updated="onUserUpdated"
    />
  </teleport>
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

.logo-link {
  text-decoration: none;
}

.navbar .left h2,
.logo-link h2 {
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

/* --- Nuevos Estilos para el Menú de Perfil --- */
.right {
  display: flex;
  align-items: center;
}

.profile-menu {
  position: relative;
  margin-left: 20px;
}

.profile-button {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: transparent;
  border: 1px solid rgba(79, 209, 197, 0.5);
  color: #efefef;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.3s ease;
}

.profile-button:hover {
  background-color: rgba(79, 209, 197, 0.15);
}

.dropdown-menu {
  position: absolute;
  top: 120%;
  right: 0;
  background: #183a37;
  border: 1px solid rgba(79, 209, 197, 0.2);
  border-radius: 8px;
  min-width: 180px;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.3);
  overflow: hidden;
}

.dropdown-item {
  display: block;
  padding: 12px 16px;
  color: #efefef;
  text-decoration: none;
  transition: background-color 0.2s ease;
}

.dropdown-item:hover {
  background-color: rgba(79, 209, 197, 0.15);
}

.dropdown-item.logout:hover {
  background-color: rgba(255, 75, 75, 0.2);
  color: #ff4b4b;
}
</style>
