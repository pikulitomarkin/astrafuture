'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { useApiKeys, useWebhookUrls } from '@/hooks/use-api-keys'
import { CreateApiKeyDialog } from '@/components/integrations/create-api-key-dialog'
import { ApiKeyRevealDialog } from '@/components/integrations/api-key-reveal-dialog'
import { Plus, Key, Trash2, Copy, Check, ExternalLink, AlertCircle } from 'lucide-react'
import { useDeleteApiKey, useUpdateApiKey } from '@/hooks/use-api-keys'
import { Badge } from '@/components/ui/badge'
import { toast } from 'sonner'

export default function IntegrationsPage() {
  const { data: apiKeys, isLoading: keysLoading } = useApiKeys()
  const { data: webhookUrls, isLoading: urlsLoading } = useWebhookUrls()
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const [revealDialogOpen, setRevealDialogOpen] = useState(false)
  const [newlyCreatedKey, setNewlyCreatedKey] = useState<any>(null)
  const [copiedUrl, setCopiedUrl] = useState<string | null>(null)

  const deleteMutation = useDeleteApiKey()
  const updateMutation = useUpdateApiKey()

  const handleCopyUrl = async (url: string, label: string) => {
    await navigator.clipboard.writeText(url)
    setCopiedUrl(label)
    toast.success(`${label} copiada!`)
    setTimeout(() => setCopiedUrl(null), 2000)
  }

  const handleCreateSuccess = (apiKey: any) => {
    setNewlyCreatedKey(apiKey)
    setCreateDialogOpen(false)
    setRevealDialogOpen(true)
  }

  const handleDelete = (id: string) => {
    if (confirm('Tem certeza que deseja deletar esta API Key? Esta ação não pode ser desfeita.')) {
      deleteMutation.mutate(id)
    }
  }

  const handleToggleActive = (id: string, name: string, description: string | undefined, isActive: boolean) => {
    updateMutation.mutate({ id, data: { name, description, isActive: !isActive } })
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Integrações</h1>
        <p className="text-muted-foreground">
          Gerencie suas integrações com WhatsApp e outros serviços externos
        </p>
      </div>

      <Tabs defaultValue="api-keys" className="space-y-6">
        <TabsList>
          <TabsTrigger value="api-keys">
            <Key className="h-4 w-4 mr-2" />
            API Keys
          </TabsTrigger>
          <TabsTrigger value="webhook-urls">
            <ExternalLink className="h-4 w-4 mr-2" />
            Webhook URLs
          </TabsTrigger>
        </TabsList>

        <TabsContent value="api-keys" className="space-y-4">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Suas API Keys</CardTitle>
                  <CardDescription>
                    Gerencie as chaves de API para autenticar requisições externas
                  </CardDescription>
                </div>
                <Button onClick={() => setCreateDialogOpen(true)}>
                  <Plus className="h-4 w-4 mr-2" />
                  Nova API Key
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              {keysLoading ? (
                <div className="text-center py-8 text-muted-foreground">
                  Carregando API Keys...
                </div>
              ) : !apiKeys || apiKeys.length === 0 ? (
                <div className="text-center py-12 space-y-4">
                  <div className="mx-auto w-12 h-12 rounded-full bg-muted flex items-center justify-center">
                    <Key className="h-6 w-6 text-muted-foreground" />
                  </div>
                  <div>
                    <p className="font-medium">Nenhuma API Key criada</p>
                    <p className="text-sm text-muted-foreground">
                      Crie sua primeira API Key para começar a integrar com WhatsApp
                    </p>
                  </div>
                  <Button onClick={() => setCreateDialogOpen(true)}>
                    <Plus className="h-4 w-4 mr-2" />
                    Criar Primeira API Key
                  </Button>
                </div>
              ) : (
                <div className="space-y-3">
                  {apiKeys.map((apiKey) => {
                    const isExpired = new Date(apiKey.expiresAt) < new Date()
                    
                    return (
                      <div
                        key={apiKey.id}
                        className="border rounded-lg p-4 space-y-3 hover:bg-muted/50 transition-colors"
                      >
                        <div className="flex items-start justify-between">
                          <div className="space-y-1 flex-1">
                            <div className="flex items-center gap-2">
                              <h3 className="font-semibold">{apiKey.name}</h3>
                              {apiKey.isActive && !isExpired ? (
                                <Badge variant="default" className="bg-green-500">Ativa</Badge>
                              ) : isExpired ? (
                                <Badge variant="destructive">Expirada</Badge>
                              ) : (
                                <Badge variant="secondary">Inativa</Badge>
                              )}
                            </div>
                            {apiKey.description && (
                              <p className="text-sm text-muted-foreground">{apiKey.description}</p>
                            )}
                            <div className="flex items-center gap-4 text-xs text-muted-foreground">
                              <span>Key: <code className="bg-muted px-1 rounded">{apiKey.key}</code></span>
                              <span>Usos: {apiKey.usageCount}</span>
                              {apiKey.lastUsedAt && (
                                <span>Último uso: {new Date(apiKey.lastUsedAt).toLocaleDateString('pt-BR')}</span>
                              )}
                              <span>Expira: {new Date(apiKey.expiresAt).toLocaleDateString('pt-BR')}</span>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => handleToggleActive(apiKey.id, apiKey.name, apiKey.description, apiKey.isActive)}
                              disabled={updateMutation.isPending || isExpired}
                            >
                              {apiKey.isActive ? 'Desativar' : 'Ativar'}
                            </Button>
                            <Button
                              size="sm"
                              variant="destructive"
                              onClick={() => handleDelete(apiKey.id)}
                              disabled={deleteMutation.isPending}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        </div>
                      </div>
                    )
                  })}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="webhook-urls" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>URLs dos Webhooks</CardTitle>
              <CardDescription>
                Use estas URLs para configurar seu bot do WhatsApp ou outras integrações
              </CardDescription>
            </CardHeader>
            <CardContent>
              {urlsLoading ? (
                <div className="text-center py-8 text-muted-foreground">
                  Carregando URLs...
                </div>
              ) : (
                <div className="space-y-6">
                  <div className="bg-blue-50 dark:bg-blue-950 p-4 rounded-lg space-y-2">
                    <div className="flex items-start gap-2">
                      <AlertCircle className="h-5 w-5 text-blue-600 dark:text-blue-400 mt-0.5" />
                      <div className="text-sm">
                        <p className="font-medium text-blue-900 dark:text-blue-100">
                          {webhookUrls?.instructions}
                        </p>
                        <p className="text-blue-700 dark:text-blue-300 mt-1">
                          Adicione o header <code className="bg-blue-100 dark:bg-blue-900 px-1 rounded">X-API-Key: sua_chave_aqui</code> em todas as requisições
                        </p>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-4">
                    <div className="space-y-2">
                      <Label>Webhook Principal (Receber Mensagens)</Label>
                      <div className="flex gap-2">
                        <code className="flex-1 bg-muted px-3 py-2 rounded text-sm overflow-x-auto">
                          {webhookUrls?.webhookUrl}
                        </code>
                        <Button
                          size="icon"
                          variant="outline"
                          onClick={() => handleCopyUrl(webhookUrls?.webhookUrl || '', 'URL principal')}
                        >
                          {copiedUrl === 'URL principal' ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                        </Button>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        POST - Configure esta URL na Evolution API ou WhatsApp Cloud API
                      </p>
                    </div>

                    <div className="space-y-2">
                      <Label>Criar Cliente</Label>
                      <div className="flex gap-2">
                        <code className="flex-1 bg-muted px-3 py-2 rounded text-sm overflow-x-auto">
                          {webhookUrls?.createCustomerUrl}
                        </code>
                        <Button
                          size="icon"
                          variant="outline"
                          onClick={() => handleCopyUrl(webhookUrls?.createCustomerUrl || '', 'URL criar cliente')}
                        >
                          {copiedUrl === 'URL criar cliente' ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                        </Button>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        POST - Body: {`{ "phoneNumber": "5511999999999", "name": "João Silva", "email": "joao@email.com" }`}
                      </p>
                    </div>

                    <div className="space-y-2">
                      <Label>Criar Agendamento</Label>
                      <div className="flex gap-2">
                        <code className="flex-1 bg-muted px-3 py-2 rounded text-sm overflow-x-auto">
                          {webhookUrls?.createAppointmentUrl}
                        </code>
                        <Button
                          size="icon"
                          variant="outline"
                          onClick={() => handleCopyUrl(webhookUrls?.createAppointmentUrl || '', 'URL criar agendamento')}
                        >
                          {copiedUrl === 'URL criar agendamento' ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                        </Button>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        POST - Body: {`{ "customerPhone": "5511999999999", "startTime": "2026-01-30T14:00:00Z", "endTime": "2026-01-30T15:00:00Z", "notes": "Consulta" }`}
                      </p>
                    </div>

                    <div className="space-y-2">
                      <Label>Verificar Cliente</Label>
                      <div className="flex gap-2">
                        <code className="flex-1 bg-muted px-3 py-2 rounded text-sm overflow-x-auto">
                          {webhookUrls?.checkCustomerUrl}?phone=5511999999999
                        </code>
                        <Button
                          size="icon"
                          variant="outline"
                          onClick={() => handleCopyUrl(webhookUrls?.checkCustomerUrl || '', 'URL verificar cliente')}
                        >
                          {copiedUrl === 'URL verificar cliente' ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                        </Button>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        GET - Retorna se o cliente existe e seus dados
                      </p>
                    </div>
                  </div>

                  <div className="bg-muted p-4 rounded-lg space-y-2 text-sm">
                    <p className="font-medium">Exemplo de uso com cURL:</p>
                    <pre className="bg-black/10 dark:bg-white/10 p-3 rounded overflow-x-auto">
{`curl -X POST ${webhookUrls?.createCustomerUrl} \\
  -H "Content-Type: application/json" \\
  -H "X-API-Key: sua_api_key_aqui" \\
  -d '{
    "phoneNumber": "5511999999999",
    "name": "João Silva",
    "email": "joao@email.com"
  }'`}
                    </pre>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <CreateApiKeyDialog
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
        onSuccess={handleCreateSuccess}
      />

      {newlyCreatedKey && (
        <ApiKeyRevealDialog
          apiKey={newlyCreatedKey}
          open={revealDialogOpen}
          onOpenChange={setRevealDialogOpen}
        />
      )}
    </div>
  )
}

function Label({ children }: { children: React.ReactNode }) {
  return <label className="text-sm font-medium">{children}</label>
}
