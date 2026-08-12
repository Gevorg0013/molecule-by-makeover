import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { z } from 'zod'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { authErrorMessage, useLogin } from '@/hooks/useAuth'

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(1),
})

type FormValues = z.infer<typeof schema>

export function LoginPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const login = useLogin()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) })

  function onSubmit(values: FormValues) {
    login.mutate(values, {
      onSuccess: () => {
        const from = (location.state as { from?: { pathname: string } } | null)?.from?.pathname ?? '/'
        navigate(from, { replace: true })
      },
    })
  }

  return (
    <div>
      <h1 className="font-serif text-2xl">{t('auth.loginTitle')}</h1>
      <p className="mt-1 text-sm text-muted-foreground">{t('auth.loginSubtitle')}</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 flex flex-col gap-4">
        <div>
          <Label className="mb-1.5">{t('common.email')}</Label>
          <Input type="email" {...register('email')} />
          {errors.email && <p className="mt-1 text-xs text-destructive">{errors.email.message}</p>}
        </div>
        <div>
          <Label className="mb-1.5">{t('auth.password')}</Label>
          <Input type="password" {...register('password')} />
          {errors.password && <p className="mt-1 text-xs text-destructive">{errors.password.message}</p>}
          <Link to="/forgot-password" className="mt-1.5 inline-block text-sm text-muted-foreground hover:underline">
            {t('auth.forgotPasswordLink')}
          </Link>
        </div>
        {login.isError && <p className="text-sm text-destructive">{authErrorMessage(login.error)}</p>}
        <Button type="submit" size="lg" disabled={login.isPending}>
          {t('auth.signIn')}
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        {t('auth.noAccount')}{' '}
        <Link to="/register" className="font-medium text-foreground hover:underline">
          {t('auth.signUp')}
        </Link>
      </p>
    </div>
  )
}
