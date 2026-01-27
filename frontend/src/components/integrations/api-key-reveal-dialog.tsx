'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Copy, Check, AlertTriangle } from 'lucide-react'

interface ApiKeyRevealDialogProps {
  apiKey: any
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function ApiKeyRevealDialog({ apiKey, open, onOpenChange }: ApiKeyRevealDialogProps) {
  const [copied, setCopied] = useState(false)

  const handleCopy = async () => {
    await navigator.clipboard.writeText(apiKey.key)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-yellow-500" />
            API Key Criada com Sucesso!
          </DialogTitle>
          <DialogDescription>
            ⚠️ <strong>ATENÇÃO:</strong> Esta é a única vez que você verá esta chave completa. 
            Copie e guarde em um lugar seguro!
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="bg-muted p-4 rounded-lg space-y-2">
            <p className="text-sm font-medium">Sua API Key:</p>
            <div className="flex items-center gap-2">
              <code className="flex-1 bg-black/10 dark:bg-white/10 px-3 py-2 rounded text-sm font-mono break-all">
                {apiKey.key}
              </code>
              <Button
                size="icon"
                variant="outline"
                onClick={handleCopy}
                className="shrink-0"
              >
                {copied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
              </Button>
            </div>
          </div>

          <div className="space-y-2 text-sm">
            <p><strong>Nome:</strong> {apiKey.name}</p>
            {apiKey.description && <p><strong>Descrição:</strong> {apiKey.description}</p>}
            <p><strong>Expira em:</strong> {new Date(apiKey.expiresAt).toLocaleDateString('pt-BR')}</p>
          </div>

          <div className="bg-blue-50 dark:bg-blue-950 p-4 rounded-lg space-y-2 text-sm">
            <p className="font-medium">Como usar:</p>
            <ol className="list-decimal list-inside space-y-1 text-muted-foreground">
              <li>Adicione o header <code className="bg-black/10 dark:bg-white/10 px-1 rounded">X-API-Key</code> em todas as requisições</li>
              <li>Use o valor da chave acima como valor do header</li>
              <li>Veja a aba "Webhook URLs" para os endpoints disponíveis</li>
            </ol>
          </div>

          <Button onClick={() => onOpenChange(false)} className="w-full">
            Entendi, já copiei a chave
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
