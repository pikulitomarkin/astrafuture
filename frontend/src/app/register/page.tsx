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
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-[#25D366]/10 to-[#075E54]/10 px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-4">
          <div className="flex items-center justify-center">
            <img 
              src="/logo.svg" 
              alt="Astra Agenda" 
              className="h-20 w-auto"
            />
          </div>
          <CardDescription className="text-center text-[#333333]">
            Crie sua conta para começar a usar o sistema
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="businessName" className="text-[#075E54] font-semibold">Nome do Negócio</Label>
              <Input
                id="businessName"
                type="text"
                placeholder="Minha Empresa"
                value={businessName}
                onChange={(e) => setBusinessName(e.target.value)}
                required
                disabled={register.isPending}
                className="h-11"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email" className="text-[#075E54] font-semibold">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="seu@email.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={register.isPending}
                className="h-11"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password" className="text-[#075E54] font-semibold">Senha</Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={6}
                disabled={register.isPending}
                className="h-11"
              />
            </div>
            <Button
              type="submit"
              size="lg"
              className="w-full"
              disabled={register.isPending}
            >
              {register.isPending ? 'Criando conta...' : 'Criar conta'}
            </Button>
          </form>
          <div className="mt-6 text-center text-sm text-[#333333]">
            Já tem uma conta?{' '}
            <Link href="/login" className="text-[#25D366] hover:underline font-semibold">
              Fazer login
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
