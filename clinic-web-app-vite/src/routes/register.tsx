import { createRoute, Link } from '@tanstack/react-router'
import { Route as rootRouteImport } from './__root'
import { RegisterForm } from '#/components/RegisterForm'

export const Route = createRoute({
  getParentRoute: () => rootRouteImport,
  path: '/register',
  component: Register,
})

function Register() {
  return (
    <div>
      <RegisterForm></RegisterForm>
      <Link to="/register" disabled={true}>
        Sign Up
      </Link>
      <Link to="/login">Sign In</Link>
    </div>
  )
}
