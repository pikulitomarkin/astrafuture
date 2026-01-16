'use client'

import { useState } from 'react'
import { useRegister } from '@/hooks/use-auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import Link from 'next/link'

export default function RegisterPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [businessName, setBusinessName] = useState('')
  const register = useRegister()

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    register.mutate({ email, password, businessName })
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <CardTitle className="text-2xl font-bold text-center">
            Criar Conta
          </CardTitle>
          <CardDescription className="text-center">
            Crie sua conta para começar a usar o AstraFuture
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="businessName">Nome do Negócio</Label>
              <Input
                id="businessName"
                type="text"
                placeholder="Minha Empresa"
                value={businessName}
                onChange={(e) => setBusinessName(e.target.value)}
                required
                disabled={register.isPending}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="seu@email.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={register.isPending}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Senha</Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={6}
                disabled={register.isPending}
              />
            </div>
            <Button
              type="submit"
              className="w-full"
              disabled={register.isPending}
            >
              {register.isPending ? 'Criando conta...' : 'Criar conta'}
            </Button>
          </form>
          <div className="mt-4 text-center text-sm">
            Já tem uma conta?{' '}
            <Link href="/login" className="text-primary hover:underline">
              Fazer login
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
