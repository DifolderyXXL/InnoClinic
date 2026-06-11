import { useAuth } from '#/context'
import { weatherService } from '#/services/weatherService'
import { Link, Outlet, createFileRoute } from '@tanstack/react-router'
import React, { Component, useState } from 'react'

export const Route = createFileRoute('/')({ component: Home })

function Home() {
  const { user, isLoading } = useAuth()
  if (isLoading) {
    return <div className="p-8 text-center">Verifying session...</div>
  }

  if (!user) {
    return (
      <div className="p-8">
        <h1 className="text-4xl font-bold">Welcome to TanStack Start</h1>

        <div className="mt-6 flex justify-center gap-4">
          <Link
            to="/login"
            className="px-4 py-2 bg-blue-300 hover:bg-blue-200 text-white rounded-lg font-medium transition"
          >
            Sign In
          </Link>
          <Link
            to="/register"
            className="px-4 py-2 border border-zinc-300 hover:bg-zinc-50 rounded-lg font-medium transition"
          >
            Sign Up
          </Link>
        </div>

        <p className="mt-4 text-lg">
          Edit <code>src/routes/index.tsx</code> to get started.
        </p>
        <WeatherApiForm />
        <Outlet />
      </div>
    )
  }

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold">Welcome back {user.email}</h1>
      <p className="mt-4 text-lg">
        Edit <code>src/routes/index.tsx</code> to get started.
      </p>
      <WeatherApiForm />
      <Outlet />
    </div>
  )
}

export default function WeatherApiForm() {
  const [result, setResult] = useState<string | null>(null)

  const handleClick = async (event: React.MouseEvent<HTMLButtonElement>) => {
    const response = await weatherService.weatherforecast()

    if (response.ok) setResult(JSON.stringify(response.data))
    else setResult(JSON.stringify(response.error))
  }

  return (
    <div>
      <button
        onClick={handleClick}
        className="bg-blue-500 text-white px-4 py-2 rounded disabled:bg-blue-300"
      >
        Click Me
      </button>
      <label className="block"> Result: {result} </label>
    </div>
  )
}
