import { weatherService } from '#/services/weatherService'
import { Outlet, createFileRoute } from '@tanstack/react-router'
import React, { Component, useState } from 'react'

export const Route = createFileRoute('/')({ component: Home })

function Home() {
  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold">Welcome to TanStack Start</h1>
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
