import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { z } from 'zod'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { authErrorMessage, useForgotPassword } from '@/hooks/useAuth'

const schema = z.object({
  email: z.string().email(),
})

type FormValues = z.infer<typeof schema>

export function ForgotPasswordPage() {
  const { t } = useTranslation()
  const forgotPassword = useForgotPassword()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) })

  function onSubmit(values: FormValues) {
    forgotPassword.mutate(values)
  }

  if (forgotPassword.isSuccess) {
    return (
      <div>
        <h1 className="font-serif text-2xl">{t('auth.forgotPasswordTitle')}</h1>
        <p className="mt-4 text-sm text-muted-foreground">{t('auth.forgotPasswordSuccess')}</p>
        <p className="mt-6 text-center text-sm text-muted-foreground">
          <Link to="/login" className="font-medium text-foreground hover:underline">
            {t('auth.backToLogin')}
          </Link>
        </p>
      </div>
    )
  }

  return (
    <div>
      <h1 className="font-serif text-2xl">{t('auth.forgotPasswordTitle')}</h1>
      <p className="mt-1 text-sm text-muted-foreground">{t('auth.forgotPasswordSubtitle')}</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 flex flex-col gap-4">
        <div>
          <Label className="mb-1.5">{t('common.email')}</Label>
          <Input type="email" {...register('email')} />
          {errors.email && <p className="mt-1 text-xs text-destructive">{errors.email.message}</p>}
        </div>
        {forgotPassword.isError && <p className="text-sm text-destructive">{authErrorMessage(forgotPassword.error)}</p>}
        <Button type="submit" size="lg" disabled={forgotPassword.isPending}>
          {t('auth.sendResetLink')}
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        <Link to="/login" className="font-medium text-foreground hover:underline">
          {t('auth.backToLogin')}
        </Link>
      </p>
    </div>
  )
}
