'use client'

import { Header } from '@/components/dashboard/header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Calendar, Users, Clock, TrendingUp } from 'lucide-react'
import { useAppointments } from '@/hooks/use-appointments'
import { useCustomers } from '@/hooks/use-customers'
import { format, startOfDay, endOfDay, addDays, isWithinInterval } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import Link from 'next/link'
import { Button } from '@/components/ui/button'

export default function DashboardPage() {
  const { data: appointments, isLoading: appointmentsLoading } = useAppointments()
  const { data: customers, isLoading: customersLoading } = useCustomers()

  const today = new Date()
  const todayStart = startOfDay(today)
  const todayEnd = endOfDay(today)
  const next7Days = endOfDay(addDays(today, 7))

  const appointmentsToday = appointments?.filter(apt => {
    const aptDate = new Date(apt.startTime)
    return isWithinInterval(aptDate, { start: todayStart, end: todayEnd })
  }).length || 0

  const appointmentsNext7Days = appointments?.filter(apt => {
    const aptDate = new Date(apt.startTime)
    return isWithinInterval(aptDate, { start: today, end: next7Days })
  }).length || 0

  const totalCustomers = customers?.length || 0

  const isLoading = appointmentsLoading || customersLoading

  return (
    <div>
      <Header 
        title="Dashboard" 
        description="Visão geral do seu negócio"
      />
      
      <div className="p-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-semibold text-[#333333]">
                Agendamentos Hoje
              </CardTitle>
              <div className="h-10 w-10 rounded-full bg-[#25D366]/10 flex items-center justify-center">
                <Calendar className="h-5 w-5 text-[#25D366]" />
              </div>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="h-8 w-16 bg-gray-200 animate-pulse rounded" />
              ) : (
                <>
                  <div className="text-3xl font-bold text-[#075E54]">{appointmentsToday}</div>
                  <p className="text-xs text-[#333333] mt-1">
                    {appointmentsToday === 0 ? 'Nenhum agendamento para hoje' : 
                     appointmentsToday === 1 ? '1 agendamento hoje' :
                     `${appointmentsToday} agendamentos hoje`}
                  </p>
                </>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-semibold text-[#333333]">
                Total de Clientes
              </CardTitle>
              <div className="h-10 w-10 rounded-full bg-[#25D366]/10 flex items-center justify-center">
                <Users className="h-5 w-5 text-[#25D366]" />
              </div>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="h-8 w-16 bg-gray-200 animate-pulse rounded" />
              ) : (
                <>
                  <div className="text-3xl font-bold text-[#075E54]">{totalCustomers}</div>
                  <p className="text-xs text-[#333333] mt-1">
                    {totalCustomers === 0 ? 'Cadastre seus primeiros clientes' :
                     totalCustomers === 1 ? '1 cliente cadastrado' :
                     `${totalCustomers} clientes cadastrados`}
                  </p>
                </>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-semibold text-[#333333]">
                Próximos 7 dias
              </CardTitle>
              <div className="h-10 w-10 rounded-full bg-[#25D366]/10 flex items-center justify-center">
                <Clock className="h-5 w-5 text-[#25D366]" />
              </div>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="h-8 w-16 bg-gray-200 animate-pulse rounded" />
              ) : (
                <>
                  <div className="text-3xl font-bold text-[#075E54]">{appointmentsNext7Days}</div>
                  <p className="text-xs text-[#333333] mt-1">
                    {appointmentsNext7Days === 0 ? 'Nenhum agendamento na próxima semana' :
                     appointmentsNext7Days === 1 ? '1 agendamento agendado' :
                     `${appointmentsNext7Days} agendamentos agendados`}
                  </p>
                </>
              )}
            </CardContent>
          </Card>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-[#075E54]">Começando</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <p className="text-[#333333]">
                  Bem-vindo ao Astra Agenda! Para começar:
                </p>
                <ol className="list-decimal list-inside space-y-3 text-sm">
                  <li className="flex items-center justify-between">
                    <span className="text-[#333333]">Cadastre seus primeiros clientes</span>
                    <Link href="/dashboard/customers">
                      <Button size="sm" variant="outline">
                        Ir para Clientes
                      </Button>
                    </Link>
                  </li>
                  <li className="flex items-center justify-between">
                    <span className="text-[#333333]">Crie um agendamento de teste</span>
                    <Link href="/dashboard/appointments">
                      <Button size="sm" variant="outline">
                        Ir para Agendamentos
                      </Button>
                    </Link>
                  </li>
                  <li>
                    <span className="text-[#333333]">Explore os recursos disponíveis no menu lateral</span>
                  </li>
                </ol>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-[#075E54]">Próximos Agendamentos</CardTitle>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <div className="space-y-3">
                  <div className="h-16 bg-gray-200 animate-pulse rounded" />
                  <div className="h-16 bg-gray-200 animate-pulse rounded" />
                </div>
              ) : appointments && appointments.length > 0 ? (
                <div className="space-y-3">
                  {appointments
                    .filter(apt => new Date(apt.startTime) >= today)
                    .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
                    .slice(0, 3)
                    .map(apt => (
                      <div key={apt.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200">
                        <div>
                          <p className="font-semibold text-sm text-[#075E54]">{apt.customer?.name || 'Cliente'}</p>
                          <p className="text-xs text-[#333333]">
                            {format(new Date(apt.startTime), "dd/MM/yyyy 'às' HH:mm", { locale: ptBR })}
                          </p>
                        </div>
                        <span className={`px-3 py-1 text-xs font-semibold rounded-full ${
                          apt.status === 'confirmed' ? 'bg-[#25D366]/20 text-[#075E54]' :
                          apt.status === 'scheduled' ? 'bg-blue-100 text-blue-800' :
                          'bg-gray-100 text-gray-800'
                        }`}>
                          {apt.status === 'confirmed' ? 'Confirmado' :
                           apt.status === 'scheduled' ? 'Agendado' :
                           apt.status}
                        </span>
                      </div>
                    ))}
                  {appointments.filter(apt => new Date(apt.startTime) >= today).length === 0 && (
                    <p className="text-sm text-muted-foreground text-center py-4">
                      Nenhum agendamento futuro
                    </p>
                  )}
                </div>
              ) : (
                <p className="text-sm text-muted-foreground text-center py-8">
                  Nenhum agendamento cadastrado ainda
                </p>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
