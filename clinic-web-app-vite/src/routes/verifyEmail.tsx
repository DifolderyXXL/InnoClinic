import { createRoute } from '@tanstack/react-router'
import { Route as rootRouteImport } from './__root'
import { LoginForm } from '#/components/LoginForm'

export const Route = createRoute({
  getParentRoute: () => rootRouteImport,
  path: '/verifyEmail',
  component: VerifyEmail,
})

function VerifyEmail() {
  return (
    <div>
      <label> Verification link sended to your mail. 'dev: in logs' </label>
    </div>
  )
}
