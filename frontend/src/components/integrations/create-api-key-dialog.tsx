'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useCreateApiKey } from '@/hooks/use-api-keys'
import type { CreateApiKeyRequest } from '@/types'

interface CreateApiKeyDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: (apiKey: any) => void
}

export function CreateApiKeyDialog({ open, onOpenChange, onSuccess }: CreateApiKeyDialogProps) {
  const [formData, setFormData] = useState<CreateApiKeyRequest>({
    name: '',
    description: '',
    expiresInDays: 365,
    rateLimit: 60,
  })

  const createMutation = useCreateApiKey()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!formData.name.trim()) {
      return
    }

    createMutation.mutate(formData, {
      onSuccess: (data) => {
        setFormData({ name: '', description: '', expiresInDays: 365, rateLimit: 60 })
        onSuccess?.(data)
        // Não fechamos o dialog aqui, deixamos o componente pai decidir após mostrar a key
      },
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Criar Nova API Key</DialogTitle>
          <DialogDescription>
            Crie uma chave de API para integrar o WhatsApp ou outros serviços com sua conta.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Nome da API Key *</Label>
            <Input
              id="name"
              placeholder="Ex: WhatsApp Bot Produção"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Descrição</Label>
            <Textarea
              id="description"
              placeholder="Para que será usada esta key?"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows={2}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="expires">Expiração</Label>
            <Select
              value={formData.expiresInDays?.toString() || '365'}
              onValueChange={(value) => setFormData({ ...formData, expiresInDays: parseInt(value) })}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="30">30 dias</SelectItem>
                <SelectItem value="90">90 dias</SelectItem>
                <SelectItem value="180">6 meses</SelectItem>
                <SelectItem value="365">1 ano</SelectItem>
                <SelectItem value="730">2 anos</SelectItem>
                <SelectItem value="3650">Sem expiração (10 anos)</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="rateLimit">Limite de Requisições (por minuto)</Label>
            <Input
              id="rateLimit"
              type="number"
              placeholder="60"
              value={formData.rateLimit || ''}
              onChange={(e) => setFormData({ ...formData, rateLimit: parseInt(e.target.value) || undefined })}
            />
            <p className="text-xs text-muted-foreground">Deixe vazio para sem limite</p>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button type="submit" disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Criando...' : 'Criar API Key'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
