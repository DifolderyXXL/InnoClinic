import { createRoute, Link } from '@tanstack/react-router'
import { Route as rootRouteImport } from './__root'
import { LoginForm } from '#/components/LoginForm'

export const Route = createRoute({
  getParentRoute: () => rootRouteImport,
  path: '/login',
  component: Login,
})

function Login() {
  return (
    <div>
      <LoginForm></LoginForm>
      <Link to="/register">Sign Up</Link>
      <Link to="/login" disabled={true}>
        Sign In
      </Link>
    </div>
  )
}
