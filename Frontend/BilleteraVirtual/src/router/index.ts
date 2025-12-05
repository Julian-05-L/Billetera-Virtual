import { createRouter, createWebHistory } from 'vue-router'
import InicioDeSesion from '../views/InicioDeSesion.vue'
import HomeView from '../views/PaginaPrincipal.vue'
import AboutView from '../views/TransaccionesView.vue'
import TransaccionesView from '../views/TransaccionesView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: InicioDeSesion,
    },
    {
      path: '/',
      name: 'pagina-principal',
      component: HomeView,
    },
    {
      path: '/about',
      name: 'about',
      component: AboutView,
    },
    {
      path: '/transacciones',
      name: 'transacciones',
      component: TransaccionesView,
    },
  ],
})

export default router
