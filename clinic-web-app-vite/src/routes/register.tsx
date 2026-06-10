import { createRoute } from '@tanstack/react-router'
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
    </div>
  )
}
