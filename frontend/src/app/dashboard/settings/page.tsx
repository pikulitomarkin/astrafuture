'use client'

import { useState, useEffect } from 'react'
import { Header } from '@/components/dashboard/header'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useSettings, useUpdateSettings } from '@/hooks/use-settings'
import { Building2, Palette, Image, Save } from 'lucide-react'

export default function SettingsPage() {
  const { data: settings, isLoading } = useSettings()
  const updateSettings = useUpdateSettings()

  const [name, setName] = useState('')
  const [logoUrl, setLogoUrl] = useState('')
  const [primaryColor, setPrimaryColor] = useState('#3B82F6')

  // Atualizar estados quando settings carregar
  useEffect(() => {
    if (settings) {
      setName(settings.name)
      setLogoUrl(settings.logoUrl || '')
      setPrimaryColor(settings.primaryColor || '#3B82F6')
    }
  }, [settings])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    await updateSettings.mutateAsync({
      name,
      logoUrl: logoUrl || undefined,
      primaryColor
    })
  }

  const isSaving = updateSettings.isPending

  return (
    <div>
      <Header
        title="Configurações"
        description="Personalize sua conta e preferências"
      />

      <div className="p-8 max-w-4xl">
        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Branding */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Palette className="h-5 w-5" />
                  Marca e Identidade
                </CardTitle>
                <CardDescription>
                  Personalize a aparência da sua plataforma
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="name">
                    <Building2 className="h-4 w-4 inline mr-2" />
                    Nome da Empresa
                  </Label>
                  <Input
                    id="name"
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Digite o nome da sua empresa"
                    required
                  />
                  <p className="text-sm text-muted-foreground">
                    Este nome aparecerá no topo do sistema e em documentos
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="logoUrl">
                    <Image className="h-4 w-4 inline mr-2" />
                    URL do Logo
                  </Label>
                  <Input
                    id="logoUrl"
                    type="url"
                    value={logoUrl}
                    onChange={(e) => setLogoUrl(e.target.value)}
                    placeholder="https://exemplo.com/logo.png"
                  />
                  <p className="text-sm text-muted-foreground">
                    Cole a URL de uma imagem (PNG, JPG ou SVG). Deixe em branco para usar o logo padrão.
                  </p>
                  
                  {logoUrl && (
                    <div className="mt-3 p-4 border rounded-lg bg-gray-50">
                      <p className="text-sm font-medium mb-2">Pré-visualização:</p>
                      <img 
                        src={logoUrl} 
                        alt="Logo preview" 
                        className="h-16 w-auto object-contain"
                        onError={(e) => {
                          e.currentTarget.src = '/placeholder-logo.png'
                          e.currentTarget.alt = 'Erro ao carregar logo'
                        }}
                      />
                    </div>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="primaryColor">
                    Cor Primária
                  </Label>
                  <div className="flex gap-3 items-center">
                    <Input
                      id="primaryColor"
                      type="color"
                      value={primaryColor}
                      onChange={(e) => setPrimaryColor(e.target.value)}
                      className="w-24 h-12 cursor-pointer"
                    />
                    <Input
                      type="text"
                      value={primaryColor}
                      onChange={(e) => setPrimaryColor(e.target.value)}
                      placeholder="#3B82F6"
                      pattern="^#[0-9A-Fa-f]{6}$"
                      className="flex-1"
                    />
                  </div>
                  <p className="text-sm text-muted-foreground">
                    Cor principal usada em botões e destaques (formato: #RRGGBB)
                  </p>
                </div>
              </CardContent>
            </Card>

            {/* Preview */}
            <Card>
              <CardHeader>
                <CardTitle>Pré-visualização</CardTitle>
                <CardDescription>
                  Veja como ficará a barra lateral
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="border rounded-lg p-4 bg-[#075E54] text-white">
                  <div className="flex items-center gap-3 mb-4">
                    {logoUrl ? (
                      <img 
                        src={logoUrl} 
                        alt={name} 
                        className="h-10 w-auto object-contain bg-white rounded px-2"
                      />
                    ) : (
                      <div className="h-10 w-10 rounded bg-white flex items-center justify-center text-[#075E54] font-bold">
                        {name.charAt(0) || 'A'}
                      </div>
                    )}
                    <span className="font-semibold text-lg">{name || 'Sua Empresa'}</span>
                  </div>
                  <div 
                    className="h-10 rounded flex items-center justify-center text-white font-medium"
                    style={{ backgroundColor: primaryColor }}
                  >
                    Botão Exemplo
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Actions */}
            <div className="flex justify-end gap-3">
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  setName(settings?.name || '')
                  setLogoUrl(settings?.logoUrl || '')
                  setPrimaryColor(settings?.primaryColor || '#3B82F6')
                }}
                disabled={isSaving}
              >
                Cancelar
              </Button>
              <Button type="submit" disabled={isSaving}>
                {isSaving ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
                    Salvando...
                  </>
                ) : (
                  <>
                    <Save className="h-4 w-4 mr-2" />
                    Salvar Alterações
                  </>
                )}
              </Button>
            </div>
          </form>
        )}
      </div>
    </div>
  )
}
