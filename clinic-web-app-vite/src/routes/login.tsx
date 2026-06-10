import { createRoute } from '@tanstack/react-router'
import { Route as rootRouteImport } from './__root'
import React, { Component } from 'react'

import { userService } from '#/services/userService'

export const Route = createRoute({
  getParentRoute: () => rootRouteImport,
  path: '/login',
  component: Login,
})

function Login() {
  return (
    <div>
      <LoginForm></LoginForm>
    </div>
  )
}

interface LoginState {
  email: string
  password: string
}

export default class LoginForm extends React.Component<{}, LoginState> {
  constructor(props: {}) {
    super(props)

    this.state = { email: '', password: '' }
    this.handleChange.bind(this)
    this.handleSubmit.bind(this)
  }

  handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const { name, value } = event.target

    this.setState({
      [name as keyof LoginState]: value,
    } as Pick<LoginState, keyof LoginState>)
  }
  async handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault()

    await userService.login({
      email: this.state.email,
      password: this.state.password,
    })
  }
  render() {
    return (
      <form onSubmit={this.handleSubmit}>
        <label>Email:</label>
        <input type="text" name="email" onChange={this.handleChange} />

        <label>Password:</label>
        <input type="password" name="password" onChange={this.handleChange} />
      </form>
    )
  }
}
