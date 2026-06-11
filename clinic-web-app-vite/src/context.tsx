import { createContext, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { userService } from './services/userService'
import { useQuery } from '@tanstack/react-query'

type User = {
  email: string
  isEmailConfirmed: boolean
}

type AuthContextType = {
  user: User | null
  isLoading: boolean
  refetch: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const fetchUser = async () => {
    setIsLoading(true)
    const response = await userService.myInfo()
    if (response.ok && response.data) {
      setUser(response.data as User)
    } else {
      setUser(null)
    }
    setIsLoading(false)
  }

  useEffect(() => {
    fetchUser()
  }, [])

  return (
    <AuthContext.Provider value={{ user, isLoading, refetch: fetchUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside an AuthProvider')
  }
  return context
}
