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
      <div className="flex gap-4">
        <Link to="/register" className="border rounded-lg" disabled={true}>
          Sign Up
        </Link>
        <Link to="/login" className="border rounded-lg">
          Sign In
        </Link>
      </div>
    </div>
  )
}
