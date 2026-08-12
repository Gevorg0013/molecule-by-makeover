import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router-dom'
import { z } from 'zod'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { authErrorMessage, useResetPassword } from '@/hooks/useAuth'

const schema = z
  .object({
    newPassword: z
      .string()
      .min(8)
      .regex(/[a-z]/, { message: 'lowercase required' })
      .regex(/[A-Z]/, { message: 'uppercase required' })
      .regex(/\d/, { message: 'digit required' }),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'auth.passwordsDontMatch',
    path: ['confirmPassword'],
  })

type FormValues = z.infer<typeof schema>

export function ResetPasswordPage() {
  const { t } = useTranslation()
  const [searchParams] = useSearchParams()
  const email = searchParams.get('email') ?? ''
  const token = searchParams.get('token') ?? ''
  const resetPassword = useResetPassword()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) })

  if (!email || !token) {
    return (
      <div>
        <h1 className="font-serif text-2xl">{t('auth.resetPasswordTitle')}</h1>
        <p className="mt-4 text-sm text-destructive">{t('auth.invalidResetLink')}</p>
        <p className="mt-6 text-center text-sm text-muted-foreground">
          <Link to="/forgot-password" className="font-medium text-foreground hover:underline">
            {t('auth.forgotPasswordTitle')}
          </Link>
        </p>
      </div>
    )
  }

  function onSubmit(values: FormValues) {
    resetPassword.mutate({ email, token, newPassword: values.newPassword })
  }

  if (resetPassword.isSuccess) {
    return (
      <div>
        <h1 className="font-serif text-2xl">{t('auth.resetPasswordTitle')}</h1>
        <p className="mt-4 text-sm text-muted-foreground">{t('auth.resetPasswordSuccess')}</p>
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
      <h1 className="font-serif text-2xl">{t('auth.resetPasswordTitle')}</h1>
      <p className="mt-1 text-sm text-muted-foreground">{t('auth.resetPasswordSubtitle')}</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 flex flex-col gap-4">
        <div>
          <Label className="mb-1.5">{t('auth.newPassword')}</Label>
          <Input type="password" {...register('newPassword')} />
          <p className="mt-1 text-xs text-muted-foreground">{t('auth.passwordHint')}</p>
          {errors.newPassword && <p className="mt-1 text-xs text-destructive">{errors.newPassword.message}</p>}
        </div>
        <div>
          <Label className="mb-1.5">{t('auth.confirmPassword')}</Label>
          <Input type="password" {...register('confirmPassword')} />
          {errors.confirmPassword && (
            <p className="mt-1 text-xs text-destructive">{t(errors.confirmPassword.message ?? '')}</p>
          )}
        </div>
        {resetPassword.isError && <p className="text-sm text-destructive">{authErrorMessage(resetPassword.error)}</p>}
        <Button type="submit" size="lg" disabled={resetPassword.isPending}>
          {t('common.submit')}
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
