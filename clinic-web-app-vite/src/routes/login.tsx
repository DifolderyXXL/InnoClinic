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
      <div className="flex gap-4">
        <Link to="/register" className="border rounded-lg">
          Sign Up
        </Link>
        <Link to="/login" className="border rounded-lg" disabled={true}>
          Sign In
        </Link>
      </div>
    </div>
  )
}
