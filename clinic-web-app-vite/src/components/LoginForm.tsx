import { userService } from '#/services/userService'
import { useForm } from '@tanstack/react-form'
import { z } from 'zod'
import React, { useState } from 'react'
import { useNavigate } from '@tanstack/react-router'
import {
  fieldSpecificErrors,
  throwResponseErrors,
} from '#/helpers/FormComponents'

const loginSchema = z.object({
  email: z.email('Invalid email address'),
  password: z
    .string()
    .min(6, { message: 'Password must have 6 characters or more' })
    .max(15, { message: 'Password must have max 15 characters' }),

  rememberMe: z.boolean(),
})

export function LoginForm() {
  const [submitError, setSubmitError] = useState<string | null>(null)
  const navigate = useNavigate()
  const form = useForm({
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
    validators: {
      onChange: loginSchema,
    },
    onSubmitInvalid: () => {
      form.setFieldValue('password', '')
    },
    onSubmit: async ({ value }) => {
      setSubmitError(null)

      const { email, password, rememberMe } = value

      try {
        const response = await userService.login(
          {
            email: email,
            password: password,
          },
          rememberMe,
        )

        await throwResponseErrors(response)

        navigate({ to: '/' })
      } catch (error: any) {
        setSubmitError(
          error.message || 'Failed to submit the form. Please try again.',
        )
      }
    },
  })

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault()
        form.handleSubmit()
      }}
    >
      <form.Field
        name="email"
        children={(field) => (
          <div>
            <label htmlFor={field.name}>Email:</label>
            <input
              id={field.name}
              name={field.name}
              value={field.state.value}
              onBlur={field.handleBlur}
              onChange={(e) => field.handleChange(e.target.value)}
            />

            {fieldSpecificErrors(field.state.meta)}
          </div>
        )}
      />

      <form.Field
        name="password"
        children={(field) => (
          <div>
            <label htmlFor={field.name}>Password:</label>
            <input
              type="password"
              id={field.name}
              name={field.name}
              value={field.state.value}
              onBlur={field.handleBlur}
              onChange={(e) => field.handleChange(e.target.value)}
            />

            {fieldSpecificErrors(field.state.meta)}
          </div>
        )}
      />

      <form.Field
        name="rememberMe"
        children={(field) => (
          <div>
            <label htmlFor={field.name}>Remember me:</label>

            <input
              type="checkbox"
              id={field.name}
              name={field.name}
              checked={field.state.value}
              onBlur={field.handleBlur}
              onChange={(e) => field.handleChange(e.target.checked)}
            />
          </div>
        )}
      />

      <form.Subscribe selector={(state) => [state.canSubmit, state.errors]}>
        {([canSubmit]) => {
          return (
            <div>
              <button
                type="submit"
                disabled={!canSubmit}
                className={`text-white px-4 py-2 rounded transition-colors ${
                  canSubmit
                    ? 'bg-blue-500 hover:bg-blue-600'
                    : 'bg-red-500 opacity-50 cursor-not-allowed'
                }`}
              >
                Submit
              </button>

              {submitError && (
                <>
                  <em className="block error-text">{submitError}</em>
                </>
              )}
            </div>
          )
        }}
      </form.Subscribe>
    </form>
  )
}
