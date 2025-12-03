import { useRouter } from 'vue-router';

export function useAuth() {
  const router = useRouter();

  const logout = () => {
    localStorage.removeItem('userToken');
    localStorage.removeItem('userId');
    localStorage.removeItem('userName');
    router.push('/login');
  };

  const isAuthenticated = () => {
    return !!localStorage.getItem('userToken');
  };

  return { logout, isAuthenticated };
}
